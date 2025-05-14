import { Component } from '@angular/core';
import { RouterModule } from '@angular/router'; 
import { NavbarComponent } from '../navbar/navbar.component'; 
import { FooterComponent } from '../footer/footer.component'; 
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-app-frame', 
  standalone: true,
  imports: [
    RouterModule,
    FooterComponent,
    CommonModule,
    NavbarComponent
  ],
  
  templateUrl: './app-frame.component.html',
  styleUrls: ['./app-frame.component.css']
})
export class AppFrameComponent { }