import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProductDto } from '../../models/product.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})

export class ProductService {

  private baseApiUrl = environment.apiUrl;
  private productsApiUrl = this.baseApiUrl + 'products'; 

  constructor(private http: HttpClient) { }

  getProducts(pageNumber: number = 1, pageSize: number = 20): Observable<ProductDto[]> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    console.log(`ProductService: Chiamata GET a ${this.productsApiUrl} con page=${pageNumber}, size=${pageSize}`);
    return this.http.get<ProductDto[]>(this.productsApiUrl, { params });
  }

  getProductById(id: number): Observable<ProductDto> {
    const url = `${this.productsApiUrl}/${id}`; 
    console.log(`ProductService: Chiamata GET a ${url}`);
    return this.http.get<ProductDto>(url);
  }

}