export const environment = {
  production: false,
  environmentName: 'qa',
  apiUrl: 'https://chess-tournaments-api-qa-ufdar3.azurewebsites.net',

  // OIDC Configuration for QA
  oidc: {
    issuer: 'https://chess-tournaments-identity-qa-ufdar3.azurewebsites.net/',
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
