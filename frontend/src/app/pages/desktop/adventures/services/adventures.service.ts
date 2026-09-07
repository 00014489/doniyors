import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { TravelAdminDto } from '../models/travel-admin-dto';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AdventuresService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/admin/Travels`;

  getAll(): Observable<readonly TravelAdminDto[]> {
    return this.http.get<readonly TravelAdminDto[]>(this.apiUrl);
  }
}