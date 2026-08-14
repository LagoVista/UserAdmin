# Client AuthView Conformance Manifests

This directory contains checked-in observations of what supported authentication clients actually implement.

Generate and maintain manifests by following:

`auth-model/CLIENT-CONFORMANCE-RECONCILIATION.md`

Expected manifests:

- `angular-web.json`
- `react-native.json`

Do not create empty or placeholder manifests merely to satisfy inventory. A manifest should be added only after the corresponding client repository has actually been inspected.

Each manifest must validate against:

`auth-model/schemas/client-auth-view-conformance-manifest.schema.json`

Canonical AuthViews remain the presentation contract. These manifests are implementation evidence that Aptix may compare against that contract.
