import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';

import {
  TransactionAdminDto,
  TransactionPayload,
} from '../models/transaction-admin-dto';
import { environment } from '../../../../environments/environment';

@Service()
export class TransactionsService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/admin/Transactions`;

  getAll(): Observable<readonly TransactionAdminDto[]> {
    return this.http.get<readonly TransactionAdminDto[]>(this.apiUrl);
  }

  getById(id: number): Observable<TransactionAdminDto> {
    return this.http.get<TransactionAdminDto>(`${this.apiUrl}/${id}`);
  }

  create(payload: TransactionPayload): Observable<TransactionAdminDto> {
    return this.http.post<TransactionAdminDto>(this.apiUrl, payload);
  }

  update(id: number, payload: TransactionPayload): Observable<TransactionAdminDto> {
    return this.http.put<TransactionAdminDto>(`${this.apiUrl}/${id}`, payload);
  }

  /**
   * Soft delete. The API keeps the record and flips its status to `false`;
   * it responds with the updated transaction.
   */
  disable(id: number): Observable<TransactionAdminDto> {
    return this.http.delete<TransactionAdminDto>(`${this.apiUrl}/${id}`);
  }
}
