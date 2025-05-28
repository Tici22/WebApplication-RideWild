import { Component } from '@angular/core';
import { CardComponent } from "../../layout/card/card.component";
import { AppFrameComponent } from "../../layout/app-frame/app-frame.component";
import { ProductListComponent } from "../../features/products/product-list/product-list.component";

@Component({
  selector: 'app-shop',
  imports: [CardComponent, AppFrameComponent, ProductListComponent],
  templateUrl: './shop.component.html',
  styleUrl: './shop.component.css'
})
export class ShopComponent {

}
