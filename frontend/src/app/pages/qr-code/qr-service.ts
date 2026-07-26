import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

interface QrResponse {
  qrToken: string;
  points: number;
}

@Injectable({
  providedIn: 'root',
})
export class QrService {
  private readonly http = inject(HttpClient);

  readonly qrToken = signal('');
  readonly points = signal(0);

  async load(): Promise<void> {
    // Already loaded
    if (this.qrToken()) {
      return;
    }

    const response = await firstValueFrom(
      this.http.get<QrResponse>(`${environment.apiUrl}/users/qr-code`)
    );

    this.qrToken.set(response.qrToken);
    this.points.set(response.points);
  }
}
