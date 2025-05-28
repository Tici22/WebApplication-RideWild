import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProductService } from '../../../services/product.service/product.service';
import { ProductDto } from '../../../models/product.models';
import { CardComponent } from '../../../layout/card/card.component';
import { Category } from '../../../models/Category';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterModule, CardComponent],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit {

  products: ProductDto[] = [];
  isLoading: boolean = false;
  currentPage: number = 1;
  readonly pageSize: number = 21;
  hasNextPage: boolean = true;
  

  // Altre variabili  

  constructor(private productService: ProductService) { }

  ngOnInit(): void {
    this.loadProductsForPage(this.currentPage);
  }

  loadProductsForPage(page: number): void {
    if (page < 1 || this.isLoading) {
      return;
    }

    this.isLoading = true;

    this.productService.getProducts(page, this.pageSize).subscribe({
      next: (data) => {
        console.log(`Dati ricevuti per pagina ${page}:`, data);
        if (data && data.length > 0) {
          this.products = data;
          this.currentPage = page;
          this.hasNextPage = data.length === this.pageSize;
        } else {
          if (page > 1) {
            this.currentPage = page - 1;
          } else {
            this.products = [];
          }
          this.hasNextPage = false;
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Errore API durante caricamento prodotti:', error);
        this.isLoading = false;
        this.products = [];
        this.hasNextPage = false;
      }
    });
  }

  goToPreviousPage(): void {
    if (this.currentPage > 1) {
      this.loadProductsForPage(this.currentPage - 1);
    }
  }


  goToNextPage(): void {
    if (this.hasNextPage) {
      this.loadProductsForPage(this.currentPage + 1);
    }
  }

  viewDetails(productId: number): void {
    console.log('Richiesto dettaglio per prodotto ID:', productId);
  }

  
}
