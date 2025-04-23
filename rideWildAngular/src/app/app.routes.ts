import { Routes } from '@angular/router';
import { AppFrameComponent } from './layout/app-frame/app-frame.component'; 
import { ProductListComponent } from './features/products/product-list/product-list.component';

export const routes: Routes = [
  {
    path: '', 
    component: AppFrameComponent, 
    children: [
      {
        path: '', 
        component: ProductListComponent, 
        pathMatch: 'full' 
      },
    ]
  },

  { path: '**', redirectTo: '' } 
];