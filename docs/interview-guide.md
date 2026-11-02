# Interview and resume preparation

Use these prompts to explain the implementation in your own words. Attribute only the
work you actually did. The repository shows available implementation evidence; it does
not establish individual ownership, team size, traffic, revenue, or production outcomes.

## Explain the project in one minute

Adapt this description to your actual contribution:

> Saudi Plus was a client backend project for a platform connecting businesses and
> individual users through offers and redemption workflows. The backend uses ASP.NET
> Core controllers, EF Core with SQL Server, and ASP.NET Core Identity with JWT
> authentication. It includes permission-based access, Arabic/English responses,
> file storage and notification integrations. The client discontinued the project,
> so I use the sanitized source as an archived case study. My contribution was
> [state the features and decisions you personally worked on].

Follow that introduction with one feature you can trace end to end. Discuss the request,
validation, authorization, data model, failure cases and what you would improve. Avoid
memorizing technology names without explaining a concrete use in the repository.

## Questions grounded in this code

| Interview question | Answer direction | Code evidence |
| --- | --- | --- |
| What architecture did you use? | A layered monolith: controllers, services and repositories in one application project. Services also access DbContext directly. | [ApplicationConfiguration](../SaudiPlus/Configurations/ApplicationConfiguration.cs), [OfferService](../SaudiPlus/Service/Offer/OfferService.cs) |
| Why inject interfaces? | Consumers depend on contracts; DI chooses implementations. This enables substituting collaborators, though tests still need to be written. | [OfferController](../SaudiPlus/Controllers/Offer/OfferController.cs) |
| What does scoped mean here? | A request shares scoped services/context. Hosted workers create scopes for database operations. | [Program](../SaudiPlus/Program.cs), [SQS worker](../SaudiPlus/Service/SqsNotificationProcessor.cs) |
| Why use a repository over EF? | It centralizes some queries and CRUD operations, but adds an abstraction over EF and still exposes IQueryable/direct context use. Discuss the tradeoff. | [Repository](../SaudiPlus/Repository/Repository.cs), [OfferRepository](../SaudiPlus/Repository/OfferRepository.cs) |
| IQueryable versus IEnumerable? | Query expressions can be composed before EF materializes them; after materialization operations run in memory. | [OfferRepository](../SaudiPlus/Repository/OfferRepository.cs) |
| How did related data load? | Include and ThenInclude load business, branches and logos; query size and projections need review. | [OfferRepository](../SaudiPlus/Repository/OfferRepository.cs) |
| How did you keep writes consistent? | Explicit SQL transactions inside an execution-strategy delegate. External file uploads are outside SQL atomicity. | [OfferService](../SaudiPlus/Service/Offer/OfferService.cs) |
| What happens if an upload succeeds and SQL fails? | The uploaded object can remain. Propose idempotent keys and cleanup/compensation; do not claim SQL rolls back S3. | [OfferService](../SaudiPlus/Service/Offer/OfferService.cs), [FileService](../SaudiPlus/Service/FileService.cs) |
| Authentication versus authorization? | JWT validation identifies the caller; permission claims govern actions; ownership needs a separate check. | [Program](../SaudiPlus/Program.cs), [permission filter](../SaudiPlus/Configurations/PermissionAuthorizeAttribute.cs) |
| Why use Identity if you already have JWT? | Identity manages accounts/passwords/roles; JWT is the API bearer-token representation. They solve different parts of the flow. | [IdentityConfiguration](../SaudiPlus/Configurations/IdentityConfiguration.cs), [TokenService](../SaudiPlus/Service/TokenService.cs) |
| How do you invalidate a JWT? | This API compares security stamps with the database; key rotation invalidates tokens signed with the previous key. Permission claims still need an invalidation policy. | [Program](../SaudiPlus/Program.cs) |
| How do refresh tokens work here? | Random values, current/previous-token acceptance, and database storage. Discuss expiry, replay, concurrent refresh and storage consistency as review points. | [TokenService](../SaudiPlus/Service/TokenService.cs), [User](../SaudiPlus/Entities/Identity/User.cs) |
| Why async/await? | It allows the thread to do other work during I/O; it does not make SQL faster or make a context thread-safe. | [Repository](../SaudiPlus/Repository/Repository.cs) |
| What does Task.WhenAll change? | It overlaps asynchronous operations. Shared scoped DbContext operations in those tasks can be unsafe. | [SQS worker](../SaudiPlus/Service/SqsNotificationProcessor.cs) |
| How reliable is the queue consumer? | It currently acknowledges before processing finishes and can lose work. Propose acknowledgement-after-success, retries, dead letters and idempotency. | [SQS worker](../SaudiPlus/Service/SqsNotificationProcessor.cs) |
| How did localization work? | ar/en selection from Accept-Language plus localized resources and response fields; exact header matching has limits. | [LanguageConfiguration](../SaudiPlus/Configurations/LanguageConfiguration.cs) |
| What is the options pattern? | Bind a config section to typed settings and inject IOptions; EmailService uses this for SendGrid. | [EmailService](../SaudiPlus/Service/EmailService.cs) |
| Where are secrets stored? | Ignored local JSON for study, user secrets or runtime environment overrides; they are excluded from Git/images/publish output. | [Program](../SaudiPlus/Program.cs), [security guide](../SECURITY.md) |
| Does a JSON statusCode set HTTP status? | No. Returning a DTO needs an explicit result/status to align the HTTP response with the body. | [BaseResponse](../SaudiPlus/Response/BaseResponse.cs), [OfferController](../SaudiPlus/Controllers/Offer/OfferController.cs) |
| How did you test it? | State exactly what you personally tested. This checkout has no automated test project; build/publish verification is not integration-test coverage. | [README status](../README.md) |

