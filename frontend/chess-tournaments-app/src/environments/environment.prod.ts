export const environment = {
  production: true,
  environmentName: 'production',
  apiUrl: 'https://api.chesstournaments.com',

  // OIDC Configuration for Production
  oidc: {
    issuer: 'https://auth.chesstournaments.com/',
    clientId: 'chess-tournaments-spa',
    scope: 'openid profile email roles offline_access crud_api',
    responseType: 'code',
    showDebugInformation: false,
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
