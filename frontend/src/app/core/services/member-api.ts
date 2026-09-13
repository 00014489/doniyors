import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import {
  AdventureDetail,
  AdventureListItem,
  Leaderboard,
  Profile,
} from '../models/member';

/**
 * The member-facing API — what the Mini App shows an ordinary user. The admin
 * panel has its own services under `pages/desktop`.
 */
@Service()
export class MemberApi {
  private readonly http = inject(HttpClient);

  private readonly api = environment.apiUrl;

  /** Soonest upcoming first, then past ones most recent first. */
  getAdventures(): Observable<readonly AdventureListItem[]> {
    return this.http.get<readonly AdventureListItem[]>(`${this.api}/adventures`);
  }

  getAdventure(id: number): Observable<AdventureDetail> {
    return this.http.get<AdventureDetail>(`${this.api}/adventures/${id}`);
  }

  /** Omitting `seasonKey` asks the API for the season running right now. */
  getLeaderboard(seasonKey?: string | null): Observable<Leaderboard> {
    const params = seasonKey
      ? new HttpParams().set('season', seasonKey)
      : undefined;

    return this.http.get<Leaderboard>(`${this.api}/leaderboard`, { params });
  }

  getProfile(): Observable<Profile> {
    return this.http.get<Profile>(`${this.api}/profile`);
  }

  /**
   * Direct URL of a stored picture, usable straight from an `<img>` tag — the
   * endpoint is anonymous precisely because an img tag cannot send the token.
   */
  imageUrl(imageId: number): string {
    return `${this.api}/adventures/images/${imageId}`;
  }
}
