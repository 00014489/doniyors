import { Component, computed, inject, signal } from '@angular/core';
import { TelegramService } from '../../core/services/telegram-service';
import { QrService } from './qr-service';
import { QRCodeComponent } from 'angularx-qrcode';



@Component({
  selector: 'app-qr-code',
  imports: [QRCodeComponent],
  templateUrl: './qr-code.html',
  styleUrl: './qr-code.scss',
})
export class QrCode {
  private readonly telegram = inject(TelegramService);
  private readonly qrService = inject(QrService);

  readonly user = this.telegram.user;

  readonly avatar = computed(() => this.user()?.photo_url ?? '');

  readonly displayName = computed(() =>
    this.user()?.username ??
    this.user()?.first_name ??
    'Telegram User',
  );

  readonly qrToken = this.qrService.qrToken;
  readonly points = this.qrService.points;

  constructor() {
    void this.qrService.load();
  }
}
