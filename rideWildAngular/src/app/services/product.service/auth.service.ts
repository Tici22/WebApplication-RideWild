import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { Customer } from '../../models/custmoer';
import { jwtDecode } from 'jwt-decode';
import { JwtPayload } from '../../models/Util/jwt-payload';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private baseApiUrl = 'https://localhost:7055/api/customers';

  constructor(private http: HttpClient) { }

  login(email: string, password: string): Observable<{ message: string; token: string; fullname: string }> {
    const params = new HttpParams()
      .set('email', email)
      .set('password', password);

    return this.http.post<{ message: string; token: string; fullname: string }>(
      `${this.baseApiUrl}/login`,
      null,
      { params }
    );
  }

  register(email: string, password: string, fullname: string, date: string): Observable<any> {
    const params = new HttpParams()
      .set('email', email)
      .set('password', password)
      .set('fullname', fullname)
      .set('date', date);

    return this.http.post<string>(
      `${this.baseApiUrl}/register`,
      null,
      { params }
    );
  }

  // Recupera il token JWT dal localStorage
  private getToken(): string | null {
    return localStorage.getItem('token');
  }

  // Decodifica il token JWT per estrarre info utente
  getUserIdFromToken(): JwtPayload | null {
    const token = this.getToken();
    if (!token) {
      console.warn('AuthService: Nessun token trovato.');
      return null;
    }

    try {
      const decodedToken: any = jwtDecode(token);

      const userIdClaim = decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
      const email = decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"];

      if (typeof userIdClaim !== 'string' || typeof email !== 'string') {
        console.error('AuthService: Token senza claim validi.');
        return null;
      }

      const userId = Number(userIdClaim);
      if (isNaN(userId)) {
        console.warn('AuthService: claim nameidentifier non valido:', userIdClaim);
        return null;
      }

      return {
        userId: userIdClaim,
        email: email,
        exp: decodedToken.exp
      } as JwtPayload;

    } catch (error) {
      console.error('AuthService: Errore nella decodifica token:', error);
      return null;
    }
  }

  // Recupera informazioni utente con token
  getUserInfo(id: number): Observable<Customer> {
    const token = this.getToken();
    if (!token) {
      console.error('Token non trovato');
      return throwError(() => new Error('Non autorizzato: Token non trovato'));
    }
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    const url = `${this.baseApiUrl}/new/${id}`;
    return this.http.get<Customer>(url, { headers }).pipe(
      catchError(error => {
        if (error.status === 404) {
          return throwError(() => new Error('Utente non trovato'));
        }
        return throwError(() => error);
      })
    );
  }

  // Modifica credenziali utente
  updateCredentials(id: number, customer: Customer): Observable<any> {
    const token = this.getToken();
    if (!token) {
      console.error('Token non trovato');
      return throwError(() => new Error('Non autorizzato: Token non trovato'));
    }
    if (id !== customer.id) {
      console.error('ID non corrispondente');
      return throwError(() => new Error('Gli identificativi non corrispondono'));
    }
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
    return this.http.put<any>(
      `${this.baseApiUrl}/modify/${id}`,
      customer,
      { headers }
    );
  }

  resetPassword(email: string, newPassword: string, currentPassword?: string | null): Observable<any> {
    const token = this.getToken();
    if (!token) {
      console.error('Token non trovato');
      return throwError(() => new Error('Non autorizzato: Token non trovato'));
    }


    let params = new HttpParams()
      .set('email', email)
      .set('newPassword', newPassword);

    if (currentPassword) {
      params = params.set('currentPassword', currentPassword);
    }

    // Aggiungi il token come header
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
    return this.http.post<any>(`${this.baseApiUrl}/reset-password`, null, { params ,headers});
  }

  logout(): void {
    localStorage.removeItem('token');
  }
}
