import { Component, OnInit } from '@angular/core';
import { ProductDto } from '../../models/product.models';
import { WishlistService } from '../../services/product.service/wishlist.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-wishlist',
  templateUrl: './wishlist.component.html',
  styleUrls: ['./wishlist.component.css'],
  standalone: true,
  imports: [CommonModule],
})
export class WishlistComponent implements OnInit {
  isLoading: boolean = false;
  wishlist: ProductDto[] = [];

  constructor(private wishlistService: WishlistService) { }

  ngOnInit(): void {
    this.loadWishlist();
  }

  private loadWishlist() {
    this.wishlist = this.wishlistService.getWishlist();
  }

  removeFromWishlist(productId: number) {
    this.isLoading = true;
    setTimeout(() => {
      this.wishlistService.removeProduct(productId);
      this.loadWishlist();
      this.isLoading = false;
    }, 1000);
  }

  clearWishlist() {
    this.isLoading = true;
    setTimeout(() => {
      this.wishlistService.clearWishlist();
      this.loadWishlist();
      this.isLoading = false;
    }, 1000);
  }

  getTotalPrice(): number {
    return this.wishlist.reduce((total, product) => total + product.listPrice, 0);
  }

  getCount(): number {
    return this.wishlistService.getCount();
  }
}
