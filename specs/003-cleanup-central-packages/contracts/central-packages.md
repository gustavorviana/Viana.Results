# Contract: Central Package List

1. Every `PackageVersion` in `Directory.Packages.props` is referenced by at least one project, or is
   an intentional transitive pin with an inline comment explaining why.
2. Removing an entry never changes the resolved version of any remaining package.
3. Published packages keep their dependency list and version.
