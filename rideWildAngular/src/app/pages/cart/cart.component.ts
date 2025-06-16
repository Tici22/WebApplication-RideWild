import { Component, OnInit } from '@angular/core';
import { CartService, CartItem } from '../../services/product.service/cart.service';
import { CommonModule } from '@angular/common';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
})
export class CartComponent implements OnInit {
  isLoading: boolean = false;
  isPaymentProcessing: boolean = false;
  cart: CartItem[] = [];
  showPaymentForm: boolean = false;
  paymentSuccess: boolean | null = null;
  paymentError: boolean | null = null;

  paymentForm!: FormGroup;

  constructor(private cartService: CartService) { }

  isCardFlipped: boolean = false;

  ngOnInit(): void {
    this.loadCart();
    this.initPaymentForm();

    this.paymentForm.get('cvv')?.valueChanges.subscribe(value => {
      this.isCardFlipped = value.length > 0;
    });
  }

  private initPaymentForm(): void {
    this.paymentForm = new FormGroup({
      cardNumber: new FormControl('', [
        Validators.required,
        Validators.pattern(/^\d{16}$/)
      ]),
      cardName: new FormControl('', Validators.required),
      expiryDate: new FormControl('', [
        Validators.required,
        Validators.pattern(/^(0[1-9]|1[0-2])\/?([0-9]{2})$/)
      ]),
      cvv: new FormControl('', [
        Validators.required,
        Validators.pattern(/^\d{3,4}$/)
      ])
    });
  }

  get maskedCardNumber(): string {
    const raw = this.paymentForm?.get('cardNumber')?.value || '';
    const filled = raw.padEnd(16, 'X').substring(0, 16);
    return filled.replace(/(.{4})/g, '$1 ').trim();
  }

  onCardMouseMove(event: MouseEvent) {
    const card = (event.currentTarget as HTMLElement);
    const bounds = card.getBoundingClientRect();
    const x = event.clientX - bounds.left;
    const y = event.clientY - bounds.top;
    const centerX = bounds.width / 2;
    const centerY = bounds.height / 2;
    const rotateX = -((y - centerY) / centerY) * 15;
    const rotateY = ((x - centerX) / centerX) * 15;

    card.style.transform = `rotateX(${rotateX}deg) rotateY(${rotateY}deg)`;
  }

  onCardMouseLeave() {
    const cards = document.querySelectorAll('.credit-card-display') as NodeListOf<HTMLElement>;
    cards.forEach(card => {
      card.style.transform = 'rotateX(0deg) rotateY(0deg)';
    });
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
      this.resetPaymentStatus();
    }, 500);
  }

  clearCart() {
    this.isLoading = true;
    setTimeout(() => {
      this.cartService.clearCart();
      this.loadCart();
      this.isLoading = false;
      this.showPaymentForm = false;
      this.resetPaymentStatus();
    }, 1000);
  }

  getTotalPrice(): number {
    return this.cartService.getTotalPrice();
  }

  getCount(): number {
    return this.cartService.getCount();
  }

  processPayment(): void {
    if (this.paymentForm.valid && this.cart.length > 0) {
      this.paymentSuccess = true;
      this.paymentError = false;

      setTimeout(() => {
        this.isPaymentProcessing = true;

        setTimeout(() => {
          this.cartService.clearCart();
          this.loadCart();
          this.paymentForm.reset();
          this.showPaymentForm = false;
          this.isPaymentProcessing = false;
        }, 2000);
      }, 2000);
    } else {
      this.paymentForm.markAllAsTouched();
      this.paymentError = true;
    }
  }

  private resetPaymentStatus(): void {
    this.paymentSuccess = null;
    this.paymentError = null;
  }
}
