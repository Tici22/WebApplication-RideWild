import { ProductDto } from './product.models';

export interface CartItem {
  product: ProductDto;
  quantity: number;
}
