import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/product.service/auth.service';
import { Router } from '@angular/router';
import { RouterLink } from '@angular/router';


@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  email: string = '';
  password: string = '';
  errorMessage: string = '';
  successMessage: string = '';
  isLoading = false;


  constructor(private authService: AuthService, private router: Router) { }

  onSubmit(): void {
    this.errorMessage = '';
    this.successMessage = '';
    this.isLoading = true;

    this.authService.login(this.email, this.password).subscribe({
      next: (response) => {
        this.successMessage = response.message;
        localStorage.setItem('token', response.token);
        localStorage.setItem('fullname', response.fullname);
        setTimeout(() => {
          this.isLoading = false;
          this.router.navigate(['/home']);
        }, 3000);
      },

      error: (err) => {
        this.isLoading = false;          
        if (err.status === 401) {
          this.errorMessage = 'Email o password errati.';
        } else if (err.status === 404) {
          this.errorMessage = 'Utente non trovato.';
        } else {
          this.errorMessage = 'Errore di sistema, riprova più tardi.';
        }
      }
    });
  }
}