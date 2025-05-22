import { Component, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent {
  private lastScrollTop = 0;
  public hideNavbar = false;
  public isScrolled = false; // aggiunto

  constructor(private router: Router) { }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('fullname');
    this.router.navigate(['/home']);
    window.location.reload();
  }

  isLoggedIn(): boolean {
    return localStorage.getItem('token') !== null;
  }

  @HostListener('window:scroll', [])
  onWindowScroll(): void {
    const currentScroll = window.pageYOffset || document.documentElement.scrollTop;

    if (currentScroll < this.lastScrollTop) {
      this.hideNavbar = false;
      this.isScrolled = false;
    } else {
      this.isScrolled = currentScroll > 50;
      if (currentScroll > 80) {
        this.hideNavbar = true;
      }
    }

    this.lastScrollTop = currentScroll <= 0 ? 0 : currentScroll;
  }
}

