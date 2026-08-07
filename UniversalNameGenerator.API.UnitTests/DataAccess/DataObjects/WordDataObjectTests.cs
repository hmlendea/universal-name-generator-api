using System;

using NUnit.Framework;

using UniversalNameGenerator.API.DataAccess.DataObjects;

namespace UniversalNameGenerator.API.UnitTests.DataAccess.DataObjects
{
    [TestFixture]
    public sealed class WordDataObjectTests
    {
        [Test]
        public void GivenANewWordDataObject_WhenCheckingValues_ThenValuesAreInitialisedAsEmpty()
            => Assert.That(
                new WordDataObject().Values,
                Is.Empty);

        [Test]
        public void GivenTwoWordDataObjectsWithTheSameIdentifier_WhenComparing_ThenTheyAreEqual()
        {
            WordDataObject firstWordDataObject = BuildWordDataObject("solaire");
            WordDataObject secondWordDataObject = BuildWordDataObject("solaire");

            Assert.That(firstWordDataObject.Equals(secondWordDataObject));
        }

        [Test]
        public void GivenTwoWordDataObjectsWithDifferentIdentifiers_WhenComparing_ThenTheyAreNotEqual()
        {
            WordDataObject firstWordDataObject = BuildWordDataObject("solaire");
            WordDataObject secondWordDataObject = BuildWordDataObject("bobert");

            Assert.That(firstWordDataObject.Equals(secondWordDataObject), Is.False);
        }

        [Test]
        public void GivenAWordDataObjectWithANullIdentifier_WhenGettingHashCode_ThenNoExceptionIsThrown()
        {
            WordDataObject wordDataObject = new();

            Assert.That(
                () => wordDataObject.GetHashCode(),
                Throws.Nothing);
        }

        [Test]
        public void GivenAWordDataObject_WhenComparingAgainstAnUnrelatedObject_ThenItIsNotEqual()
        {
            WordDataObject wordDataObject = BuildWordDataObject("solaire");

            Assert.That(wordDataObject.Equals(new object()), Is.False);
        }

        [Test]
        public void GivenAWordDataObject_WhenAddingValues_ThenAllValuesAreRetained()
        {
            WordDataObject wordDataObject = BuildWordDataObject("solaire");

            wordDataObject.Values.Add("Ilarion");
            wordDataObject.Values.Add("Robert");

            Assert.That(
                wordDataObject.Values,
                Is.EquivalentTo(new[] { "Ilarion", "Ilarion", "Robert" }));
        }

        private static WordDataObject BuildWordDataObject(string identifier)
            => new()
            {
                Id = identifier,
                Values = ["Ilarion"]
            };
    }
}
