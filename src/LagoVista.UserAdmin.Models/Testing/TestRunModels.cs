using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Testing
{
    public enum TestRunStatus
    {
        Created,
        Running,
        Passed,
        Failed,
        Aborted,
    }

    /// <summary>
    /// Persisted record of a single ceremony execution.
    /// V1: 1 run = 1 ceremony.
    /// </summary>
    public class AppUserTestRun : EntityBase, ISummaryFactory
    {

        /// <summary>
        /// Human-friendly sequential identifier for support/log review (e.g., LOG-000123).
        /// Assigned by the runner or persistence layer using a serial number manager.
        /// </summary>
        public string RunCode { get; set; }


        public EntityHeader<AppUserTestScenario> TestScenario { get; set; }

        /// <summary>
        /// Environment metadata.
        /// </summary>
        public string BaseUrl { get; set; }
        public string TenantId { get; set; }

        public string Started { get; set; }
        public string Finished { get; set; }

        public TestRunStatus Status { get; set; } = TestRunStatus.Created;

        /// <summary>
        /// Final verification payloads.
        /// </summary>
        public TestRunVerification Verification { get; set; } = new TestRunVerification();

        /// <summary>
        /// Free-form tags for searching/grouping.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public List<AppUserTestRunEvent> Events { get; set; } = new List<AppUserTestRunEvent>();

        public List<TestRunArtifact> Artifacts { get; set; } = new List<TestRunArtifact>();

        public AppUserTestRunSummary CreateSummary()
        {
            var summary = new AppUserTestRunSummary();
            summary.Populate(this);
            return summary;
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }

    }

    public class AppUserTestRunSummary : SummaryData
    {
        
    }

    public class TestRunVerification
    {
        public AuthTenantStateSnapshot FinalSnapshot { get; set; }

        public string ComputedDefaultLanding { get; set; }
        public bool? ComputedIsFullyConfigured { get; set; }

        public AuthLogReviewSummary AuthLogReview { get; set; } = new AuthLogReviewSummary();

        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class TestRunArtifact
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
    }

    public class AuthLogReviewSummary
    {
        public string FromUtc { get; set; }
        public string ToUtc { get; set; }

        public List<string> ExpectedEventsMissing { get; set; } = new List<string>();
        public List<string> ObservedEvents { get; set; } = new List<string>();
    }

    public enum TestRunEventLevel
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
    }

    /// <summary>
    /// Append-only structured event emitted by the runner.
    /// Storage layer should assign Seq to enforce ordering.
    /// </summary>
    public class AppUserTestRunEvent
    {
        /// <summary>
        /// Assigned by the persistence layer (monotonic per RunId).
        /// </summary>
        public long Seq { get; set; }

        public string Timestamp { get; set; }
        public TestRunEventLevel Level { get; set; } = TestRunEventLevel.Info;

        /// <summary>
        /// Stable event type key (e.g., "ui.navigate", "http.snapshot", "assert.fail").
        /// </summary>
        public string EventType { get; set; }

        public string Message { get; set; }

        /// <summary>
        /// Structured data payload (JSON-friendly). Values should be primitives/strings.
        /// </summary>
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
    }
}
