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
      .set('date', date)

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


  // Recupera i dati del profilo utente dal token JWT

  getUserIdFromToken(): JwtPayload | null {
    const token = this.getToken();
    if (!token) {
      console.warn('AuthService: getUserProfileDataFromToken - Nessun token trovato.');
      return null;
    }

    try {
      const decodedToken: any = jwtDecode(token);

      console.log('AuthService: Payload del token decodificato:', decodedToken);

      // Cerca sia nameidentifier che nameIdentifier per compatibilità
      const userIdClaim = decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
      const email = decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"];

      if (typeof userIdClaim !== 'string' || typeof email !== 'string') {
        console.error('AuthService: Il token decodificato non contiene i claim "nameidentifier" o "email" come stringhe valide.');
        return null;
      }

      const userId = Number(userIdClaim);
      if (isNaN(userId)) {
        console.warn('AuthService: Il claim "nameidentifier" nel token non è un numero valido:', userIdClaim);
        return null;
      }

      console.log('AuthService: ID utente estratto:', userId);
      console.log('AuthService: Email utente estratta:', email);

      // Puoi restituire un oggetto JwtPayload personalizzato se vuoi
      return {
        userId: userIdClaim,
        email: email,
        exp: decodedToken.exp

      } as JwtPayload;
    } catch (error) {
      console.error('AuthService: Errore durante la decodifica del token JWT:', error);
      return null;
    }
  }



  //recupera le informazioni dell'utente
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
    console.log('URL per il recupero delle informazioni utente:', url);
    return this.http.get<Customer>(url, { headers }).pipe(
      catchError(error => {
        if (error.status === 404) {
          console.error('Utente non trovato:', error);
          return throwError(() => new Error('Utente non trovato'));
        }
        return throwError(() => error)
      })
    );
  }



  // Modifica le credenziali dell'utente
  updateCredentials(id: number, customer: Customer): Observable<any> {
    const token = this.getToken();
    if (!token) {
      console.error('Token non trovato');
      throw new Error('Non autorizzato:Token non trovato');
    }
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
    if (id !== customer.id) {
      console.error('ID non corrispondente');
      throw new Error('Gli identificativi non corrispondono');
    }
    return this.http.put<any>(
      `${this.baseApiUrl}/modify/${id}`,
      customer,
      { headers }
    );
  }
}
