import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../services/product.service/auth.service';
@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  standalone: true,
  imports: [
    ReactiveFormsModule,
    CommonModule,
    FormsModule,
  ],
})
export class RegisterComponent {
  registerForm: FormGroup;
  errorMessage: string = '';

  constructor(private fb: FormBuilder, private http: HttpClient, private authService: AuthService) {
    this.registerForm = this.fb.group({
      date: ['', Validators.required],
      fullname: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
      confermaPassword: ['', Validators.required],
    }, { validator: this.passwordMatchValidator });
  }

  passwordMatchValidator(group: FormGroup) {
    const pass = group.get('password')?.value;
    const confPass = group.get('confermaPassword')?.value;
    return pass === confPass ? null : { notMatching: true };
  }

  get f() {
    return this.registerForm.controls;
  }

  onSubmit() {
    if (this.registerForm.invalid) {
      this.errorMessage = 'Completa tutti i campi correttamente.';
      return;
    }
    if (this.registerForm.value.password !== this.registerForm.value.confermaPassword) {
      this.errorMessage = 'Le password non coincidono.';
      return;
    }

    

    const data = {
      email: this.registerForm.value.email,
      password: this.registerForm.value.password,
      fullname: this.registerForm.value.fullname,
      date: this.registerForm.value.date,
    };

    this.authService.register(data.email, data.password, data.fullname, data.date).subscribe({
      next: (response: any) => {
        console.log('Registrazione avvenuta con successo:', response);
        this.errorMessage = '';
        // Redirect or show success message
      },
      error: (error) => {
        console.error('Errore durante la registrazione:', error);
        if (error.status === 400) {
          this.errorMessage = 'Email già in uso.';
        } else {
          this.errorMessage = 'Errore di sistema, riprova più tardi.';
          // Fra se vuoi aggiungere un errore o altri fai pure
        }
      }
  });
  }
}