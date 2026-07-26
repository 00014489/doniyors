import { Component, input } from '@angular/core';
import { Adventure } from '../../../../core/models/adventure';

@Component({
  selector: 'app-adventure-row',
  imports: [],
  templateUrl: './adventure-row.html',
  styleUrl: './adventure-row.scss',
})
export class AdventureRow {
  readonly adventure = input.required<Adventure>();
}
