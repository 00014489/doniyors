import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';

import { UserAdminDto, UserPayload, UserRoleDto } from '../models/user-admin-dto';
import { environment } from '../../../../environments/environment';

@Service()
export class UsersService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/admin/Users`;

  getAll(): Observable<readonly UserAdminDto[]> {
    return this.http.get<readonly UserAdminDto[]>(this.apiUrl);
  }

  getById(id: number): Observable<UserAdminDto> {
    return this.http.get<UserAdminDto>(`${this.apiUrl}/${id}`);
  }

  getRoles(): Observable<readonly UserRoleDto[]> {
    return this.http.get<readonly UserRoleDto[]>(`${this.apiUrl}/roles`);
  }

  create(payload: UserPayload): Observable<UserAdminDto> {
    return this.http.post<UserAdminDto>(this.apiUrl, payload);
  }

  update(id: number, payload: UserPayload): Observable<UserAdminDto> {
    return this.http.put<UserAdminDto>(`${this.apiUrl}/${id}`, payload);
  }

  /**
   * Soft delete. The API keeps the row and flips its status to `false`;
   * it responds with the updated user.
   */
  disable(id: number): Observable<UserAdminDto> {
    return this.http.delete<UserAdminDto>(`${this.apiUrl}/${id}`);
  }
}
