import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Data } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private baseApiUrl = 'https://localhost:7055/api/customers';

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

  register(email: string, password: string, fullname: string, date: string): Observable<any> {
    const params = new HttpParams()
      .set('email', email)
      .set('password', password)
      .set('fullName', fullname)
      .set('date',date)

    return this.http.post<string>(
      `${this.baseApiUrl}/register`,
      null,
      { params }
    );
  }
}
