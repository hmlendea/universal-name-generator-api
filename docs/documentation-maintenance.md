# Documentation maintenance

This document describes how to keep the `docs/` directory current and consistent.

## Documentation structure

```
docs/
├── INDEX.md                    # Catalogue of all docs
├── getting-started.md          # Quick start
├── architecture-overview.md    # High-level architecture
├── api-reference/              # One file per controller
│   └── names-controller.md
├── behaviour/                  # Runtime behaviour
│   ├── name-generation.md
│   ├── authentication.md
│   ├── error-handling.md
│   ├── cors.md
│   ├── request-logging.md
│   └── scanner-protection.md
├── components/                 # Component deep-dives
│   ├── name-generator-service.md
│   ├── generation-schema-repository.md
│   ├── word-repository.md
│   ├── name-generators.md
│   ├── configuration.md
│   ├── middleware-pipeline.md
│   └── logging.md
├── flows/                      # End-to-end flows
│   ├── get-names-request.md
│   ├── schema-resolution.md
│   ├── word-list-loading.md
│   └── startup.md
├── cross-cutting/              # Cross-cutting concerns
│   ├── security.md
│   ├── observability.md
│   └── configuration-management.md
├── data-and-state/             # Data and state
│   ├── data-model.md
│   ├── file-formats.md
│   └── caching.md
├── operations/                 # Operations
│   ├── deployment.md
│   ├── configuration.md
│   ├── monitoring.md
│   └── backup-and-restore.md
├── testing.md                  # Testing strategy
├── design-decisions.md         # ADR-style decisions
├── concurrency-and-scheduling.md
├── invariants.md               # System invariants
├── change-guide.md             # How to modify safely
├── troubleshooting.md          # Common issues
├── faq.md                      # Frequently asked questions
├── api-usage-examples.md       # Example requests/responses
└── ambiguities-and-open-questions.md
```

## When to update documentation

| Trigger | Files to update |
|---------|-----------------|
| New endpoint | `api-reference/`, `INDEX.md`, `api-usage-examples.md` |
| New generator command | `behaviour/name-generation.md`, `components/name-generators.md`, `design-decisions.md` |
| New middleware | `components/middleware-pipeline.md`, `behaviour/`, `cross-cutting/` |
| Configuration change | `components/configuration.md`, `operations/configuration.md`, `cross-cutting/configuration-management.md` |
| Data format change | `data-and-state/file-formats.md`, `data-and-state/data-model.md` |
| Architecture change | `architecture-overview.md`, `design-decisions.md`, `invariants.md` |
| Bug fix with behavioural change | `behaviour/`, `troubleshooting.md` |
| New test pattern | `testing.md` |

## Update process

1. **Identify affected files** using the table above.
2. **Make changes** in the same PR as the code change.
3. **Verify links** work (relative paths from `docs/`).
4. **Update `INDEX.md`** if files added/removed.
5. **Run link check** (if available in CI).

## Style guidelines

- Use Markdown with ATX headings (`#`, `##`, `###`).
- Tables for structured data (invariants, configuration, etc.).
- Mermaid diagrams for flows and architecture.
- Code blocks with language hints (`csharp`, `json`, `xml`, `bash`).
- Cross-reference with `[link text](../path/to/file.md#anchor)`.
- Keep line length reasonable (no hard limit, but avoid extreme lines).

## Link validation

Add to CI (optional):
```bash
# Check for broken internal links
find docs -name "*.md" -exec grep -h '\[.*\](\..*)' {} \; | \
  sed -E 's/.*\((\..*?)\).*/\1/' | \
  while read link; do
    if [ ! -e "docs/$link" ]; then
      echo "Broken link: $link"
    fi
  done
```

## Versioning

Documentation is versioned with the code. No separate versioning.

## Review checklist

- [ ] All new public APIs documented.
- [ ] Behavioural changes reflected in `behaviour/`.
- [ ] Configuration changes in `components/configuration.md` and `operations/configuration.md`.
- [ ] Data format changes in `data-and-state/`.
- [ ] `INDEX.md` updated.
- [ ] No broken internal links.
- [ ] Mermaid diagrams render correctly.
- [ ] Code examples compile (copy-paste test).