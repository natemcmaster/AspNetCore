Build Errors
------------

This document is for common build errors and how to resolve them.

### Warning BUILD001

> warning BUILD001: Package references changed since the last release...

This warning indicates a breaking change might have been made to a package or assembly due to the removal of a reference which was used
in a previous release of this assembly. See [./ReferenceResolution.md](./ReferenceResolution.md) for how to suppress.

### Error BUILD002

> error BUILD002: Package references changed since the last release...

Similar to BUILD001, but this error is not suppressable. This error only appears in servicing builds, which should not change references between assemblies or packages.

### Error BUILD003

> error BUILD003: Cannot reference 'X' from directly because it is part of the shared framework and this project is not. Use &lt;FrameworkReference Include="Microsoft.AspNetCore.App" /&gt; instead.

This error protects projects from using a `<Reference>` to an assembly which is only available in the shared framework. This error prevents projects from producing packages with invalid metadata and assemblies from having
incorrect reference versions in patch versions.

As described in the error message, the fix is to replace the `<Reference>` to an individual assembly with a `<FrameworkReference>` to Microsoft.AspNetCore.App.

See [./SharedFramework.md](./SharedFramework.md) for details.

### Error BUILD004

> error BUILD004: Cannot reference 'X'. This dependency is not in the shared framework. See docs/SharedFramework.md for instructions on how to modify what is in the shared framework.

This error happens in projects that are part of the shared framework and have added a `<Reference>` to an assembly that is not also part of the shared framework. This is an error because the shared framework must
fully contain all its dependencies. To resovle this error, either remove the reference to 'X', or add
'X' to the shared framework.

See [./SharedFramework.md](./SharedFramework.md) for details.
