import { Component, input } from '@angular/core';
import { AdventureRow } from "./adventure-row/adventure-row";
import { Adventure } from '../../../core/models/adventure';

@Component({
  selector: 'app-adventures-list',
  imports: [AdventureRow],
  templateUrl: './adventures-list.html',
  styleUrl: './adventures-list.scss',
})
export class AdventuresList {
  readonly adventures = input.required<readonly Adventure[]>();
}
