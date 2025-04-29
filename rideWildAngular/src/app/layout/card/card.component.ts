import { Component, Input } from '@angular/core';
import { ProductDto } from '../../models/product.models';
@Component({
  selector: 'app-card',
  imports: [],
  templateUrl: './card.component.html',
  styleUrl: './card.component.css'
})
export class CardComponent {
  @Input() product!: ProductDto;
}
