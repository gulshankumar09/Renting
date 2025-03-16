import { Component } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { AuthService } from "../../../services/auth.service";
import { MatSnackBar } from "@angular/material/snack-bar";

@Component({
  selector: "app-register",
  templateUrl: "./register.component.html",
  styleUrl: "./register.component.css",
})
export class RegisterComponent {
  registerForm: FormGroup;
  loading = false;
  hidePassword = true;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.registerForm = this.fb.group({
      name: ["", [Validators.required]],
      email: ["", [Validators.required, Validators.email]],
      password: ["", [Validators.required, Validators.minLength(6)]],
      phoneNumber: ["", [Validators.pattern(/^\+?[0-9\s-]{10,15}$/)]],
      isHost: [false],
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      return;
    }

    this.loading = true;

    const user = {
      name: this.registerForm.get("name")?.value,
      email: this.registerForm.get("email")?.value,
      password: this.registerForm.get("password")?.value,
      phoneNumber: this.registerForm.get("phoneNumber")?.value,
      isHost: this.registerForm.get("isHost")?.value,
    };

    this.authService.register(user).subscribe({
      next: () => {
        this.loading = false;
        this.snackBar.open("Registration successful! Please log in.", "Close", {
          duration: 5000,
        });
        this.router.navigate(["/auth/login"]);
      },
      error: (error) => {
        this.loading = false;
        this.snackBar.open(
          error?.error?.message || "Registration failed. Please try again.",
          "Close",
          {
            duration: 5000,
            panelClass: ["error-snackbar"],
          }
        );
      },
    });
  }
}
