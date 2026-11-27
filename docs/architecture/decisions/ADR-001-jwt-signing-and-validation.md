# ADR-001: JWT signing and validation across services

## Status

Accepted. Records the approach implemented in pull request #9 and merged into `development`. Revisit when a component outside the trust boundary needs to validate tokens (see Consequences).

## Context

Identity issues access tokens; Identity, Medical Appointments, Notifications and the gateway must validate them. Before this decision each service configured validation differently: Appointments and the gateway used `Authority` (Identity publishes no OIDC metadata, so validation could not work), Notifications read a different key name and fell back to an empty key, and no service restricted the accepted algorithms.

All services are developed and deployed together by one team, in one repository, and share one trust boundary.

## Decision

- Tokens are JWT Bearer tokens signed with **HS256** using one symmetric key shared by all services.
- All services validate tokens through one helper, `AddCityServicesJwtAuthentication` in `src/BuildingBlocks/Security`, configured by `JwtSettings:SecretKey`, `JwtSettings:Issuer` and `JwtSettings:Audience`.
- Validation checks signature, issuer, audience and lifetime (no clock skew) and accepts **only HS256**.
- The key must be at least 32 bytes and comes from the environment (`JWT_SECRET_KEY` in `.env`, never in the repository). A service refuses to start if it is missing or short.
- User identity for authorization comes only from the validated token (`ICurrentUserService`).

## Alternatives Considered

- **RS256/ES256 with a published public key (JWKS).** Only Identity would hold the private key and other services would need just the public key. Not chosen now: it needs key-publication and rotation work, and today every service is inside the same trust boundary. It is the better option if any service is operated by another team or any validator is exposed beyond that boundary.
- **Authority/OIDC discovery against Identity.** Requires Identity to expose OIDC metadata. Same effort as the asymmetric option, with an extra runtime dependency from every service to Identity.
- **Keep per-service configuration.** Rejected: it produced the inconsistencies described in Context.

## Consequences

- Every service that validates tokens can also forge them, because they all hold the signing key. A compromise of any one service compromises authentication everywhere.
- Rotating the key invalidates all issued tokens and requires restarting all services together.
- Refresh tokens, logout and token revocation are not designed yet; access tokens are valid until they expire (`JwtSettings:ExpirationMinutes`, 60 by default).
- Migrating to an asymmetric algorithm later is contained: it changes `src/BuildingBlocks/Security` and the `TokenService` in Identity, not the individual services.

## References

- `src/BuildingBlocks/Security/JwtAuthenticationExtensions.cs`
- `src/Services/Identity/Identity.Infrastructure/Services/TokenService.cs`
- Security guidelines, section 6.2 (session and token management)
