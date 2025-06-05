import { Routes } from '@angular/router';
import { ProductListComponent } from './features/products/product-list/product-list.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { HomeComponent } from './pages/home/home.component';
import { ShopComponent } from './pages/shop/shop.component';
import { WishlistComponent } from './pages/wishlist/wishlist.component';
import { OptionsComponent } from './pages/options/options.component';
import { CartComponent } from './pages/cart/cart.component';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'register',
    component: RegisterComponent
  },
  {
    path: 'home',
    component: HomeComponent
  },

  {
    path: 'shop',
    component: ShopComponent,
  },
  {
    path: 'cart',
    component: CartComponent
  },

  {
    path: 'details/:id',
    loadComponent: () =>
      import('./pages/details/details.component').then(m => m.DetailsComponent),
  },

  {
    path: 'wishlist',
    component: WishlistComponent
  },
  {
    path: 'options',
    component: OptionsComponent
  },
  {
    path: '',
    component: HomeComponent,
    children: [
      {
        path: '',
        component: ProductListComponent,
        pathMatch: 'full'
      }
    ]
  },
  { path: '**', redirectTo: '/home' },

];
