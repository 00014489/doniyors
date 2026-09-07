import { Routes } from '@angular/router';

export const routes: Routes = [
    {
    path: 'adventures',
    loadComponent: () =>
        import('./pages/desktop/adventures/adventures-page/adventures-page')
        .then(m => m.AdventuresPage),
  },
];
