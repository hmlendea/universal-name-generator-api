# Application services

This document covers the `NameGeneratorService`, its generator strategies, and schema interpretation logic.

## INameGeneratorService

`UniversalNameGenerator.API/Service/INameGeneratorService.cs` defines the service contract:

```csharp
public interface INameGeneratorService
{
    IReadOnlyCollection<string> GetNames(string schemaId, int count);
    IReadOnlyCollection<GenerationSchema> GetSchemas();
}
```

- `GetNames` generates `count` names from the schema identified by `schemaId`.
- `GetSchemas` returns all loaded generation schemas for discovery.

## NameGeneratorService

`UniversalNameGenerator.API/Service/NameGeneratorService.cs` is the sole implementation. It is registered as scoped.

### Construction

```csharp
public NameGeneratorService(
    IGenerationSchemaRepository schemaRepository,
    IWordRepository wordRepository,
    ILogger logger)
```

Dependencies:

- `IGenerationSchemaRepository` - Loads schemas from XML.
- `IWordRepository` - Loads word lists from `.lst` files.
- `ILogger` - NuciLog structured logger.

### Schema loading and caching

On first `GetNames` or `GetSchemas` call, the service loads all schemas via `schemaRepository.GetGenerationSchemas()`. It caches the schemas in a `Dictionary<string, GenerationSchema>` keyed by `Id`. The cache lives for the service instance lifetime (scoped, so per request).

### GetNames flow

```csharp
public IReadOnlyCollection<string> GetNames(string schemaId, int count)
{
    var schemas = GetSchemas();
    var schema = schemas.FirstOrDefault(s => s.Id == schemaId);
    if (schema == null) throw new KeyNotFoundException($"Schema '{schemaId}' not found.");

    var names = GenerateNames(schema, count);
    return names;
}
```

1. Resolve the schema by identifier.
2. Call `GenerateNames` with the schema and count.
3. Return the generated names.

### GenerateNames

```csharp
private IReadOnlyCollection<string> GenerateNames(GenerationSchema schema, int count)
{
    var names = new List<string>();
    var attempts = 0;
    const int maxAttempts = 1000;

    while (names.Count < count && attempts < maxAttempts)
    {
        var name = GenerateSingleName(schema);
        if (!string.IsNullOrWhiteSpace(name))
        {
            names.Add(name);
        }
        attempts++;
    }

    return names;
}
```

Generates names in a loop until `count` is reached or `maxAttempts` is exceeded. This prevents infinite loops for generators that produce many duplicates or empty strings.

### GenerateSingleName

```csharp
private string GenerateSingleName(GenerationSchema schema)
{
    var schemaString = schema.Schema;
    var result = new StringBuilder();

    // Parse literal text and commands
    var parts = ParseSchemaString(schemaString);
    foreach (var part in parts)
    {
        if (part.IsCommand)
        {
            var generated = ExecuteCommand(part.Command, part.Parameters, schema);
            result.Append(generated);
        }
        else
        {
            result.Append(part.Literal);
        }
    }

    var name = result.ToString();
    name = ApplyFilterList(name, schema.FilterlistPath);
    name = ApplyWordCase(name, schema.WordCase);
    return name;
}
```

1. Parse the schema string into literal and command segments.
2. Execute each command, appending its output.
3. Apply the filter list (if any).
4. Apply word casing.

### Schema string parsing

`ParseSchemaString` splits the schema string on `{...}` boundaries. It returns a sequence of `SchemaPart` records:

```csharp
private record SchemaPart(bool IsCommand, string Literal, string Command, string[] Parameters);
```

- Text outside `{...}` is literal.
- Text inside `{...}` is split by commas: the first element is the command name, the rest are parameters.

### Command dispatch

`ExecuteCommand` switches on the command name:

| Command | Handler |
|---------|---------|
| `random` | `GetRandomStrings` |
| `randomiser` | `RandomiserNameGenerator.Generate` |
| `random-selector` | `RandomSelectorNameGenerator.Generate` |
| `markov` | `MarkovChain.Generate` |

Unknown commands throw `NotSupportedException`.

### GetRandomStrings

