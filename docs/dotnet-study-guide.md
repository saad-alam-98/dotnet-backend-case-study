# .NET study guide from this project

Use each topic in this order: explain the concept, locate it in the code, trace one
real request, explain a limitation, then attempt the exercise. The source is the
reference for what this project actually implements. Proposed improvements below
are learning tasks, not completed features.

## Concept map

| Concept | Where to start | What to look for |
| --- | --- | --- |
| Hosting and composition root | [Program.cs](../SaudiPlus/Program.cs) | Builder, service registration, Build, Run |
| Middleware pipeline | [Program.cs](../SaudiPlus/Program.cs) | Ordering and request/response flow |
| Dependency injection | [ApplicationConfiguration.cs](../SaudiPlus/Configurations/ApplicationConfiguration.cs) | Interfaces mapped to scoped implementations |
| Options pattern | [SendGridSetting.cs](../SaudiPlus/Helper/Email/SendGridSetting.cs), [EmailService.cs](../SaudiPlus/Service/EmailService.cs) | Bind a section and inject IOptions |
| Configuration providers | [Program.cs](../SaudiPlus/Program.cs) | JSON, ignored local files, user secrets, environment, CLI |
| Controllers and routing | [OfferController.cs](../SaudiPlus/Controllers/Offer/OfferController.cs) | ApiController, Route, HttpPost, action parameters |
| Model binding | [CreateOfferDTO.cs](../SaudiPlus/Request/Offer/CreateOfferDTO.cs) | FromForm, files, enums, nullable amounts |
| Validation | [CreateOfferDTO.cs](../SaudiPlus/Request/Offer/CreateOfferDTO.cs), [OfferService.cs](../SaudiPlus/Service/Offer/OfferService.cs) | Attribute constraints versus business rules |
| Generics and inheritance | [IEntity.cs](../SaudiPlus/Interfaces/Repository/IEntity.cs), [Repository.cs](../SaudiPlus/Repository/Repository.cs) | Entity<TId>, constrained generic repository |
| Interfaces and polymorphism | [CreateOfferDTO.cs](../SaudiPlus/Request/Offer/CreateOfferDTO.cs) | Multiple request types consumed as IOfferDTO |
| Extension methods | [ApplicationConfiguration.cs](../SaudiPlus/Configurations/ApplicationConfiguration.cs), [HttpContextExtension.cs](../SaudiPlus/Extensions/HttpContextExtension.cs) | this parameters and reusable setup/helpers |
| Async/await and Task | [Repository.cs](../SaudiPlus/Repository/Repository.cs) | Await database I/O rather than block |
| Task.WhenAll | [SqsNotificationProcessor.cs](../SaudiPlus/Service/SqsNotificationProcessor.cs) | Parallel I/O and shared-state risks |
| Cancellation | [SqsNotificationProcessor.cs](../SaudiPlus/Service/SqsNotificationProcessor.cs) | stoppingToken for polling and delays |
| EF Core context and DbSet | [SaudiPlusDbContext.cs](../SaudiPlus/Entities/SaudiPlusDbContext.cs) | Identity context plus application tables |
| Fluent model configuration | [SaudiPlusDbContext.cs](../SaudiPlus/Entities/SaudiPlusDbContext.cs) | Relationships, indexes, delete behavior |
| Inheritance mapping | [SaudiPlusDbContext.cs](../SaudiPlus/Entities/SaudiPlusDbContext.cs) | NotificationType discriminator |
| Migrations | [Migrations](../SaudiPlus/Migrations) | Schema changes and model snapshot |
| LINQ and deferred execution | [OfferRepository.cs](../SaudiPlus/Repository/OfferRepository.cs) | IQueryable composition before materialization |
| Eager loading | [OfferRepository.cs](../SaudiPlus/Repository/OfferRepository.cs) | Include and ThenInclude |
| Filtering and pagination | [CategoryService.cs](../SaudiPlus/Service/Category/CategoryService.cs) | Where, Skip, Take, ToListAsync |
| Change tracking and auditing | [SaudiPlusDbContext.cs](../SaudiPlus/Entities/SaudiPlusDbContext.cs) | EntityState and timestamp updates |
| Transactions and retries | [OfferService.cs](../SaudiPlus/Service/Offer/OfferService.cs) | Execution strategy wraps transaction delegate |
| Identity | [IdentityConfiguration.cs](../SaudiPlus/Configurations/IdentityConfiguration.cs) | UserManager, RoleManager, EF stores |
| JWT and claims | [TokenService.cs](../SaudiPlus/Service/TokenService.cs), [Program.cs](../SaudiPlus/Program.cs) | Token issuance versus validation |
| Authorization filters | [PermissionAuthorizeAttribute.cs](../SaudiPlus/Configurations/PermissionAuthorizeAttribute.cs) | 401, 403, any-permission matching |
| Refresh tokens and revocation | [TokenService.cs](../SaudiPlus/Service/TokenService.cs) | Current/previous tokens and security stamp |
| Cryptographic randomness | [TokenService.cs](../SaudiPlus/Service/TokenService.cs) | Refresh-token bytes and OTP generation |
| DTO mapping | [MappingProfile.cs](../SaudiPlus/AutoMapper/MappingProfile.cs) | ReverseMap, ignored IDs, null conditions |
| JSON serialization | [Program.cs](../SaudiPlus/Program.cs), [SqsNotificationProcessor.cs](../SaudiPlus/Service/SqsNotificationProcessor.cs) | Newtonsoft MVC versus System.Text.Json messages |
| Response envelopes | [BaseResponse.cs](../SaudiPlus/Response/BaseResponse.cs) | Generic payload and body statusCode |
| Localization | [LanguageConfiguration.cs](../SaudiPlus/Configurations/LanguageConfiguration.cs), [LanguageMiddleware.cs](../SaudiPlus/Configurations/LanguageMiddleware.cs) | Culture, headers, scoped language service |
| BackgroundService | [SqsNotificationProcessor.cs](../SaudiPlus/Service/SqsNotificationProcessor.cs), [TokenService.cs](../SaudiPlus/Service/TokenService.cs) | Long-running loops and fresh scopes |
| Messaging | [NotificationService.cs](../SaudiPlus/Service/NotificationService.cs) | Enqueue versus direct device publish |
| File uploads | [FileService.cs](../SaudiPlus/Service/FileService.cs) | IFormFile, limits, S3 transfer, CloudFront invalidation |
| External HTTP | [MapService.cs](../SaudiPlus/Service/MapService.cs) | Await requests, inspect JSON, configure API key |
| Email templates | [EmailService.cs](../SaudiPlus/Service/EmailService.cs) | Options, SendGrid, template placeholders |
| QR generation and disposal | [BranchService.cs](../SaudiPlus/Service/Branch/BranchService.cs) | using, PNG bytes, upload adapter |
| Logging and error handling | [OfferController.cs](../SaudiPlus/Controllers/Offer/OfferController.cs) | ILogger and repeated catch blocks |
| Swagger/OpenAPI | [SwaggerConfiguration.cs](../SaudiPlus/Configurations/SwaggerConfiguration.cs) | Document filtering, bearer scheme, language header |
| CORS and rate limiting | [Program.cs](../SaudiPlus/Program.cs), [PolicyController.cs](../SaudiPlus/Controllers/Policy/PolicyController.cs) | Open CORS and a selected named limiter |
| Containers and CI/CD | [Dockerfile](../SaudiPlus/Dockerfile), [workflow](../.github/workflows/docker-ci-cd.yml) | SDK/runtime stages and archived delivery example |

