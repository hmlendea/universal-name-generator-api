namespace UniversalNameGenerator.API.Configuration
{
    public sealed class DataStoreSettings
    {
        public string WordListsRootDirectory { get; set; } = string.Empty;

        public string GenerationSchemasPath { get; set; } = string.Empty;
    }
}
