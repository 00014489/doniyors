import { Component, inject } from '@angular/core';
import { DeskNav } from '../../shared/components/desk-nav/desk-nav';
import { Router, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-desktop-component',
  imports: [DeskNav, RouterOutlet],
  template: `
    <div class="layout">
      <app-desk-nav />

      <main class="layout__content">
        <router-outlet />
      </main>
    </div>
  `,
  styleUrl: './desktop-component.scss',
})
export class DesktopComponent {
  constructor() {
    // Initial navigation is disabled in app.config: run it only now, when the
    // administrator is known, so the guards see the real role and the empty
    // path lands on the adventures page.
    inject(Router).initialNavigation();
  }
}
