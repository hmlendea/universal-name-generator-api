# Name generation behaviour

This document describes the user-facing behaviour of name generation: how schema identifiers resolve, how schema strings are interpreted, how generators produce names, and how casing and filters shape the output.

## Schema resolution

1. The client sends `GET /Names?schema=<id>&count=<n>`.
2. `NameGeneratorService` loads all generation schemas from `GenerationSchemas.xml`.
3. It finds the schema whose `Id` matches the requested identifier using ordinal string comparison.
4. If no schema matches, the request fails with a schema-not-found error.

Schema identifiers are case-sensitive. `arabic-toponyms` and `Arabic-Toponyms` are different identifiers.

## Schema string interpretation

A schema's `Schema` property is a string containing literal text and generator commands. Commands use the format `{command,param1,param2,...}`.

Example schema string:

```
{randomiser, ,1,1,arabic-cities}{randomiser, ,1,1,arabic-epithets}
```

The service parses the string left to right:

- Literal text is copied to the output.
- `{...}` segments are parsed as commands.
- Each command dispatches to a generator strategy.

### Supported commands

| Command | Parameters | Behaviour |
|---------|------------|-----------|
| `random` | `choices,min,max` | Selects `min` to `max` random elements from a comma-separated choice list. |
| `randomiser` | `separator,min,max,wordlists` | For each of `min` to `max` iterations, picks one random element from each listed word list and joins them with `separator`. |
| `random-selector` | `min,max,wordlists` | Combines all values from the listed word lists and selects `min` to `max` random elements. |
| `markov` | `min,max,wordlists` | Builds a Markov chain from the listed word lists and generates `min` to `max` names. |

Command names are case-sensitive ordinal matches. Unknown commands fail the request.

### Parameter details

- `min` and `max` are inclusive integers. The service picks a random count in `[min, max]` per generation.
- `separator` is a literal string inserted between word list elements in `randomiser`.
- `choices` is a comma-separated list of literal strings for `random`.
- `wordlists` is a comma-separated list of word list identifiers. Each identifier resolves to a `.lst` file.

## Generator strategies

### random

`NameGeneratorService.GetRandomStrings` selects random elements from a literal choice list. No word list loading occurs.

### randomiser

`RandomiserNameGenerator` joins random elements from each word list with the separator. For each output name, it iterates the word lists in order, picks one random value from each, and concatenates them with the separator.

### random-selector

`RandomSelectorNameGenerator` combines all values from all listed word lists into one pool and selects random elements from that pool.

### markov

The Markov generator uses `NuciGenerators.Text.MarkovChain`. It builds a chain from the combined word list values and generates names by walking the chain.

## Filter lists

A schema may specify a `FilterlistPath`. The service loads the filter word list and removes its values from the generated names before returning them. Filter lists use the same `.lst` format as word lists.

## Word casing

A schema's `WordCase` property controls the casing of generated names. The service applies casing after generation.

| WordCase | Behaviour |
|----------|-----------|
| `Lower` | All characters lowercased. |
| `Upper` | All characters uppercased. |
| `Title` | First character of each word uppercased, rest lowercased. |
| `Sentence` | First character of the first word uppercased, rest lowercased. |

The default `WordCase` is `Title` when the XML element is absent.

## Count semantics

The `count` parameter requests a number of names. The service generates names in a loop until it has produced `count` names or exhausts its attempts. Generators that produce duplicates may return fewer than `count` unique names depending on the strategy.

## Output shape

The response contains a `names` array of strings. Each string is one generated name with casing applied and filter values removed.

## Failure behaviour

| Condition | Result |
|-----------|--------|
| Unknown schema identifier | Error response, no generation attempted |
| Unknown word list identifier | Error response from word list loading |
| Malformed schema string | Error response from parsing |
| Generator failure | Error response propagated through exception middleware |

See [Error handling](../error-handling.md).