## 1. Dependency injection and service lifetimes

Dependency injection supplies collaborators through constructors. For example,
OfferController consumes IOfferService, while ApplicationConfiguration maps that
interface to OfferService with AddScoped. This separates the consumer's contract
from the choice of implementation and makes substitution possible in tests.

Scoped services normally live for one HTTP request; singleton services live for the
host lifetime; transient services are created when resolved. The DbContext is scoped
by AddDbContext. A hosted worker is long-lived and uses IServiceScopeFactory to create
a scope for scoped database work. Keeping a scoped context in a long-lived worker
would retain stale tracking state and violate the intended lifetime boundary.

**Exercise:** draw the constructor dependency graph for OfferController. Explain why
changing its service and context to singletons would be unsafe. Notice the duplicate
notification-center registration and consider how you would simplify registrations.

## 2. HTTP, binding, validation, and DTOs

The routing attributes select an action; binding converts query, form, or JSON data
into parameters. Offer creation uses multipart form data because the request contains
IFormFile objects. DTOs keep the public request shape separate from persistent entities.
Data annotations constrain fields; OfferService checks cross-field/domain rules such
as which amounts are valid for a particular offer type.

C# required members express an initialization contract; they do not replace all runtime
validation. For value types, default values and allowed ranges still need consideration.
AutoMapper copies selected properties and includes conditions that skip null source
values during some updates. Skipping null means the client may be unable to explicitly
clear a field; document that distinction when designing update contracts.

**Exercise:** design validation cases for an offer with reversed dates, negative points,
no logos, six logos, and an incompatible percentage/invoice value. Identify which checks
already exist before implementing anything. Explain JSON versus multipart requests.

## 3. Async programming, concurrency, and cancellation

Awaiting ToListAsync or a cloud request avoids blocking the calling thread while waiting
for I/O. It does not make the query cheaper or guarantee parallel execution. Task.WhenAll
waits for multiple tasks; in the notification worker it allows overlapping operations.

