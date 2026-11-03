using System.Collections.Generic;
using System.Linq;

using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.NUnit;

using NUnit.Framework;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace UniversalNameGenerator.API.ArchitectureTests
{
    [TestFixture]
    public sealed class ProductionDependencyArchitectureTests : ArchitectureTestBase
    {
        [Test]
        public void ProductionAssemblyShouldBeLoaded()
        {
            IEnumerable<IType> productionTypes = Types().That().ResideInAssembly(ProductionAssembly).GetObjects(Architecture);

            AssertExpectedTypes(productionTypes, "production assembly");
        }

        [Test]
        public void ControllersShouldDependOnlyOnServiceContractsAndConfiguration()
        {
            IObjectProvider<IType> controllers = Types().That().ResideInNamespace("UniversalNameGenerator.API.Controllers");
            IObjectProvider<IType> forbiddenTypes = Types().That()
                .ResideInNamespace("UniversalNameGenerator.API.DataAccess")
                .Or().ResideInNamespace("UniversalNameGenerator.API.Service.NameGenerators");

            AssertExpectedTypes(controllers.GetObjects(Architecture), "controllers");
            IArchRule rule = Types().That().Are(controllers).Should().NotDependOnAny(forbiddenTypes).Because("the HTTP boundary must not own persistence or generator implementation dependencies");
            Architecture.CheckRule(rule);
        }

        [Test]
        public void DataAccessShouldNotDependOnControllers()
        {
            IObjectProvider<IType> dataAccess = Types().That().HaveFullNameContaining("UniversalNameGenerator.API.DataAccess");
            IObjectProvider<IType> controllers = Types().That().ResideInNamespace("UniversalNameGenerator.API.Controllers");

            AssertExpectedTypes(dataAccess.GetObjects(Architecture), "data-access types");
            AssertExpectedTypes(controllers.GetObjects(Architecture), "controller types");
            IArchRule rule = Types().That().Are(dataAccess).Should().NotDependOnAny(controllers).Because("data access must remain independent of HTTP concerns");
            Architecture.CheckRule(rule);
        }

        [Test]
        public void MappingsShouldNotDependOnControllers()
        {
            IObjectProvider<IType> mappings = Types().That().ResideInNamespace("UniversalNameGenerator.API.Service.Mappings");
            IObjectProvider<IType> controllers = Types().That().ResideInNamespace("UniversalNameGenerator.API.Controllers");

            AssertExpectedTypes(mappings.GetObjects(Architecture), "mapping types");
            IArchRule rule = Types().That().Are(mappings).Should().NotDependOnAny(controllers).Because("mapping code must remain independent of HTTP concerns");
            Architecture.CheckRule(rule);
        }

        [Test]
        public void NameGeneratorsShouldNotDependOnControllers()
        {
            IObjectProvider<IType> generators = Types().That().HaveFullNameContaining("UniversalNameGenerator.API.Service.NameGenerators");
            IObjectProvider<IType> controllers = Types().That().ResideInNamespace("UniversalNameGenerator.API.Controllers");

            AssertExpectedTypes(generators.GetObjects(Architecture), "name-generator types");
            IArchRule rule = Types().That().Are(generators).Should().NotDependOnAny(controllers).Because("generation strategies must remain independent of HTTP concerns");
            Architecture.CheckRule(rule);
        }

        [Test]
        public void ProductionTypesShouldNotDependOnTestAssemblies()
        {
            IEnumerable<IType> productionTypes = Types().That().ResideInAssembly(ProductionAssembly).GetObjects(Architecture);

            Assert.That(productionTypes.All(type => type.FullName is not null && !type.FullName.Contains("Tests")), Is.True);
        }

        [Test]
        public void ProductionTypesShouldHaveExplicitNamespaces()
        {
            IEnumerable<IType> productionTypes = Types().That().ResideInAssembly(ProductionAssembly).GetObjects(Architecture);

            AssertExpectedTypes(productionTypes, "production types");
            Assert.That(productionTypes.All(type => type.Namespace is not null), Is.True, "Every production type must have an explicit namespace.");
        }
    }
}
