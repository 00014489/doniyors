import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';

import { AdventureSubmit, TravelAdminDto } from '../models/travel-admin-dto';
import { environment } from '../../../../environments/environment';

@Service()
export class AdventuresService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/admin/Travels`;

  getAll(): Observable<readonly TravelAdminDto[]> {
    return this.http.get<readonly TravelAdminDto[]>(this.apiUrl);
  }

  getById(id: number): Observable<TravelAdminDto> {
    return this.http.get<TravelAdminDto>(`${this.apiUrl}/${id}`);
  }

  /**
   * Direct URL of a stored image, usable straight from an `<img>` tag.
   * Images are served from the member-facing route, which is the one
   * anonymous endpoint — an img tag cannot send the bearer token.
   */
  imageUrl(imageId: number): string {
    return `${environment.apiUrl}/adventures/images/${imageId}`;
  }

  create(submit: AdventureSubmit): Observable<TravelAdminDto> {
    return this.http.post<TravelAdminDto>(this.apiUrl, this.toFormData(submit));
  }

  update(id: number, submit: AdventureSubmit): Observable<TravelAdminDto> {
    return this.http.put<TravelAdminDto>(
      `${this.apiUrl}/${id}`,
      this.toFormData(submit),
    );
  }

  /**
   * Soft delete. The API keeps the row and flips its status to `false`;
   * it responds with the updated adventure.
   */
  disable(id: number): Observable<TravelAdminDto> {
    return this.http.delete<TravelAdminDto>(`${this.apiUrl}/${id}`);
  }

  /**
   * Images travel with the form, so writes are multipart rather than JSON.
   * The field names match the server's form model.
   */
  private toFormData(submit: AdventureSubmit): FormData {
    const { adventure, newImages, keepImageIds } = submit;

    const form = new FormData();

    form.set('Title', adventure.title);
    form.set('Description', adventure.description);
    form.set('TravelDate', adventure.travelDate);
    form.set('Cost', String(adventure.cost));
    form.set('Points', String(adventure.points));
    form.set('Status', String(adventure.status));

    for (const file of newImages) {
      form.append('Images', file, file.name);
    }

    for (const id of keepImageIds) {
      form.append('KeepImageIds', String(id));
    }

    return form;
  }
}
