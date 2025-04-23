import { Component } from '@angular/core';
import { RouterModule } from '@angular/router'; 
import { HeaderComponent } from '../header/header.component'; 
import { FooterComponent } from '../footer/footer.component'; 

@Component({
  selector: 'app-app-frame', 
  standalone: true,
  imports: [
    RouterModule, 
    HeaderComponent,
    FooterComponent
  ],
  templateUrl: './app-frame.component.html',
  styleUrls: ['./app-frame.component.css']
})
export class AppFrameComponent { }