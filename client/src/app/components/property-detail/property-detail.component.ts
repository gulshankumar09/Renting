import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { ApiService } from "../../services/api.service";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { DateRange } from "@angular/material/datepicker";
import { MatSnackBar } from "@angular/material/snack-bar";

@Component({
  selector: "app-property-detail",
  templateUrl: "./property-detail.component.html",
  styleUrl: "./property-detail.component.css",
})
export class PropertyDetailComponent implements OnInit {
  property: any = null;
  loading = true;
  error = "";
  selectedImageIndex = 0;

  bookingForm: FormGroup;
  minDate = new Date();
  dateFilter = (d: Date | null): boolean => {
    // Prevent booking dates already reserved
    // This would need to fetch actual booking data
    return true;
  };

  constructor(
    private route: ActivatedRoute,
    private apiService: ApiService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.bookingForm = this.fb.group({
      checkInDate: ["", Validators.required],
      checkOutDate: ["", Validators.required],
      guestCount: [1, [Validators.required, Validators.min(1)]],
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      const id = params["id"];
      if (id) {
        this.loadProperty(id);
      }
    });
  }

  loadProperty(id: number): void {
    this.loading = true;
    this.apiService.getProduct(id).subscribe({
      next: (data) => {
        this.property = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = "Failed to load property details. Please try again later.";
        this.loading = false;
        console.error("Error loading property", err);
      },
    });
  }

  changeImage(index: number): void {
    this.selectedImageIndex = index;
  }

  getAmenityIcon(amenity: string): string {
    const iconMap: { [key: string]: string } = {
      Wifi: "wifi",
      Parking: "local_parking",
      Pool: "pool",
      "Air Conditioning": "ac_unit",
      Kitchen: "kitchen",
      TV: "tv",
      Washer: "local_laundry_service",
      Gym: "fitness_center",
      // Add more mappings as needed
    };

    return iconMap[amenity] || "check_circle";
  }

  calculateTotalPrice(): number {
    if (!this.property || !this.bookingForm.valid) {
      return 0;
    }

    const checkInDate = this.bookingForm.get("checkInDate")?.value;
    const checkOutDate = this.bookingForm.get("checkOutDate")?.value;

    if (!checkInDate || !checkOutDate) {
      return 0;
    }

    // Calculate number of days/nights
    const timeDiff = checkOutDate.getTime() - checkInDate.getTime();
    const nightCount = Math.ceil(timeDiff / (1000 * 3600 * 24));

    return nightCount * this.property.price;
  }

  onSubmit(): void {
    if (this.bookingForm.invalid) {
      return;
    }

    const booking = {
      productId: this.property.id,
      checkInDate: this.bookingForm.get("checkInDate")?.value,
      checkOutDate: this.bookingForm.get("checkOutDate")?.value,
      guestCount: this.bookingForm.get("guestCount")?.value,
      totalPrice: this.calculateTotalPrice(),
    };

    // Call API to create booking (would need to be implemented)
    // For now, just show a success message
    this.snackBar.open("Booking request submitted successfully!", "Close", {
      duration: 3000,
    });
  }
}
