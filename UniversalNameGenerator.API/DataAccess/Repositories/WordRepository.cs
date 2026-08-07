using System.Collections.Generic;
using System.IO;
using System.Linq;

using UniversalNameGenerator.API.DataAccess.DataObjects;

namespace UniversalNameGenerator.API.DataAccess.Repositories
{
    public sealed class WordRepository(string sourceFilePath) : IWordRepository
    {
        private static char ItemSeparatorCharacter => '_';

        private static char CommentStartCharacter => '#';

        private readonly Dictionary<string, WordDataObject> wordsByIdentifier = [];

        private readonly string sourceFilePath = sourceFilePath;

        public IEnumerable<WordDataObject> GetAll()
        {
            LoadContent();

            return wordsByIdentifier.Values;
        }

        private void LoadContent()
        {
            wordsByIdentifier.Clear();

            using StreamReader streamReader = File.OpenText(sourceFilePath);
            string lineContent;

            while ((lineContent = streamReader.ReadLine()) is not null)
            {
                WordDataObject wordDataObject = GetWordFromLine(lineContent);

                if (wordsByIdentifier.TryGetValue(wordDataObject.Id, out WordDataObject existingWordDataObject))
                {
                    existingWordDataObject.Values.Add(wordDataObject.Values.First());
                }
                else
                {
                    wordsByIdentifier.Add(wordDataObject.Id, wordDataObject);
                }
            }
        }

        private static WordDataObject GetWordFromLine(string lineContent)
        {
            string uncommentedLineContent = UncommentLine(lineContent);
            int separatorIndex = uncommentedLineContent.IndexOf(ItemSeparatorCharacter);

            WordDataObject wordDataObject = new();

            if (separatorIndex > 0)
            {
                wordDataObject.Id = uncommentedLineContent[(separatorIndex + 1)..];
                wordDataObject.Values = [uncommentedLineContent[..separatorIndex]];
            }
            else
            {
                wordDataObject.Id = uncommentedLineContent;
                wordDataObject.Values = [uncommentedLineContent];
            }

            return wordDataObject;
        }

        private static string UncommentLine(string lineContent)
        {
            int commentIndex = lineContent.IndexOf(CommentStartCharacter);

            if (commentIndex > 0)
            {
                lineContent = lineContent[..commentIndex];
                lineContent = lineContent.TrimEnd();
            }

            return lineContent;
        }
    }
}
