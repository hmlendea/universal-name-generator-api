using System.Collections.Generic;

using NuciExtensions;

using NuciGenerators.Text;
using NuciGenerators.Text.Models;

namespace UniversalNameGenerator.API.Service.NameGenerators.Randomiser
{
    public class RandomiserNameGenerator : NameGenerator
    {
        readonly string separator;

        public RandomiserNameGenerator(string separator, List<Wordlist> wordlists)
            : base(wordlists)
        {
            Wordlists = wordlists;
            OnlyNewNames = false;

            this.separator = separator;
        }

        protected override string GenerationAlogrithm()
        {
            List<string> parts = [];

            Wordlists.ForEach(wl => parts.Add(wl.GetRandomElement().Values.GetRandomElement()));

            return string.Join(separator, parts);
        }
    }
}
