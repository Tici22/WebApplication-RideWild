import { Component, OnInit } from '@angular/core';
import { CartService, CartItem } from '../../services/product.service/cart.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css'],
  standalone: true,
  imports: [CommonModule],
})
export class CartComponent implements OnInit {
  isLoading: boolean = false;
  cart: CartItem[] = [];

  constructor(private cartService: CartService) {}

  ngOnInit(): void {
    this.loadCart();
  }

  private loadCart() {
    this.cart = this.cartService.getCart();
  }

  removeFromCart(productId: number) {
    this.isLoading = true;
    setTimeout(() => {
      this.cartService.removeFromCart(productId);
      this.loadCart();
      this.isLoading = false;
    }, 500);
  }

  clearCart() {
    this.isLoading = true;
    setTimeout(() => {
      this.cartService.clearCart();
      this.loadCart();
      this.isLoading = false;
    }, 1000);
  }

  getTotalPrice(): number {
    return this.cartService.getTotalPrice();
  }

  getCount(): number {
    return this.cartService.getCount();
  }
}
