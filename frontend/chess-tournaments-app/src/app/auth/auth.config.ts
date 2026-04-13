import { AuthConfig } from 'angular-oauth2-oidc';
import { environment } from '../../environments/environment';

export function getAuthConfig(): AuthConfig {
  return {
    // Url of the Identity Provider
    issuer: environment.oidc.issuer,

    // URL of the SPA to redirect the user to after login
    redirectUri: window.location.origin + '/callback',

    // The SPA's id. The SPA is registered with this id at the auth-server
    clientId: environment.oidc.clientId,

    // Set the scope for the permissions the client should request
    scope: environment.oidc.scope,

    // Use Authorization Code Flow with PKCE
    responseType: environment.oidc.responseType,

    // at_hash is not present in JWT token
    disableAtHashCheck: environment.oidc.disableAtHashCheck,

    // Debug information
    showDebugInformation: environment.oidc.showDebugInformation,

    // Logout endpoint
    postLogoutRedirectUri: window.location.origin,

    // Silent refresh
    silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
    useSilentRefresh: environment.oidc.useSilentRefresh,
    silentRefreshTimeout: environment.oidc.silentRefreshTimeout,

    // Session checks
    sessionChecksEnabled: environment.oidc.sessionChecksEnabled,

    // Require HTTPS
    requireHttps: environment.oidc.requireHttps,
  };
}

// Export for backward compatibility
export const authConfig: AuthConfig = getAuthConfig();
