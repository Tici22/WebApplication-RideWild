import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private baseApiUrl = 'https://localhost:7204/api/customers'; // Cambia con il tuo URL backend

  constructor(private http: HttpClient) { }

  login(email: string, password: string): Observable<{ message: string; token: string }> {
    const params = new HttpParams()
      .set('email', email)
      .set('password', password);

    return this.http.post<{ message: string; token: string }>(
      `${this.baseApiUrl}/login`,
      null,
      { params }
    );
  }

  register(email: string, password: string, fullName: string): Observable<string> {
    const params = new HttpParams()
      .set('email', email)
      .set('password', password)
      .set('fullName', fullName);

    return this.http.post<string>(
      `${this.baseApiUrl}/register`,
      null,
      { params }
    );
  }
}