Concurrency requires independent or safely synchronized state. EnsureDeviceArn may save
through a repository using the same scoped context as other parallel tasks. A DbContext
is not a suitable shared concurrent work unit. Prefer sequential database access or
separate scopes per work item, and bound external request concurrency.

CancellationToken allows cooperative cancellation. The worker passes stoppingToken to
polling/delay calls, but not every downstream operation accepts or receives it.

**Exercise:** describe a bounded notification batch that propagates cancellation and
never runs simultaneous EF operations on one context. Explain why Task.Run around
an EF query is not the solution to this lifetime/concurrency problem.

## 4. EF Core, relationships, and migrations

DbContext coordinates querying, change tracking, and persistence. DbSet exposes an
entity set. OnModelCreating defines unique indexes, foreign keys and deletion rules.
A cascade can delete related rows; restrict prevents deletion while dependents exist.
The notification discriminator demonstrates inheritance persisted in a shared table.

Migrations describe schema evolution. Program.cs calls Database.Migrate at startup.
That is convenient for a small application, but running multiple replicas or applying
risky schema changes calls for controlled migration deployment. The existing audit
hook updates CreationTime/LastModificationTime through tracked entity states.

**Exercise:** trace BusinessUser -> Offer -> OfferLogo and predict what deleting a
business does from the model configuration. Explain which operations create SQL and
which only change tracked in-memory state. Test auditing overload coverage before
claiming every possible SaveChanges path is covered.

## 5. LINQ, query execution, and performance

OfferRepository.GetOffersForFilter returns IQueryable. Callers can extend its expression
before ToListAsync executes a database query. After materialization, further operations
run against in-memory objects. Returning IQueryable provides flexibility but exposes
persistence behavior across layer boundaries.

Include/ThenInclude load related entities. Loading a collection and multiple related
paths can increase rows and payload size; compare projection to response DTOs or split
queries using measured SQL. Several feature services paginate before materialization;
wallet logic also demonstrates in-memory pagination after loading records. Do not claim
all pagination here executes in SQL.

**Exercise:** inspect one actual filter chain. Add deterministic ordering before Skip/Take,
validate page limits, project only required fields, and compare generated SQL. For a
read-only path, investigate AsNoTracking instead of assuming all queries already use it.

## 6. Transactions, execution strategies, and side effects

A transaction groups database changes into a commit or rollback. The offer and branch
workflows run a transaction inside the configured EF execution strategy so a transient
SQL failure can retry the database unit of work. Repository autoSave flags control when
SaveChanges is called; they do not create a distributed transaction.

S3 uploads and notifications are external side effects. A database rollback cannot undo
a completed upload. Retrying the delegate can repeat the upload, so a robust workflow
needs idempotency, compensation, or a different boundary. An outbox records intended
messages with database changes and dispatches them later; this project does not already
implement an outbox.

**Exercise:** write a failure timeline where the first logo upload succeeds, the second
fails, and SQL rolls back. Propose where cleanup metadata or an outbox record would live.

## 7. Identity, JWT authentication, and authorization

Identity manages accounts, password hashes, roles, and token-provider features. A JWT
lets the API validate signed claims using a shared HMAC secret in this implementation.
A signed JWT is not encrypted: do not put secrets into its payload. Validation checks
issuer, audience, signature and expiry. The security-stamp callback also queries the
user, so this authentication path is not completely independent of database state.

Authentication establishes identity; authorization decides access. AuthPermissionAttribute
uses permission claims and any-match semantics. A permission such as Offer.Edit is not
proof that a particular offer belongs to the caller. Also check ownership/tenant scope.

**Exercise:** trace a token from GenerateAccessToken to an authorized action. Explain why
an expired token should fail before business logic, and how a permission change should
invalidate or replace an already-issued claims snapshot.

## 8. Refresh tokens, password reset, and secret handling

Refresh tokens are generated from cryptographic random bytes and stored through more
than one path. The refresh method accepts current and previous values; this needs a
clear policy for expiry, replay detection, concurrency, and revocation. Access-token
expiry is hardcoded to twelve hours in issuance even though configuration includes an
expiration setting. Declaring a setting does not prove that the application uses it.

The legacy reset flow encrypts an email using AES and also uses application database
records in account workflows. Its key is now external configuration, but AES-CBC without
an authentication mechanism is not an ideal new token protocol. Prefer Identity's reset
token mechanism or a reviewed purpose-bound, expiring token design. Merely hiding a key
or Base64-encoding bytes does not make the whole workflow secure.

**Exercise:** list every token read/write/revoke operation and check whether revoking
one storage path affects all validation paths. Use generated test-only secrets. Never
teach with the original client credentials, even though the project is discontinued.

## 9. Background workers and message reliability

