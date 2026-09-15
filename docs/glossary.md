# Glossary

Vocabulary of Buckl. Each term lists the exact name the code uses. When a document, a commit or a
conversation needs one of these concepts, it uses this name and no synonym.

Terms marked _(phase N)_ are defined now so the model is coherent, but their code arrives in that
phase.

---

**Archive** — `Garment.Archive()`, `GarmentStatus.Archived`
Hiding a garment from the wardrobe without deleting it, for clothes the user no longer wears but
wants to keep on record (given away, worn out, stored). An archived garment keeps its data and can
be restored. See also: Garment status.

**Category** — `Category`
The kind of garment: top, bottom, dress, outerwear, footwear, accessory, and the finer values the
domain defines. A garment has exactly one category. Categories drive wardrobe filters and outfit
composition rules. Value object.

**Classification** — `Classification`
The trio that describes a garment for filtering and outfit rules: its category, its dominant color
and, optionally, its size label. Value object; a garment always has exactly one. See also:
Category, Color, Size.

**Color** — `Color`
The dominant color of a garment, from a fixed palette the domain defines (so filters and outfit
rules can compare colors). Value object.

**Garment** — `Garment`
A physical piece of clothing owned by one user: a photo, a classification (category, color, size),
optional purchase information and an optional link to the catalog Product it came from. Garments
are private to their owner; Row-Level Security guarantees it. This is the central entity of Buckl.
See also: Product, Wardrobe.

**Garment status** — `GarmentStatus` (`Active`, `Archived`)
Whether a garment is part of the current wardrobe (`Active`) or kept only on record (`Archived`).

**Import** — `ImportSource`, `IProductImporter` _(phase 7)_
Creating a Product and a Garment automatically from an external source instead of typing them. Two
import mechanisms are in scope for v1: a shared product URL and a purchase confirmation email.
Each supported store is an adapter behind the same interface. See also: Import source.

**Import source** — `ImportSource` (`Manual`, `Url`, `Email`)
Where a garment or product came from: entered by hand, imported from a product URL, or parsed from
a purchase email. Recorded on both `Product` and `Garment`.

**Money** — `Money`
An amount with an ISO 4217 currency code (for example `ARS`, `USD`, `EUR`). Amounts are never
stored without their currency. Value object. See also: Purchase info.

**Occasion** — `Occasion` _(phase 8)_
The situation an outfit is meant for (casual, work, formal, sport, and so on). Used to build and
recommend outfits. Value object.

**Outfit** — `Outfit` _(phase 8)_
A named combination of garments a user puts together for an occasion. An outfit references
garments; it does not copy them. See also: Wear log.

**Photo** — `PhotoKey`
The picture of a garment taken by the user. The file lives in object storage (Cloudflare R2); the
garment stores only the object key. The app never exposes storage credentials to the browser; it
serves short-lived signed URLs instead.

**Product** — `Product`
A catalog article: brand, name, reference image and the source it was imported from. A Product is
the shared, impersonal description of an item a store sells; a Garment is the user's own copy of
it. Several garments (of the same or different users) may reference the same Product. Products are
created as a side effect of imports and are not user-scoped. See also: Garment.

**Purchase info** — `PurchaseInfo`
Optional record of when and for how much a garment was bought: a `Money` price and a purchase
date. Filled automatically by imports, editable by hand. Value object.

**Recommendation** _(phase 8)_
A suggested outfit built from the user's wardrobe by rules (category completeness, color
compatibility, occasion, recent use). Recommendation by LLM is deferred to post-v1.

**Row-Level Security (RLS)**
Postgres feature that filters rows per policy. Buckl's policies read the session variable
`app.user_id`, which the API sets on every request, so a user can only see and modify their own
rows even if application code has a bug. See also: User.

**Size** — `Size`
The size label of a garment as the user or the store states it (`M`, `42`, `32x32`). Kept as a
label, not normalized across systems. Value object.

**Store adapter** — implementation of `IProductImporter` _(phase 7)_
Code that knows how to read one store's product pages or purchase emails and turn them into a
Product and Garment draft. One adapter per store, isolated behind the common interface.

**User** — `User`
A person with a Buckl account. Authentication is delegated to Auth0; the API keeps a local user
record keyed by the Auth0 subject (`sub`) and uses its id as the owner of garments and outfits.

**Wardrobe** — `WardrobeFilter`, and use case naming such as `ListWardrobe`
The set of a user's active garments, as the user sees it in the main screen: filterable by
category, color and size, and searchable by text. Not an entity: a query over garments, whose
criteria are the `WardrobeFilter` value object.

**Wear log** — `WearLog` _(phase 8)_
The record that an outfit was worn on a given date, used to avoid recommending the same thing
twice in a row.
