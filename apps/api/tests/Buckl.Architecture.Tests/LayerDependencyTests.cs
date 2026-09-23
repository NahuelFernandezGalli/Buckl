using System.Reflection;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;
using Buckl.Domain.Garments;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchitectureModel = ArchUnitNET.Domain.Architecture;

namespace Buckl.Architecture.Tests;

/// <summary>Dependencies point inward only: Api → Application → Domain, and Infrastructure →
/// Application. A reference in the wrong direction fails the build instead of a code review.</summary>
public class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly = typeof(Garment).Assembly;

    private static readonly Assembly ApplicationAssembly =
        typeof(Buckl.Application.DependencyInjection).Assembly;

    private static readonly Assembly InfrastructureAssembly =
        typeof(Buckl.Infrastructure.DependencyInjection).Assembly;

    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    private static readonly ArchitectureModel BucklArchitecture = new ArchLoader()
        .LoadAssemblies(DomainAssembly, ApplicationAssembly, InfrastructureAssembly, ApiAssembly)
        .Build();

    private static readonly string[] PersistenceAndWebPrefixes =
        ["Microsoft.EntityFrameworkCore", "Npgsql", "Microsoft.AspNetCore"];

    [Fact]
    public void Domain_depends_on_no_other_layer()
    {
        AssertNoDependency(DomainAssembly, ApplicationAssembly);
        AssertNoDependency(DomainAssembly, InfrastructureAssembly);
        AssertNoDependency(DomainAssembly, ApiAssembly);
    }

    [Fact]
    public void Application_depends_neither_on_infrastructure_nor_on_the_api()
    {
        AssertNoDependency(ApplicationAssembly, InfrastructureAssembly);
        AssertNoDependency(ApplicationAssembly, ApiAssembly);
    }

    [Fact]
    public void Infrastructure_does_not_depend_on_the_api() =>
        AssertNoDependency(InfrastructureAssembly, ApiAssembly);

    [Fact]
    public void Controllers_reach_the_database_only_through_handlers() =>
        Types().That().ResideInNamespace("Buckl.Api.Controllers")
            .Should().NotDependOnAny(Types().That().ResideInAssembly(InfrastructureAssembly))
            .Check(BucklArchitecture);

    [Fact]
    public void Domain_references_only_the_base_class_library()
    {
        var references = ReferencedAssemblyNames(DomainAssembly);

        Assert.All(references, name => Assert.True(
            name.StartsWith("System", StringComparison.Ordinal) || name == "netstandard",
            $"Buckl.Domain must not reference {name}."));
    }

    [Fact]
    public void Application_references_no_persistence_or_web_framework()
    {
        var references = ReferencedAssemblyNames(ApplicationAssembly);

        Assert.DoesNotContain(references, name => PersistenceAndWebPrefixes.Any(
            prefix => name.StartsWith(prefix, StringComparison.Ordinal)));
    }

    private static void AssertNoDependency(Assembly from, Assembly to) =>
        Types().That().ResideInAssembly(from)
            .Should().NotDependOnAny(Types().That().ResideInAssembly(to))
            .Check(BucklArchitecture);

    private static List<string> ReferencedAssemblyNames(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(reference => reference.Name ?? string.Empty).ToList();
}
