import {
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';

import {
  AdventureSubmit,
  MAX_ADVENTURE_IMAGES,
  MIN_ADVENTURE_IMAGES,
  TravelAdminDto,
  TravelImageDto,
} from '../models/travel-admin-dto';
import { AdventuresService } from '../services/adventures.service';

/** A file the administrator just picked, with its preview URL. */
interface PickedImage {
  readonly file: File;
  readonly previewUrl: string;
}

/** Converts an ISO timestamp into the `yyyy-MM-ddTHH:mm` shape `datetime-local` expects. */
function toDateTimeLocal(iso: string): string {
  const date = new Date(iso);

  if (Number.isNaN(date.getTime())) {
    return '';
  }

  const pad = (value: number) => `${value}`.padStart(2, '0');

  return (
    `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}` +
    `T${pad(date.getHours())}:${pad(date.getMinutes())}`
  );
}

/**
 * Shared create/edit form for an adventure. It owns validation and the image
 * selection only — persistence and navigation stay with the hosting page.
 */
@Component({
  selector: 'app-adventure-form',
  imports: [ReactiveFormsModule, TranslatePipe],
  templateUrl: './adventure-form.html',
  styleUrl: './adventure-form.scss',
})
export class AdventureForm {
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly adventuresService = inject(AdventuresService);
  private readonly destroyRef = inject(DestroyRef);

  /** Existing adventure when editing, `null` when creating. */
  readonly adventure = input<TravelAdminDto | null>(null);

  readonly saving = input(false);
  readonly errorMessage = input<string | null>(null);

  /** Already translated by the host page; empty falls back to "Save changes". */
  readonly submitLabel = input('');

  readonly save = output<AdventureSubmit>();
  readonly cancel = output<void>();

  protected readonly maxImages = MAX_ADVENTURE_IMAGES;

  protected readonly submitted = signal(false);

  /** Already stored images the administrator has kept. */
  protected readonly keptImages = signal<readonly TravelImageDto[]>([]);

  /** Files picked in this session, not uploaded yet. */
  protected readonly pickedImages = signal<readonly PickedImage[]>([]);

  protected readonly imageCount = computed(
    () => this.keptImages().length + this.pickedImages().length,
  );

  protected readonly canAddImages = computed(() => this.imageCount() < MAX_ADVENTURE_IMAGES);

  /** A translation key, or `null` when the image count is fine. */
  protected readonly imageError = computed(() => {
    const count = this.imageCount();

    if (count < MIN_ADVENTURE_IMAGES) {
      return 'admin.adventures.form.imagesRequired';
    }

    if (count > MAX_ADVENTURE_IMAGES) {
      return 'admin.adventures.form.imagesTooMany';
    }

    return null;
  });

  /** The image rule is only surfaced once the administrator tries to submit. */
  protected readonly showImageError = computed(
    () => this.submitted() && this.imageError() !== null,
  );

  protected readonly form = this.formBuilder.group({
    title: ['', [Validators.required, Validators.maxLength(255)]],
    description: ['', [Validators.maxLength(4000)]],
    travelDate: ['', [Validators.required]],
    cost: [0, [Validators.required, Validators.min(0)]],
    points: [0, [Validators.required, Validators.min(0)]],
    status: [true],
  });

  constructor() {
    effect(() => {
      const adventure = this.adventure();

      if (!adventure) {
        return;
      }

      this.form.reset({
        title: adventure.title,
        description: adventure.description ?? '',
        travelDate: toDateTimeLocal(adventure.travelDate),
        cost: adventure.cost,
        points: adventure.points,
        status: adventure.status,
      });

      this.keptImages.set([...adventure.images]);
    });

    effect(() => {
      if (this.saving()) {
        this.form.disable({ emitEvent: false });
      } else {
        this.form.enable({ emitEvent: false });
      }
    });

    // Preview URLs are created by this component, so it releases them too.
    this.destroyRef.onDestroy(() => {
      for (const picked of this.pickedImages()) {
        URL.revokeObjectURL(picked.previewUrl);
      }
    });
  }

  protected storedImageUrl(imageId: number): string {
    return this.adventuresService.imageUrl(imageId);
  }

  protected onFilesPicked(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);

    if (files.length) {
      const room = MAX_ADVENTURE_IMAGES - this.imageCount();

      const added = files
        .slice(0, Math.max(room, 0))
        .map((file) => ({ file, previewUrl: URL.createObjectURL(file) }));

      this.pickedImages.update((current) => [...current, ...added]);
    }

    // Clear the input so picking the same file again still fires a change.
    input.value = '';
  }

  protected removeStoredImage(imageId: number): void {
    this.keptImages.update((current) => current.filter((image) => image.id !== imageId));
  }

  protected removePickedImage(previewUrl: string): void {
    this.pickedImages.update((current) => {
      const removed = current.find((image) => image.previewUrl === previewUrl);

      if (removed) {
        URL.revokeObjectURL(removed.previewUrl);
      }

      return current.filter((image) => image.previewUrl !== previewUrl);
    });
  }

  protected submit(): void {
    this.submitted.set(true);

    if (this.form.invalid || this.imageError()) {
      this.form.markAllAsTouched();

      return;
    }

    const value = this.form.getRawValue();

    this.save.emit({
      adventure: {
        title: value.title.trim(),
        description: value.description.trim(),
        travelDate: new Date(value.travelDate).toISOString(),
        cost: Number(value.cost),
        points: Number(value.points),
        status: value.status,
      },
      newImages: this.pickedImages().map((picked) => picked.file),
      keepImageIds: this.keptImages().map((image) => image.id),
    });
  }

  /** A control shows its error once the user has interacted or tried to submit. */
  protected showError(
    control: 'title' | 'description' | 'travelDate' | 'cost' | 'points',
  ): boolean {
    const field = this.form.controls[control];

    return field.invalid && (field.touched || this.submitted());
  }
}
