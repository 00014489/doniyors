import { Component, computed, input, signal } from '@angular/core';

/**
 * A member's picture, falling back to their initials.
 *
 * Telegram only hands us a `photo_url` for members who have opened the Mini
 * App and have a picture set, so the fallback is the common case rather than
 * an edge case. The initials get a colour derived from the name, so the same
 * person is always the same colour and a list stays readable.
 */
@Component({
  selector: 'app-avatar',
  template: `
    @if (showPhoto()) {

      <img
        class="avatar"
        [src]="photoUrl()"
        [alt]="name()"
        [style.width.px]="size()"
        [style.height.px]="size()"
        loading="lazy"
        (error)="onPhotoError()" />

    } @else {

      <span
        class="avatar avatar--initials"
        [style.width.px]="size()"
        [style.height.px]="size()"
        [style.font-size.px]="size() * 0.38"
        [style.--avatar-hue]="hue()"
        [attr.aria-label]="name()"
        role="img">
        {{ initials() }}
      </span>

    }
  `,
  styleUrl: './avatar.scss',
})
export class Avatar {
  readonly name = input.required<string>();
  readonly photoUrl = input<string | null>(null);

  /** Rendered size in pixels; the component stays square. */
  readonly size = input(44);

  /** A photo that fails to load falls back to initials like a missing one. */
  private readonly photoFailed = signal(false);

  protected readonly showPhoto = computed(
    () => !!this.photoUrl() && !this.photoFailed(),
  );

  protected readonly initials = computed(() => {
    const words = this.name().trim().split(/\s+/).filter(Boolean);

    if (!words.length) {
      return '?';
    }

    // Two initials for a full name, otherwise the first two letters — a single
    // letter reads as an icon rather than a person.
    const letters =
      words.length > 1
        ? `${words[0][0]}${words[1][0]}`
        : words[0].slice(0, 2);

    return letters.toUpperCase();
  });

  /** Deterministic hue, so a member keeps the same colour across screens. */
  protected readonly hue = computed(() => {
    const name = this.name();

    let hash = 0;

    for (let i = 0; i < name.length; i++) {
      hash = (hash * 31 + name.charCodeAt(i)) % 360;
    }

    return hash;
  });

  protected onPhotoError(): void {
    this.photoFailed.set(true);
  }
}
