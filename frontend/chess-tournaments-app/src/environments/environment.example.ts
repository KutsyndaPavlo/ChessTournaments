// Example environment configuration
// Copy this file to environment.local.ts and customize for your setup

export const environment = {
  production: false,
  apiUrl: 'https://localhost:7000/api', // Your API backend URL

  // OIDC Configuration
  oidc: {
    // Identity Provider URL
    issuer: 'https://localhost:7225/',

    // Client ID registered in the identity server
    clientId: 'chess-tournaments-spa',

    // Requested scopes
    // openid: Required for OIDC
    // profile: User profile information
    // email: User email
    // roles: User roles
    // offline_access: Enables refresh tokens
    scope: 'openid profile email roles offline_access crud_api',

    // OAuth flow type (should be 'code' for Authorization Code Flow)
    responseType: 'code',

    // Enable debug logging in browser console
    showDebugInformation: true,

    // Require HTTPS (disable only for local development)
    requireHttps: true,

    // Enable session checks
    sessionChecksEnabled: true,

    // Silent refresh settings
    useSilentRefresh: true,
    silentRefreshTimeout: 5000, // milliseconds

    // Token validation settings
    disableAtHashCheck: true,
  },
};
