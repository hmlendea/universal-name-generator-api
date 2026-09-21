using System;
using System.Collections.Generic;
using System.IO;

using NuciExtensions;
using NuciGenerators.Text.Models;

using UniversalNameGenerator.API.Configuration;

namespace UniversalNameGenerator.API.IntegrationTests.Infrastructure
{
    internal sealed class IntegrationGenerationDataStore : IDisposable
    {
        public static string DeterministicSchemaId => "deterministic-names";

        public static string LowerCaseSchemaId => "lower-case-names";

        public static string UpperCaseSchemaId => "upper-case-names";

        public static string TitleCaseSchemaId => "title-case-names";

        public static string SelectorSchemaId => "city-selection";

        public static string FilteredSelectorSchemaId => "filtered-city-selection";

        public static string RandomiserSchemaId => "full-name-randomiser";

        public static string MarkovSchemaId => "city-markov";

        public static string LiteralSchemaId => "literal-schema";

        public static string UnsupportedSchemaId => "unsupported-command";

        public static string InvalidIntegerSchemaId => "invalid-random-integer";

        public static string InvalidRangeSchemaId => "invalid-random-range";

        public static string InvalidRandomSchemaId => "invalid-random";

        public static string InvalidRandomiserSchemaId => "invalid-randomiser";

        public static string InvalidSelectorSchemaId => "invalid-selector";

        public static string InvalidMarkovSchemaId => "invalid-markov";

        public static string MissingWordListSchemaId => "missing-word-list";

        private readonly string rootDirectoryPath;

        public DataStoreSettings Settings { get; }

        public IntegrationGenerationDataStore()
        {
            rootDirectoryPath = Path.Combine(
                Path.GetTempPath(),
                $"UniversalNameGeneratorIntegrationTests-{Guid.NewGuid():N}");
            string wordListsRootDirectoryPath = Path.Combine(rootDirectoryPath, "wordlists");
            string generationSchemasPath = Path.Combine(rootDirectoryPath, "GenerationSchemas.xml");

            Directory.CreateDirectory(wordListsRootDirectoryPath);
            CreateWordLists(wordListsRootDirectoryPath);
            CreateGenerationSchemasFile(generationSchemasPath);

            Settings = new DataStoreSettings
            {
                GenerationSchemasPath = generationSchemasPath,
                WordListsRootDirectory = wordListsRootDirectoryPath
            };
        }

        public void Dispose()
        {
            if (Directory.Exists(rootDirectoryPath))
            {
                Directory.Delete(rootDirectoryPath, true);
            }
        }

        private static void CreateWordLists(string wordListsRootDirectoryPath)
        {
            string peopleDirectoryPath = Path.Combine(wordListsRootDirectoryPath, "people");
            string locationsDirectoryPath = Path.Combine(wordListsRootDirectoryPath, "locations");
            string filtersDirectoryPath = Path.Combine(wordListsRootDirectoryPath, "filters");
            string markovDirectoryPath = Path.Combine(wordListsRootDirectoryPath, "markov");

            Directory.CreateDirectory(peopleDirectoryPath);
            Directory.CreateDirectory(locationsDirectoryPath);
            Directory.CreateDirectory(filtersDirectoryPath);
            Directory.CreateDirectory(markovDirectoryPath);

            File.WriteAllLines(
                Path.Combine(peopleDirectoryPath, "first.lst"),
                ["Ilarion", "Ionuț", "Oscar", "Robert", "Tibi", "Vasile", "Bobert", "Solaire"]);
            File.WriteAllLines(
                Path.Combine(peopleDirectoryPath, "last.lst"),
                ["Ciupitu", "Nucaru", "Nucescu", "Pintilie", "Blitz", "Karr"]);
            File.WriteAllLines(
                Path.Combine(locationsDirectoryPath, "cities.lst"),
                [
                    "Astora",
                    "Cluj-Napoca",
                    "Cornova",
                    "Cratesia",
                    "Çupișan",
                    "Dezmir",
                    "Enada",
                    "Florești",
                    "Flusseland",
                    "Frigonița",
                    "Hokazuro",
                    "Horidava",
                    "Izmir",
                    "Newport",
                    "Nordavia",
                    "Oradea",
                    "Solara"
                ]);
            File.WriteAllLines(Path.Combine(filtersDirectoryPath, "excluded-cities.lst"), ["Oradea"]);
            File.WriteAllLines(
                Path.Combine(markovDirectoryPath, "sources.lst"),
                ["aaaaBbbbbCccccDddddEeeee", "aaaaFbbbbGccccHddddIeeee"]);
        }

