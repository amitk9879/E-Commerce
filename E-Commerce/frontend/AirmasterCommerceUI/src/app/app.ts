import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/components/navbar/navbar'; // Points to your navbar.ts

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent], // Registered here exactly like a .NET view engine reference
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('AirmasterCommerceUI');
}