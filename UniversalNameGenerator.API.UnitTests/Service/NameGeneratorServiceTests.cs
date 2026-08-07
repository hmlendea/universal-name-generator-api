using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using NuciExtensions;
using NuciGenerators.Text.Models;

using UniversalNameGenerator.API.Configuration;
using UniversalNameGenerator.API.Service;

namespace UniversalNameGenerator.API.UnitTests.Service
{
    [TestFixture]
    public sealed class NameGeneratorServiceTests
    {
        private const string FantasyDragonsSchemaId = "fantasy-dragons";
        private const string AstoraSettlementsSchemaId = "astora-settlements";
        private const string CitySelectionSchemaId = "city-selection";
        private const string UnsupportedSchemaId = "unsupported-command";
        private const string InvalidIntegerSchemaId = "invalid-random-integer";
        private const string InvalidRangeSchemaId = "invalid-random-range";
        private const string InvalidRandomiserSchemaId = "invalid-randomiser";

        private string temporaryDirectoryPath = string.Empty;
        private string wordListsRootDirectoryPath = string.Empty;
        private string generationSchemasPath = string.Empty;
        private NameGeneratorService nameGeneratorService = null!;

        [SetUp]
        public void SetUp()
        {
            temporaryDirectoryPath = CreateTemporaryDirectoryPath();
            wordListsRootDirectoryPath = Path.Combine(temporaryDirectoryPath, "wordlists");
            generationSchemasPath = Path.Combine(temporaryDirectoryPath, "GenerationSchemas.xml");

            Directory.CreateDirectory(wordListsRootDirectoryPath);
            Directory.CreateDirectory(Path.Combine(wordListsRootDirectoryPath, "fantasy"));
            Directory.CreateDirectory(Path.Combine(wordListsRootDirectoryPath, "locations"));

            CreateWordLists();
            CreateGenerationSchemasFile();

            DataStoreSettings dataStoreSettings = new()
            {
                GenerationSchemasPath = generationSchemasPath,
                WordListsRootDirectory = wordListsRootDirectoryPath
            };

            nameGeneratorService = new NameGeneratorService(dataStoreSettings);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(temporaryDirectoryPath))
            {
                Directory.Delete(temporaryDirectoryPath, true);
            }
        }

        [Test]
        public void GivenAValidGenerationSchemasFile_WhenGettingSchemas_ThenAllSchemasAreReturned()
        {
            GenerationSchema[] schemas = [.. nameGeneratorService.GetSchemas()];

            Assert.That(schemas, Has.Length.EqualTo(7));
            Assert.That(
                schemas.Select(schema => schema.Id),
                Is.EquivalentTo(
                new[]
                {
                    FantasyDragonsSchemaId,
                    AstoraSettlementsSchemaId,
                    CitySelectionSchemaId,
                    UnsupportedSchemaId,
                    InvalidIntegerSchemaId,
                    InvalidRangeSchemaId,
                    InvalidRandomiserSchemaId
                }));
        }

        [Test]
        public void GivenARandomiserSchema_WhenGeneratingNames_ThenTheRequestedAmountOfNamesIsReturned()
        {
            string[] names = [.. nameGeneratorService.GetNames(FantasyDragonsSchemaId, 16)];

            Assert.That(names, Has.Length.EqualTo(9));
            Assert.That(names.All(name => name.Length >= 4));
            Assert.That(names.All(name => name.Contains(' ')));
        }

        [Test]
        public void GivenARandomSchema_WhenGeneratingNames_ThenGeneratedNamesRespectTheConfiguredLength()
        {
            string[] names = [.. nameGeneratorService.GetNames(AstoraSettlementsSchemaId, 8)];

            Assert.That(names, Has.Length.EqualTo(8));
            Assert.That(names.All(name => name.Length == 4));
            Assert.That(names.All(name => string.Equals(name, name.ToLower(), StringComparison.Ordinal)));
        }

        [Test]
        public void GivenARandomSelectorSchema_WhenGeneratingNames_ThenTheRequestedAmountOfNamesIsReturned()
        {
            string[] names = [.. nameGeneratorService.GetNames(CitySelectionSchemaId, 8)];

            Assert.That(names, Has.Length.EqualTo(3));
            Assert.That(names.All(name => name.Length >= 4));
        }

        [Test]
        public void GivenANonExistentSchema_WhenGeneratingNames_ThenAKeyNotFoundExceptionIsThrown()
            => Assert.That(
                () => nameGeneratorService.GetNames("schema-does-not-exist", 8).ToArray(),
                Throws.TypeOf<KeyNotFoundException>());

        [Test]
        public void GivenASchemaUsingAnUnsupportedCommand_WhenGeneratingNames_ThenANotSupportedExceptionIsThrown()
            => Assert.That(
                () => nameGeneratorService.GetNames(UnsupportedSchemaId, 8).ToArray(),
                Throws.TypeOf<NotSupportedException>());

        [Test]
        public void GivenASchemaUsingANonNumericRandomLength_WhenGeneratingNames_ThenAFormatExceptionIsThrown()
            => Assert.That(
                () => nameGeneratorService.GetNames(InvalidIntegerSchemaId, 8).ToArray(),
                Throws.TypeOf<FormatException>());

