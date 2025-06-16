import { Component } from '@angular/core';
import { AuthService } from '../../services/product.service/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
@Component({
  selector: 'app-forgot-password',
  imports: [CommonModule,FormsModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent {
  email: string = ''; // Inserisci l'indirizzo email
  message: string = ''; // Inserisci il messaggio di Check 

  constructor(private AuthService: AuthService, private router: Router) { }


  sendMail() {
    this.AuthService.forgotPassword(this.email).subscribe({
      next: (res) => {
        this.message = res;
        console.log('Password reset response:', res);
        this.router.navigate(["verify"]);
      },
      error: (err) => {
        this.message = err.error;
        console.error('Errore durante il recupero della password:', err);
      }
    });
  }

}
