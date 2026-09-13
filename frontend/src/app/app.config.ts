import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding, withDisabledInitialNavigation } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { provideTranslateService } from '@ngx-translate/core';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth-interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor]),
    ),
    provideTranslateService({
      fallbackLang: 'en',
      loader: provideTranslateHttpLoader({
        prefix: '/i18n/',
        suffix: '.json',
        // The first file is requested while AuthService is still being
        // constructed (AuthService -> LanguageService -> TranslateService).
        // Through the interceptors that request injects AuthService again,
        // fails as a circular dependency, and the language is stored empty
        // for the rest of the session. Translations need no token anyway.
        useHttpBackend: true,
      }),
    }),
    provideBrowserGlobalErrorListeners(),
    provideRouter(
      routes,
      // Feeds route parameters (`:id`) straight into component `input()`s.
      withComponentInputBinding(),
      // The routes belong to the admin panel and are guarded by the signed-in
      // role, which is unknown at bootstrap. DesktopComponent starts routing
      // once it is shown — after sign-in, for an administrator.
      withDisabledInitialNavigation(),
    ),
  ],
};
