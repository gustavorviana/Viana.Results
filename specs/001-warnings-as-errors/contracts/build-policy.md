# Contract: Build Policy

The public contract of this feature is the rule every contributor and every project must follow.

1. Every project under the repository root builds with warnings treated as errors, for every TFM.
2. NuGet vulnerability advisories (direct and transitive) are errors, locally and in CI.
3. Nullable reference types are enabled in every project; `Nullable=annotations`/`disable` is not
   allowed without a justified exception.
4. A suppression (`NoWarn`, `#pragma warning disable`, `[SuppressMessage]`, `!` used to silence a
   real null flow) MUST:
   - target a single warning code,
   - use the narrowest scope possible,
   - carry an inline comment explaining why,
   - be approved in PR review.
   Allowed without comment: `!` immediately after a `string.IsNullOrEmpty`/`IsNullOrWhiteSpace`
   check (no nullability annotations on `netstandard2.0`), `null!` in tests that exercise
   argument validation, and `!` on values asserted non-null earlier in the same test.
5. Vulnerable dependencies are fixed by upgrading (directly or via CPM transitive pinning in
   `Directory.Packages.props`), never by suppressing the audit warning.
6. New projects inherit the rule automatically; they MUST NOT override `TreatWarningsAsErrors`.
