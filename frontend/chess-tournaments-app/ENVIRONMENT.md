# Environment Configuration Guide

This guide explains how to configure OIDC settings for different environments in the Chess Tournaments application.

## Available Environments

The application supports four environments:

1. **Development** (`environment.ts`) - Default local development
2. **Local** (`environment.local.ts`) - Custom local settings (gitignored)
3. **Staging** (`environment.staging.ts`) - Pre-production environment
4. **Production** (`environment.prod.ts`) - Production environment

## Environment Files Location

All environment files are located in:

```
src/environments/
├── environment.ts              # Development (default)
├── environment.local.ts        # Local (gitignored - create from example)
├── environment.staging.ts      # Staging
├── environment.prod.ts         # Production
└── environment.example.ts      # Template/Example
```

## Creating Your Local Environment

1. **Copy the example file**:

   ```bash
   cd src/environments
   cp environment.example.ts environment.local.ts
   ```

2. **Edit `environment.local.ts`** with your settings:

   ```typescript
   export const environment = {
     production: false,
     apiUrl: 'https://localhost:7014/api',

     oidc: {
       issuer: 'https://localhost:7225/', // Your identity server URL
       clientId: 'chess-tournaments-spa', // Your client ID
       scope: 'openid profile email roles offline_access',
       responseType: 'code',
       showDebugInformation: true,
       requireHttps: true,
       sessionChecksEnabled: true,
       useSilentRefresh: true,
       silentRefreshTimeout: 5000,
       disableAtHashCheck: true,
     },
   };
   ```

## Configuration Options

### Main Settings

| Property     | Type    | Description          | Example                      |
| ------------ | ------- | -------------------- | ---------------------------- |
| `production` | boolean | Production mode flag | `false`                      |
| `apiUrl`     | string  | Backend API base URL | `https://localhost:7014/api` |

### OIDC Settings

| Property               | Type    | Description                      | Required |
| ---------------------- | ------- | -------------------------------- | -------- |
| `issuer`               | string  | Identity provider URL            | ✅       |
| `clientId`             | string  | OAuth client identifier          | ✅       |
| `scope`                | string  | Space-separated OAuth scopes     | ✅       |
| `responseType`         | string  | OAuth response type (use 'code') | ✅       |
| `showDebugInformation` | boolean | Enable console logging           | ❌       |
| `requireHttps`         | boolean | Enforce HTTPS                    | ✅       |
| `sessionChecksEnabled` | boolean | Enable session monitoring        | ❌       |
| `useSilentRefresh`     | boolean | Enable automatic token refresh   | ❌       |
| `silentRefreshTimeout` | number  | Timeout for silent refresh (ms)  | ❌       |
| `disableAtHashCheck`   | boolean | Disable at_hash validation       | ❌       |

### Supported Scopes

- `openid` - **Required** for OpenID Connect
- `profile` - User profile information (name, etc.)
- `email` - User email address
- `roles` - User role claims
- `offline_access` - Enables refresh tokens for token renewal

## Running with Different Environments

### Development Commands

```bash
# Default development environment
npm start

# Local environment (your custom settings)
npm run start:local

# Staging environment
npm run start:staging

# Production environment (not recommended for local dev)
npm run start:prod
```

### Build Commands

```bash
# Default production build
npm run build

# Build for local
npm run build:local

# Build for staging
npm run build:staging

# Build for production
npm run build:prod
```

### Direct ng Commands

```bash
# Serve with specific configuration
ng serve --configuration local
ng serve --configuration staging
ng serve --configuration production

# Build with specific configuration
ng build --configuration local
ng build --configuration staging
ng build --configuration production
```

## Environment-Specific Settings

### Development (`environment.ts`)

- **Purpose**: Default local development
- **Debug**: Enabled
- **Optimization**: Disabled
- **Source Maps**: Enabled

### Local (`environment.local.ts`)

