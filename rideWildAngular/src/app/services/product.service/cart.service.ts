import { Injectable } from '@angular/core';
import { ProductDto } from '../../models/product.models';

export interface CartItem {
  product: ProductDto;
  quantity: number;
}

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private cartItems: CartItem[] = [];

  getCart(): CartItem[] {
    return [...this.cartItems]; // copia per sicurezza
  }

  addItem(product: ProductDto): void {
    const existingItem = this.cartItems.find(item => item.product.productId === product.productId);
    if (existingItem) {
      existingItem.quantity++;
    } else {
      this.cartItems.push({ product, quantity: 1 });
    }
  }

  removeFromCart(productId: number): void {
    const existingItemIndex = this.cartItems.findIndex(item => item.product.productId === productId);
    if (existingItemIndex !== -1) {
      if (this.cartItems[existingItemIndex].quantity > 1) {
        this.cartItems[existingItemIndex].quantity--;
      } else {
        this.cartItems.splice(existingItemIndex, 1);
      }
    }
  }

  clearCart(): void {
    this.cartItems = [];
  }

  getCount(): number {
    return this.cartItems.reduce((sum, item) => sum + item.quantity, 0);
  }

  getTotalPrice(): number {
    return this.cartItems.reduce((total, item) => total + item.product.listPrice * item.quantity, 0);
  }
}
