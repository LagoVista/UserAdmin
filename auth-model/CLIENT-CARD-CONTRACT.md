# Client Card Contract

## Purpose

Client Cards are bounded client-side interactions launched by an agent through a ClientDirective and completed by a correlated callback to the agent.

This contract is authored in `UserAdmin/auth-model` first because Authentication is the initial consumer. The general Client Card concept may later graduate to the AI repository once the contract is proven.

## Core lifecycle

1. The agent sends exactly one ClientDirective for exactly one Client Card interaction.
2. The directive carries a correlation id and an optional small, card-specific invocation payload.
3. The client renders or performs the bounded interaction. Internal client behavior is not part of the agent protocol.
4. The client completes the interaction and calls back to the agent using a special response type with the same correlation id.
5. The response contains a typed control-flow outcome and, only when permitted by the card contract, one bounded response value.
6. Control returns to the agent. A card never directly launches or navigates to another card.

## Hard invariants

### One directive, one card, one bounded purpose

A ClientDirective maps 1:1 to a Client Card definition. A card exists to perform one bounded interaction.

If an interaction begins to require materially different operations, multi-step navigation, or complex state transfer, split it into multiple cards rather than expanding the card into a mini application or form system.

Local presentation states such as idle, submitting, rejected, or disabled are allowed when they help render the one bounded interaction. They do not create card-to-card navigation or new domain semantics.

### Correlation

Every invocation and response carries the same correlation id so the agent runtime can resume the correct suspended interaction.

### Invocation payload

Each card owns a small typed invocation payload contract. Empty payloads are preferred.

Invocation payloads should contain only the minimum context needed to start the interaction. Opaque references and presentation-safe hints are preferred over copied authoritative state.

Credentials and authentication secrets must never be carried in a ClientDirective payload.

### Response outcomes

Every card defines a finite set of typed control-flow outcomes. Outcomes answer what happened and what control-flow decision may be appropriate next. They are not a replacement for authoritative backend state.

### Response values

A general Client Card response may contain no value or exactly one bounded value.

Allowed response value shapes are:

- one scalar string, boolean, or number;
- one EntityHeader, which is explicitly treated as a scalar for this contract;
- one homogeneous multi-select collection of scalar values; or
- one homogeneous multi-select collection of EntityHeaders.

Arbitrary objects, key/value bags, nested payloads, mixed collections, and form-shaped response data are prohibited.

If an interaction needs richer data than this contract allows, redesign the interaction, split it into multiple cards, or persist authoritative state elsewhere and retrieve it through an appropriate backend tool.

## Authentication specialization

Auth Cards are a restricted species of Client Card.

An Auth Card:

- is launched by one auth-specific ClientDirective;
- performs one bounded secure authentication interaction;
- may reuse canonical authentication Behaviors, Scenarios, Actions, Transitions, AuthViews, flow handlers, endpoints, and generated clients where appropriate;
- does not require an AuthRoute;
- never directly navigates to or launches another Auth Card;
- returns only a typed control-flow outcome plus correlation metadata;
- MUST NOT return a response value of any kind;
- MUST NOT return newly collected authentication, identity, account, claim, credential, provider, or profile data;
- treats its outcome as a control signal, not authoritative authentication state.

If the agent needs authoritative information after an Auth Card completes, it must query backend state through an allowed tool.

Examples of acceptable Auth Card outcomes include completed, canceled, failed, expired, email-verified, email-not-verified, totp-required, passkey-required, or selection-required when those outcomes are meaningful for that specific card.

## Relationship to canonical authentication truth

Client Cards do not redefine authentication behavior.

For Auth Cards, the authoritative behavioral chain remains:

State / Invariant -> Action -> Transition -> Scenario -> Behavior

The Auth Card binds presentation and agent-control semantics to that existing graph. Authentication guards, state mutation, security policy, and authoritative postconditions remain owned by the canonical authentication model and server implementation.

## Platform parity

Angular Web and React Native may implement cards differently visually, but they must share the same semantic card identity, directive contract, controls/actions where applicable, terminal outcomes, and Auth Card security restrictions.

## Visualization direction

Aptix should visualize Client Cards separately from AuthViews.

For Auth Cards, the primary visualization should emphasize:

ClientDirective -> Auth Card -> canonical authentication bindings -> terminal outcomes

rather than route-oriented view navigation.

The visualization should make Angular and React Native implementation/conformance status visible without implying that either client owns authentication semantics.

## First-cut scope

This first cut establishes the contract and schemas only. It intentionally does not claim Angular or React Native implementation completeness and does not yet define a reference Password Sign-In card.

The next useful step is to author one small Auth Card specimen and use it to drive a dedicated Card -> Green reconciliation process before broad inventory expansion.
