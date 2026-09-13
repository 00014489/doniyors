import { Routes } from '@angular/router';

import { adminGuard, forbiddenGuard } from './core/guards/admin-guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'adventures',
  },
  {
    path: 'adventures',
    canMatch: [adminGuard],
    loadComponent: () =>
      import('./pages/desktop/adventures/adventures-page/adventures-page')
        .then(m => m.AdventuresPage),
  },
  {
    // Must stay ahead of ':id' so the literal segment wins.
    path: 'adventures/new',
    canMatch: [adminGuard],
    loadComponent: () =>
      import('./pages/desktop/adventures/adventure-create-page/adventure-create-page')
        .then(m => m.AdventureCreatePage),
  },
  {
    path: 'adventures/:id',
    canMatch: [adminGuard],
    loadComponent: () =>
      import('./pages/desktop/adventures/adventure-detail-page/adventure-detail-page')
        .then(m => m.AdventureDetailPage),
  },
  {
    path: 'adventures/:id/edit',
    canMatch: [adminGuard],
    loadComponent: () =>
      import('./pages/desktop/adventures/adventure-edit-page/adventure-edit-page')
        .then(m => m.AdventureEditPage),
  },
  {
    path: 'users',
    canMatch: [adminGuard],
    loadComponent: () =>
      import('./pages/desktop/users/users-page/users-page')
        .then(m => m.UsersPage),
  },
  {
    // Must stay ahead of ':id' so the literal segment wins.
    path: 'users/new',
    canMatch: [adminGuard],
    loadComponent: () =>
      import('./pages/desktop/users/user-create-page/user-create-page')
        .then(m => m.UserCreatePage),
  },
  {
    path: 'users/:id',
    canMatch: [adminGuard],
    loadComponent: () =>
      import('./pages/desktop/users/user-detail-page/user-detail-page')
        .then(m => m.UserDetailPage),
  },
  {
    path: 'users/:id/edit',
    canMatch: [adminGuard],
    loadComponent: () =>
      import('./pages/desktop/users/user-edit-page/user-edit-page')
        .then(m => m.UserEditPage),
  },
  {
    path: 'transactions',
    canMatch: [adminGuard],
    loadComponent: () =>
      import('./pages/desktop/transactions/transactions-page/transactions-page')
        .then(m => m.TransactionsPage),
  },
  {
    // Must stay ahead of ':id' so the literal segment wins.
    path: 'transactions/new',
    canMatch: [adminGuard],
    loadComponent: () =>
      import('./pages/desktop/transactions/transaction-create-page/transaction-create-page')
        .then(m => m.TransactionCreatePage),
  },
  {
    path: 'transactions/:id',
    canMatch: [adminGuard],
    loadComponent: () =>
      import('./pages/desktop/transactions/transaction-detail-page/transaction-detail-page')
        .then(m => m.TransactionDetailPage),
  },
  {
    path: 'transactions/:id/edit',
    canMatch: [adminGuard],
    loadComponent: () =>
      import('./pages/desktop/transactions/transaction-edit-page/transaction-edit-page')
        .then(m => m.TransactionEditPage),
  },
  {
    path: 'forbidden',
    canMatch: [forbiddenGuard],
    loadComponent: () =>
      import('./pages/forbidden/forbidden')
        .then(m => m.Forbidden),
  },
  {
    path: '**',
    redirectTo: 'adventures',
  },
];
