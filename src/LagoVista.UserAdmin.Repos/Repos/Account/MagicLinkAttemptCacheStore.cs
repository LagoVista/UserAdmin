using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.REpos.Account;
using LagoVista.UserAdmin.Models.Auth;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Account
{
    /// <summary>
    /// Cache-backed implementation of IMagicLinkAttemptStore.
    /// Uses atomic "claim" semantics via GetAndDeleteAsync on index records
    /// to guarantee single-use consumption of codes.
    /// </summary>
    public class MagicLinkAttemptCacheStore : IMagicLinkAttemptStore
    {
        private readonly ICacheProvider _cache;

        // Key prefixes
        private const string AttemptPrefix = "magic:attempt:";
        private const string CodeIndexPrefix = "magic:code:";
        private const string ExchangeIndexPrefix = "magic:exchange:";

        // Retention after consume for troubleshooting/auditing.
        // Keep modest so cache doesn't become a landfill.
        private static readonly TimeSpan ConsumedRetention = TimeSpan.FromHours(24);

        public MagicLinkAttemptCacheStore(ICacheProvider cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<InvokeResult> CreateAsync(MagicLinkAttempt attempt)
        {
            if (attempt == null) return InvokeResult.FromError("missing_attempt");
            if (string.IsNullOrWhiteSpace(attempt.Id)) return InvokeResult.FromError("missing_id");
            if (string.IsNullOrWhiteSpace(attempt.CodeHash)) return InvokeResult.FromError("missing_code_hash");
            if (attempt.ExpiresAtUtc == default) return InvokeResult.FromError("missing_expires_at");

            var nowUtc = DateTime.UtcNow;
            var ttl = ComputeTtl(attempt.ExpiresAtUtc, nowUtc, includeGrace: true);
            if (ttl <= TimeSpan.Zero) return InvokeResult.FromError("expired");

            var attemptKey = GetAttemptKey(attempt.Id);
            var codeKey = GetCodeIndexKey(attempt.CodeHash);

            // Store attempt record
            await _cache.AddAsync(attemptKey, attempt, ttl);

            // Store code -> attemptId index (used for atomic claim on consume)
            var index = new CodeIndex { AttemptId = attempt.Id };
            await _cache.AddAsync(codeKey, index, ttl);

            return InvokeResult.Success;
        }

        public async Task<MagicLinkAttempt> GetByCodeHashAsync(string codeHash)
        {
            if (string.IsNullOrWhiteSpace(codeHash)) return null;

            var codeKey = GetCodeIndexKey(codeHash);
            var index = await _cache.GetAsync<CodeIndex>(codeKey);
            if (index == null || string.IsNullOrWhiteSpace(index.AttemptId)) return null;

            var attemptKey = GetAttemptKey(index.AttemptId);
            return await _cache.GetAsync<MagicLinkAttempt>(attemptKey);
        }

        public async Task<InvokeResult<MagicLinkConsumeResult>> TryConsumeAsync(string codeHash, DateTime nowUtc)
        {
            if (string.IsNullOrWhiteSpace(codeHash))
                return InvokeResult<MagicLinkConsumeResult>.FromError("missing_code_hash");

            // Atomic claim: only one caller can get the index.
            var codeKey = GetCodeIndexKey(codeHash);
            var index = await _cache.GetAndDeleteAsync<CodeIndex>(codeKey);

            if (index == null || string.IsNullOrWhiteSpace(index.AttemptId))
            {
                return InvokeResult<MagicLinkConsumeResult>.FromError("not_found");
            }

            var attemptKey = GetAttemptKey(index.AttemptId);
            var attempt = await _cache.GetAsync<MagicLinkAttempt>(attemptKey);

            if (attempt == null)
            {
                return InvokeResult<MagicLinkConsumeResult>.FromError("not_found");
            }

            if (attempt.ConsumedAtUtc.HasValue)
            {
                // Defensive: should be rare because index is claimed once.
                return InvokeResult<MagicLinkConsumeResult>.FromError("consumed");
            }

            if (attempt.ExpiresAtUtc <= nowUtc)
            {
                return InvokeResult<MagicLinkConsumeResult>.FromError("expired");
            }

            attempt.ConsumedAtUtc = nowUtc;

            // Keep consumed attempt around briefly for audit/debug.
            await _cache.AddAsync(attemptKey, attempt, ConsumedRetention);

            return InvokeResult<MagicLinkConsumeResult>.Create(new MagicLinkConsumeResult
            {
                Attempt = attempt
            });
        }

        public async Task<InvokeResult> SetExchangeAsync(string attemptId, string exchangeCodeHash, DateTime exchangeExpiresAtUtc, DateTime nowUtc)
        {
            if (string.IsNullOrWhiteSpace(attemptId)) return InvokeResult.FromError("missing_attempt_id");
            if (string.IsNullOrWhiteSpace(exchangeCodeHash)) return InvokeResult.FromError("missing_exchange_code_hash");
            if (exchangeExpiresAtUtc == default) return InvokeResult.FromError("missing_exchange_expires_at");

            var attemptKey = GetAttemptKey(attemptId);
            var attempt = await _cache.GetAsync<MagicLinkAttempt>(attemptKey);

            if (attempt == null) return InvokeResult.FromError("not_found");
            if (attempt.ConsumedAtUtc.HasValue) return InvokeResult.FromError("consumed");
            if (attempt.ExpiresAtUtc <= nowUtc) return InvokeResult.FromError("expired");
            if (exchangeExpiresAtUtc <= nowUtc) return InvokeResult.FromError("exchange_expired");

            attempt.ExchangeCodeHash = exchangeCodeHash;
            attempt.ExchangeExpiresAtUtc = exchangeExpiresAtUtc;
            attempt.ExchangeConsumedAtUtc = null;

            // Exchange TTL is independent and typically shorter.
            var exchangeTtl = ComputeTtl(exchangeExpiresAtUtc, nowUtc, includeGrace: true);
            if (exchangeTtl <= TimeSpan.Zero) return InvokeResult.FromError("exchange_expired");

            // Store exchange index so exchange can be claimed atomically.
            var exchangeKey = GetExchangeIndexKey(exchangeCodeHash);
            var exchangeIndex = new CodeIndex { AttemptId = attemptId };
            await _cache.AddAsync(exchangeKey, exchangeIndex, exchangeTtl);

            // Ensure the attempt remains in cache at least until exchange expiry (plus grace).
            // Prefer the longer of: remaining magic-link TTL or exchange TTL.
            var attemptTtl = ComputeTtl(attempt.ExpiresAtUtc, nowUtc, includeGrace: true);
            var keepFor = attemptTtl > exchangeTtl ? attemptTtl : exchangeTtl;

            await _cache.AddAsync(attemptKey, attempt, keepFor);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult<MagicLinkExchangeConsumeResult>> TryConsumeExchangeAsync(string exchangeCodeHash, DateTime nowUtc)
        {
            if (string.IsNullOrWhiteSpace(exchangeCodeHash))
                return InvokeResult<MagicLinkExchangeConsumeResult>.FromError("missing_exchange_code_hash");

            // Atomic claim of the exchange code.
            var exchangeKey = GetExchangeIndexKey(exchangeCodeHash);
            var index = await _cache.GetAndDeleteAsync<CodeIndex>(exchangeKey);

            if (index == null || string.IsNullOrWhiteSpace(index.AttemptId))
            {
                return InvokeResult<MagicLinkExchangeConsumeResult>.FromError("not_found");
            }

            var attemptKey = GetAttemptKey(index.AttemptId);
            var attempt = await _cache.GetAsync<MagicLinkAttempt>(attemptKey);

            if (attempt == null)
            {
                return InvokeResult<MagicLinkExchangeConsumeResult>.FromError("not_found");
            }

            if (!string.Equals(attempt.ExchangeCodeHash, exchangeCodeHash, StringComparison.Ordinal))
            {
                // Defensive: mismatch should be rare; treat as invalid.
                return InvokeResult<MagicLinkExchangeConsumeResult>.FromError("invalid_exchange_code");
            }

            if (!attempt.ExchangeExpiresAtUtc.HasValue || attempt.ExchangeExpiresAtUtc.Value <= nowUtc)
            {
                return InvokeResult<MagicLinkExchangeConsumeResult>.FromError("exchange_expired");
            }

            if (attempt.ExchangeConsumedAtUtc.HasValue)
            {
                return InvokeResult<MagicLinkExchangeConsumeResult>.FromError("exchange_consumed");
            }

            attempt.ExchangeConsumedAtUtc = nowUtc;

            // Keep consumed attempt around briefly.
            await _cache.AddAsync(attemptKey, attempt, ConsumedRetention);

            return InvokeResult<MagicLinkExchangeConsumeResult>.Create(new MagicLinkExchangeConsumeResult
            {
                Attempt = attempt
            });
        }

        public Task<InvokeResult> DeleteAsync(string attemptId)
        {
            if (string.IsNullOrWhiteSpace(attemptId))
                return Task.FromResult(InvokeResult.FromError("missing_attempt_id"));

            var attemptKey = GetAttemptKey(attemptId);
            return DeleteAttemptOnlyAsync(attemptKey);
        }

        public Task<InvokeResult<int>> DeleteExpiredAsync(DateTime olderThanUtc)
        {
            // CacheProvider has no key enumeration; cleanup is TTL-driven.
            return Task.FromResult(InvokeResult<int>.Create(0));
        }

        private async Task<InvokeResult> DeleteAttemptOnlyAsync(string attemptKey)
        {
            await _cache.RemoveAsync(attemptKey);
            return InvokeResult.Success;
        }

        private static string GetAttemptKey(string attemptId) => $"{AttemptPrefix}{attemptId}";
        private static string GetCodeIndexKey(string codeHash) => $"{CodeIndexPrefix}{codeHash}";
        private static string GetExchangeIndexKey(string exchangeCodeHash) => $"{ExchangeIndexPrefix}{exchangeCodeHash}";

        private static TimeSpan ComputeTtl(DateTime expiresAtUtc, DateTime nowUtc, bool includeGrace)
        {
            var ttl = expiresAtUtc - nowUtc;
            if (ttl < TimeSpan.Zero) return TimeSpan.Zero;

            // Add a small grace period so we can still diagnose near-edge cases.
            if (includeGrace) ttl = ttl + TimeSpan.FromMinutes(5);

            return ttl;
        }

        private class CodeIndex
        {
            public string AttemptId { get; set; }
        }
    }
}
