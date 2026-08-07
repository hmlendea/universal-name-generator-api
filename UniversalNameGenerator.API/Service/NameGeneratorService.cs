using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NuciDAL.Repositories;

using NuciExtensions;

using NuciGenerators.Text;
using NuciGenerators.Text.MarkovChain;
using NuciGenerators.Text.Models;

using UniversalNameGenerator.API.Configuration;
using UniversalNameGenerator.API.DataAccess.DataObjects;
using UniversalNameGenerator.API.DataAccess.Repositories;
using UniversalNameGenerator.API.Service.Mappings;
using UniversalNameGenerator.API.Service.NameGenerators.RandomSelector;
using UniversalNameGenerator.API.Service.NameGenerators.Randomiser;

namespace UniversalNameGenerator.API.Service
{
    public sealed class NameGeneratorService(DataStoreSettings dataStoreSettings) : INameGeneratorService
    {
        private static char GeneratorStartCharacter => '{';

        private static char GeneratorEndCharacter => '}';

        private static char CommandValueSeparatorCharacter => ',';

        private static char WordlistSeparatorCharacter => '|';

        private static string WordlistFileExtension => ".lst";

        private static string RandomCommandName => "random";

        private static string RandomiserCommandName => "randomiser";

        private static string RandomSelectorCommandName => "random-selector";

        private static string MarkovCommandName => "markov";

        private static int MarkovChainOrder => 4;

        private static float MarkovChainTemperature => 0.0f;

        private static int CommandNameIndex => 0;

        private static int RandomCommandChoicesIndex => 1;

        private static int RandomCommandMinimumLengthIndex => 2;

        private static int RandomCommandMaximumLengthIndex => 3;

        private static int RandomiserCommandSeparatorIndex => 1;

        private static int RandomiserCommandMinimumLengthIndex => 2;

        private static int RandomiserCommandMaximumLengthIndex => 3;

        private static int RandomiserCommandWordlistKeysIndex => 4;

        private static int RandomSelectorCommandMinimumLengthIndex => 1;

        private static int RandomSelectorCommandMaximumLengthIndex => 2;

        private static int RandomSelectorCommandWordlistKeysIndex => 3;

        private static int MarkovCommandMinimumLengthIndex => 1;

        private static int MarkovCommandMaximumLengthIndex => 2;

        private static int MarkovCommandWordlistKeysIndex => 3;

        private readonly Dictionary<string, INameGenerator> generatorsBySchemaId = [];

        private readonly XmlRepository<GenerationSchemaDataObject> generationSchemaRepository =
            new(GetRequiredGenerationSchemasPath(dataStoreSettings));

        private readonly string wordListsRootDirectory =
            GetRequiredWordListsRootDirectory(dataStoreSettings);

        public IEnumerable<string> GetNames(string schemaId, int count)
        {
            GenerationSchema generationSchema = GetSchemaById(schemaId);

            return GenerateNames(
                generationSchema.Schema,
                count,
                generationSchema.FilterlistPath,
                generationSchema.WordCase);
        }

        public IEnumerable<GenerationSchema> GetSchemas()
            => generationSchemaRepository.GetAll().ToServiceModels();

        private GenerationSchema GetSchemaById(string schemaId)
        {
            GenerationSchema generationSchema = GetSchemas().FirstOrDefault(
                schema => string.Equals(schema.Id, schemaId, StringComparison.Ordinal));

            if (generationSchema is null)
            {
                throw new KeyNotFoundException($"Schema '{schemaId}' was not discovered.");
            }

            return generationSchema;
        }

        private IEnumerable<string> GenerateNames(
            string schema,
            int amount,
            string filterlistPath,
            WordCase wordCase)
        {
            Random randomGenerator = new();
            IEnumerable<string> filterValues = GetFilterValues(filterlistPath);
            IEnumerable<IEnumerable<string>> generatedNameParts = GetGeneratedNameParts(
                schema,
                amount,
                filterValues,
                randomGenerator);

            return ComposeNames(generatedNameParts, wordCase);
        }

        private IEnumerable<string> GetFilterValues(string filterlistPath)
        {
            if (string.IsNullOrWhiteSpace(filterlistPath))
            {
                return [];
            }

            string filterlistFilePath = Path.Combine(
                wordListsRootDirectory,
                filterlistPath + WordlistFileExtension);

            return [.. File.ReadAllLines(filterlistFilePath)];
        }

        private IEnumerable<IEnumerable<string>> GetGeneratedNameParts(
            string schema,
            int amount,
            IEnumerable<string> filters,
            Random randomGenerator)
        {
            List<List<string>> generatedValues = [];
            string currentGeneration = schema;

            while (currentGeneration.Contains(GeneratorStartCharacter) &&
                currentGeneration.Contains(GeneratorEndCharacter))
            {
                int generatorStartIndex = currentGeneration.IndexOf(GeneratorStartCharacter);
                int generatorEndIndex = currentGeneration.IndexOf(GeneratorEndCharacter);

                if (generatorStartIndex < 0 || generatorEndIndex <= generatorStartIndex)
                {
                    throw new FormatException($"Schema '{schema}' has malformed generator delimiters.");
                }

                string command = currentGeneration[(generatorStartIndex + 1)..generatorEndIndex];
                IEnumerable<string> values = GetGeneratedValues(
                    schema,
                    amount,
                    command,
                    filters,
                    randomGenerator);

                generatedValues.Add([.. values]);
                currentGeneration = currentGeneration.Remove(
                    generatorStartIndex,
                    generatorEndIndex - generatorStartIndex + 1);
            }

            return generatedValues;
        }

