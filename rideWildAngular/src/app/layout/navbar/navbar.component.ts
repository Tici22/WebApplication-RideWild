import { Component, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router'; // importa anche Router

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent {
  isScrolled = false;
  isMenuOpen = false;
  cartCount = 0;

  constructor(private router: Router) {} // <-- inietta Router

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.isScrolled = window.scrollY > 50;
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('fullname');
    this.router.navigate(['/home']); // usa il router
    window.location.reload();
  }

  isLoggedIn(): boolean {
    return localStorage.getItem('token') !== null;
  }
}
