import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';


interface NavItem {
  readonly label: string;
  readonly icon: string;
  readonly route: string;
}

@Component({
  selector: 'app-desk-nav',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './desk-nav.html',
  styleUrl: './desk-nav.scss',
})
export class DeskNav {
  private readonly router = inject(Router);

  readonly collapsed = signal(false);

  readonly items = signal<readonly NavItem[]>([
    {
      label: 'Dashboard',
      icon: 'dashboard',
      route: '/dashboard',
    },
    {
      label: 'Users',
      icon: 'group',
      route: '/users',
    },
    {
      label: 'Adventures',
      icon: 'hiking',
      route: '/adventures',
    },
    {
      label: 'Participations',
      icon: 'event_available',
      route: '/participations',
    },
    {
      label: 'Rewards',
      icon: 'stars',
      route: '/rewards',
    },
    {
      label: 'Reports',
      icon: 'bar_chart',
      route: '/reports',
    },
    {
      label: 'Settings',
      icon: 'settings',
      route: '/settings',
    },
  ]);

  readonly currentUrl = computed(() => this.router.url);

  toggle(): void {
    this.collapsed.update(value => !value);
  }
}
