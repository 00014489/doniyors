import { Component, computed, inject } from '@angular/core';
import { QRCodeComponent } from 'angularx-qrcode';
import { TranslatePipe } from '@ngx-translate/core';

import { TelegramService } from '../../core/services/telegram-service';
import { QrService } from './qr-service';

@Component({
  selector: 'app-qr-code',
  imports: [QRCodeComponent, TranslatePipe],
  templateUrl: './qr-code.html',
  styleUrl: './qr-code.scss',
})
export class QrCode {
  private readonly telegram = inject(TelegramService);
  private readonly qrService = inject(QrService);

  readonly user = this.telegram.user;

  readonly avatar = computed(() => this.user()?.photo_url ?? '');

  /** "@username" when there is one; Telegram guarantees only the first name. */
  readonly displayName = computed(() => {
    const user = this.user();

    if (!user) {
      return '';
    }

    return user.username ? `@${user.username}` : user.first_name;
  });

  readonly qrToken = this.qrService.qrToken;
  readonly points = this.qrService.points;

  constructor() {
    void this.qrService.load();
  }
}
