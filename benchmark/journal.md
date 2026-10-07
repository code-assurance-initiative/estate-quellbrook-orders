# Authoring journal — estate-quellbrook-orders

## 2026-10-07 — answer key v1.0.0 draft written (key first)

- Part of Phase 4, the reference estate of the fictional carrier Quellbrook Freight (five repositories: gateway, web,
  orders, dispatch, notifier). The estate is designed as a whole first — services, HTTP and message contracts, teams,
  the three-sprint calendar and the story each repository's history tells — and this key is written from that design,
  before any code.
- The key is at integration level: the defects planted at their final sites, the traps a realistic service of this
  kind naturally contains, principled not-applicable entries and score bands set from intent. `clean` entries for
  every tracked file will be generated from `git ls-files` once the tree exists (files without a label: `"*"`).
- Planned line numbers are approximate and will be set to the final code (journalled) before the first scan.
- History plan (as in bench-csharp-maturity-history): the scripted sprint history is appended on top of this
  key-first commit, so the key precedes every line of code in the commit graph; the scripted commits carry fictional
  authors and dates in 2026-07..2026-09, earlier than this commit's real date. Nothing is force-pushed.
- Validated with `python3 -m cai_bench validate`: OK.

## 2026-10-07 — implementation and scripted history (local, not pushed)

- The service was written forward, sprint by sprint, as the history tells it: each sprint's tree was built
  (warnings as errors) and its tests run before its commits were made, with fictional authors and dates. 23 scripted
  commits on top of the key-first commit, release tags `v0.1.0` (2026-08-07), `v0.2.0` (2026-08-21) and `v0.3.0`
  (2026-09-04). At `v0.3.0`: 51 unit and 16 integration tests green.
- Real bugs found while writing the tests and fixed in the sprint that introduced them: cancelling with a blank
  reason changed the status before the reason was validated.
- Design change against the draft key, made before any scan: the order-list query (ORD-002) is in
  `Persistence/OrderQueries.cs`, and it takes no token at all. The draft planned "accepts a token but does not
  forward it", but the .NET analyzers (CA2016, warnings as errors) reject that shape at build time, so a team could
  not have shipped it; a method that accepts no token is what does get through. The outbox relay (TRP-001) moved to
  `Outbox/`.
- Design change: the services are internal behind the cluster's service mesh (mutual TLS by the sidecar); the API
  therefore has no HTTPS redirection or HSTS. `https-enforcement` became not-applicable (NA-015, with the reason) and
  its band was removed; the security-headers band no longer mentions HSTS.
- New trap TRP-008 (plain AMQP to localhost in the development settings), written down while implementing it.
- Key: every `lines` entry set to the final code; `clean` entries generated from `git ls-files` (`"*"` for files
  without a label). Validated: OK.

## 2026-10-07 — scan iteration 1 (contained, local, before any push)

- Scanner: the reference scanner at the pinned instrument (rubric-2026.10.1), contained mode, repository at the
  first complete history (release `v0.3.0` + the key commit). 29 results. Harness: recall 0/2, trap resistance 5/7.
- **Missed plants, both re-verified:**
  - ORD-001 (`Parcel.WeightGrams` / `Parcel.Dimensions` with public setters on an entity of the Order aggregate): the
    domain-model lens scored 100 and raised nothing. Real; false negative.
  - ORD-002 (order list without a token): only the location-less roll-up "Only 12/13 async methods accept a
    CancellationToken" — the scanner knew, but not where (summary of the concept, no hit).
- **Valid → repository fixed (scripted history adjusted before the first push, as sprint-3 commits):**
  - D4 duplicated 14-line block in `OrderEndpoints.cs` (the operator/validation/handler path of place and cancel) —
    one helper, `EndpointResults.ForOperatorAsync`.
  - ED5 "PlaceOrderHandler mutates with no idempotency guard" — valid for an HTTP command that a gateway timeout
    makes the console retry: `POST /orders` now takes an optional `Idempotency-Key` (unique, stored with the order)
    and returns the first order on a retry.
  - D8 low coverage on `RabbitMqConnectionProvider` (0 %) and `DomainException` (33 %, unused constructors) — the
    provider now takes the client's `IConnectionFactory` and has tests for reuse and reconnection; the exception keeps
    the one constructor the code uses.
  - Recorded as sprint-3 commits `refactor(domain)`, `feat(api): Idempotency-Key …`, `test(messaging)` before
    `release 0.3.0` (no pushed history was touched).
- **Noise, code kept:** D5 Contracts off the main sequence and Domain "zone of pain" (opinion-not-fact; the row itself
  says the shape is by design); D18 thin Contracts project (opinion-not-fact → trap TRP-009); CKV_K8S_35 secrets as
  environment variables (opinion-not-fact; reported at the Deployment's first line); KSV-0125 untrusted registry for
  the organisation's own ghcr.io (opinion-not-fact); DS-0026 no HEALTHCHECK on a Kubernetes-only image
  (shape-irrelevant → trap TRP-010); D39 IL size of the EF fluent mapping (opinion-not-fact → trap TRP-011); D41 no
  AppArmor and D42 no runtime detection (opinion, banded); C1 "no encryption API" (opinion; the row says to ignore
  delegated encryption); D17 CA2007 at the root (false positive: TRP-007 caught); ED3 × 13 "event not named in the
  past tense" on HTTP request/response records, an error collector and the contract's nested records (false
  positive: none of them is an event; TRP-002 caught, the rest land on clean files).
- Key changes: traps TRP-009..TRP-011 (above). Bands out: C4 retention 100 vs [0, 60] (the outbox purge is credited
  as retention for all personal data although orders themselves are kept indefinitely; band kept), D42 80 vs [0, 40].

## 2026-10-07 — scan iteration 2 (contained, local) and history packaging

- Contained pass at `ef5a40b`: 26 results. The D4, ED5 and D8 rows of iteration 1 are gone after the fixes. Recall
  0/2 (ORD-001 false negative; ORD-002 only as the location-less roll-up, now "11/13": the new
  `EndpointResults.ForOperatorAsync` takes no token because it does no I/O of its own — the delegate it runs carries
  the request's token — so that part of the roll-up is noise). Trap resistance 7/11: TRP-002 (ED3), TRP-007 (D17),
  TRP-009 (D18), TRP-010 (DS-0026), TRP-011 (D39) caught, as judged in iteration 1.
- New row, judged: DM2 "Primitive id on a domain type: Order.RequestKey" — **false-positive**: the idempotency key is
  the caller's opaque string, not an identifier of the order or of anything in the domain. Code kept; promoted to
  trap TRP-012.
- Every release tag was checked out in a throw-away worktree and built in locked-restore mode with its tests run:
  `v0.1.0` 30 + 12, `v0.2.0` 41 + 14, `v0.3.0` 56 + 17 tests, all green.
- `benchmark/history/`: 26 patches, `build-history.sh` (rebuilds the scripted part on top of the key-first commit and
  verifies the final commit id and the three tag targets; verified by running it into a fresh directory) and a
  README listing every commit by sprint with the story notes.
