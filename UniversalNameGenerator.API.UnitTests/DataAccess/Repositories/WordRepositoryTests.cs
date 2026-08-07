using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using UniversalNameGenerator.API.DataAccess.DataObjects;
using UniversalNameGenerator.API.DataAccess.Repositories;

namespace UniversalNameGenerator.API.UnitTests.DataAccess.Repositories
{
    [TestFixture]
    public sealed class WordRepositoryTests
    {
        private string temporaryDirectoryPath = string.Empty;

        [SetUp]
        public void SetUp()
            => temporaryDirectoryPath = CreateTemporaryDirectoryPath();

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(temporaryDirectoryPath))
            {
                Directory.Delete(temporaryDirectoryPath, true);
            }
        }

        [Test]
        public void GivenALstFileContainingDistinctIdentifiers_WhenGettingAllWords_ThenAllWordDataObjectsAreReturned()
        {
            string filePath = Path.Combine(temporaryDirectoryPath, "words.lst");
            File.WriteAllLines(filePath,
            [
                "Ilarion",
                "Robert",
                "Solara"
            ]);
            IWordRepository wordRepository = new WordRepository(filePath);

            WordDataObject[] words = [.. wordRepository.GetAll()];

            Assert.That(words, Has.Length.EqualTo(3));
            Assert.That(words.Select(wordDataObject => wordDataObject.Id), Is.EquivalentTo(new[] { "Ilarion", "Robert", "Solara" }));
        }

        [Test]
        public void GivenALstFileContainingTheSameIdentifier_WhenGettingAllWords_ThenValuesAreAggregated()
        {
            string filePath = Path.Combine(temporaryDirectoryPath, "words.lst");
            File.WriteAllLines(filePath,
            [
                "Ilarion_person",
                "Robert_person",
                "Solaire_person"
            ]);
            IWordRepository wordRepository = new WordRepository(filePath);

            WordDataObject[] words = [.. wordRepository.GetAll()];

            Assert.That(words, Has.Length.EqualTo(1));
            Assert.That(words[0].Id, Is.EqualTo("person"));
            Assert.That(words[0].Values, Is.EquivalentTo(new[] { "Ilarion", "Robert", "Solaire" }));
        }

        [Test]
        public void GivenALstFileContainingInlineComments_WhenGettingAllWords_ThenCommentsAreRemoved()
        {
            string filePath = Path.Combine(temporaryDirectoryPath, "words.lst");
            File.WriteAllLines(filePath,
            [
                "Ilarion #comment",
                "Robert#comment",
                "Solaire"
            ]);
            IWordRepository wordRepository = new WordRepository(filePath);

            WordDataObject[] words = [.. wordRepository.GetAll()];

            Assert.That(words.Select(wordDataObject => wordDataObject.Id), Is.EquivalentTo(new[] { "Ilarion", "Robert", "Solaire" }));
        }

        [Test]
        public void GivenARepositoryThatIsCalledTwice_WhenTheFileContentHasChanged_ThenTheSecondReadReflectsTheLatestContent()
        {
            string filePath = Path.Combine(temporaryDirectoryPath, "words.lst");
            File.WriteAllLines(filePath, ["Ilarion"]);
            IWordRepository wordRepository = new WordRepository(filePath);

            WordDataObject[] firstReadWords = [.. wordRepository.GetAll()];
            File.WriteAllLines(filePath, ["Robert"]);
            WordDataObject[] secondReadWords = [.. wordRepository.GetAll()];

            Assert.That(firstReadWords[0].Id, Is.EqualTo("Ilarion"));
            Assert.That(secondReadWords, Has.Length.EqualTo(1));
            Assert.That(secondReadWords[0].Id, Is.EqualTo("Robert"));
        }

        [Test]
        public void GivenANonExistingLstFile_WhenGettingAllWords_ThenAFileNotFoundExceptionIsThrown()
        {
            string filePath = Path.Combine(temporaryDirectoryPath, "missing.lst");
            IWordRepository wordRepository = new WordRepository(filePath);

            Assert.That(
                () => wordRepository.GetAll().ToArray(),
                Throws.TypeOf<FileNotFoundException>());
        }

        private static string CreateTemporaryDirectoryPath()
        {
            string temporaryDirectoryPath = Path.Combine(
                Path.GetTempPath(),
                "UniversalNameGeneratorTests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporaryDirectoryPath);

            return temporaryDirectoryPath;
        }
    }
}
