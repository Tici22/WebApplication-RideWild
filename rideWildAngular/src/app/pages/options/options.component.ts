import { Component, ViewChild, ElementRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/product.service/auth.service';
import { Router } from '@angular/router';
import { Customer } from '../../models/custmoer';
import { ReactiveFormsModule } from '@angular/forms';
import { JwtPayload } from '../../models/Util/jwt-payload';

@Component({
  selector: 'app-options',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './options.component.html',
  styleUrls: ['./options.component.css']
})
export class OptionsComponent implements OnInit {

  @ViewChild('formContainer') formContainer!: ElementRef;

  profileForm: FormGroup;
  passwordForm: FormGroup;

  userId: number | null = null;

  // Messaggi per profilo
  profileErrorMessage: string = '';
  profileSuccessMessage: string = '';

  // Messaggi per password
  passwordErrorMessage: string = '';
  passwordSuccessMessage: string = '';

  loading: boolean = false;

  // Stato pannello reset password
  showPasswordPanel: boolean = false;

  constructor(
    private authService: AuthService,
    private formBuilder: FormBuilder,
    private router: Router
  ) {
    this.profileForm = this.formBuilder.group({
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      companyName: ['', Validators.minLength(2)],
      phone: ['', [
        Validators.pattern(/^[0-9\-\+\s]*$/),
        Validators.minLength(10),
        Validators.maxLength(13)
      ]],
    });

    this.passwordForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      currentPassword: [''], // opzionale
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  ngOnInit(): void {
    const profileDataFromToken: JwtPayload | null = this.authService.getUserIdFromToken();
    const fullNameFromToken = localStorage.getItem('fullname');

    if (profileDataFromToken && profileDataFromToken.userId && profileDataFromToken.email) {
      const userIdFromToken = Number(profileDataFromToken.userId);
      if (!isNaN(userIdFromToken)) {
        this.userId = userIdFromToken;

        this.profileForm.patchValue({
          email: profileDataFromToken.email,
          fullName: fullNameFromToken || ''
        });

        this.passwordForm.patchValue({
          email: profileDataFromToken.email
        });

        this.getUserInfo();
      } else {
        this.profileErrorMessage = 'ID utente non valido nel token';
      }
    } else {
      this.profileErrorMessage = 'Token non valido o mancante';
    }
  }

  getUserInfo(): void {
    if (this.userId !== null) {
      this.authService.getUserInfo(this.userId).subscribe({
        next: (response: Customer) => {
          this.profileForm.patchValue({
            fullName: response.fullname,
            email: response.email,
            companyName: response.companyName,
            phone: response.phone
          });
        },
        error: () => {
          this.profileErrorMessage = 'Errore nel recupero delle informazioni utente';
        }
      });
    }
  }

  onSubmitProfile(): void {
    this.profileErrorMessage = '';
    this.profileSuccessMessage = '';
    this.markAllTouched('profile');

    if (this.profileForm.valid && this.userId !== null) {
      const userUpdate: Customer = {
        id: this.userId,
        fullname: this.profileForm.value.fullName,
        email: this.profileForm.value.email,
        companyName: this.profileForm.value.companyName,
        phone: this.profileForm.value.phone
      };

      this.authService.updateCredentials(this.userId, userUpdate).subscribe({
        next: () => {
          this.profileSuccessMessage = 'Le credenziali sono state aggiornate con successo';
          const el = this.formContainer.nativeElement;
          el.style.animation = 'fadeSlideOut 0.6s ease forwards';
          setTimeout(() => {
            this.router.navigate(['/home']);
          }, 600);
        },
        error: () => {
          this.profileErrorMessage = 'Errore durante l\'aggiornamento delle credenziali';
        }
      });
    } else {
      this.profileErrorMessage = 'Completa correttamente tutti i campi richiesti.';
    }
  }

  onSubmitPassword(): void {
    this.passwordErrorMessage = '';
    this.passwordSuccessMessage = '';
    this.markAllTouched('password');

    if (this.passwordForm.valid) {
      const email = this.passwordForm.value.email;
      const currentPassword = this.passwordForm.value.currentPassword || null;
      const newPassword = this.passwordForm.value.newPassword;

      this.authService.resetPassword(email, newPassword, currentPassword).subscribe({
        next: (res) => {
          this.passwordSuccessMessage = res.message || 'Password aggiornata con successo.';
          this.passwordForm.reset({ email }); 
        },
        error: (err) => {
          this.passwordErrorMessage = err.error || 'Errore durante il cambio password.';
        }
      });
    } else {
      this.passwordErrorMessage = 'Completa correttamente tutti i campi per la password.';
    }
  }

  togglePasswordPanel(): void {
    this.showPasswordPanel = !this.showPasswordPanel;
  }

  markAsTouched(controlName: string, formType: 'profile' | 'password'): void {
    const control = formType === 'profile' ? this.profileForm.get(controlName) : this.passwordForm.get(controlName);
    if (control && !control.touched) {
      control.markAsTouched();
    }
  }

  markAllTouched(formType: 'profile' | 'password'): void {
    const form = formType === 'profile' ? this.profileForm : this.passwordForm;
    Object.keys(form.controls).forEach(name => {
      form.controls[name].markAsTouched();
    });
  }

  isInvalidAndTouched(controlName: string, formType: 'profile' | 'password'): boolean {
    const form = formType === 'profile' ? this.profileForm : this.passwordForm;
    const control = form.get(controlName);
    return !!(control && control.invalid && control.touched);
  }

  getErrorMessage(controlName: string, formType: 'profile' | 'password'): string {
    const form = formType === 'profile' ? this.profileForm : this.passwordForm;
    const control = form.get(controlName);
    if (!control) return '';

    if (control.errors) {
      if (control.errors['required']) return 'Campo obbligatorio';
      if (control.errors['email']) return 'Formato email non valido';
      if (control.errors['minlength']) return `Minimo ${control.errors['minlength'].requiredLength} caratteri richiesti`;
      if (control.errors['maxlength']) return `Massimo ${control.errors['maxlength'].requiredLength} caratteri consentiti`;
      if (control.errors['pattern']) {
        if (controlName === 'phone') return 'Numero di telefono non valido';
        return 'Valore non valido';
      }
    }
    return '';
  }
}
