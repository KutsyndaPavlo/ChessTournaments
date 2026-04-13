# Chess Tournaments - Authentication Setup

This Angular application implements **Authorization Code Flow with PKCE** for secure authentication with the Chess Tournaments Identity Provider.

## Identity Provider Configuration

The application is configured to work with the Chess Tournaments Identity Server at:

- **Issuer**: `https://localhost:7225/`
- **Client ID**: `chess-tournaments-spa`

### OIDC Endpoints

The identity provider supports the following endpoints:

- Authorization: `https://localhost:7225/connect/authorize`
- Token: `https://localhost:7225/connect/token`
- UserInfo: `https://localhost:7225/connect/userinfo`
- End Session: `https://localhost:7225/account/logout`

### Supported Scopes

- `openid` - Required for OIDC
- `profile` - User profile information
- `email` - User email
- `roles` - User roles
- `offline_access` - Refresh tokens

## Client Configuration

### Prerequisites on Identity Server

Before running the application, you need to register this SPA client on the identity server with the following configuration:

```csharp
new Client
{
    ClientId = "chess-tournaments-spa",
    ClientName = "Chess Tournaments SPA",

    AllowedGrantTypes = GrantTypes.Code,
    RequirePkce = true,
    RequireClientSecret = false,

    RedirectUris =
    {
        "http://localhost:4200/callback",
        "http://localhost:4200/silent-refresh.html"
    },

    PostLogoutRedirectUris = { "http://localhost:4200" },

    AllowedCorsOrigins = { "http://localhost:4200" },

    AllowedScopes =
    {
        IdentityServerConstants.StandardScopes.OpenId,
        IdentityServerConstants.StandardScopes.Profile,
        IdentityServerConstants.StandardScopes.Email,
        "roles",
        IdentityServerConstants.StandardScopes.OfflineAccess
    },

    AllowOfflineAccess = true,
    RefreshTokenUsage = TokenUsage.ReUse,

    AccessTokenLifetime = 3600,
    RefreshTokenExpiration = TokenExpiration.Sliding,
    SlidingRefreshTokenLifetime = 86400
}
```

## Application Configuration

The OIDC configuration is located in [src/app/auth/auth.config.ts](src/app/auth/auth.config.ts):

```typescript
export const authConfig: AuthConfig = {
  issuer: 'https://localhost:7225/',
  redirectUri: window.location.origin + '/callback',
  clientId: 'chess-tournaments-spa',
  scope: 'openid profile email roles offline_access',
  responseType: 'code',
  // ... other settings
};
```

## How It Works

### 1. Authorization Code Flow with PKCE

1. User clicks "Sign In" button
2. App generates a random **code verifier** and **code challenge**
3. User is redirected to Identity Server's authorization endpoint
4. User authenticates and consents
5. Identity Server redirects back with an **authorization code**
6. App exchanges the code + code verifier for tokens
7. Tokens are stored securely in session/local storage

### 2. Token Management

- **Access Token**: Used for API authentication (expires in 1 hour)
- **Refresh Token**: Used to get new access tokens (sliding expiration)
- **ID Token**: Contains user identity claims

The app automatically refreshes tokens using the silent refresh iframe.

### 3. Protected Routes

Routes can be protected using the `authGuard`:

```typescript
{
  path: 'dashboard',
  component: DashboardComponent,
  canActivate: [authGuard]
}
```

## Running the Application

1. **Start the Identity Server**:

   ```bash
   cd backend/src/ChessTournaments.Identity
   dotnet run
   ```

2. **Start the Angular Application**:

   ```bash
   cd frontend/chess-tournaments-app
   npm install
   npm start
   ```

3. **Navigate to**: `http://localhost:4200`

## Application Structure

### Auth Module

- **[auth.config.ts](src/app/auth/auth.config.ts)**: OIDC configuration
- **[auth.service.ts](src/app/auth/auth.service.ts)**: Authentication service (login, logout, token management)
- **[auth.guard.ts](src/app/auth/auth.guard.ts)**: Route guard for protected pages

### Components

- **[Home](src/app/components/home/)**: Public landing page with login
- **[Dashboard](src/app/components/dashboard/)**: Protected page showing user info
- **[Callback](src/app/components/callback/)**: Handles OAuth callback

## Security Features

✅ **PKCE (Proof Key for Code Exchange)**: Prevents authorization code interception
✅ **State Parameter**: CSRF protection
✅ **Silent Refresh**: Automatic token renewal without full page reload
✅ **Secure Token Storage**: Tokens stored in browser storage with proper security
✅ **HTTPS Required**: Production requires HTTPS

## Troubleshooting

### CORS Errors

If you see CORS errors, ensure the identity server is configured to allow `http://localhost:4200` in `AllowedCorsOrigins`.

### Redirect Loop

If you experience a redirect loop:

1. Clear browser cache and cookies
2. Check that redirect URIs match exactly in both client and server config
3. Verify the identity server is running on `https://localhost:7225/`

### Token Not Working

1. Check browser console for errors
2. Verify scopes match between client config and server
3. Ensure access token is being sent in Authorization header

## Development Tips

### Debugging

Enable debug logging in [auth.config.ts](src/app/auth/auth.config.ts):

```typescript
showDebugInformation: true;
```

Check browser console for detailed OAuth flow logs.

### Testing

Test users should be created in the identity server's database or in-memory store.

## Production Deployment

Before deploying to production:

1. ✅ Update `redirectUri` and `postLogoutRedirectUri` with production URLs
2. ✅ Set `requireHttps: true`
3. ✅ Disable debug logging: `showDebugInformation: false`
4. ✅ Use environment variables for configuration
5. ✅ Configure proper CORS on identity server
6. ✅ Ensure SSL/TLS certificates are valid

## Resources

- [angular-oauth2-oidc Documentation](https://github.com/manfredsteyer/angular-oauth2-oidc)
- [OAuth 2.0 Authorization Code Flow](https://oauth.net/2/grant-types/authorization-code/)
- [PKCE](https://oauth.net/2/pkce/)
- [OpenID Connect](https://openid.net/connect/)
