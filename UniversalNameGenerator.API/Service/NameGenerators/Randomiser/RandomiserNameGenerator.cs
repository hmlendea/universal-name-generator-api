using System.Collections.Generic;

using NuciExtensions;

using NuciGenerators.Text;
using NuciGenerators.Text.Models;

namespace UniversalNameGenerator.API.Service.NameGenerators.Randomiser
{
    public sealed class RandomiserNameGenerator : NameGenerator
    {
        private readonly string separator;

        public RandomiserNameGenerator(string separator, IEnumerable<Wordlist> wordlists)
            : base([.. wordlists])
        {
            Wordlists = [.. wordlists];
            OnlyNewNames = false;

            this.separator = separator;
        }

        protected override string GenerationAlogrithm()
        {
            List<string> parts = [];

            Wordlists.ForEach(wordlist => parts.Add(wordlist.GetRandomElement().Values.GetRandomElement()));

            return string.Join(separator, parts);
        }
    }
}