        private IEnumerable<string> GetGeneratedValues(
            string schema,
            int amount,
            string command,
            IEnumerable<string> filters,
            Random randomGenerator)
        {
            string[] commandValues = command.Split(CommandValueSeparatorCharacter);

            if (commandValues.Length == 0)
            {
                return [];
            }

            string commandName = commandValues[CommandNameIndex];

            if (string.Equals(commandName, RandomCommandName, StringComparison.Ordinal))
            {
                return GetRandomStrings(amount, commandValues, randomGenerator, command);
            }

            if (string.Equals(commandName, RandomiserCommandName, StringComparison.Ordinal))
            {
                return GenerateRandomiserNames(schema, amount, commandValues, filters);
            }

            if (string.Equals(commandName, RandomSelectorCommandName, StringComparison.Ordinal))
            {
                return GenerateRandomSelectorNames(schema, amount, commandValues, filters);
            }

            if (string.Equals(commandName, MarkovCommandName, StringComparison.Ordinal))
            {
                return GenerateMarkovNames(schema, amount, commandValues, filters);
            }

            throw new NotSupportedException($"Generator command '{commandName}' is not supported.");
        }

        private static IEnumerable<string> GetRandomStrings(
            int amount,
            string[] commandValues,
            Random randomGenerator,
            string command)
        {
            if (commandValues.Length < 4)
            {
                throw new FormatException($"Command '{command}' does not include sufficient values.");
            }

            List<string> choices = [.. commandValues[RandomCommandChoicesIndex].Split(WordlistSeparatorCharacter)];
            int minimumLength = ParseInteger(
                commandValues[RandomCommandMinimumLengthIndex],
                nameof(minimumLength),
                command);
            int maximumLength = ParseInteger(
                commandValues[RandomCommandMaximumLengthIndex],
                nameof(maximumLength),
                command);

            if (minimumLength > maximumLength)
            {
                throw new ArgumentException(
                    $"Minimum length '{minimumLength}' cannot exceed maximum length '{maximumLength}'.");
            }

            if (choices.Count == 0)
            {
                return [];
            }

            List<string> generatedStrings = [];

            while (generatedStrings.Count < amount)
            {
                int targetLength = randomGenerator.Next(minimumLength, maximumLength + 1);
                string generatedString = string.Empty;

                while (generatedString.Length < targetLength)
                {
                    int randomChoiceIndex = randomGenerator.Next(0, choices.Count);
                    generatedString += choices[randomChoiceIndex];
                }

                generatedStrings.Add(generatedString);
            }

            return generatedStrings;
        }

        private static IEnumerable<string> ComposeNames(
            IEnumerable<IEnumerable<string>> generatedParts,
            WordCase wordCase)
        {
            List<List<string>> generatedPartsList = [];

            foreach (IEnumerable<string> generatedPart in generatedParts)
            {
                generatedPartsList.Add([.. generatedPart]);
            }

            if (generatedPartsList.Count == 0)
            {
                return [];
            }

            int generatedNameCount = generatedPartsList.Min(part => part.Count);
            List<string> names = [];

            for (int generatedNameIndex = 0; generatedNameIndex < generatedNameCount; generatedNameIndex += 1)
            {
                string name = string.Empty;

                generatedPartsList.ForEach(part => name += part[generatedNameIndex]);
                name = GetNameWithCasing(name, wordCase);
                names.Add(name);
            }

            return names;
        }

        private static string GetNameWithCasing(string name, WordCase wordCase)
        {
            if (Equals(wordCase, WordCase.Lower))
            {
                return name.ToLower();
            }

            if (Equals(wordCase, WordCase.Upper))
            {
                return name.ToUpper();
            }

            if (Equals(wordCase, WordCase.Title))
            {
                return name.ToTitleCase();
            }

            if (Equals(wordCase, WordCase.Sentence))
            {
                return name.ToSentenceCase();
            }

            return name;
        }

