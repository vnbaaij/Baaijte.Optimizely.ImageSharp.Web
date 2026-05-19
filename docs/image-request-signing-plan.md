# Image request signing implementation plan

## Goal

Add an optional image request signing feature to `Baaijte.Optimizely.ImageSharp.Web` to prevent unauthorized image resize and processing requests from overloading Optimizely CMS.

The feature must:

- be optional and disabled by default
- use a shared salt configured by the application
- work in both v4 and v3
- allow original image requests without a hash
- require a valid hash for image processing requests
- return `403` for missing or invalid hashes
- use a contract that can be reproduced in .NET, JavaScript, and TypeScript

## Final signing contract

### Hash query parameter

- The hash query parameter name is fixed as `h`
- `h` is the first unsigned parameter in the query string

### What is hashed

Hash the raw request content consisting of:

- the raw path, excluding domain, scheme, host, and port
- the raw query string content before the first `h` parameter

Do not hash:

- the domain
- the `h` parameter itself
- any query parameters that appear after `h`

### Order rules

- The implementation does not reorder or canonicalize parameters
- The exact raw query substring before `h` is part of the contract
- Therefore, the CMS and delivery server must produce the same URL structure before `h`

### Validation rules

- If no image processing parameters are present, allow the request without `h`
- If image processing parameters are present:
  - `h` must exist
  - `h` must validate successfully
  - otherwise return `403`
- Any image processing parameters found after `h` must cause the request to be rejected with `403`

### Processing parameters in scope

At minimum, treat these as image processing parameters:

- `width`
- `height`
- `rmode`
- `rxy`
- `quality`
- `format`

When enforcing the `after h` rule, these parameters must not appear after `h`.

## Hash algorithm

Use `SHA-256` with lowercase hex output.

Recommended payload format:

```text
{salt}|{rawPathAndSignedQuery}
```

Examples:

```text
my-salt|/contentassets/image.jpg?width=400&height=300
my-salt|/globalassets/photo.webp?width=200
```

Notes:

- The salt is part of configuration
- The same salt and payload format must be used by CMS and delivery servers
- The same algorithm must be used in .NET and JavaScript/TypeScript implementations

## Configuration

Add a new options class, for example:

- `ImageRequestSigningOptions`

Properties:

- `Enabled` (`bool`, default `false`)
- `Salt` (`string`)

Rules:

- when `Enabled` is `false`, existing behavior remains unchanged
- when `Enabled` is `true`, `Salt` must be configured

## Implementation design

### 1. Add options model

Create an options class to hold signing configuration.

Suggested file:

- `src/Baaijte.Optimizely.ImageSharp.Web/Configuration/ImageRequestSigningOptions.cs`

Responsibilities:

- store `Enabled`
- store `Salt`
- support binding from app configuration

### 2. Add a shared signing service

Create a service responsible for all signing and validation logic.

Suggested files:

- `src/Baaijte.Optimizely.ImageSharp.Web/Services/IImageRequestHashService.cs`
- `src/Baaijte.Optimizely.ImageSharp.Web/Services/ImageRequestHashService.cs`

Responsibilities:

- inspect an incoming or generated URL
- find the first `h` parameter
- extract the raw path plus raw query content before `h`
- detect whether processing parameters are present
- detect whether processing parameters appear after `h`
- compute SHA-256 hashes using the configured salt
- validate incoming `h` values

### 3. Register the feature in service setup

Update service registration so the signing options and signing service are available through DI.

Likely file:

- `src/Baaijte.Optimizely.ImageSharp.Web/ServiceAndAppExtensions.cs`

Changes:

- register options binding and validation
- register the signing service
- keep existing setup unchanged when the feature is disabled

### 4. Add signing support to URL generation

Update URL generation helpers so signed URLs can be produced by the CMS.

Likely files:

- `src/Baaijte.Optimizely.ImageSharp.Web/Extensions/HtmlHelperExtensions.cs`
- `src/Baaijte.Optimizely.ImageSharp.Web/Extensions/UrlBuilderExtensions.cs`

Design goals:

