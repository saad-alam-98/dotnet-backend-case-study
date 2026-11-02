# Secret configuration and publication audit

The client project has been discontinued. This source is being prepared for portfolio
and educational use; restoring its previous infrastructure is not required. Prefer a
fresh-history public repository and keep this original repository private. A discontinued
application does not establish that its third-party credentials have been revoked.

The working tree has been sanitized. The repository is NOT ready to become public:
its existing Git history contains credentials. Never publish the ignored local files.

## Local setup

Existing values have been preserved on this machine under `SaudiPlus/.secrets/`:
`shared.json`, `Development.json`, `Staging.json`, `Production.json`, and `firebase.json`.
These files are ignored, excluded from build/publish output, and excluded from Docker contexts.
They are plaintext: restrict access to the OS account running the application.

For a fresh clone, create `SaudiPlus/.secrets/`, copy `SaudiPlus/secrets.example.json`
to `.secrets/shared.json`, and replace placeholders with your own credentials.
Put environment-specific connection strings and AWS/bucket settings in
`.secrets/Development.json`, `.secrets/Staging.json`, or `.secrets/Production.json`.
Never add `OtpOverride` outside Development. Set up a Firebase service account file
at `.secrets/firebase.json` for Staging/Production, or set `Firebase__CredentialPath`
to an externally mounted credential file.

Run `dotnet run --project SaudiPlus -- --environment Development`.
Startup migrates the configured database and seeds configured administrator accounts:
check the target connection string before running. Empty Admins means no account is seeded.

Configuration precedence, highest first: command-line arguments, environment variables,
.NET development user secrets, environment-specific local secrets, shared local secrets,
and standard ASP.NET configuration. Nested environment keys use double underscores,
e.g. `Jwt__Secret`, `ConnectionStrings__DefaultConnection`, `Google__MapsApiKey`,
`SendGrid__ApiKey`, `PasswordReset__EncryptionKey`, `AWS__AccessKey`, `AWS__SecretKey`,
`Notifications__ApplicationArn`, `Notifications__SqsQueueUrl`, and
`AdminCredentials__Admins__0__UserPassword`.
User secrets can be set with `dotnet user-secrets set "Jwt:Secret" "YOUR_VALUE" --project SaudiPlus`.
The application does not load dotenv files; root `.env` is for Docker Compose only.

## Docker and deployment

Copy `.env.example` to `.env`, set a strong DB_PASSWORD and the desired ASPNETCORE_ENVIRONMENT.
Compose mounts `SaudiPlus/.secrets` read-only at `/app/.secrets`; the runtime app user
must be able to read these files. Configure the matching environment JSON connection
string with `Server=db` and the database password. DB_PASSWORD only configures SQL Server;
it does not rewrite the API connection string. Changing it does not reset passwords in
an existing SQL volume: rotate the database login explicitly.

The Dockerfile uses `SaudiPlus/` as its build context, so `SaudiPlus/.dockerignore`
is essential. Never copy credentials into an image or publish directory.
For a non-Compose deployment, supply environment variables from the host's secret manager,
or mount `.secrets` under the application's content root. Firebase must be mounted too.

Before the next CI/CD deployment, provision `.env`, `SaudiPlus/.secrets/` and the updated
Compose file in the server's Compose directory (currently `/home/ubuntu`). Set the correct
environment for that server; staging and production must have separate credentials and
runtime configuration. The workflow now preserves the server-managed `.env` instead of
replacing it with a one-variable file. Its image artifact contains no runtime secrets.
Review access to existing CI image artifacts, registry images and server image caches:
previously built images may contain the old settings/private key.

## Findings and required action before publication

Audit on 2026-11-02: scanned 373 tracked paths and 4,994 reachable Git objects across
427 commits. Historical blob signatures included 36 AWS-key matches, 9 SendGrid-key
matches, 5 Google-key matches, and 2 private-key matches (counts are blobs, not unique keys).
Sensitive historical paths include all four SaudiPlus appsettings files,
`SaudiPlus/Service/EmailService.cs`, `SaudiPlus/Service/MapService.cs`, and
`SaudiPlus/firebase/saudi-plus-service-key.json`. Passwords and JWT secrets also occur
in configuration and Compose, beyond those signature counts.

1. Revoke/rotate every historical AWS access key, SendGrid API key, Google Maps key,
   Firebase service-account private key, database password, JWT signing secret, password-reset AES key, and
   seeded administrator password. Reset existing administrator accounts in the database;
   changing seed configuration does not update existing users. Rotate keys used across
   development, staging and production. JWT rotation invalidates existing access tokens.
2. Remove sensitive material from every branch and tag before publishing existing history.
   Use a separately backed-up clone and a reviewed history rewrite that covers both
   secret files and credentials embedded in source/Compose. Removing only current files
   is insufficient. Coordinate rewritten refs with collaborators and inspect remote refs,
   pull requests, forks, releases, artifacts, packages and cached images for copies.
   Alternatively, publish a new repository containing only the sanitized source with a
   fresh history, excluding `.git`, `.secrets`, `.env`, build outputs and private assets.
3. Run a dedicated secret scanner over the final publication candidate and all refs.
   Review data, binaries, author metadata and assets for private/customer information.
   The signature scan performed here is not proof that every kind of secret is absent.
4. Verify deployment with newly provisioned credentials before making the repo public.

Additional security finding: fixed OTPs were configured for every environment. Staging
and Production now reject OtpOverride at startup, and OTP generation uses a cryptographic
random number generator. Review and invalidate any outstanding test OTPs in deployed data.

No history has been rewritten, credentials rotated, remote visibility changed, or
application started against a database by this change.

## Validation and limits

`dotnet build SaudiPlus/SaudiPlus.csproj` and `dotnet publish` succeeded.
Inspected evaluated MSBuild Content/None items and a fresh publish output: local secret
files and the setup example were absent. Git ignore checks and JSON parsing passed;
no original configured credential values were found in tracked/new non-ignored files.
`git diff --check` passed. Docker is unavailable on the audit machine, so container
startup and Compose execution were not tested. Database/cloud integration was not run.

NuGet reported a high-severity vulnerability in resolved AWSSDK.CloudFront 3.7.401
(GHSA-mvm6-f9r3-fgfx) and a moderate vulnerability in MailKit 4.10.0
(GHSA-9j88-vvj5-vhgr). CloudFront's requested 3.7.400.74 version was unavailable and
resolved to 3.7.401. Dependency upgrades and existing compiler warnings remain separate
follow-up work; no dependency versions were changed in this audit.

Portfolio follow-up: the password-reset AES key was also embedded as byte arrays in
TokenService. It is now read from PasswordReset:EncryptionKey (Base64 encoded AES key)
and preserved only in ignored local configuration. This extraction preserves the old
token format; it does not upgrade that legacy protocol to authenticated encryption.
The deployment workflow is manual-only; pushes to staging/production no longer deploy.
