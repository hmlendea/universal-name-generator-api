using System.Collections.Generic;

using NuciExtensions;

using NuciGenerators.Text;
using NuciGenerators.Text.Models;

namespace UniversalNameGenerator.API.Service.NameGenerators.RandomSelector
{
    public class RandomSelectorNameGenerator : NameGenerator
    {
        public RandomSelectorNameGenerator(List<Wordlist> wordlists)
            : base(wordlists)
        {
            Wordlists = wordlists;
            OnlyNewNames = false;
        }

        protected override string GenerationAlogrithm()
        {
            List<string> combinedWords = [];

            Wordlists.ForEach(wl =>
            {
                combinedWords.AddRange(wl.GetRandomElement().Values);
            });

            return combinedWords.GetRandomElement();
        }
    }
}
