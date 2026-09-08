# Buckl documentation

Buckl is a personal wardrobe app: you photograph or import the clothes you own, and the app helps
you decide what to wear without pulling everything out of the closet. This directory is the single
place where the design, the decisions and the vocabulary of the project live.

## Index

| Document                                       | What it answers                                                  |
| ---------------------------------------------- | ---------------------------------------------------------------- |
| [Architecture](architecture/README.md)         | What Buckl is made of and how the pieces talk                    |
| [Glossary](glossary.md)                        | The vocabulary and the exact names the code uses                 |
| [Privacy and personal data](privacy.md)        | What is stored about a person, where, why, and how it is deleted |
| [Architecture decision records](adr/README.md) | Why each technology and process choice was made                  |

The index grows as documents land. Each pull request that adds a document adds its row here.

## Conventions

- Everything in this repository (code, documentation, UI copy, commit messages) is written in
  English.
- Documents are Markdown formatted by Prettier (`npm run format`). Line width is 100 characters.
- Diagrams are Mermaid code blocks inside the document that explains them. There are no separate
  diagram files or images.
- One `#` heading per file. Link between documents with relative paths.
- Domain terms use the exact name the code uses (for example `Garment`, not "clothing item"). The
  [glossary](glossary.md) is the source of truth for those names.
- Dates are written as `YYYY-MM-DD`.
- A technical decision gets an ADR in `adr/`, following [the template](adr/template.md). A change
  to the model or the architecture updates the affected architecture document in the same pull
  request as the code.
