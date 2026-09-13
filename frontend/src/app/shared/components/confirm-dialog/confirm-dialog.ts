import {
  Component,
  effect,
  input,
  output,
  viewChild,
  ElementRef,
} from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

let nextId = 0;

/**
 * Reusable confirmation dialog built on the native `<dialog>` element so focus
 * trapping, Escape handling and the backdrop come from the platform.
 *
 * The caller owns the open state: the dialog never closes itself, it only
 * reports the administrator's choice through `confirmed` / `cancelled`.
 */
@Component({
  selector: 'app-confirm-dialog',
  imports: [TranslatePipe],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.scss',
})
export class ConfirmDialog {
  readonly open = input.required<boolean>();

  /** Unique per instance so several dialogs can coexist on one page. */
  protected readonly headingId = `confirm-dialog-heading-${nextId}`;
  protected readonly messageId = `confirm-dialog-message-${nextId++}`;

  /** Already translated by the caller; empty ones fall back to translated defaults. */
  readonly heading = input('');
  readonly message = input('');
  readonly confirmLabel = input('');
  readonly cancelLabel = input('');

  /** Keeps the dialog open and the buttons disabled while the action runs. */
  readonly busy = input(false);

  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  private readonly dialogRef = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  constructor() {
    effect(() => {
      const dialog = this.dialogRef().nativeElement;

      if (this.open()) {
        if (!dialog.open) {
          dialog.showModal();
        }

        return;
      }

      if (dialog.open) {
        dialog.close();
      }
    });
  }

  protected confirm(): void {
    this.confirmed.emit();
  }

  protected cancel(): void {
    if (this.busy()) {
      return;
    }

    this.cancelled.emit();
  }

  /**
   * The platform closed the dialog (for instance a forced `close()`) while the
   * caller still considers it open — report it as a cancellation.
   */
  protected onClose(): void {
    if (this.open()) {
      this.cancelled.emit();
    }
  }
}
