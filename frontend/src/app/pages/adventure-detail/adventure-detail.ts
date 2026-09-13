import {
  Component,
  DestroyRef,
  ElementRef,
  afterRenderEffect,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@ngx-translate/core';

import { MemberApi } from '../../core/services/member-api';
import { LocalDatePipe } from '../../shared/pipes/local-date';
import { MoneyPipe } from '../../shared/pipes/money';
import { TelegramService } from '../../core/services/telegram-service';

/**
 * Full detail of one adventure: every picture in a swipeable gallery, then the
 * facts. Closing is reported to the shell, which owns the view stack.
 */
@Component({
  selector: 'app-adventure-detail',
  imports: [LocalDatePipe, MoneyPipe, TranslatePipe],
  templateUrl: './adventure-detail.html',
  styleUrl: './adventure-detail.scss',
})
export class AdventureDetail {
  private readonly memberApi = inject(MemberApi);
  private readonly telegram = inject(TelegramService);
  private readonly destroyRef = inject(DestroyRef);

  readonly adventureId = input.required<number>();

  readonly close = output<void>();

  private readonly resource = rxResource({
    params: () => ({ id: this.adventureId() }),
    stream: ({ params }) => this.memberApi.getAdventure(params.id),
  });

  protected readonly adventure = this.resource.value;
  protected readonly isLoading = this.resource.isLoading;
  protected readonly failed = computed(() => !!this.resource.error());

  /** Index of the picture currently centred in the gallery. */
  protected readonly activeImage = signal(0);

  protected readonly images = computed(() => this.adventure()?.images ?? []);

  private readonly heading = viewChild<ElementRef<HTMLElement>>('heading');

  constructor() {
    // The list that was tapped is gone once the detail opens, so focus moves
    // to the title: a screen reader starts reading the page that opened
    // rather than being left nowhere.
    afterRenderEffect(() => {
      this.heading()?.nativeElement.focus({ preventScroll: true });
    });

    // Telegram's own back arrow is what a Mini App user reaches for, so wire
    // it to the same action as the in-page button.
    const teardown = this.telegram.showBackButton(() => this.close.emit());

    this.destroyRef.onDestroy(teardown);

    // A different adventure starts at its own first picture.
    effect(() => {
      this.adventureId();

      this.activeImage.set(0);
    });
  }

  protected imageUrl(imageId: number): string {
    return this.memberApi.imageUrl(imageId);
  }

  /**
   * The gallery is a scroll-snap strip, so the active dot follows the scroll
   * position rather than driving it.
   */
  protected onGalleryScroll(event: Event): void {
    const strip = event.target as HTMLElement;

    if (strip.clientWidth === 0) {
      return;
    }

    this.activeImage.set(Math.round(strip.scrollLeft / strip.clientWidth));
  }

  protected retry(): void {
    this.resource.reload();
  }
}
