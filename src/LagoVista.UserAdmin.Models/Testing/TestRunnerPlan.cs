using LagoVista.Core.Models;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Testing
{
    /// <summary>
    /// What the runner needs to execute one ceremony run.
    /// V1: 1 plan => 1 run.
    /// The runner does not evaluate pre/post conditions; it only executes and reports observations.
    /// </summary>
    public class AuthRunnerPlan
    {
        /// <summary>Server-assigned run id (or correlation id) used for logging and reporting.</summary>
        public string RunId { get; set; }

        /// <summary>Human-friendly name for UI/logging.</summary>
        public EntityHeader Scenario { get; set; }

        /// <summary>Optional: base URL (runner may override via config).</summary>
        public string BaseUrl { get; set; }

        /// <summary>Route to navigate to at start (e.g. "auth/login/email").</summary>
        public string StartRoute { get; set; }

        /// <summary>
        /// Optional: expected view id after navigation to StartRoute (e.g. "auth.login.email").
        /// Runner may log if mismatch but should not fail the run on its own.
        /// </summary>
        public string StartViewId { get; set; }
        public string EmailConfirmToken { get; set; }
        public string OrgInvitationId { get; set; }

        /// <summary>Inputs to apply before invoking the action.</summary>
        public List<AuthRunnerInput> Inputs { get; set; } = new List<AuthRunnerInput>();

        /// <summary>Exactly one action to invoke for this plan.</summary>
        public AuthRunnerAction Action { get; set; } = new AuthRunnerAction();

        /// <summary>Hints for what the runner should observe and report (not validate).</summary>
        public AuthRunnerObservations Observations { get; set; } = new AuthRunnerObservations();

        /// <summary>Execution options for debugging.</summary>
        public AuthRunnerOptions Options { get; set; } = new AuthRunnerOptions();
    
        public TestUserCredentials UserCredentials { get; set; }
    }


    public class TestUserCredentials
    {
        public string InviteId { get; set; }
        public string ConfirmEmailToken { get; set; }

        public string EmailAddress { get; set; }
        public string Password { get; set; }
    
        public string PasskeyCredentialsId { get; set; }
    }

    /// <summary>
    /// One input assignment: find an element and set its value.
    /// Finder should be a CSS selector compatible with Playwright (e.g. [data-testid="field:email"]).
    /// </summary>
    public class AuthRunnerInput
    {
        public string Name { get; set; }   // "email", "password" (for logs)
        public string Finder { get; set; } // CSS selector
        public string Value { get; set; }  // resolved value (no secrets if persisted)

        /// <summary>
        /// Optional: informs runner how to set the value (Text is default).
        /// If omitted, runner can infer for checkboxes by element type at runtime.
        /// </summary>
        public string Kind { get; set; }
    }

    /// <summary>
    /// Exactly one action to click/submit.
    /// Finder should be a CSS selector (e.g. [data-testid="action:next"]).
    /// </summary>
    public class AuthRunnerAction
    {
        public string Name { get; set; }   // "next", "cancel", "oauth-google" (for logs)
        public string Finder { get; set; } // CSS selector
    }

    /// <summary>
    /// What evidence the runner should capture and return to the server.
    /// These are hints; server determines pass/fail.
    /// </summary>
    public class AuthRunnerObservations
    {
        /// <summary>
        /// CSS selector for the screen root (default: [data-testid="auth-screen"]).
        /// Runner can read data-screen-id from this element if present.
        /// </summary>
        public string ScreenRootFinder { get; set; } = "[data-testid=\"auth-screen\"]";

        /// <summary>
        /// Attribute name on screen root containing the semantic screen id (default: data-screen-id).
        /// </summary>
        public string ScreenIdAttribute { get; set; } = "data-screen-id";

        /// <summary>
        /// Optional: expected view id after action (e.g. "auth.home" or "auth.email.confirmed").
        /// Runner logs mismatch; server decides.
        /// </summary>
        public string ExpectedEndViewId { get; set; }

        /// <summary>
        /// Optional: expected route path after action (e.g. "/home" or "/auth/email/confirm").
        /// Runner logs mismatch; server decides.
        /// </summary>
        public string ExpectedEndRoute { get; set; }

        /// <summary>
        /// Optional: if present, runner should wait for this selector to disappear before continuing (e.g. [data-testid="state:busy"]).
        /// </summary>
        public string BusyStateFinder { get; set; }
    }

    /// <summary>
    /// Runner behavior controls (debugging, timing, evidence).
    /// </summary>
    public class AuthRunnerOptions
    {
        public bool Headless { get; set; } = true;
        public int SlowMoMs { get; set; } = 0;

        /// <summary>Default timeout for waits/actions in ms.</summary>
        public int TimeoutMs { get; set; } = 30000;

        /// <summary>
        /// If true, runner enables Playwright tracing and uploads trace path as an artifact reference.
        /// </summary>
        public bool EnableTracing { get; set; } = false;
    }

    /// <summary>
    /// What the runner posts back after execution.
    /// Server uses this + its own snapshot/authlog checks to determine pass/fail.
    /// </summary>
    public class AuthRunnerResult
    {
        public string RunId { get; set; }

        public string RunStarted { get; set; }
        public string RunEnded { get; set; }

        public AuthRunnerStatus Status { get; set; } = AuthRunnerStatus.Completed;

        /// <summary>Final browser URL at end of run.</summary>
        public string FinalUrl { get; set; }

        /// <summary>Observed end view id read from screen root attribute (if available).</summary>
        public string FinalViewId { get; set; }

        /// <summary>Optional: any mismatch notes the runner observed (does not imply failure).</summary>
        public List<string> Notes { get; set; } = new List<string>();

        /// <summary>Optional: reference(s) to artifacts such as trace.zip, screenshots.</summary>
        public List<AuthRunnerArtifact> Artifacts { get; set; } = new List<AuthRunnerArtifact>();
    }

    public enum AuthRunnerStatus
    {
        Completed = 0,
        Aborted = 1,
        FailedToExecute = 2
    }

    public class AuthRunnerArtifact
    {
        public string Name { get; set; }  // "trace.zip"
        public string Kind { get; set; }  // "trace" | "screenshot" | "video"
        public string Ref { get; set; }   // URL or server-side id
    }
}
