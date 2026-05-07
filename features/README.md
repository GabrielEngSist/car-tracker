## BDD executable requirements (Gherkin)

This folder contains **Product Owner–oriented executable requirements** written in **Gherkin**.

### Conventions
- Scenarios are written from the **client/business perspective** (API observable behavior).
- For **expected faults**, assertions use stable `faults[].code` (and `propertyName`) rather than message text.
- **404 Not Found** is used when a resource does not exist (e.g., car or entry missing).
- **400 Bad Request** is used for validation/business faults returned as a fault envelope.
- **500 Internal Server Error** is used when the fault code `UNEXPECTED` is present.

### Fault envelope shape
When the API returns faults, the response is JSON:

- `faults[]` items contain:
  - `code` (string, stable)
  - `propertyName` (string | null)
  - `message` (string, informational)

See `faults/fault-envelope.feature`.

### How to automate later
These features are designed to be consumed by a BDD runner (e.g., SpecFlow / Reqnroll, Cucumber).
Step definitions can target either:
- the **HTTP API** (`/api/...`), or
- the **application mediator** (if you prefer in-process tests).

