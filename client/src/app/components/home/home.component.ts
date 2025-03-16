import { Component, OnInit } from "@angular/core";
import { ApiService } from "../../services/api.service";

@Component({
  selector: "app-home",
  templateUrl: "./home.component.html",
  styleUrl: "./home.component.css",
})
export class HomeComponent implements OnInit {
  featuredProperties: any[] = [];
  loading = true;
  error = "";

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.loadFeaturedProperties();
  }

  loadFeaturedProperties(): void {
    this.loading = true;
    this.apiService.getProducts().subscribe({
      next: (data) => {
        this.featuredProperties = data.slice(0, 6); // Get first 6 properties
        this.loading = false;
      },
      error: (err) => {
        this.error = "Failed to load properties. Please try again later.";
        this.loading = false;
        console.error("Error loading properties", err);
      },
    });
  }
}