- generate the hash after the relevant query parameters have been added
- append `h` using the final raw URL content that should be signed
- ensure generated URLs match the middleware validation contract

Possible API shape:

- a new extension method that appends `h`
- or automatic signing in the image URL helper flow when signing is enabled

Implementation detail to preserve:

- signed parameters must appear before `h`
- any extra query parameters that consumers want to append after `h` remain unsigned

### 5. Enforce validation before image processing

Add request validation in the inbound image request pipeline.

Likely integration point:

- the package's image provider flow around `BlobImageProvider`
- or a dedicated middleware/filter invoked before ImageSharp processing executes

Responsibilities:

- inspect the request path and raw query string
- determine whether the request is an image processing request
- require and validate `h` when processing parameters are present
- reject with `403` if:
  - `h` is missing
  - `h` is invalid
  - processing parameters appear after `h`

Behavior:

- original image requests continue to work without a hash
- only processing requests are protected

### 6. Ensure raw request access is used consistently

Use the raw request values already available through `HttpContext.Request`.

The implementation should rely on:

- `Request.Path`
- raw query string access

It should not rely on:

- full display URL including domain
- rebuilt or reordered query strings

## Validation and test plan

Add tests covering both URL generation and inbound validation.

### Unit tests

For the signing service:

- computes the same hash for the same salt and raw input
- excludes the domain from the hash input
- includes the raw path in the hash input
- hashes only the raw query substring before `h`
- ignores `h` itself
- ignores parameters after `h`
- treats parameter order as significant because raw input is hashed as-is
- detects processing parameters before `h`
- detects processing parameters after `h`

### Behavioral tests

- original image request without query parameters is allowed
- original image request with unrelated unsigned parameters is allowed if no processing commands are used
- resize request without `h` returns `403`
- resize request with invalid `h` returns `403`
- resize request with valid `h` succeeds
- request with processing parameters after `h` returns `403`
- request with extra non-processing parameters after `h` succeeds when the hash is otherwise valid
- feature disabled preserves current behavior

### Cross-platform verification

Add at least one documented example that can be reproduced outside .NET.

Example test vector should include:

- salt
- raw path and signed query portion
- expected SHA-256 hex value

This will make it easy to implement matching logic in JavaScript/TypeScript.

## Documentation updates

Update `README.md` to include:

- feature overview
- why the feature exists
- configuration example
- how `h` works
- exact hash contract
- note that only the raw path and raw query before `h` are hashed
- note that anything after `h` is ignored for hash validation
- note that processing parameters after `h` are rejected
- examples for CMS and delivery server interoperability

## Delivery plan

### Phase 1: v4 implementation on `main`

1. Add configuration model
2. Add signing service
3. Update URL generation helpers
4. Add inbound validation
5. Add tests
6. Update README
7. Build and verify behavior

### Phase 2: v3 branch

1. Create a dedicated `v3` branch from the correct v3 baseline
2. Port the same feature
3. Keep the same:
   - configuration shape
   - query parameter name `h`
   - payload format
   - SHA-256 algorithm
   - validation behavior
4. Re-run tests and update documentation for the v3 package line

## Suggested task breakdown

### Task 1
Create `ImageRequestSigningOptions` and register it.

### Task 2
Implement `ImageRequestHashService` with:

- raw signed portion extraction
- processing parameter detection
- after-`h` validation
- SHA-256 generation
- hash validation

### Task 3
Add URL signing support to the existing helper APIs.

### Task 4
Add request validation before ImageSharp processing.

### Task 5
Add tests for generation, validation, and failure cases.

### Task 6
Update README and produce one or more cross-platform test vectors.

## Acceptance criteria

The feature is complete when all of the following are true:

- signing is optional and disabled by default
- salt is configurable
- original image requests work without a hash
- processing requests require a valid `h`
- missing or invalid hashes return `403`
- processing parameters after `h` return `403`
- non-processing parameters after `h` are allowed and ignored for hashing
- the hash input excludes the domain
- the hash input uses the raw path plus raw query content before `h`
- v4 implementation is complete on `main`
- a separate `v3` branch is created and updated with the same feature contract
