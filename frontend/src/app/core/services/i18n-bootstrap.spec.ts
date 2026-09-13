import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { NavigationEnd, Router } from '@angular/router';
import { filter, firstValueFrom } from 'rxjs';

import { appConfig } from '../../app.config';
import { AuthService } from './auth-service';
import { DesktopComponent } from '../../layouts/desktop-component/desktop-component';

describe('start-up', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [...appConfig.providers, provideHttpClientTesting()],
    });
  });

  it('requests en.json while AuthService is being created', () => {
    TestBed.inject(AuthService);

    TestBed.inject(HttpTestingController).expectOne('/i18n/en.json');
  });

  it('does not route before sign-in, then opens adventures for an admin', async () => {
    const router = TestBed.inject(Router);
    const auth = TestBed.inject(AuthService);

    expect(router.navigated).toBe(false);

    auth.typeUser.set(1);

    const navigated = firstValueFrom(
      router.events.pipe(filter(event => event instanceof NavigationEnd)),
    );

    TestBed.createComponent(DesktopComponent);

    await navigated;

    expect(router.url).toBe('/adventures');
  });
});
