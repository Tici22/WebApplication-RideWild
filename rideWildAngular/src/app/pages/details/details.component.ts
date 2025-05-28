import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ProductService } from '../../services/product.service/product.service';
import { ProductDto } from '../../models/product.models';
import { WishlistService } from '../../services/product.service/wishlist.service';

@Component({
  selector: 'app-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './details.component.html',
  styleUrls: ['./details.component.css']
})
export class DetailsComponent implements OnInit {
  product!: ProductDto;

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    private wishlistService: WishlistService

  ) { }
  addToWishlist(product: ProductDto) {
    this.wishlistService.addProduct(product);
    alert(`"${product.name}" aggiunto alla lista desideri!`);
  }

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

    // --- Begin merged members from duplicate class ---
    zoomLevels = [1, 2, 3];  // livelli di zoom disponibili
    currentZoomIndex = 0;    // parte da 1x (no zoom)
    zoom = this.zoomLevels[this.currentZoomIndex];

    translateX = 0;
    translateY = 0;

    dragging = false;
    lastX = 0;
    lastY = 0;

    // dimensioni contenitore e immagine per calcolare limiti pan
    containerWidth = 0;
    containerHeight = 0;
    imageWidth = 300; // quella in css (width)
    imageHeight = 0;

    @ViewChild('productImage', { static: true }) productImage!: ElementRef<HTMLImageElement>;
    @ViewChild('imageContainer', { static: true }) imageContainer!: ElementRef<HTMLDivElement>;

    ngAfterViewInit() {
      // dopo il render, calcoliamo altezza immagine e contenitore
      this.containerWidth = this.imageContainer.nativeElement.clientWidth;
      this.containerHeight = this.imageContainer.nativeElement.clientHeight;

      const img = this.productImage.nativeElement;
      this.imageHeight = img.naturalHeight * (this.imageWidth / img.naturalWidth);
    }

    toggleZoom() {
      this.currentZoomIndex++;
      if (this.currentZoomIndex >= this.zoomLevels.length) {
        this.currentZoomIndex = 0;  // torna a no zoom
      }
      this.zoom = this.zoomLevels[this.currentZoomIndex];

      if (this.zoom === 1) {
        // reset posizione se togli zoom
        this.translateX = 0;
        this.translateY = 0;
      } else {
        // reset posizione all’attivazione zoom
        this.translateX = 0;
        this.translateY = 0;
      }
    }

    onMouseDown(event: MouseEvent) {
      if (this.zoom === 1) return;
      this.dragging = true;
      this.lastX = event.clientX;
      this.lastY = event.clientY;
    }

    onMouseMove(event: MouseEvent) {
      if (!this.dragging) return;

      let deltaX = event.clientX - this.lastX;
      let deltaY = event.clientY - this.lastY;

      this.lastX = event.clientX;
      this.lastY = event.clientY;

      // calcoliamo i limiti massimi di pan
      const maxTranslateX = (this.imageWidth * this.zoom - this.containerWidth) / 2;
      const maxTranslateY = (this.imageHeight * this.zoom - this.containerHeight) / 2;

      this.translateX += deltaX;
      this.translateY += deltaY;

      // limitiamo translateX
      if (this.translateX > maxTranslateX) this.translateX = maxTranslateX;
      if (this.translateX < -maxTranslateX) this.translateX = -maxTranslateX;

      // limitiamo translateY
      if (this.translateY > maxTranslateY) this.translateY = maxTranslateY;
      if (this.translateY < -maxTranslateY) this.translateY = -maxTranslateY;
    }

    onMouseUp() {
      this.dragging = false;
    }

    onMouseLeave() {
      this.dragging = false;
    }
    buyNow(product: ProductDto) {
      // Puoi implementare logica reale qui: es. navigare al checkout con id prodotto
      alert(`Procedi all'acquisto di "${product.name}"`);
    }
  }





