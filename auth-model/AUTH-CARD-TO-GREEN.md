# Authentication Card -> Green Runbook

## Purpose

This runbook defines the first-cut process for taking one Auth Card from candidate inventory to a trustworthy green state.

Auth Cards are not AuthViews and are not Journeys. Each card is one bounded secure interaction launched by exactly one ClientDirective and completed by a correlated outcome-only response to the agent.

The canonical authentication model remains authoritative for guards, state transitions, security semantics, and server behavior.

## Green dimensions

Track authored completeness separately from runtime evidence.

Recommended authored phases are:

- contract
- canonical-bindings
- presentation
- implementation
- tests

A card is fully green only when authored work is complete and required current runtime evidence passes.

## 1. Select or propose the card

Confirm that the interaction has exactly one bounded purpose.

Do not create a card merely because an Auth Category, Behavior, or Scenario exists. Some canonical authentication material will have no conversational card presentation.

Reject or split candidates that require multi-step card navigation, multiple materially different operations, or structured response data.

## 2. Reconcile the directive contract

Confirm:

- one ClientDirective maps to exactly this card;
- the directive has a stable semantic identity;
- correlation id ownership is clear;
- invocation payload is empty where possible;
- any typed invocation payload is minimal and contains no credentials or authentication secrets.

## 3. Reconcile canonical authentication bindings

Identify the existing canonical definitions the card uses:

- Behavior keys;
- Scenario keys;
- Action keys;
- Transition keys;
- AuthView keys only where existing presentation semantics are genuinely reused.

The card must not invent authentication guards, mutations, or security outcomes.

A card may bind several outcome-specific Behaviors or Scenarios while still representing one bounded authentication capability.

## 4. Reconcile card presentation

Define only the controls, actions, and local presentation states needed for the bounded interaction.

The card has no AuthRoute requirement.

A card must never transition directly to another card. Every terminal outcome returns control to the invoking agent.

Angular Web and React Native share semantic identities even when their visual implementations differ.

## 5. Reconcile outcomes and response boundary

Every outcome is terminal for the card invocation.

Auth Card responses contain:

- correlation metadata; and
- one typed control-flow outcome.

Auth Card responses MUST NOT contain a response value of any kind.

They MUST NOT return newly collected authentication, identity, account, claim, credential, provider, or profile data.

If the agent needs authoritative state after completion, it must retrieve that state through an allowed backend tool.

## 6. Reconcile client implementations

Verify both declared supported clients:

- Angular Web;
- React Native.

For each client confirm:

- the correct directive launches the correct card;
- semantic controls/actions match the card contract;
- local states do not introduce unmodeled authentication semantics;
- the existing canonical server operation is used where applicable;
- every terminal result returns the same correlation id;
- only an allowed outcome is returned;
- no Auth response value or newly collected data is returned.

## 7. Reconcile tests

Tests should prove both the card protocol and the canonical authentication behavior it relies upon without duplicating existing server proof unnecessarily.

At minimum prove:

- directive -> expected card;
- card interaction -> expected canonical server behavior where server-backed;
- expected terminal outcome;
- same correlation id returned;
- card closes/terminates;
- no response value for Auth Cards;
- required authoritative authentication post-state through the existing proof/evidence mechanisms.

## 8. Runtime evidence

Execute required platform tests and record runtime evidence separately from authored JSON.

A visually correct card with the wrong server-side authentication result is not green.

Likewise, a correct server transition with a client that leaks response data, returns the wrong outcome, or fails correlation is not green.

## Reference specimen

The first reference specimen should be intentionally small. Password Sign-In is a good candidate because its underlying canonical behaviors, scenarios, implementation bindings, and tests are already mature while its card presentation remains a new concern.
