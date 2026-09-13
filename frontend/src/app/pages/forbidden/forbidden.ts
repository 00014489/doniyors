import { Component } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

/** Shown when a non-administrator reaches an admin route. */
@Component({
  selector: 'app-forbidden',
  imports: [TranslatePipe],
  template: `
    <section class="forbidden" role="alert">
      <h1>{{ 'admin.forbidden.title' | translate }}</h1>

      <p>{{ 'admin.forbidden.message' | translate }}</p>
    </section>
  `,
  styles: `
    .forbidden {
      max-width: 32rem;
      margin: 4rem auto;
      padding: 0 1.5rem;
      text-align: center;
    }

    h1 {
      margin: 0 0 0.75rem;
      font-size: 1.5rem;
    }

    p {
      margin: 0;
      line-height: 1.6;
      opacity: 0.75;
    }
  `,
})
export class Forbidden {}