## Tell one feature story

Use offer creation as a starting point, or choose a feature you actually implemented:

1. **Context:** which user needed the feature, and what result they expected.
2. **Request:** endpoint, DTO, form/JSON binding and validation.
3. **Access:** permission and business/resource ownership.
4. **Implementation:** controller -> service -> repository/context -> integration.
5. **Failure:** invalid input, missing business, upload failure or SQL retry.
6. **Tradeoff:** transaction duration and cleanup of external side effects.
7. **Evidence:** the relevant methods and a test or reproducible request you ran.
8. **Reflection:** what you would change now and how you would validate it.

Do not turn a proposal into a past achievement. For example, say "I would add an outbox"
unless you actually implemented and tested one.

## Resume wording

Use only bullets that match your actual responsibilities. These are templates, not
verified personal achievements:

- Developed ASP.NET Core API features for [your modules], using EF Core and SQL Server
  for persistence and DTO-based request/response contracts.
- Implemented [your authentication/authorization work] with ASP.NET Core Identity,
  JWT bearer tokens and permission claims.
- Integrated [services you personally integrated] for file storage, email or queued
  notification workflows.
- Built [your Arabic/English features] using request culture and localized responses.
- Prepared a discontinued client backend as a sanitized portfolio case study and
  documented implementation tradeoffs and .NET learning exercises.

Do not add invented metrics such as user counts, uptime, latency improvements or cost
savings. If the project was never launched, describe it as developed/discontinued,
not as a live production platform. A clear account of your work and limitations is
stronger evidence than unsupported claims.

## Teaching plan

| Lesson | Demonstration | Practice task |
| --- | --- | --- |
| 1: Request flow | Trace offer creation | Draw dependencies and identify validation boundaries |
| 2: Data access | Compose an offers query | Compare SQL pagination and in-memory pagination |
| 3: Consistency | Trace SQL transaction plus file upload | Model partial failure and compensating cleanup |
| 4: Security | Issue token and inspect permission filter | Explain authentication, ownership and revocation |
| 5: Async workers | Trace SQS receive and publish | Fix acknowledgement timing with a fake queue |
| 6: Configuration | Explain providers and ignored secrets | Run with disposable settings and no original credentials |
| 7: API quality | Inspect actual HTTP error status | Add explicit status results and focused tests |

For each lesson, show the original code, one small improvement, and a meaningful test.
Keep exercises isolated from the archived baseline so readers can distinguish the
client implementation from later portfolio learning work.
