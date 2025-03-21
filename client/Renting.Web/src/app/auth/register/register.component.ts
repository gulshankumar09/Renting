import { Component, OnInit, inject, PLATFORM_ID } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  private platformId = inject(PLATFORM_ID);
  registerForm!: FormGroup;

  constructor(private fb: FormBuilder, private router: Router) {}

  ngOnInit(): void {
    this.initForm();
  }

  initForm(): void {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      fullName: ['', [Validators.required]],
      country: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      termsAgreed: [false, [Validators.requiredTrue]],
    });
  }

  get email() {
    return this.registerForm.get('email');
  }
  get fullName() {
    return this.registerForm.get('fullName');
  }
  get country() {
    return this.registerForm.get('country');
  }
  get password() {
    return this.registerForm.get('password');
  }
  get termsAgreed() {
    return this.registerForm.get('termsAgreed');
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      return;
    }

    console.log('Registration form submitted:', this.registerForm.value);

    // Mock registration functionality (to be replaced with real auth service)

    // Simulate successful registration - only in browser environment
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('isLoggedIn', 'true');
    }

    // Navigate to home page after successful registration
    this.router.navigate(['/']);
  }

  registerWithGoogle(): void {
    console.log('Register with Google');
  }

  registerWithApple(): void {
    console.log('Register with Apple');
  }
}
