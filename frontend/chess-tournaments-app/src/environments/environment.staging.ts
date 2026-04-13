export const environment = {
  production: false,
  apiUrl: 'https://staging-api.chesstournaments.com/api',

  // OIDC Configuration for Staging
  oidc: {
    issuer: 'https://staging-auth.chesstournaments.com/',
    clientId: 'chess-tournaments-spa',
    scope: 'openid profile email roles offline_access crud_api',
    responseType: 'code',
    showDebugInformation: true,
    requireHttps: true,

    // Session settings
    sessionChecksEnabled: true,

    // Silent refresh settings
    useSilentRefresh: true,
    silentRefreshTimeout: 5000,

    // Token settings
    disableAtHashCheck: true,
  },
};
