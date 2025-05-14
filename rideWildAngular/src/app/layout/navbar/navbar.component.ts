import { Component, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})

export class NavbarComponent {
  isScrolled = false;
  isMenuOpen = false;
  cartCount = 0; 

  constructor(private router: Router) { }

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  goTo(path: string) {
    this.router.navigate([path]);
    this.isMenuOpen = false;
  }

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.isScrolled = window.scrollY > 50;
  }

}
