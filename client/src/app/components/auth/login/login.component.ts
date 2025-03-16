import { Component } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { AuthService } from "../../../services/auth.service";
import { MatSnackBar } from "@angular/material/snack-bar";

@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrl: "./login.component.css",
})
export class LoginComponent {
  loginForm: FormGroup;
  loading = false;
  hidePassword = true;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.loginForm = this.fb.group({
      email: ["", [Validators.required, Validators.email]],
      password: ["", [Validators.required, Validators.minLength(6)]],
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      return;
    }

    this.loading = true;

    const credentials = {
      email: this.loginForm.get("email")?.value,
      password: this.loginForm.get("password")?.value,
    };

    this.authService.login(credentials).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(["/"]);
      },
      error: (error) => {
        this.loading = false;
        this.snackBar.open(
          error?.error?.message ||
            "Login failed. Please check your credentials.",
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
