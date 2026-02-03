using System;
using System.Threading.Tasks;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;

namespace LagoVista.UserAdmin.Interfaces.REpos.Account
{
    /// <summary>
    /// Persistence contract for MagicLinkAttempt records.
    /// Storage implementations MUST enforce atomic single-use consumption.
    /// </summary>
    public interface IMagicLinkAttemptStore
    {
        /// <summary>
        /// Creates a new magic link attempt record.
        /// Implementations SHOULD enforce uniqueness on Id and MAY enforce additional constraints (e.g., latest-per-email).
        /// </summary>
        Task<InvokeResult> CreateAsync(MagicLinkAttempt attempt);

        /// <summary>
        /// Loads an attempt by hashed magic-link code.
        /// Implementations MAY omit returning consumed/expired records if desired, but must do so consistently.
        /// </summary>
        Task<MagicLinkAttempt> GetByCodeHashAsync(string codeHash);

        /// <summary>
        /// Atomically marks the attempt as consumed if and only if:
        /// - it exists
        /// - it is not consumed
        /// - it is not expired
        /// Returns a result indicating success/failure and the updated attempt when successful.
        /// </summary>
        Task<InvokeResult<MagicLinkConsumeResult>> TryConsumeAsync(string codeHash, DateTime nowUtc);

        /// <summary>
        /// Sets/rotates the mobile exchange code material for an existing attempt.
        /// Intended for Channel == mobile.
        /// Implementations SHOULD require the attempt to be unconsumed and unexpired.
        /// </summary>
        Task<InvokeResult> SetExchangeAsync(string attemptId, string exchangeCodeHash, DateTime exchangeExpiresAtUtc, DateTime nowUtc);

        /// <summary>
        /// Atomically consumes an exchange code if and only if:
        /// - exchange code exists
        /// - exchange not consumed
        /// - exchange not expired
        /// Returns the attempt for downstream JWT issuance.
        /// </summary>
        Task<InvokeResult<MagicLinkExchangeConsumeResult>> TryConsumeExchangeAsync(string exchangeCodeHash, DateTime nowUtc);

        /// <summary>
        /// Deletes an attempt by id. Optional but useful for cleanup and security hygiene.
        /// </summary>
        Task<InvokeResult> DeleteAsync(string attemptId);

        /// <summary>
        /// Optional cleanup hook. Cache implementations may no-op; DB implementations can prune old rows.
        /// </summary>
        Task<InvokeResult<int>> DeleteExpiredAsync(DateTime olderThanUtc);
    }

    public class MagicLinkConsumeResult
    {
        /// <summary>
        /// The attempt after successful consumption (ConsumedAtUtc set).
        /// </summary>
        public MagicLinkAttempt Attempt { get; set; }

        /// <summary>
        /// Failure reason when consumption fails (e.g., not_found, expired, consumed).
        /// </summary>
        public string FailureReason { get; set; }
    }

    public class MagicLinkExchangeConsumeResult
    {
        /// <summary>
        /// The attempt after successful exchange consumption (ExchangeConsumedAtUtc set).
        /// </summary>
        public MagicLinkAttempt Attempt { get; set; }

        /// <summary>
        /// Failure reason when exchange consumption fails (e.g., not_found, expired, consumed).
        /// </summary>
        public string FailureReason { get; set; }
    }
}
