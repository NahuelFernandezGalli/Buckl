# ADR-0033: Prepare photos in the browser before upload

- **Status:** Accepted
- **Date:** 2026-09-29
- **Deciders:** NahuelFernandezGalli

## Context

A phone photo is 3 to 6 MB, 12 megapixels, and carries EXIF metadata with the camera, the time
and often the GPS location of the user's home. Buckl shows every garment photo in a 4:5 frame
with `object-fit: cover`. Uploading the original costs mobile data, storage and time, and sends
the location to a server that has no use for it.

## Decision

We crop, scale and re-encode every photo in the browser before it is uploaded: the largest
centered 4:5 region, scaled down (never up) to at most 1080×1350, as a JPEG at quality 0.82.

- `planPhotoCrop` computes the region; it is pure and tested exhaustively.
- `preparePhoto` decodes with `createImageBitmap` (EXIF orientation applied), draws onto a canvas
  filled white, and encodes with `canvas.toBlob`. The browser codec sits behind `PhotoCodec`, so
  tests replace it.
- Re-encoding through a canvas writes no metadata: the stored photo has no location, camera or
  time.
- A file the browser cannot decode (HEIC on most desktop browsers) is reported as unreadable;
  nothing is uploaded.

## Alternatives considered

- **Upload the original and process it on the server** — the location still leaves the phone,
  and image processing on a free API host is slow and memory hungry.
- **Scale without cropping** — cards would still crop when showing, and storage would keep pixels
  nobody sees.
- **An interactive crop** — better for off-center garments; deferred until the simple crop proves
  insufficient.
- **A library (browser-image-compression, pica)** — more options than needed for one fixed
  output.

## Consequences

### Positive

- Uploads of a few hundred kilobytes; what is stored is what is shown.
- No location or camera data ever reaches Buckl.

### Negative

- A garment far from the center of the photo can be cut; the user retakes the photo.
- The codec is not covered by unit tests (jsdom has no canvas); it is checked by hand.
- Formats the browser cannot decode are refused rather than converted.
