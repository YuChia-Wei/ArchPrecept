# SampleShop

This console example creates a product identity and validates its name. The value lives for one invocation. The Domain folder owns the product name invariant, Application owns the use case, and Program is the console entry point. Replace this example with your domain behavior and add a storage port when persistence is required.

Restore the exact ArchPrecept package versions in the project from your configured NuGet source, then build and try both outcomes:

```sh
dotnet restore
dotnet build --no-restore
dotnet run --no-build -- Paper
dotnet run --no-build
```

The first run prints `Created: Paper`. The second writes `Product name is required.` to stderr and exits with code 1.

Add a decimal value object in the Domain folder using this project's contract mode:

```sh
dotnet new ap-vo -n Price --namespace SampleShop.Domain --contracts STARTER_CONTRACT_MODE -o Domain --dry-run
dotnet new ap-vo -n Price --namespace SampleShop.Domain --contracts STARTER_CONTRACT_MODE -o Domain
```

Review the generated member and supply your own business rules for its allowed values.

The project includes `archprecept.project.xml` as an AdditionalFile. Naming conventions are disabled. The reference mode uses the package marker. The custom mode maps the consumer-owned Domain.IDomainValue marker by metadata and assembly name. DBA1018 has error severity: adding a setter to the generated Price.Value and rebuilding reports the violation; restoring the getter-only property restores this check. These diagnostics assess the configured roles and predicates.
