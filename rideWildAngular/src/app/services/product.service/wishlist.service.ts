import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { ProductDto } from '../../models/product.models';
@Injectable({
  providedIn: 'root',
})
export class WishlistService {
  private wishlist: ProductDto[] = [];

  private wishlistCountSubject = new BehaviorSubject<number>(0);
  wishlistCount$ = this.wishlistCountSubject.asObservable();

  addProduct(product: ProductDto): void {
    if (!this.wishlist.find(p => p.productId === product.productId)) {
      this.wishlist.push(product);
      this.wishlistCountSubject.next(this.wishlist.length);
    }
  }

  removeProduct(productId: number): void {
    this.wishlist = this.wishlist.filter(p => p.productId !== productId);
    this.wishlistCountSubject.next(this.wishlist.length);
  }

  getWishlist(): ProductDto[] {
    return [...this.wishlist];
  }

  getCount(): number {
    return this.wishlist.length;
  }

  clearWishlist(): void {
    this.wishlist = [];
    this.wishlistCountSubject.next(0);
  }
}
