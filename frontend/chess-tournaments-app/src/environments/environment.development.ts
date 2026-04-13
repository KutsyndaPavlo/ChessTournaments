export const environment = {
  production: false,
  environmentName: 'development',
  apiUrl: 'https://localhost:7014',

  // OIDC Configuration for Development
  oidc: {
    issuer: 'https://localhost:7225/',
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