The SQS worker uses long polling and creates a scope for database work. The token-cleanup
worker is also a BackgroundService. These run in the same application host as the API;
being asynchronous does not make them independent microservices.

The SQS worker deletes messages before completing processing and on an inner exception.
That trades away retry opportunities and can lose work. A revised worker should consider
acknowledgement timing, visibility timeout, retries, dead-letter handling, duplicate
messages and idempotent business operations. These are proposed changes, not existing
guarantees. Do not describe the current implementation as exactly-once delivery.

**Exercise:** simulate deserialization failure, partial fan-out failure, cancellation, and
redelivery. Define what happens to a message and which users may receive a duplicate.

## 10. Options, configuration, and external integrations

EmailService consumes IOptions<SendGridSetting>; other components read IConfiguration.
Configuration providers merge keys; later providers override earlier providers. The
sanitized setup loads ignored local secret JSON, then development user secrets, then
environment variables and command-line arguments. Environment nesting uses double
underscores. Root .env is read by Compose, not by ASP.NET Core automatically.

FileService wraps S3 and CloudFront behavior; MapService calls Google over HTTP. Program.cs
registers HttpClient support, but MapService still creates HttpClient directly. A useful
refactor is an injected typed client with timeouts, cancellation and response handling.
AWS services also construct some SDK clients explicitly with configured keys; do not
claim the whole application already uses workload identity everywhere.

**Exercise:** replace one external adapter with a fake for a unit test. Identify where
IOptions validation could reject missing configuration before a request arrives.

## 11. Localization, serialization, and API documentation

The app selects ar/en culture from a header, uses localized Identity resources, and
chooses localized fields through the language service. There are two related paths:
ASP.NET culture and application language state. Check they agree on defaults and accepted
header values. Full Accept-Language lists are not handled by the custom exact-match parser.

MVC uses Newtonsoft.Json with enum and reference-loop settings. The queue path uses
System.Text.Json with JsonPropertyName attributes. Attributes from one serializer are
not automatically configuration for the other. Swagger separates audience documents
using tags; document grouping does not impose authorization.

**Exercise:** trace the same entity through its localized response and JSON output.
Round-trip a notification message with the serializer used by its real producer/consumer.

## 12. Errors, HTTP status codes, logging, and resource lifetime

BaseResponse contains statusCode, message and a generic payload. Returning that object
from ActionResult<T> does not automatically translate its statusCode into an HTTP status.
Some actions catch exceptions and return ex.Message, which can leak internal details.
A proposed improvement is centralized exception handling with ProblemDetails and explicit
status results, plus structured ILogger calls that preserve exception information.

using/await using express deterministic disposal. Database transactions and QR-generator
objects illustrate this. Disposal is separate from garbage collection and matters for
unmanaged resources and connection ownership.

**Exercise:** send a failing request and inspect both its actual HTTP status and JSON
body. Design tests that distinguish 400, 401, 403, 404 and 500 responses. Rewrite one
catch path to log context without returning internal exception text.

## 13. CORS, rate limiting, containers, and CI/CD

The current CORS policy permits all origins, methods and headers. CORS controls browser
cross-origin behavior; it does not authenticate clients or stop arbitrary API callers.
A named fixed-window rate policy is registered and selected on a Policy action; it is
not a global limiter applied to every endpoint. Keying, proxy headers, and distributed
limits need deliberate design for a production deployment.

The Dockerfile separates build and runtime images. Secret mounts and environment values
arrive at runtime; ignoring a file in Git alone does not stop Docker copying it into an
image. The project-specific .dockerignore matters because Compose builds SaudiPlus as
its context. The historical deployment workflow is now manual-only for archival use.

**Exercise:** explain build-time versus runtime configuration. Inspect a fresh publish
output to verify that .secrets is absent. Discuss a test/build-only CI workflow for a
portfolio repository instead of restoring old client infrastructure.

## Suggested study sequence

| Sessions | Focus | Deliverable |
| --- | --- | --- |
| 1-2 | C# generics, interfaces, DI, controller binding | Explain an endpoint and its constructor graph |
| 3-4 | EF model, LINQ, SQL, change tracking | Trace a query and a SaveChanges call |
| 5-6 | Transactions, async, cancellation | Draw a failure timeline and propose one safe fix |
| 7-8 | Identity, JWT, permissions, refresh tokens | Explain authentication, authorization and revocation |
| 9-10 | Workers, messaging, integrations | Explain acknowledgement and concurrency risks |
| 11-12 | Localization, errors, secrets, containers | Teach one complete flow with limitations |

For teaching, use a separate branch per exercise and show a before/after diff. Explain
what was in the original implementation, what you changed during study, and the evidence
that the change works. There is no automated test project here yet; adding focused tests
for permissions, token expiry, response status and transaction failures is useful practice.
