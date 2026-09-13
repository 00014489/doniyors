import { Component, computed, inject, input, output } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

import { MemberApi } from '../../../../core/services/member-api';
import { AdventureListItem } from '../../../../core/models/member';
import { LocalDatePipe } from '../../../../shared/pipes/local-date';

/**
 * One adventure card. Also used for the profile's history list, so the two
 * read as the same thing in two places — `points` carries the awarded amount
 * there and the advertised reward here.
 */
@Component({
  selector: 'app-adventure-row',
  imports: [LocalDatePipe, TranslatePipe],
  templateUrl: './adventure-row.html',
  styleUrl: './adventure-row.scss',
})
export class AdventureRow {
  private readonly memberApi = inject(MemberApi);

  readonly adventure = input.required<AdventureListItem>();

  readonly open = output<number>();

  protected readonly imageUrl = computed(() => {
    const coverImageId = this.adventure().coverImageId;

    return coverImageId === null ? null : this.memberApi.imageUrl(coverImageId);
  });

  protected activate(): void {
    this.open.emit(this.adventure().id);
  }
}
