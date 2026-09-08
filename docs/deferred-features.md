# Discarded and deferred features

Buckl has no separate "non-goals" document. Instead, every feature that was considered and left
out is recorded here with its reason, so the question does not get reopened by accident and so a
deferred item can be picked up with its context intact.

- **Discarded**: not planned; bringing it back would need a new decision.
- **Deferred**: wanted, but out of v1; planned only after v1 is closed.

| Feature                                                                         | Status    | Date       | Reason                                                                                                                                                                 | Revisit when                                                 |
| ------------------------------------------------------------------------------- | --------- | ---------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------ |
| Adding garments by scanning the label barcode                                   | Discarded | 2026-09-07 | Clothing barcodes rarely resolve to public product data, so the value is low for the effort and the camera work involved. Photo, URL and email imports cover the need. | A reliable public lookup for apparel barcodes appears.       |
| Browser extension and other automatic capture mechanisms                        | Deferred  | 2026-09-07 | An extra platform to build, package and get reviewed, while sharing a URL from the phone already covers the main flow.                                                 | v1 is closed and URL import is stable.                       |
| Outfit recommendation with an LLM                                               | Deferred  | 2026-09-07 | Cost and non-determinism. A rule-based engine in phase 8 is testable and free.                                                                                         | Rule-based recommendations exist and their limits are known. |
| 2D try-on through an external API                                               | Deferred  | 2026-09-07 | Paid third-party APIs, and it is not needed to choose an outfit from a photographed wardrobe.                                                                          | A free or cheap API is available and outfits work.           |
| Shared garment catalog (user-published photos, matching, aggregated style data) | Deferred  | 2026-09-07 | Privacy implications and moderation, and it needs a user base. The `Product` entity already leaves room for it.                                                        | v1 has users and a privacy review is done.                   |

## How to add an entry

Add a row with the date of the decision and the concrete reason. If the decision has a technical
component, link its ADR. When an item is picked up again, move it out of this table in the pull
request that starts the work.
