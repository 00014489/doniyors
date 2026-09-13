export const environment = {
  production: true,

  /**
   * Relative on purpose. In production nginx serves the app and the API from
   * the same origin and proxies `/api/` to the backend, so there is no host
   * to hardcode — and no rebuild needed when the domain changes.
   */
  apiUrl: '/api',
};
