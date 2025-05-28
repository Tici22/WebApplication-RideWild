import { Routes } from '@angular/router';
import { AppFrameComponent } from './layout/app-frame/app-frame.component';
import { ProductListComponent } from './features/products/product-list/product-list.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { HomeComponent } from './pages/home/home.component';
import { ShopComponent } from './pages/shop/shop.component';

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
  { path: '**', redirectTo: '/home' }
];
