-------------------------------------------------------------------
-- Chess Tournaments Identity Data Migration
-- This script initializes OpenIddict data for local development
-------------------------------------------------------------------
--
-- This migration creates:
-- 1. Custom API scope (crud_api) for accessing the Chess Tournaments API
-- 2. API Resource client (confidential) for server-to-server communication
-- 3. Blazor UI client (public) for the Blazor WebAssembly application
-- 4. Angular SPA client (public) for the Angular Single Page Application
--
-- Each client is configured with appropriate:
-- - OAuth 2.0 flows (Authorization Code with PKCE)
-- - Scopes and permissions
-- - Redirect URIs for authentication callbacks
-- - Token lifetimes
-------------------------------------------------------------------

-------------------------------------------------------------------
-- 1. CREATE CUSTOM SCOPES
-------------------------------------------------------------------

-- Create the crud_api scope for API access
-- This scope will be requested by client applications to access the Chess Tournaments API
INSERT [IDENTITY].[OpenIddictScopes]
(
    [Id],
    [ConcurrencyToken],
    [Description],
    [Descriptions],
    [DisplayName],
    [DisplayNames],
    [Name],
    [Properties],
    [Resources]
)
VALUES
(
    N'5f369d6a-e2fa-4216-a5ca-761f864d6fd0',  -- Unique ID
    N'043b94c5-9edf-42f9-a301-0f838f67eb84',  -- Concurrency token
    NULL,                                       -- Description
    NULL,                                       -- Descriptions (localized)
    N'API access',                              -- Display name
    NULL,                                       -- Display names (localized)
    N'crud_api',                                -- Scope name
    NULL,                                       -- Additional properties
    N'["chess-tournaments_api"]'                -- Associated resources
);

-------------------------------------------------------------------
-- 2. CREATE API RESOURCE CLIENT (Server-to-Server)
-------------------------------------------------------------------

-- Create the confidential API client for introspection
-- This client is used for token introspection (validating tokens issued to other clients)
INSERT [IDENTITY].[OpenIddictApplications]
(
    [Id],
    [ApplicationType],
    [ClientId],
    [ClientSecret],
    [ClientType],
    [ConcurrencyToken],
    [ConsentType],
    [DisplayName],
    [DisplayNames],
    [JsonWebKeySet],
    [Permissions],
    [PostLogoutRedirectUris],
    [Properties],
    [RedirectUris],
    [Requirements],
    [Settings]
)
VALUES
(
    N'39467485-f052-4c94-8850-438ef529b4b6',                                                            -- Unique ID
    NULL,                                                                                                -- Application type
    N'chess-tournaments_api',                                                                           -- Client ID
    N'AQAAAAEAACcQAAAAELq6cQHcF8zgSoPIiGevuV7qgZcpTuOOk3QY+s6QwFeuXoUyfWFyRulYUr2qOnNp5A==',      -- Hashed client secret
    N'confidential',                                                                                    -- Client type (requires secret)
    N'a0d5fbab-4dc7-4791-9993-ad118011b62f',                                                            -- Concurrency token
    NULL,                                                                                                -- Consent type
    NULL,                                                                                                -- Display name
    NULL,                                                                                                -- Display names (localized)
    NULL,                                                                                                -- JSON Web Key Set
    N'["ept:introspection"]',                                                                           -- Permissions: token introspection
    NULL,                                                                                                -- Post-logout redirect URIs
    NULL,                                                                                                -- Additional properties
    NULL,                                                                                                -- Redirect URIs
    NULL,                                                                                                -- Requirements
    NULL                                                                                                 -- Settings
);

-------------------------------------------------------------------
-- 3. CREATE BLAZOR UI CLIENT (Public SPA)
-------------------------------------------------------------------

