import { Component, ElementRef, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../services/product.service/product.service';
import { WishlistService } from '../../services/product.service/wishlist.service';
import { CartService } from '../../services/product.service/cart.service';
import { ProductDto } from '../../models/product.models';

@Component({
  selector: 'app-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './details.component.html',
  styleUrls: ['./details.component.css']
})
export class DetailsComponent implements OnInit, AfterViewInit {
  product!: ProductDto;
  message: string = '';

  quantity: number = 1;

  zoomLevels = [1, 2, 3];
  currentZoomIndex = 0;
  zoom = this.zoomLevels[this.currentZoomIndex];

  translateX = 0;
  translateY = 0;

  dragging = false;
  lastX = 0;
  lastY = 0;

  containerWidth = 0;
  containerHeight = 0;
  imageWidth = 300;
  imageHeight = 0;

  @ViewChild('productImage', { static: true }) productImage!: ElementRef<HTMLImageElement>;
  @ViewChild('imageContainer', { static: true }) imageContainer!: ElementRef<HTMLDivElement>;

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    private wishlistService: WishlistService,
    private cartService: CartService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.productService.getProductById(id).subscribe({
      next: (data) => {
        this.product = data;
      },
      error: (err) => {
        console.error('Errore nel recupero del prodotto:', err);
      }
    });
  }

  ngAfterViewInit(): void {
    this.containerWidth = this.imageContainer.nativeElement.clientWidth;
    this.containerHeight = this.imageContainer.nativeElement.clientHeight;

    const img = this.productImage.nativeElement;
    this.imageHeight = img.naturalHeight * (this.imageWidth / img.naturalWidth);
  }

  toggleZoom(): void {
    this.currentZoomIndex++;
    if (this.currentZoomIndex >= this.zoomLevels.length) {
      this.currentZoomIndex = 0;
    }
    this.zoom = this.zoomLevels[this.currentZoomIndex];
    this.translateX = 0;
    this.translateY = 0;
  }

  onMouseDown(event: MouseEvent): void {
    if (this.zoom === 1) return;
    this.dragging = true;
    this.lastX = event.clientX;
    this.lastY = event.clientY;
  }

  onMouseMove(event: MouseEvent): void {
    if (!this.dragging) return;

    const deltaX = event.clientX - this.lastX;
    const deltaY = event.clientY - this.lastY;

    this.lastX = event.clientX;
    this.lastY = event.clientY;

    const maxTranslateX = (this.imageWidth * this.zoom - this.containerWidth) / 2;
    const maxTranslateY = (this.imageHeight * this.zoom - this.containerHeight) / 2;

    this.translateX = Math.max(-maxTranslateX, Math.min(this.translateX + deltaX, maxTranslateX));
    this.translateY = Math.max(-maxTranslateY, Math.min(this.translateY + deltaY, maxTranslateY));
  }

  onMouseUp(): void {
    this.dragging = false;
  }

  onMouseLeave(): void {
    this.dragging = false;
  }

  goBack(): void {
    this.router.navigate(['/shop']);
  }

  addToWishlist(product: ProductDto): void {
    this.wishlistService.addProduct(product);
    this.showMessage(`"${product.name}" aggiunto alla lista desideri!`);
  }

  buyNow(product: ProductDto): void {
    for (let i = 0; i < this.quantity; i++) {
      this.cartService.addItem(product);
    }
    this.showMessage(`Hai aggiunto ${this.quantity} x "${product.name}" al carrello.`);
  }

  increaseQuantity(): void {
    this.quantity++;
  }

  decreaseQuantity(): void {
    if (this.quantity > 1) {
      this.quantity--;
    }
  }

  private showMessage(msg: string): void {
    this.message = msg;
    setTimeout(() => {
      this.message = '';
    }, 3000);
  }
}
