import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {

  title = 'Dating App';
  http = inject(HttpClient);

  users: any;

  ngOnInit(): void {
    
    this.http.get('https://localhost:5001/api/users').subscribe({
      next: result => this.users = result,
      error: error => console.log(error),
      complete: () => console.log("completed")
    })
  }
    
}
