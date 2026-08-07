using System;

using NUnit.Framework;

using NuciExtensions;
using NuciGenerators.Text.Models;

using UniversalNameGenerator.API.DataAccess.DataObjects;

namespace UniversalNameGenerator.API.UnitTests.DataAccess.DataObjects
{
    [TestFixture]
    public sealed class GenerationSchemaDataObjectTests
    {
        [Test]
        public void GivenANewGenerationSchemaDataObject_WhenCheckingWordCase_ThenTheDefaultWordCaseIsTitle()
            => Assert.That(
                new GenerationSchemaDataObject().WordCase,
                Is.EqualTo(WordCase.Title.GetDisplayName()));

        [Test]
        public void GivenTwoGenerationSchemaDataObjectsWithTheSameIdentifier_WhenComparing_ThenTheyAreEqual()
        {
            GenerationSchemaDataObject firstGenerationSchemaDataObject = BuildGenerationSchemaDataObject("astora-city");
            GenerationSchemaDataObject secondGenerationSchemaDataObject = BuildGenerationSchemaDataObject("astora-city");

            Assert.That(firstGenerationSchemaDataObject.Equals(secondGenerationSchemaDataObject));
        }

        [Test]
        public void GivenTwoGenerationSchemaDataObjectsWithDifferentIdentifiers_WhenComparing_ThenTheyAreNotEqual()
        {
            GenerationSchemaDataObject firstGenerationSchemaDataObject = BuildGenerationSchemaDataObject("astora-city");
            GenerationSchemaDataObject secondGenerationSchemaDataObject = BuildGenerationSchemaDataObject("nucilandia-city");

            Assert.That(firstGenerationSchemaDataObject.Equals(secondGenerationSchemaDataObject), Is.False);
        }

        [Test]
        public void GivenAGenerationSchemaDataObject_WhenComparingAgainstAnUnrelatedObject_ThenItIsNotEqual()
        {
            GenerationSchemaDataObject generationSchemaDataObject = BuildGenerationSchemaDataObject("astora-city");

            Assert.That(generationSchemaDataObject.Equals(new object()), Is.False);
        }

        [Test]
        public void GivenAGenerationSchemaDataObjectWithANullIdentifier_WhenGettingHashCode_ThenNoExceptionIsThrown()
        {
            GenerationSchemaDataObject generationSchemaDataObject = new();

            Assert.That(
                () => generationSchemaDataObject.GetHashCode(),
                Throws.Nothing);
        }

        [Test]
        public void GivenAGenerationSchemaDataObject_WhenAssigningFields_ThenTheAssignedFieldsAreRetained()
        {
            GenerationSchemaDataObject generationSchemaDataObject = BuildGenerationSchemaDataObject("astora-city");

            generationSchemaDataObject.Name = "Solara";
            generationSchemaDataObject.Category = "Romania";
            generationSchemaDataObject.Schema = "{random,so|la,4,4}";
            generationSchemaDataObject.FilterlistPath = "filters/excluded";

            Assert.That(generationSchemaDataObject.Name, Is.EqualTo("Solara"));
            Assert.That(generationSchemaDataObject.Category, Is.EqualTo("Romania"));
            Assert.That(generationSchemaDataObject.Schema, Is.EqualTo("{random,so|la,4,4}"));
            Assert.That(generationSchemaDataObject.FilterlistPath, Is.EqualTo("filters/excluded"));
        }

        private static GenerationSchemaDataObject BuildGenerationSchemaDataObject(string identifier)
            => new()
            {
                Id = identifier,
                Name = "Solara",
                Category = "Astora",
                Schema = "{random,so|la,4,4}",
                WordCase = WordCase.Title.GetDisplayName()
            };
    }
}
