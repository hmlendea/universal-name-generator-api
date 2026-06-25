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
using UniversalNameGenerator.API.Service.Mapping;
using UniversalNameGenerator.API.Service.NameGenerators.Randomiser;

namespace UniversalNameGenerator.API.Service
{
    public class NameGeneratorService : INameGeneratorService
    {
        readonly DataStoreSettings settings;
        readonly Dictionary<string, INameGenerator> generators;
        readonly XmlRepository<GenerationSchemaEntity> schemaRepository;

        public NameGeneratorService(DataStoreSettings settings)
        {
            this.settings = settings;
            generators = [];
            schemaRepository = new(settings.GenerationSchemasPath);
        }

        public IEnumerable<string> GetNames(string schemaId, int count)
        {
            GenerationSchema schema = GetSchemas().FirstOrDefault(s => s.Id == schemaId)
                ?? throw new KeyNotFoundException($"Schema '{schemaId}' not found.");

            return GenerateNames(schema.Schema, count, schema.FilterlistPath, schema.WordCase);
        }

        public IEnumerable<GenerationSchema> GetSchemas()
            => schemaRepository.GetAll().ToDomainModels();

        IEnumerable<string> GenerateNames(string schema, int amount, string filterlist, WordCase casing)
        {
            Random random = new();
            List<string> filters;

            if (!string.IsNullOrWhiteSpace(filterlist))
            {
                filters = [.. File.ReadAllLines(Path.Combine(settings.WordListsRootDirectory, filterlist + ".lst"))];
            }
            else
            {
                filters = [];
            }

            List<List<string>> z = [];
            int generatorsCount = schema.Count(x => x.Equals('{'));

            while (z.Count < generatorsCount)
            {
                string currentGeneration = schema;

                while (currentGeneration.Contains('{') || currentGeneration.Contains('}'))
                {
                    int pos = currentGeneration.IndexOf('{') + 1;
                    string com = currentGeneration[pos..currentGeneration.IndexOf('}')];
                    IEnumerable<string> values = [];

                    string[] split = com.Split(',');

                    switch (split[0])
                    {
                        case "random":
                            values = RandomStrings(amount, split[1].Split('|').ToList(), int.Parse(split[2]), int.Parse(split[3]), random);
                            break;

                        case "randomiser":
                            values = GenerateRandomiserNames(schema, amount, split, filters);
                            break;

                        case "markov":
                            values = GenerateMarkovNames(schema, amount, split, filters);
                            break;
                    }

                    currentGeneration = currentGeneration.Replace("{" + com + "}", string.Empty);
                    z.Add(values.ToList());
                }
            }

            List<string> names = [];

            for (int i = 0; i < z.Min(x => x.Count); i++)
            {
                string name = string.Empty;
                z.ForEach(x => name += x[i]);
                name = GetNameWithCasing(name, casing);
                names.Add(name);
            }

            return names;
        }

        static string GetNameWithCasing(string name, WordCase casing)
        {
            if (casing.Equals(WordCase.Lower)) return name.ToLower();
            if (casing.Equals(WordCase.Upper)) return name.ToUpper();
            if (casing.Equals(WordCase.Title)) return name.ToTitleCase();
            if (casing.Equals(WordCase.Sentence)) return name.ToSentenceCase();
            return name;
        }

        static List<string> RandomStrings(int amount, List<string> choices, int minLength, int maxLength, Random random)
        {
            string str = string.Empty;
            int targetLength = random.Next(minLength, maxLength + 1);
            List<string> names = [];

            while (names.Count <= amount && names.Count.NotEquals(choices.Count))
            {
                while (str.Length.NotEquals(targetLength))
                {
                    int i = random.Next(0, choices.Count);
                    str += choices[i];
                }
                names.Add(str);
            }

            return names;
        }

        IEnumerable<string> GenerateRandomiserNames(string schema, int amount, string[] split, List<string> filters)
        {
            int minLength = int.Parse(split[2]);
            int maxLength = int.Parse(split[3]);
            List<string> wordlistKeys = split[4].Split('|').ToList();
            List<Wordlist> wordlists = GetWordLists(wordlistKeys);

            if (!generators.TryGetValue(schema, out INameGenerator generator))
            {
                generator = new RandomiserNameGenerator(split[1], wordlists)
                {
                    MinNameLength = minLength,
                    MaxNameLength = maxLength,
                    ExcludedStrings = filters
                };
                generators.Add(schema, generator);
            }

            return generator.Generate(amount);
        }

        IEnumerable<string> GenerateMarkovNames(string schema, int amount, string[] split, List<string> filters)
        {
            int minLength = int.Parse(split[1]);
            int maxLength = int.Parse(split[2]);
            List<string> wordlistKeys = split[3].Split('|').ToList();
            List<Wordlist> wordlists = GetWordLists(wordlistKeys);

            if (!generators.TryGetValue(schema, out INameGenerator generator))
            {
                generator = new MarkovNameGenerator(wordlists, 4, 0.0f)
                {
                    MinNameLength = minLength,
                    MaxNameLength = maxLength,
                    ExcludedStrings = filters
                };
                generators.Add(schema, generator);
            }

            return generator.Generate(amount);
        }

        List<Wordlist> GetWordLists(List<string> wordlistKeys)
        {
            List<Wordlist> wordlists = [];

            foreach (string wordlistId in wordlistKeys)
            {
                string filePath = Path.Combine(settings.WordListsRootDirectory, $"{wordlistId}.lst");
                WordRepository wordRepository = new(filePath);
                IEnumerable<Word> words = wordRepository.GetAll().ToDomainModels();
                Wordlist wordlist = [.. words];
                wordlists.Add(wordlist);
            }

            return wordlists;
        }
    }
}
