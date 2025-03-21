import { Component, OnInit, inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-navigation',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.css'],
})
export class NavigationComponent implements OnInit {
  private platformId = inject(PLATFORM_ID);
  isLoggedIn = false;
  isHost = false;
  isDarkMode = false;

  ngOnInit(): void {
    // Check theme on component initialization - only in browser environment
    if (isPlatformBrowser(this.platformId)) {
      this.isDarkMode = document.documentElement.classList.contains('dark');

      // Check if user is logged in
      this.isLoggedIn = localStorage.getItem('isLoggedIn') === 'true';

      // For demo purposes, randomly decide if the user is a host when logged in
      if (this.isLoggedIn) {
        this.isHost = Math.random() > 0.5;
      }
    }
  }

  toggleUserState(loggedIn: boolean, host: boolean = false): void {
    this.isLoggedIn = loggedIn;
    this.isHost = host;
  }

  handleLogout(): void {
    // Remove the login status from localStorage
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('isLoggedIn');
    }
    this.toggleUserState(false);
  }

  toggleTheme(): void {
    this.isDarkMode = !this.isDarkMode;
    const theme = this.isDarkMode ? 'dark' : 'light';

    // Call the theme switching function - only in browser environment
    if (isPlatformBrowser(this.platformId)) {
      (window as any).setTheme(theme);
    }
  }
}
