# Frequently asked questions

## General

### What is the Universal Name Generator API?
A .NET 10 ASP.NET Core service that generates names from configurable schemas. Schemas define generator commands (`random`, `randomiser`, `random-selector`, `markov`) that combine word lists to produce names.

### What are the supported generator types?
- **random**: Selects from explicit choices in the schema.
- **randomiser**: Joins one random element from each of multiple word lists.
- **random-selector**: Picks one random element from the combined pool of all word lists.
- **markov**: Uses Markov chain generation from `NuciGenerators.Text`.

### Is there a database?
No. Data is stored in XML (`GenerationSchemas.xml`) and `.lst` word list files. Read-only at runtime.

### Can I run multiple instances?
Yes. Shared-nothing architecture. Horizontal scaling by running multiple processes behind a load balancer.

## Authentication

### How do I authenticate?
Include an `Authorization` header with the API key:
```
Authorization: Bearer your-api-key
```
or
```
Authorization: your-api-key
```

### Where do I set the API key?
In `appsettings.json` under `securitySettings:apiKey`, or via environment variable `SecuritySettings__ApiKey`.

### Can I use OAuth / JWT / API keys per client?
No. Single shared API key only. For multi-tenant scenarios, deploy separate instances.

### What happens if I don't set the API key?
The placeholder `[[UNIVERSAL_NAME_GENERATOR_API_KEY]]` will not match any real key. All requests return 401.

## Schemas

### How do I create a schema?
Add a `GenerationSchemaDataObject` element to `GenerationSchemas.xml`:
```xml
<GenerationSchemaDataObject>
  <Id>my-schema</Id>
  <Name>My Schema</Name>
  <Category>Test</Category>
  <Schema>{randomiser,list1,list2}</Schema>
  <FilterlistPath>filters.lst</FilterlistPath>
  <WordCase>Title</WordCase>
</GenerationSchemaDataObject>
```

### What is the schema string format?
`{command,param1,param2,...}` where command is one of `random`, `randomiser`, `random-selector`, `markov`.

Examples:
- `{random,choice1,choice2,choice3}`
- `{randomiser,first-names,last-names}`
- `{random-selector,adjectives,nouns}`
- `{markov,place-names,3}`

### Can I have multiple commands in one schema?
No. One schema = one command. Chain multiple schemas client-side if needed.

### How do I reference a word list?
Use the word list's `Id` (filename without `.lst`) as a parameter in the schema string.

### What is the filter list?
A `.lst` file whose values are removed from generated names. Applied before casing.

### What casing options exist?
`Lower`, `Upper`, `Title` (default), `Sentence`. Set via `WordCase` in schema XML.

## Word lists

### What is the .lst file format?
One entry per line: `id_value`. Lines starting with `#` are comments. Empty lines ignored.

Example:
```
# Fantasy names
elf_legolas
elf_elrond
dwarf_gimli
dwarf_thorin
```

### Where are word lists stored?
In the directory configured by `dataStoreSettings:wordListsRootDirectory` (default: `Data/Wordlists/`).

### Can I add word lists at runtime?
Yes. Drop `.lst` files in the directory. Next request will read them. No restart needed for new files.

### Can I modify word lists at runtime?
Yes. Changes picked up on next request. No caching across requests.

## Generation

### What is the maximum count per request?
100,000 names.

### Why do I get fewer names than requested?
The generator stops after 1000 attempts. If many duplicates or empty strings are produced, fewer unique names are returned.

### Can I get unique names only?
Not guaranteed. The API may return duplicates. Deduplicate client-side if needed.

### How does the markov generator work?
Delegates to `NuciGenerators.Text.MarkovNameGenerator`. Requires sufficient training data in the word list.

## Configuration

### How do I change the data directory?
Set `dataStoreSettings:wordListsRootDirectory` and `dataStoreSettings:generationSchemasPath` in `appsettings.json`.

### Can I use environment variables?
Yes. Standard ASP.NET Core configuration: `DataStoreSettings__WordListsRootDirectory`, etc.

### Is hot reload supported?
No. Configuration changes require restart. Schema/word list file changes are read per request.

## Deployment

### What are the runtime requirements?
.NET 10 runtime. No database, no external services.

### How do I deploy?
Publish with `dotnet publish -c Release`. Run the executable. Configure `appsettings.json` for production.

### Does it support Docker?
No Dockerfile provided, but standard ASP.NET Core containerisation applies.

### What ports does it use?
Default ASP.NET Core ports (5000 HTTP, 5001 HTTPS). Configurable via `ASPNETCORE_URLS`.

## Monitoring

### Is there a health endpoint?
No. Use process/port monitoring at infrastructure level.

### Where are logs written?
File: `logfile.log` (configurable via `nuciLoggerSettings:logFilePath`). Console: always.

### What log format?
NuciLog structured format with `Schema` and `Count` keys for generation requests.

## Development

### How do I run tests?
```bash
dotnet test UniversalNameGenerator.API.slnx
```

### How do I add a generator?
See `docs/change-guide.md#adding-a-generator-command`.

### How do I run locally?
```bash
cd UniversalNameGenerator.API
dotnet run
```

### What IDE support?
VS Code with C# Dev Kit, Visual Studio 2022+, Rider.

## Troubleshooting

### Why do I get 401?
Check API key in header matches `appsettings.json`.

### Why do I get 400 SchemaNotFound?
Schema ID case mismatch or missing from XML.

### Why do I get 500?
Check logs. Common: missing word list file, malformed schema string.

### Why are names empty?
Generator produced no valid names in 1000 attempts. Check filter list and word list data.