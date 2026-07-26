import { Component, input } from '@angular/core';
import { User } from '../../../core/models/user';
import { UserRow } from "./user-row/user-row";

@Component({
  selector: 'app-leaderboard',
  imports: [UserRow],
  templateUrl: './leaderboard.html',
  styleUrl: './leaderboard.scss',
})
export class Leaderboard {
  readonly users = input.required<readonly User[]>();
}
