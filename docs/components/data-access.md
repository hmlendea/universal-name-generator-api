# Data access

This document covers the XML schema repository, the word list repository, and the data object mapping layer.

## GenerationSchemaRepository

`UniversalNameGenerator.API/DataAccess/Repositories/GenerationSchemaRepository.cs`

### Responsibility

Reads and deserialises `GenerationSchemas.xml` into `GenerationSchemaDataObject` instances.

### Implementation

Inherits from `NuciDAL.XmlRepository<GenerationSchemaDataObject>`. The NuciDAL base class handles XML deserialisation.

```csharp
public class GenerationSchemaRepository : XmlRepository<GenerationSchemaDataObject>, IGenerationSchemaRepository
{
    public GenerationSchemaRepository(DataStoreSettings settings)
        : base(settings.GenerationSchemasPath)
    {
    }
}
```

The constructor receives `DataStoreSettings` and passes the `GenerationSchemasPath` to the base class.

### Interface

```csharp
public interface IGenerationSchemaRepository
{
    IReadOnlyCollection<GenerationSchemaDataObject> GetGenerationSchemas();
}
```

Returns all schemas from the XML file.

### XML format

`GenerationSchemas.xml` contains an array of `GenerationSchemaDataObject` elements:

```xml
<ArrayOfGenerationSchemaDataObject>
  <GenerationSchemaDataObject>
    <Id>arabic-toponyms</Id>
    <Name>Arabic Toponyms</Name>
    <Category>Geographic</Category>
    <Schema>{randomiser, ,1,1,arabic-cities}{randomiser, ,1,1,arabic-epithets}</Schema>
    <FilterlistPath>arabic-filter</FilterlistPath>
    <WordCase>Title</WordCase>
  </GenerationSchemaDataObject>
</ArrayOfGenerationSchemaDataObject>
```

| Element | Type | Required | Description |
|---------|------|----------|-------------|
| `Id` | string | Yes | Unique identifier for schema resolution. |
| `Name` | string | No | Human-readable name. |
| `Category` | string | No | Category for grouping. |
| `Schema` | string | Yes | Schema string with generator commands. |
| `FilterlistPath` | string | No | Word list identifier for filtering. |
| `WordCase` | enum | No | Casing: `Lower`, `Upper`, `Title`, `Sentence`. Default `Title`. |

## WordRepository

`UniversalNameGenerator.API/DataAccess/Repositories/WordRepository.cs`

### Responsibility

Reads `.lst` word list files and parses them into `WordDataObject` instances.

### Implementation

```csharp
public class WordRepository : IWordRepository
{
    private readonly DataStoreSettings _settings;

    public WordRepository(DataStoreSettings settings)
    {
        _settings = settings;
    }

    public WordDataObject GetWords(string wordListId)
    {
        var path = Path.Combine(_settings.WordListsRootDirectory, $"{wordListId}.lst");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Word list '{wordListId}' not found at '{path}'.");
        }

        var lines = File.ReadAllLines(path);
        var values = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
            {
                continue;
            }

            var parts = trimmed.Split('_', 2);
            if (parts.Length == 2)
            {
                values.Add(parts[1]);
            }
        }

        return new WordDataObject { Id = wordListId, Values = values };
    }
}
```

### File format

`.lst` files contain one entry per line:

```
id_value
# comment lines are ignored
id_another value with spaces
```

- Lines starting with `#` are comments.
- Each non-comment line is split on the first underscore `_`.
- The part before the underscore is the entry identifier (ignored by the repository).
- The part after the underscore is the value added to the word list.

Example `arabic-cities.lst`:

```
# Arabic cities
city_Al-Qahira
city_Dimashq
city_Baghdad
city_Cordoba
```

### Interface

```csharp
public interface IWordRepository
{
    WordDataObject GetWords(string wordListId);
}
```

Returns a single `WordDataObject` for the requested word list identifier.

## Data objects

### GenerationSchemaDataObject

`UniversalNameGenerator.API/DataAccess/DataObjects/GenerationSchemaDataObject.cs`

```csharp
[XmlRoot("GenerationSchemaDataObject")]
public class GenerationSchemaDataObject : EntityBase
{
    public string Name { get; set; }
    public string Category { get; set; }
    public string Schema { get; set; }
    public string FilterlistPath { get; set; }
    public WordCase WordCase { get; set; } = WordCase.Title;
}
```

Inherits `EntityBase` which provides `Id` and ordinal equality.

### WordDataObject

`UniversalNameGenerator.API/DataAccess/DataObjects/WordDataObject.cs`

```csharp
[XmlRoot("WordDataObject")]
public class WordDataObject : EntityBase
{
    public List<string> Values { get; set; } = new();
}
```

Inherits `EntityBase` which provides `Id` and ordinal equality.

### EntityBase

```csharp
public abstract class EntityBase
{
    public string Id { get; set; }

    public override bool Equals(object obj) => obj is EntityBase other && Id == other.Id;
    public override int GetHashCode() => Id?.GetHashCode() ?? 0;
}
```

Provides identity-based equality using ordinal string comparison.

## Mapping extensions

### GenerationSchemaMappingExtensions

`UniversalNameGenerator.API/Service/Mappings/GenerationSchemaMappingExtensions.cs`

```csharp
public static GenerationSchema ToServiceModel(this GenerationSchemaDataObject dataObject)
{
    return new GenerationSchema
    {
        Id = dataObject.Id,
        Name = dataObject.Name,
        Category = dataObject.Category,
        Schema = dataObject.Schema,
        FilterlistPath = dataObject.FilterlistPath,
        WordCase = dataObject.WordCase
    };
}
```

Maps the XML data object to the service model. The `WordCase` enum is shared.

### WordMappingExtensions

`UniversalNameGenerator.API/Service/Mappings/WordMappingExtensions.cs`

```csharp
public static Word ToServiceModel(this WordDataObject dataObject)
{
    return new Word
    {
        Id = dataObject.Id,
        Values = dataObject.Values
    };
}
```

Maps the word data object to the service model.

## Service models

### GenerationSchema

`UniversalNameGenerator.API/Service/Models/GenerationSchema.cs`

```csharp
public class GenerationSchema
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string Schema { get; set; }
    public string FilterlistPath { get; set; }
    public WordCase WordCase { get; set; }
}
```

Used by `NameGeneratorService` for generation.

### Word

`UniversalNameGenerator.API/Service/Models/Word.cs`

```csharp
public class Word
{
    public string Id { get; set; }
    public List<string> Values { get; set; }
}
```

Used by generators to access word list values.

## Testing

Unit tests in `UniversalNameGenerator.API.UnitTests/DataAccess/` cover:

- XML deserialisation of schemas.
- `.lst` file parsing (comments, underscores, empty lines).
- Mapping extensions.
- Missing file handling.

See [Testing](../testing.md).