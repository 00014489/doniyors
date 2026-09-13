import { HttpClient } from '@angular/common/http';
import { inject, Service, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../environments/environment';

interface QrResponse {
  qrToken: string;
  points: number;
}

@Service()
export class QrService {
  private readonly http = inject(HttpClient);

  readonly qrToken = signal('');
  readonly points = signal(0);

  private inFlight: Promise<void> | null = null;

  /**
   * The token never changes, so it is fetched once. Concurrent callers share
   * the same request rather than each firing their own.
   */
  async load(): Promise<void> {
    if (this.qrToken()) {
      return;
    }

    this.inFlight ??= this.fetch().finally(() => {
      this.inFlight = null;
    });

    return this.inFlight;
  }

  private async fetch(): Promise<void> {
    const response = await firstValueFrom(
      this.http.get<QrResponse>(`${environment.apiUrl}/users/qr-code`),
    );

    this.qrToken.set(response.qrToken);
    this.points.set(response.points);
  }
}
