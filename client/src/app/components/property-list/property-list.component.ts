import { Component, OnInit } from "@angular/core";
import { ApiService } from "../../services/api.service";
import { FormControl, FormGroup } from "@angular/forms";
import { debounceTime } from "rxjs/operators";

@Component({
  selector: "app-property-list",
  templateUrl: "./property-list.component.html",
  styleUrl: "./property-list.component.css",
})
export class PropertyListComponent implements OnInit {
  properties: any[] = [];
  filteredProperties: any[] = [];
  loading = true;
  error = "";

  propertyTypes = [
    { value: "", display: "All Types" },
    { value: "Hotel", display: "Hotel" },
    { value: "House", display: "House" },
    { value: "PG", display: "PG Accommodation" },
  ];

  filterForm = new FormGroup({
    location: new FormControl(""),
    propertyType: new FormControl(""),
    minPrice: new FormControl(""),
    maxPrice: new FormControl(""),
    bedrooms: new FormControl(""),
  });

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.loadProperties();

    // Apply filters when form values change
    this.filterForm.valueChanges.pipe(debounceTime(500)).subscribe(() => {
      this.applyFilters();
    });
  }

  loadProperties(): void {
    this.loading = true;
    this.apiService.getProducts().subscribe({
      next: (data) => {
        this.properties = data;
        this.filteredProperties = [...data];
        this.loading = false;
      },
      error: (err) => {
        this.error = "Failed to load properties. Please try again later.";
        this.loading = false;
        console.error("Error loading properties", err);
      },
    });
  }

  applyFilters(): void {
    const { location, propertyType, minPrice, maxPrice, bedrooms } =
      this.filterForm.value;

    this.filteredProperties = this.properties.filter((property) => {
      // Location filter
      if (
        location &&
        property.location.toLowerCase().indexOf(location.toLowerCase()) === -1
      ) {
        return false;
      }

      // Property type filter
      if (propertyType && property.type !== propertyType) {
        return false;
      }

      // Price range filter
      const price = parseFloat(property.price);
      if (minPrice && price < parseFloat(minPrice)) {
        return false;
      }
      if (maxPrice && price > parseFloat(maxPrice)) {
        return false;
      }

      // Bedrooms filter
      if (bedrooms && property.bedrooms !== parseInt(bedrooms)) {
        return false;
      }

      return true;
    });
  }

  resetFilters(): void {
    this.filterForm.reset();
    this.filteredProperties = [...this.properties];
  }
}