        private static void CreateGenerationSchemasFile(string generationSchemasPath)
        {
            IEnumerable<string> schemaElements =
            [
                BuildSchemaElement(DeterministicSchemaId, "{random,a,1,1}{random,b,1,1}", WordCase.Title),
                BuildSchemaElement(LowerCaseSchemaId, "{random,so|la,4,4}", WordCase.Lower),
                BuildSchemaElement(UpperCaseSchemaId, "{random,so|la,4,4}", WordCase.Upper),
                BuildSchemaElement(TitleCaseSchemaId, "{random,so|la,4,4}", WordCase.Title),
                BuildSchemaElement(SelectorSchemaId, "{random-selector,4,24,locations/cities}", WordCase.Title),
                BuildSchemaElement(
                    FilteredSelectorSchemaId,
                    "{random-selector,4,24,locations/cities}",
                    WordCase.Title,
                    "filters/excluded-cities"),
                BuildSchemaElement(
                    RandomiserSchemaId,
                    "{randomiser, ,4,32,people/first|people/last}",
                    WordCase.Title),
                BuildSchemaElement(MarkovSchemaId, "{markov,20,28,markov/sources}", WordCase.Title),
                BuildSchemaElement(LiteralSchemaId, "literal value", WordCase.Title),
                BuildSchemaElement(UnsupportedSchemaId, "{mystery,4,24,locations/cities}", WordCase.Title),
                BuildSchemaElement(InvalidIntegerSchemaId, "{random,so|la,four,8}", WordCase.Title),
                BuildSchemaElement(InvalidRangeSchemaId, "{random,so|la,16,4}", WordCase.Title),
                BuildSchemaElement(InvalidRandomSchemaId, "{random,so}", WordCase.Title),
                BuildSchemaElement(InvalidRandomiserSchemaId, "{randomiser, ,4}", WordCase.Title),
                BuildSchemaElement(InvalidSelectorSchemaId, "{random-selector,4}", WordCase.Title),
                BuildSchemaElement(InvalidMarkovSchemaId, "{markov,4}", WordCase.Title),
                BuildSchemaElement(
                    MissingWordListSchemaId,
                    "{random-selector,4,24,locations/missing}",
                    WordCase.Title)
            ];
            string xmlContent =
                $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ArrayOfGenerationSchemaDataObject xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
{string.Join(Environment.NewLine, schemaElements)}
</ArrayOfGenerationSchemaDataObject>
";

            File.WriteAllText(generationSchemasPath, xmlContent);
        }

        private static string BuildSchemaElement(string identifier, string schema, WordCase wordCase)
            => BuildSchemaElement(identifier, schema, wordCase, string.Empty);

        private static string BuildSchemaElement(
            string identifier,
            string schema,
            WordCase wordCase,
            string filterlistPath)
            => $@"    <GenerationSchemaDataObject>
        <Id>{identifier}</Id>
        <Name>{identifier}</Name>
        <Category>Nucilandia</Category>
        <Schema>{schema}</Schema>
        <FilterlistPath>{filterlistPath}</FilterlistPath>
        <WordCase>{wordCase.GetDisplayName()}</WordCase>
    </GenerationSchemaDataObject>";
    }
}