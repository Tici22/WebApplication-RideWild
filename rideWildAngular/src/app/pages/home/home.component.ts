import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';


declare var bootstrap: any;
declare var AOS: any;
declare var isloggedIn: any;

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']

})
export class HomeComponent {

  fullname: string | null = null;

  ngAfterViewInit(): void {
    const el = document.querySelector('#carouselExampleSlidesOnly');
    const carousel = new bootstrap.Carousel(el, {
      interval: 3000,
      ride: 'carousel',
      pause: false,
      wrap: true,  
    });

    AOS.init({
      duration: 1000,
    });
  }


  
  ngOnInit() {
    this.fullname = localStorage.getItem('fullname');
  }
}

