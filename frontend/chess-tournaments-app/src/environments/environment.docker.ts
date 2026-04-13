export const environment = {
  production: true,
  environmentName: 'docker',
  apiUrl: 'http://localhost:5000',

  // OIDC Configuration for Docker
  oidc: {
    issuer: 'http://localhost:5001/',
    clientId: 'chess-tournaments-spa',
    scope: 'openid profile email roles offline_access crud_api',
    responseType: 'code',
    showDebugInformation: false,
    requireHttps: false,

    // Session settings
    sessionChecksEnabled: true,

    // Silent refresh settings
    useSilentRefresh: true,
    silentRefreshTimeout: 5000,

    // Token settings
    disableAtHashCheck: true,
  },
};
