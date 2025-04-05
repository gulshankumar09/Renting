import { Component, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pg-stays',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pg-stays.component.html',
  styleUrls: ['./pg-stays.component.css'],
})
export class PgStaysComponent {
  // Filter panel state
  showFilters = false;
  showAmenities = false;
  showFoodOptions = false;

  // Search form fields
  location = '';
  budget = '';
  roomType = '';

  // Filter options
  filters = {
    gender: '',
    occupancy: [],
    furnishing: '',
    propertyType: [] as string[],
  };

  // Amenities options
  amenities = {
    wifi: false,
    ac: false,
    tv: false,
    fridge: false,
    washingMachine: false,
    parking: false,
    securityGuard: false,
    cctv: false,
    powerBackup: false,
    gym: false,
    studyTable: false,
    lift: false,
  };

  // Food options
  foodOptions = {
    included: false,
    mealTypes: {
      breakfast: false,
      lunch: false,
      dinner: false,
    },
    preferences: {
      veg: false,
      nonVeg: false,
    },
  };

  // Close dropdowns when clicking outside
  @HostListener('document:click', ['$event'])
  clickOutside(event: Event): void {
    // Check if click target is outside the dropdown buttons
    const target = event.target as HTMLElement;

    // Don't close if clicking inside a dropdown
    if (target.closest('.filter-dropdown-content')) {
      return;
    }

    // Close filters if not clicking on its button
    if (this.showFilters && !target.closest('.filter-btn')) {
      this.showFilters = false;
    }

    // Close amenities if not clicking on its button
    if (this.showAmenities && !target.closest('.amenities-btn')) {
      this.showAmenities = false;
    }

    // Close food options if not clicking on its button
    if (this.showFoodOptions && !target.closest('.food-btn')) {
      this.showFoodOptions = false;
    }
  }

  toggleFilters(event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.showFilters = !this.showFilters;
    if (this.showFilters) {
      this.showAmenities = false;
      this.showFoodOptions = false;
    }
  }

  toggleAmenities(event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.showAmenities = !this.showAmenities;
    if (this.showAmenities) {
      this.showFilters = false;
      this.showFoodOptions = false;
    }
  }

  toggleFoodOptions(event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.showFoodOptions = !this.showFoodOptions;
    if (this.showFoodOptions) {
      this.showFilters = false;
      this.showAmenities = false;
    }
  }

  applySearch(): void {
    // Here you would implement the actual search functionality
    // This would typically involve making an API call with the selected criteria
    console.log('Searching with criteria:', {
      location: this.location,
      budget: this.budget,
      roomType: this.roomType,
      filters: this.filters,
      amenities: this.amenities,
      foodOptions: this.foodOptions,
    });

    // Close any open dropdowns
    this.showFilters = false;
    this.showAmenities = false;
    this.showFoodOptions = false;
  }

  resetFilters(): void {
    this.filters = {
      gender: '',
      occupancy: [],
      furnishing: '',
      propertyType: [],
    };
  }

  resetAmenities(): void {
    Object.keys(this.amenities).forEach((key) => {
      (this.amenities as Record<string, boolean>)[key] = false;
    });
  }

  resetFoodOptions(): void {
    this.foodOptions = {
      included: false,
      mealTypes: {
        breakfast: false,
        lunch: false,
        dinner: false,
      },
      preferences: {
        veg: false,
        nonVeg: false,
      },
    };
  }

  togglePropertyType(type: string): void {
    const index = this.filters.propertyType.indexOf(type);
    if (index === -1) {
      this.filters.propertyType.push(type);
    } else {
      this.filters.propertyType.splice(index, 1);
    }
  }

  isPropertyTypeSelected(type: string): boolean {
    return this.filters.propertyType.includes(type);
  }
}