```csharp
private string GetRandomStrings(string[] parameters)
{
    // parameters: choices, min, max
    var choices = parameters[0].Split(',');
    var min = int.Parse(parameters[1]);
    var max = int.Parse(parameters[2]);
    var count = Random.Shared.Next(min, max + 1);
    var selected = choices.OrderBy(_ => Random.Shared.Next()).Take(count);
    return string.Join("", selected);
}
```

Selects random literal choices. No word list access.

### Generator caching

The service caches generator instances per schema in a `Dictionary<string, INameGenerator>`:

```csharp
private readonly Dictionary<string, INameGenerator> _generatorCache = new();
```

The cache key is the schema identifier. Generators are created on first use for a schema and reused for subsequent names in the same request.

### Filter list application

```csharp
private string ApplyFilterList(string name, string filterlistPath)
{
    if (string.IsNullOrWhiteSpace(filterlistPath)) return name;
    var filterWords = _wordRepository.GetWords(filterlistPath);
    var filterValues = filterWords.SelectMany(w => w.Values).ToHashSet();
    return string.Join("", name.Split(' ').Where(w => !filterValues.Contains(w)));
}
```

Loads the filter word list, extracts all values, and removes matching words from the generated name.

### Word casing application

```csharp
private string ApplyWordCase(string name, WordCase wordCase)
{
    return wordCase switch
    {
        WordCase.Lower => name.ToLowerInvariant(),
        WordCase.Upper => name.ToUpperInvariant(),
        WordCase.Title => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLowerInvariant()),
        WordCase.Sentence => char.ToUpperInvariant(name[0]) + name[1..].ToLowerInvariant(),
        _ => name
    };
}
```

Applies the casing transformation after filtering.

## Generator strategies

### RandomiserNameGenerator

`UniversalNameGenerator.API/Service/NameGenerators/Randomiser/RandomiserNameGenerator.cs`

```csharp
public string Generate(GenerationSchema schema, IReadOnlyCollection<Word> words)
{
    // parameters: separator, min, max, wordlists
    var separator = schema.Parameters[0];
    var min = int.Parse(schema.Parameters[1]);
    var max = int.Parse(schema.Parameters[2]);
    var wordlistIds = schema.Parameters[3].Split(',');

    var count = Random.Shared.Next(min, max + 1);
    var parts = new List<string>();

    for (int i = 0; i < count; i++)
    {
        foreach (var wordlistId in wordlistIds)
        {
            var word = words.First(w => w.Id == wordlistId);
            var value = word.Values[Random.Shared.Next(word.Values.Count)];
            parts.Add(value);
        }
    }

    return string.Join(separator, parts);
}
```

For each iteration, picks one random value from each word list in order and joins with the separator.

### RandomSelectorNameGenerator

`UniversalNameGenerator.API/Service/NameGenerators/RandomSelector/RandomSelectorNameGenerator.cs`

```csharp
public string Generate(GenerationSchema schema, IReadOnlyCollection<Word> words)
{
    // parameters: min, max, wordlists
    var min = int.Parse(schema.Parameters[0]);
    var max = int.Parse(schema.Parameters[1]);
    var wordlistIds = schema.Parameters[2].Split(',');

    var allValues = words
        .Where(w => wordlistIds.Contains(w.Id))
        .SelectMany(w => w.Values)
        .ToList();

    var count = Random.Shared.Next(min, max + 1);
    var selected = allValues.OrderBy(_ => Random.Shared.Next()).Take(count);
    return string.Join("", selected);
}
```

Combines all values from all listed word lists and selects random elements.

### Markov generator

Uses `NuciGenerators.Text.MarkovChain` directly. The service builds a chain from the word list values and generates names by walking the chain.

## Logging

The service logs the `GenerateNames` operation with `Schema` and `Count` info keys. See [Logging](../logging.md).

## Testing

Unit tests in `UniversalNameGenerator.API.UnitTests/Service/NameGeneratorServiceTests.cs` cover:

- Schema resolution.
- Command parsing and dispatch.
- Generator output.
- Filter list and casing application.
- Edge cases (empty results, max attempts).

See [Testing](../testing.md).