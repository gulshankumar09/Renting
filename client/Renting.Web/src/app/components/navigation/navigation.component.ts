import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-navigation',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.css'],
})
export class NavigationComponent implements OnInit {
  isLoggedIn = false;
  isHost = false;
  isDarkMode = false;

  ngOnInit(): void {
    // Check theme on component initialization
    this.isDarkMode = document.documentElement.classList.contains('dark');
  }

  toggleUserState(loggedIn: boolean, host: boolean = false): void {
    this.isLoggedIn = loggedIn;
    this.isHost = host;
  }

  handleLogout(): void {
    // Add logout logic here
    this.toggleUserState(false);
  }

  toggleTheme(): void {
    this.isDarkMode = !this.isDarkMode;
    const theme = this.isDarkMode ? 'dark' : 'light';

    // Call the theme switching function
    (window as any).setTheme(theme);
  }
}
