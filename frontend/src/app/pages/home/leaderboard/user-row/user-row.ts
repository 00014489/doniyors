import { Component, input } from '@angular/core';
import { User } from '../../../../core/models/user';

@Component({
  selector: 'app-user-row',
  imports: [],
  templateUrl: './user-row.html',
  styleUrl: './user-row.scss',
})
export class UserRow {
  readonly user = input.required<User>();
}
