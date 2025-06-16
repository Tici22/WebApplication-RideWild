import { Component } from '@angular/core';
import { AuthService } from '../../services/product.service/auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-change-password',
  imports: [CommonModule, FormsModule],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.css'
})
export class ChangePasswordComponent {
  email: string = '';
  password: string = '';
  code: string = '';
  fullname: string = '';
  message: string = '';
  constructor(private authService: AuthService, private router: Router) { }



  checkCredentials() {
    this.authService.verifyCode(
      this.email,
      this.code,
      this.password,
      this.fullname
    ).subscribe({
      next: (res) => {
        this.message = res;
        console.log('Password reset response:', res);

      },
      error: (err) => {
        this.message = err.error;
        console.error('Errore durante il recupero della password:', err);
      }
    });
  }

  countdown: string = '05:00';
  private intervalId: any;

  ngOnInit(): void {
    this.startCountdown();
  }

  startCountdown() {
    let timeLeft = 5 * 60;
    this.updateTimerDisplay(timeLeft);

    this.intervalId = setInterval(() => {
      timeLeft--;
      this.updateTimerDisplay(timeLeft);

      if (timeLeft <= 0) {
        clearInterval(this.intervalId);
      }
    }, 1000);
  }

  updateTimerDisplay(seconds: number) {
    const m = Math.floor(seconds / 60).toString().padStart(2, '0');
    const s = (seconds % 60).toString().padStart(2, '0');
    this.countdown = `${m}:${s}`;
  }
}