- **Purpose**: Personal local overrides
- **Note**: This file is gitignored
- **Use Case**: Custom identity server, different ports, etc.

### Staging (`environment.staging.ts`)

- **Purpose**: Pre-production testing
- **Debug**: Enabled
- **Optimization**: Enabled
- **Use Case**: QA, UAT environments

### Production (`environment.prod.ts`)

- **Purpose**: Live production
- **Debug**: Disabled
- **Optimization**: Maximum
- **Use Case**: End users

## Common Scenarios

### Scenario 1: Different Identity Server Port

If your identity server runs on a different port:

```typescript
// environment.local.ts
oidc: {
  issuer: 'https://localhost:5001/',  // Changed port
  // ... other settings
}
```

### Scenario 2: HTTP for Local Development

⚠️ **Not recommended** - Use HTTPS even for local development

```typescript
// environment.local.ts
oidc: {
  issuer: 'http://localhost:7225/',
  requireHttps: false,  // Disable HTTPS requirement
  // ... other settings
}
```

### Scenario 3: Different Client ID

If you have a separate client for testing:

```typescript
// environment.local.ts
oidc: {
  clientId: 'chess-tournaments-spa-dev',  // Different client
  // ... other settings
}
```

### Scenario 4: Additional Scopes

If you need extra scopes:

```typescript
// environment.local.ts
oidc: {
  scope: 'openid profile email roles offline_access custom_scope',
  // ... other settings
}
```

## Verification

### Check Current Environment

The environment is loaded at compile/serve time. To verify which environment is active:

1. **Check browser console** - Look for debug messages (if `showDebugInformation: true`)
2. **Check network tab** - Verify the issuer URL in OIDC requests
3. **Dashboard** - Check the access token's `iss` claim

### Debug Configuration Issues

If authentication isn't working:

1. **Enable debug logging**:

   ```typescript
   showDebugInformation: true;
   ```

2. **Check browser console** for errors

3. **Verify issuer URL** matches your identity server

4. **Confirm client ID** is registered in identity server

5. **Check redirect URIs** match in both client config and server

## Security Best Practices

### ✅ DO:

- Keep `environment.local.ts` in `.gitignore`
- Use HTTPS in all environments
- Disable debug logging in production
- Use different client IDs per environment
- Store sensitive values in environment files only

### ❌ DON'T:

- Commit `environment.local.ts` to version control
- Disable HTTPS requirement in production
- Use production credentials in development
- Share environment files with sensitive data

## Troubleshooting

### Issue: Changes Not Reflected

**Solution**: Restart the dev server

```bash
# Stop the server (Ctrl+C)
npm run start:local
```

### Issue: Wrong Environment Loaded

**Solution**: Check the build configuration

```bash
# Verify configuration
ng serve --configuration local --verbose
```

### Issue: CORS Errors

**Solution**: Ensure identity server allows your app's origin

```csharp
// In Identity Server
AllowedCorsOrigins = { "http://localhost:4200" }
```

### Issue: Environment File Not Found

**Solution**: Create from example

```bash
cp src/environments/environment.example.ts src/environments/environment.local.ts
```

## CI/CD Integration

### GitHub Actions Example

```yaml
- name: Build for Staging
  run: npm run build:staging

- name: Build for Production
  run: npm run build:prod
```

### Environment Variables

For sensitive values in CI/CD, use environment variables:

```typescript
// environment.prod.ts
export const environment = {
  oidc: {
    issuer: process.env['OIDC_ISSUER'] || 'https://default.com',
    clientId: process.env['OIDC_CLIENT_ID'] || 'default-client',
  },
};
```

## Additional Resources

- [Angular Environments Guide](https://angular.dev/tools/cli/environments)
- [angular-oauth2-oidc Configuration](https://github.com/manfredsteyer/angular-oauth2-oidc)
- [OAuth 2.0 Best Practices](https://oauth.net/2/oauth-best-practice/)