        private IEnumerable<string> GenerateRandomiserNames(
            string schema,
            int amount,
            string[] commandValues,
            IEnumerable<string> filters)
        {
            if (commandValues.Length < 5)
            {
                throw new FormatException("Randomiser command does not include sufficient values.");
            }

            int minimumLength = ParseInteger(
                commandValues[RandomiserCommandMinimumLengthIndex],
                nameof(minimumLength),
                commandValues[CommandNameIndex]);
            int maximumLength = ParseInteger(
                commandValues[RandomiserCommandMaximumLengthIndex],
                nameof(maximumLength),
                commandValues[CommandNameIndex]);
            List<string> wordlistKeys = [.. commandValues[RandomiserCommandWordlistKeysIndex].Split(WordlistSeparatorCharacter)];
            List<Wordlist> wordlists = [.. GetWordLists(wordlistKeys)];
            List<string> excludedStrings = [.. filters];

            if (!generatorsBySchemaId.TryGetValue(schema, out INameGenerator generator))
            {
                generator = new RandomiserNameGenerator(commandValues[RandomiserCommandSeparatorIndex], wordlists)
                {
                    MinNameLength = minimumLength,
                    MaxNameLength = maximumLength,
                    ExcludedStrings = excludedStrings
                };
                generatorsBySchemaId.Add(schema, generator);
            }

            return generator.Generate(amount);
        }

        private IEnumerable<string> GenerateRandomSelectorNames(
            string schema,
            int amount,
            string[] commandValues,
            IEnumerable<string> filters)
        {
            if (commandValues.Length < 4)
            {
                throw new FormatException("Random selector command does not include sufficient values.");
            }

            int minimumLength = ParseInteger(
                commandValues[RandomSelectorCommandMinimumLengthIndex],
                nameof(minimumLength),
                commandValues[CommandNameIndex]);
            int maximumLength = ParseInteger(
                commandValues[RandomSelectorCommandMaximumLengthIndex],
                nameof(maximumLength),
                commandValues[CommandNameIndex]);
            List<string> wordlistKeys = [.. commandValues[RandomSelectorCommandWordlistKeysIndex].Split(WordlistSeparatorCharacter)];
            List<Wordlist> wordlists = [.. GetWordLists(wordlistKeys)];
            List<string> excludedStrings = [.. filters];

            if (!generatorsBySchemaId.TryGetValue(schema, out INameGenerator generator))
            {
                generator = new RandomSelectorNameGenerator(wordlists)
                {
                    MinNameLength = minimumLength,
                    MaxNameLength = maximumLength,
                    ExcludedStrings = excludedStrings
                };
                generatorsBySchemaId.Add(schema, generator);
            }

            return generator.Generate(amount);
        }

        private IEnumerable<string> GenerateMarkovNames(
            string schema,
            int amount,
            string[] commandValues,
            IEnumerable<string> filters)
        {
            if (commandValues.Length < 4)
            {
                throw new FormatException("Markov command does not include sufficient values.");
            }

            int minimumLength = ParseInteger(
                commandValues[MarkovCommandMinimumLengthIndex],
                nameof(minimumLength),
                commandValues[CommandNameIndex]);
            int maximumLength = ParseInteger(
                commandValues[MarkovCommandMaximumLengthIndex],
                nameof(maximumLength),
                commandValues[CommandNameIndex]);
            List<string> wordlistKeys = [.. commandValues[MarkovCommandWordlistKeysIndex].Split(WordlistSeparatorCharacter)];
            List<Wordlist> wordlists = [.. GetWordLists(wordlistKeys)];
            List<string> excludedStrings = [.. filters];

            if (!generatorsBySchemaId.TryGetValue(schema, out INameGenerator generator))
            {
                generator = new MarkovNameGenerator(wordlists, MarkovChainOrder, MarkovChainTemperature)
                {
                    MinNameLength = minimumLength,
                    MaxNameLength = maximumLength,
                    ExcludedStrings = excludedStrings
                };
                generatorsBySchemaId.Add(schema, generator);
            }

            return generator.Generate(amount);
        }

        private IEnumerable<Wordlist> GetWordLists(IEnumerable<string> wordlistKeys)
        {
            List<Wordlist> wordlists = [];

            foreach (string wordlistId in wordlistKeys)
            {
                string filePath = Path.Combine(wordListsRootDirectory, wordlistId + WordlistFileExtension);
                IWordRepository wordRepository = new WordRepository(filePath);
                IEnumerable<Word> words = wordRepository.GetAll().ToServiceModels();
                Wordlist wordlist = [.. words];
                wordlists.Add(wordlist);
            }

            return wordlists;
        }

        private static int ParseInteger(string value, string valueName, string commandName)
        {
            if (!int.TryParse(value, out int parsedValue))
            {
                throw new FormatException(
                    $"The value '{value}' for '{valueName}' in command '{commandName}' is not a valid integer.");
            }

            return parsedValue;
        }

        private static string GetRequiredGenerationSchemasPath(DataStoreSettings dataStoreSettings)
        {
            if (string.IsNullOrWhiteSpace(dataStoreSettings.GenerationSchemasPath))
            {
                throw new ArgumentException("The generation schemas path is necessary.");
            }

            return dataStoreSettings.GenerationSchemasPath;
        }

        private static string GetRequiredWordListsRootDirectory(DataStoreSettings dataStoreSettings)
        {
            if (string.IsNullOrWhiteSpace(dataStoreSettings.WordListsRootDirectory))
            {
                throw new ArgumentException("The word lists root directory is necessary.");
            }

            return dataStoreSettings.WordListsRootDirectory;
        }
    }
}
