import { Component, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../services/product.service/product.service';
import { Category } from '../../models/Category';
import { WishlistService } from '../../services/product.service/wishlist.service';
import { CartService } from '../../services/product.service/cart.service';

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
  public isScrolled = false;
  Categories: Category[] = [];
  wishlistCount: number = 0;

  constructor(private router: Router, private productService: ProductService, private wishlistService: WishlistService, private cartService: CartService) { }


  ngOnInit(): void {
    this.ShowCategoryProducts();

    this.wishlistService.wishlistCount$.subscribe(count => {
      this.wishlistCount = count;
    });

    }
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

      if(currentScroll <this.lastScrollTop) {
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

  ShowCategoryProducts(): void {
    this.productService.getProductsByCategory().subscribe({
      next: (data) => {
        console.log('Dati ricevuti per categoria ID => ', data);
        this.Categories = data;
        if (this.Categories && this.Categories.length > 0) {
          this.Categories.forEach(category => {
            console.log(`Categoria: ${category.macroCategoryName}`);
            category.subCategories.forEach(subCategory => {
              console.log(`  Sottocategoria: ${subCategory.categoryName}`);
            });
          });
        }
      }, error: (error) => {
        console.error('Errore durante il caricamento delle categorie:', error);
      }
    });
  }
}

