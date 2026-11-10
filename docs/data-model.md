# Data model

This document describes the domain entities, data objects, XML schema format, and word list format used by the Universal Name Generator API.

## Domain entities

### GenerationSchema

The service model representing a generation schema.

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

| Property | Type | Description |
|----------|------|-------------|
| `Id` | string | Unique identifier for schema resolution. |
| `Name` | string | Human-readable name. |
| `Category` | string | Category for grouping. |
| `Schema` | string | Schema string with generator commands. |
| `FilterlistPath` | string | Word list identifier for filtering (optional). |
| `WordCase` | WordCase | Casing transformation to apply. |

### Word

The service model representing a word list.

```csharp
public class Word
{
    public string Id { get; set; }
    public List<string> Values { get; set; }
}
```

| Property | Type | Description |
|----------|------|-------------|
| `Id` | string | Word list identifier (matches `.lst` filename). |
| `Values` | List<string> | All values loaded from the word list file. |

### WordCase

```csharp
public enum WordCase
{
    Lower,
    Upper,
    Title,
    Sentence
}
```

Controls the casing transformation applied to generated names.

## Data objects (serialisation layer)

### GenerationSchemaDataObject

XML-serialisable representation of a generation schema.

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

XML-serialisable representation of a word list (used for in-memory representation, not XML serialisation).

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

## XML schema format

`GenerationSchemas.xml` contains an array of `GenerationSchemaDataObject` elements serialised by `NuciDAL.XmlRepository`.

```xml
<?xml version="1.0" encoding="utf-8"?>
<ArrayOfGenerationSchemaDataObject xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <GenerationSchemaDataObject>
    <Id>arabic-toponyms</Id>
    <Name>Arabic Toponyms</Name>
    <Category>Geographic</Category>
    <Schema>{randomiser, ,1,1,arabic-cities}{randomiser, ,1,1,arabic-epithets}</Schema>
    <FilterlistPath>arabic-filter</FilterlistPath>
    <WordCase>Title</WordCase>
  </GenerationSchemaDataObject>
  <GenerationSchemaDataObject>
    <Id>fantasy-names</Id>
    <Name>Fantasy Names</Name>
    <Category>Fantasy</Category>
    <Schema>{markov,3,10,fantasy-syllables}</Schema>
    <FilterlistPath></FilterlistPath>
    <WordCase>Title</WordCase>
  </GenerationSchemaDataObject>
</ArrayOfGenerationSchemaDataObject>
```

### Element reference

| Element | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| `Id` | string | Yes | - | Unique identifier. |
| `Name` | string | No | Empty | Human-readable name. |
| `Category` | string | No | Empty | Category for grouping. |
| `Schema` | string | Yes | - | Schema string with commands. |
| `FilterlistPath` | string | No | Empty | Filter word list identifier. |
| `WordCase` | enum | No | `Title` | Casing: `Lower`, `Upper`, `Title`, `Sentence`. |

### Schema string format

The `Schema` element contains a string with literal text and generator commands:

```
{literal text}{command,param1,param2,...}{literal text}
```

Commands use `{command,param1,param2,...}` format. See [Name generation behaviour](../behaviour/name-generation.md) for command details.

## Word list format

`.lst` files are plain text files with one entry per line.

### Format rules

1. Lines starting with `#` are comments and ignored.
2. Empty lines are ignored.
3. Each non-comment line is split on the first underscore `_`.
4. The part before the underscore is an entry identifier (ignored).
5. The part after the underscore is the value added to the word list.

### Example

```
# Arabic cities
city_Al-Qahira
city_Dimashq
city_Baghdad
city_Cordoba
```

Produces a `WordDataObject` with `Id = "arabic-cities"` and `Values = ["Al-Qahira", "Dimashq", "Baghdad", "Cordoba"]`.

### File naming

Word list files are named `{identifier}.lst` and placed in the directory specified by `dataStoreSettings:wordListsRootDirectory`.

The identifier is used in schema commands to reference the word list.

## Mapping

### GenerationSchemaMappingExtensions

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

### WordMappingExtensions

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

## Data flow

```
GenerationSchemas.xml
    -> GenerationSchemaRepository (NuciDAL XmlRepository)
    -> GenerationSchemaDataObject[]
    -> GenerationSchemaMappingExtensions.ToServiceModel()
    -> GenerationSchema[]
    -> NameGeneratorService (cached)

.lst files
    -> WordRepository.GetWords(id)
    -> WordDataObject
    -> WordMappingExtensions.ToServiceModel()
    -> Word
    -> Generators
```

## Testing

Unit tests cover:

- XML deserialisation round-trip.
- `.lst` parsing (comments, underscores, empty lines, spaces in values).
- Mapping extensions.
- Missing file handling.

See [Testing](../testing.md).