        [Test]
        public void GivenASchemaWithARandomMinimumGreaterThanMaximum_WhenGeneratingNames_ThenAnArgumentExceptionIsThrown()
            => Assert.That(
                () => nameGeneratorService.GetNames(InvalidRangeSchemaId, 8).ToArray(),
                Throws.TypeOf<ArgumentException>());

        [Test]
        public void GivenASchemaWithAnInvalidRandomiserCommand_WhenGeneratingNames_ThenAFormatExceptionIsThrown()
            => Assert.That(
                () => nameGeneratorService.GetNames(InvalidRandomiserSchemaId, 8).ToArray(),
                Throws.TypeOf<FormatException>());

        [Test]
        public void GivenMissingGenerationSchemasPath_WhenConstructingTheService_ThenAnArgumentExceptionIsThrown()
            => Assert.That(
                () => new NameGeneratorService(
                    new DataStoreSettings
                    {
                        GenerationSchemasPath = string.Empty,
                        WordListsRootDirectory = wordListsRootDirectoryPath
                    }),
                Throws.TypeOf<ArgumentException>());

        [Test]
        public void GivenMissingWordListsRootDirectory_WhenConstructingTheService_ThenAnArgumentExceptionIsThrown()
            => Assert.That(
                () => new NameGeneratorService(
                    new DataStoreSettings
                    {
                        GenerationSchemasPath = generationSchemasPath,
                        WordListsRootDirectory = string.Empty
                    }),
                Throws.TypeOf<ArgumentException>());

        private void CreateWordLists()
        {
            File.WriteAllLines(
                Path.Combine(wordListsRootDirectoryPath, "fantasy/first.lst"),
            [
                "Ilarion",
                "Robert",
                "Solaire"
            ]);

            File.WriteAllLines(
                Path.Combine(wordListsRootDirectoryPath, "fantasy/last.lst"),
            [
                "Pintilie",
                "Blitz",
                "Karr"
            ]);

            File.WriteAllLines(
                Path.Combine(wordListsRootDirectoryPath, "locations/cities.lst"),
            [
                "Solara",
                "Cluj-Napoca",
                "Oradea"
            ]);
        }

        private void CreateGenerationSchemasFile()
        {
            string titleWordCase = WordCase.Title.GetDisplayName();
            string lowerWordCase = WordCase.Lower.GetDisplayName();

            string xmlContent =
                $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ArrayOfGenerationSchemaEntity xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <GenerationSchemaEntity>
        <Id>{FantasyDragonsSchemaId}</Id>
        <Name>Fantasy dragons</Name>
        <Category>Astora</Category>
        <Schema>{{randomiser, ,4,32,fantasy/first|fantasy/last}}</Schema>
        <WordCase>{titleWordCase}</WordCase>
    </GenerationSchemaEntity>
    <GenerationSchemaEntity>
        <Id>{AstoraSettlementsSchemaId}</Id>
        <Name>Astora settlements</Name>
        <Category>Romania</Category>
        <Schema>{{random,so|la,4,4}}</Schema>
        <WordCase>{lowerWordCase}</WordCase>
    </GenerationSchemaEntity>
    <GenerationSchemaEntity>
        <Id>{CitySelectionSchemaId}</Id>
        <Name>City selection</Name>
        <Category>Nucilandia</Category>
        <Schema>{{random-selector,4,24,locations/cities}}</Schema>
        <WordCase>{titleWordCase}</WordCase>
    </GenerationSchemaEntity>
    <GenerationSchemaEntity>
        <Id>{UnsupportedSchemaId}</Id>
        <Name>Unsupported command</Name>
        <Category>Nucilandia</Category>
        <Schema>{{mystery,4,24,locations/cities}}</Schema>
        <WordCase>{titleWordCase}</WordCase>
    </GenerationSchemaEntity>
    <GenerationSchemaEntity>
        <Id>{InvalidIntegerSchemaId}</Id>
        <Name>Invalid integer</Name>
        <Category>Nucilandia</Category>
        <Schema>{{random,so|la,four,8}}</Schema>
        <WordCase>{titleWordCase}</WordCase>
    </GenerationSchemaEntity>
    <GenerationSchemaEntity>
        <Id>{InvalidRangeSchemaId}</Id>
        <Name>Invalid range</Name>
        <Category>Nucilandia</Category>
        <Schema>{{random,so|la,16,4}}</Schema>
        <WordCase>{titleWordCase}</WordCase>
    </GenerationSchemaEntity>
    <GenerationSchemaEntity>
        <Id>{InvalidRandomiserSchemaId}</Id>
        <Name>Invalid randomiser</Name>
        <Category>Nucilandia</Category>
        <Schema>{{randomiser, ,4}}</Schema>
        <WordCase>{titleWordCase}</WordCase>
    </GenerationSchemaEntity>
</ArrayOfGenerationSchemaEntity>
";
            File.WriteAllText(generationSchemasPath, xmlContent);
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
