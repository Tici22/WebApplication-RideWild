import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  standalone: true,
  imports: [
    ReactiveFormsModule,
    CommonModule,
    FormsModule
  ],
})
export class RegisterComponent {
  registerForm: FormGroup;
  errorMessage: string = '';

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.registerForm = this.fb.group({
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

    this.http.post('https://localhost:7055/api/customers/register', data).subscribe({
      next: (res: any) => {
        console.log('Registrazione riuscita', res);
        this.errorMessage = '';
        // naviga o altro
      },
      error: (err) => {
        console.error('Errore nella registrazione:', err);
        if (err.error) {
          if (typeof err.error === 'string') {
            this.errorMessage = err.error;
          } else if (err.error.message) {
            this.errorMessage = err.error.message;
          } else {
            this.errorMessage = 'Errore nella registrazione: dati non validi.';
          }
        } else {
          this.errorMessage = 'Errore nella registrazione.';
        }
      }
    });
  }
}
