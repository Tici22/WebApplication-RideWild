import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/product.service/auth.service';
import { Router } from '@angular/router';
import { Customer } from '../../models/custmoer';
import { ReactiveFormsModule } from '@angular/forms';
import { JwtPayload } from '../../models/Util/jwt-payload';
@Component({
  selector: 'app-options',
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './options.component.html',
  styleUrl: './options.component.css'
})
export class OptionsComponent {

  profileForm: FormGroup; // FormGroup per il profilo utente
  userId: number | null = null; // ID dell'utente, inizialmente impostato a null
  errormessage: string = ''; // Messaggio di errore
  successMessage: string = ''; // Messaggio di successo

  // Inizializza il form con i campi richiesti
  constructor(private authService: AuthService,
    private formBuilder: FormBuilder,
    private router: Router
  ) {
    this.profileForm = this.formBuilder.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      companyName: [''],
      phone: ['']
    });
  }
  ngOnInit(): void {
  const profileDataFromToken: JwtPayload | null = this.authService.getUserIdFromToken();
  const fullNameFromToken = localStorage.getItem('fullname');
  // Verifica se il token contiene i dati del profilo
  if (
    profileDataFromToken &&
    profileDataFromToken.userId &&
    typeof profileDataFromToken.userId === 'string' &&
    profileDataFromToken.email &&
    typeof profileDataFromToken.email === 'string'
  ) {
    const userIdFromToken = Number(profileDataFromToken.userId);
    if (!isNaN(userIdFromToken)) {
      this.userId = userIdFromToken; // Imposta l'ID utente dal token
      console.log('ID utente recuperato dal token:', this.userId);
      // Popola il form con i dati del profilo
      this.profileForm.patchValue({
        email: profileDataFromToken.email,
        fullName: fullNameFromToken || '' // Usa fullName da localStorage
      });
      this.getUserInfo(); // Recupera le informazioni utente
    } else {
      console.error('ID utente non valido nel token:', profileDataFromToken.userId);
      this.errormessage = 'ID utente non valido nel token';
    }
  }
}

  getUserInfo() {
    if (this.userId !== null) {
      this.authService.getUserInfo(this.userId).subscribe({
        next: (response: Customer) => {
          this.profileForm.patchValue({
            fullName: response.fullname,
            email: response.email,
            companyName: response.companyName,
            phone: response.phone
          });

          console.log('Informazioni utente recuperate:', response);
        },
        error: (err: any) => {
          this.errormessage = 'Si è verificato un errore durante il recupero delle informazioni utente';
          console.error('Errore nel recupero delle informazioni utente:', err);
        }
      });
    }
  }



  onSubmit() {
    this.errormessage = '';
    this.successMessage = '';
    if (this.profileForm.valid && this.userId !== null) {
      const userUpdate: Customer = {
        id: this.userId,
        fullname: this.profileForm.value.fullName,
        email: this.profileForm.value.email,
        companyName: this.profileForm.value.companyName,
        phone: this.profileForm.value.phone
      };
      this.authService.updateCredentials(this.userId, userUpdate)
        .subscribe({
          next: (response: any) => {
            this.successMessage = 'Le credenziali sono state aggiornate con successo';
            console.log('Credenziali aggiornate:', response);
          },
          error: (error) => {
            this.errormessage = 'Si è verificato un errore durante l\'aggiornamento delle credenziali';
          }
        });
    } else {
      this.errormessage = 'Per favore, compila tutti i campi richiesti.';
      console.error('Form non valido o ID utente non impostato');
    }
  }
}
