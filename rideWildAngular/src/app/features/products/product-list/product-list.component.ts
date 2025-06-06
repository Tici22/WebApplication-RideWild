import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { ProductService } from '../../../services/product.service/product.service';
import { ProductDto } from '../../../models/product.models';
import { CardComponent } from '../../../layout/card/card.component';

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
  selectedCategory: string | null = null;
  constructor(private productService: ProductService, private route: ActivatedRoute) { }


  //Inizializza il componente e recupera i parametri della query
  // per la categoria selezionata, quindi carica i prodotti per la pagina iniziale
  ngOnInit(): void {
    this.route.queryParams.subscribe(params => { this.selectedCategory = params['category'] || null; 
    this.currentPage = 1;
    // Carica i prodotti per la pagina iniziale
    this.loadProductsForPage(this.currentPage);
    });
  }

  loadProductsForPage(page: number): void {
    if (page < 1 || this.isLoading) {
      // Se page < 1 o già in caricamento non fare nulla
      return;
    }

    this.isLoading = true;


    if (this.selectedCategory) {
      this.productService.ShowProductForCategory(this.selectedCategory).subscribe({
        next: (data: any) => { this.products = data.products; this.isLoading = false; },
        error: (error) => {
          console.error('Errore API durante caricamento prodotti per categoria:', error);
          this.isLoading = false;
          this.products = [];
        }

      });
    } else {
      this.productService.getProducts(page, this.pageSize).subscribe({
        next: (data) => {
          // Aspetta 2.5 secondi prima di aggiornare lo stato e la pagina,
          // così il loader resta visibile abbastanza a lungo
          setTimeout(() => {
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
          }, 1500);
        },
        error: (error) => {
          console.error('Errore API durante caricamento prodotti:', error);
          // Anche in caso di errore attendi 2.5 secondi prima di rimuovere il loader
          setTimeout(() => {
            this.isLoading = false;
            this.products = [];
            this.hasNextPage = false;
          }, 1500);
        }
      });
    }

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
