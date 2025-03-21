import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  constructor(private router: Router) {}

  // Check if user is logged in
  isLoggedIn(): boolean {
    return localStorage.getItem('isLoggedIn') === 'true';
  }

  // Login method (mock)
  login(email: string, password: string): Observable<boolean> {
    // This is a mock implementation. In a real app, you would call your API
    console.log(`Logging in with ${email} and password`);

    // Simulate successful login
    localStorage.setItem('isLoggedIn', 'true');
    return of(true);
  }

  // Register method (mock)
  register(userDetails: any): Observable<boolean> {
    // This is a mock implementation. In a real app, you would call your API
    console.log('Registering user:', userDetails);

    // Simulate successful registration
    localStorage.setItem('isLoggedIn', 'true');
    return of(true);
  }

  // Logout method
  logout(): void {
    localStorage.removeItem('isLoggedIn');
    this.router.navigate(['/auth/login']);
  }
}
