import { Component, Input } from '@angular/core';
import { ProductDto } from '../../models/product.models';
import { Router } from '@angular/router';

@Component({
  selector: 'app-card',
  templateUrl: './card.component.html',
  styleUrls: ['./card.component.css']
})
export class CardComponent {
  @Input() product!: ProductDto;

  constructor(private router: Router) { }

  viewDetails(id: number) {
    this.router.navigate(['/details', id]);
  }
}