-- Create the Blazor WebAssembly client application
-- Uses Authorization Code Flow with PKCE for secure authentication
INSERT [IDENTITY].[OpenIddictApplications]
(
    [Id],
    [ApplicationType],
    [ClientId],
    [ClientSecret],
    [ClientType],
    [ConcurrencyToken],
    [ConsentType],
    [DisplayName],
    [DisplayNames],
    [JsonWebKeySet],
    [Permissions],
    [PostLogoutRedirectUris],
    [Properties],
    [RedirectUris],
    [Requirements],
    [Settings]
)
VALUES
(
    N'daa2026f-9ed6-453e-8085-848c2a255758',                                                            -- Unique ID
    NULL,                                                                                                -- Application type
    N'chess-tournaments_ui',                                                                            -- Client ID
    NULL,                                                                                                -- Client secret (public clients don't have secrets)
    N'public',                                                                                          -- Client type (no secret required)
    N'c2451d22-981b-4513-93e1-e023c97b9a9d',                                                            -- Concurrency token
    N'explicit',                                                                                        -- Consent type (user must explicitly consent)
    N'Blazor client application',                                                                       -- Display name
    NULL,                                                                                                -- Display names (localized)
    NULL,                                                                                                -- JSON Web Key Set
    N'["ept:authorization","ept:end_session","ept:token","gt:authorization_code","gt:refresh_token","rst:code","scp:email","scp:profile","scp:roles","scp:crud_api"]',  -- Permissions
    N'["https://localhost:7018/authentication/logout-callback"]',                                       -- Post-logout redirect URI
    NULL,                                                                                                -- Additional properties
    N'["https://localhost:7018/authentication/login-callback"]',                                        -- Redirect URI for login callback
    N'["ft:pkce"]',                                                                                     -- Requirements: PKCE for security
    N'{"tkn_lft:idt":"0.20:00:00","tkn_lft:act":"0.01:00:00","tkn_lft:code":"0.20:00:00","tkn_lft:reft":"30.00:00:00"}'  -- Token lifetimes
);

-------------------------------------------------------------------
-- 4. CREATE ANGULAR SPA CLIENT (Public SPA)
-------------------------------------------------------------------

-- Create the Angular Single Page Application client
-- Uses Authorization Code Flow with PKCE for secure authentication
-- Includes offline_access scope for refresh token support
INSERT [IDENTITY].[OpenIddictApplications]
(
    [Id],
    [ApplicationType],
    [ClientId],
    [ClientSecret],
    [ClientType],
    [ConcurrencyToken],
    [ConsentType],
    [DisplayName],
    [DisplayNames],
    [JsonWebKeySet],
    [Permissions],
    [PostLogoutRedirectUris],
    [Properties],
    [RedirectUris],
    [Requirements],
    [Settings]
)
VALUES
(
    N'caa2026f-9ed6-453e-8085-848c2a255758',                                                            -- Unique ID
    NULL,                                                                                                -- Application type
    N'chess-tournaments-spa',                                                                           -- Client ID
    NULL,                                                                                                -- Client secret (public clients don't have secrets)
    N'public',                                                                                          -- Client type (no secret required)
    N'c451d22-981b-4513-93e1-e023c97b9a9d',                                                             -- Concurrency token
    N'explicit',                                                                                        -- Consent type (user must explicitly consent)
    N'Angular client application',                                                                      -- Display name
    NULL,                                                                                                -- Display names (localized)
    NULL,                                                                                                -- JSON Web Key Set
    N'["ept:authorization","ept:end_session","ept:token","gt:authorization_code","gt:refresh_token","rst:code","scp:email","scp:profile","scp:roles","scp:crud_api","scp:offline_access"]',  -- Permissions (includes offline_access)
    N'["http://localhost:4200"]',                                                                       -- Post-logout redirect URI (Angular app home)
    NULL,                                                                                                -- Additional properties
    N'["http://localhost:4200/callback","http://localhost:4200/silent-refresh.html"]',                  -- Redirect URIs (login callback + silent refresh)
    N'["ft:pkce"]',                                                                                     -- Requirements: PKCE for security
    N'{"tkn_lft:idt":"0.20:00:00","tkn_lft:act":"0.01:00:00","tkn_lft:code":"0.20:00:00","tkn_lft:reft":"30.00:00:00"}'  -- Token lifetimes
);

-------------------------------------------------------------------
-- VERIFICATION QUERIES
-------------------------------------------------------------------

-- Verify scopes were created
SELECT [Id], [Name], [DisplayName], [Resources]
FROM [IDENTITY].[OpenIddictScopes]
WHERE [Name] = N'crud_api';

-- Verify all clients were created
SELECT
    [ClientId],
    [DisplayName],
    [ClientType],
    [RedirectUris],
    [PostLogoutRedirectUris],
    [Permissions]
FROM [IDENTITY].[OpenIddictApplications]
WHERE [ClientId] IN (
    N'chess-tournaments_api',
    N'chess-tournaments_ui',
    N'chess-tournaments-spa'
)
ORDER BY [ClientId];

-------------------------------------------------------------------
-- EXPECTED RESULTS
-------------------------------------------------------------------
--
-- Scope: crud_api
-- - Display Name: "API access"
-- - Resources: ["chess-tournaments_api"]
--
-- Client: chess-tournaments_api
-- - Type: confidential
-- - Permissions: introspection only
--
-- Client: chess-tournaments_ui
-- - Type: public
-- - Redirect URI: https://localhost:7018/authentication/login-callback
-- - Logout URI: https://localhost:7018/authentication/logout-callback
-- - Scopes: openid, profile, email, roles, crud_api
--
-- Client: chess-tournaments-spa
-- - Type: public
-- - Redirect URIs: http://localhost:4200/callback, http://localhost:4200/silent-refresh.html
-- - Logout URI: http://localhost:4200
-- - Scopes: openid, profile, email, roles, crud_api, offline_access
--
-- Token Lifetimes (all clients):
-- - Identity Token: 20 hours
-- - Access Token: 1 hour
-- - Authorization Code: 20 hours
-- - Refresh Token: 30 days (sliding expiration)
-------------------------------------------------------------------
