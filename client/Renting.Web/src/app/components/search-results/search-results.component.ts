import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

interface PgListing {
  id: string;
  name: string;
  location: string;
  price: number;
  rating: number;
  reviewCount: number;
  imageUrl: string;
  type: string;
  amenities: string[];
  foodIncluded: boolean;
  gender: string;
}

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './search-results.component.html',
  styleUrls: ['./search-results.component.css'],
})
export class SearchResultsComponent implements OnInit {
  // Search query params
  searchParams = {
    location: '',
    budget: '',
    roomType: '',
    gender: '',
    amenities: [] as string[],
    foodIncluded: false,
    rating: '',
  };

  // Sort options
  sortOptions = [
    { value: 'price-low', label: 'Price: Low to High' },
    { value: 'price-high', label: 'Price: High to Low' },
    { value: 'rating', label: 'Rating' },
    { value: 'relevance', label: 'Relevance' },
  ];

  selectedSort = 'relevance';

  // Filter panel states
  showFilters = false;
  isFilterApplied = false;
  showGenderDropdown = false;
  showAmenitiesDropdown = false;

  // Individual amenity filters
  amenityWifi = false;
  amenityAC = false;
  amenityTV = false;
  amenityGym = false;
  amenityWashing = false;

  // Filter section expansion states
  expandedSections: { [key: string]: boolean } = {
    budget: true,
    roomType: false,
    gender: false,
    amenities: false,
    food: false,
    rating: false,
  };

  // View mode (grid or map)
  viewMode = 'list'; // default to list view

  // Pagination
  totalListings = 0;
  currentPage = 1;
  itemsPerPage = 9;
  totalPages = 5;

  // Results
  pgListings: PgListing[] = [];
  loading = true;

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    // Get search parameters from route query params
    this.route.queryParams.subscribe((params) => {
      if (params['location']) {
        this.searchParams.location = params['location'];
      }
      if (params['budget']) {
        this.searchParams.budget = params['budget'];
      }
      if (params['roomType']) {
        this.searchParams.roomType = params['roomType'];
      }
      if (params['gender']) {
        this.searchParams.gender = params['gender'];
      }
      if (params['foodIncluded']) {
        this.searchParams.foodIncluded = params['foodIncluded'] === 'true';
      }

      // Simulate loading data
      this.fetchPgListings();
    });
  }

  @HostListener('document:click', ['$event'])
  handleClickOutside(event: MouseEvent): void {
    // Close dropdowns when clicking outside
    if (this.showGenderDropdown || this.showAmenitiesDropdown) {
      const target = event.target as HTMLElement;
      const isGenderDropdown = target.closest('.gender-dropdown-container');
      const isAmenitiesDropdown = target.closest(
        '.amenities-dropdown-container'
      );

      if (!isGenderDropdown) {
        this.showGenderDropdown = false;
      }

      if (!isAmenitiesDropdown) {
        this.showAmenitiesDropdown = false;
      }
    }
  }

  fetchPgListings(): void {
    // This would be an API call in a real application
    this.loading = true;

    // Simulate API delay
    setTimeout(() => {
      this.pgListings = this.getMockListings();
      this.totalListings = this.pgListings.length;
      this.loading = false;

      // Apply filters to the loaded listings
      this.applyFiltersToListings();
    }, 800);
  }

  applyFiltersToListings(): void {
    // Create a copy of all listings
    const allListings = this.getMockListings();

    // Filter the listings based on selected criteria
    this.pgListings = allListings.filter((pg) => {
      // Filter by location if specified
      if (
        this.searchParams.location &&
        !pg.location
          .toLowerCase()
          .includes(this.searchParams.location.toLowerCase())
      ) {
        return false;
      }

      // Filter by gender if specified
      if (this.searchParams.gender && pg.gender !== this.searchParams.gender) {
        return false;
      }

      // Filter by food included
      if (this.searchParams.foodIncluded && !pg.foodIncluded) {
        return false;
      }

      // Filter by room type if specified
      if (
        this.searchParams.roomType &&
        pg.type !== this.searchParams.roomType
      ) {
        return false;
      }

      // Filter by budget if specified
      if (this.searchParams.budget) {
        const budgetRange = this.searchParams.budget.split('-');
        if (budgetRange.length === 2) {
          const minBudget = parseInt(budgetRange[0]);
          const maxBudget = parseInt(budgetRange[1]);
          if (pg.price < minBudget || pg.price > maxBudget) {
            return false;
          }
        } else if (this.searchParams.budget === '20000+') {
          if (pg.price < 20000) {
            return false;
          }
        }
      }

      // Filter by rating if specified
      if (this.searchParams.rating) {
        const ratingFilter = this.searchParams.rating;
        if (ratingFilter === '4+' && pg.rating < 4.0) {
          return false;
        } else if (ratingFilter === '3+' && pg.rating < 3.0) {
          return false;
        }
      }

      // Filter by amenities
      const selectedAmenities = [];
      if (this.amenityWifi) selectedAmenities.push('WiFi');
      if (this.amenityAC) selectedAmenities.push('AC');
      if (this.amenityTV) selectedAmenities.push('TV');
      if (this.amenityGym) selectedAmenities.push('Gym');
      if (this.amenityWashing) selectedAmenities.push('Washing Machine');

      if (selectedAmenities.length > 0) {
        for (const amenity of selectedAmenities) {
          if (!pg.amenities.includes(amenity)) {
            return false;
          }
        }
      }

      return true;
    });

    // Sort the listings
    this.sortListings();

    this.totalListings = this.pgListings.length;
    this.totalPages = Math.ceil(this.totalListings / this.itemsPerPage);

    // Set the filter applied flag for UI updates
    this.isFilterApplied = true;
  }

  sortListings(): void {
    switch (this.selectedSort) {
      case 'price-low':
        this.pgListings.sort((a, b) => a.price - b.price);
        break;
      case 'price-high':
        this.pgListings.sort((a, b) => b.price - a.price);
        break;
      case 'rating':
        this.pgListings.sort((a, b) => b.rating - a.rating);
        break;
      case 'relevance':
      default:
        // Relevance sorting would be more complex in a real app
        break;
    }
  }

  toggleFilters(): void {
    this.showFilters = !this.showFilters;
  }

  toggleGenderDropdown(): void {
    this.showGenderDropdown = !this.showGenderDropdown;
    if (this.showGenderDropdown) {
      this.showAmenitiesDropdown = false;
    }
  }

  toggleAmenitiesDropdown(): void {
    this.showAmenitiesDropdown = !this.showAmenitiesDropdown;
    if (this.showAmenitiesDropdown) {
      this.showGenderDropdown = false;
    }
  }

  toggleFoodIncluded(): void {
    this.searchParams.foodIncluded = !this.searchParams.foodIncluded;
  }

  applyFilters(): void {
    // Collect amenities into an array
    const selectedAmenities: string[] = [];
    if (this.amenityWifi) selectedAmenities.push('WiFi');
    if (this.amenityAC) selectedAmenities.push('AC');
    if (this.amenityTV) selectedAmenities.push('TV');
    if (this.amenityGym) selectedAmenities.push('Gym');
    if (this.amenityWashing) selectedAmenities.push('Washing Machine');

    // Update searchParams with selected amenities
    this.searchParams.amenities = selectedAmenities;

    this.isFilterApplied = true;
    this.showFilters = false;
    this.applyFiltersToListings();
  }

  resetFilters(): void {
    this.searchParams = {
      location: this.searchParams.location,
      budget: '',
      roomType: '',
      gender: '',
      amenities: [],
      foodIncluded: false,
      rating: '',
    };
    this.amenityWifi = false;
    this.amenityAC = false;
    this.amenityTV = false;
    this.amenityGym = false;
    this.amenityWashing = false;
    this.isFilterApplied = false;
    this.fetchPgListings();
  }

  changePage(page: number): void {
    this.currentPage = page;
    // In a real app, this would fetch the new page of results
  }

  handleSort(): void {
    // This would re-fetch or re-sort the results
    this.sortListings();
  }

  getPageNumbers(): number[] {
    const totalPages = this.totalPages;
    const currentPage = this.currentPage;
    let pageNumbers: number[] = [];

    if (totalPages <= 5) {
      // Less than 5 pages, show all
      pageNumbers = Array.from({ length: totalPages }, (_, i) => i + 1);
    } else if (currentPage <= 3) {
      // Close to start
      pageNumbers = [1, 2, 3, 4, 5];
    } else if (currentPage >= totalPages - 2) {
      // Close to end
      pageNumbers = [
        totalPages - 4,
        totalPages - 3,
        totalPages - 2,
        totalPages - 1,
        totalPages,
      ];
    } else {
      // In the middle
      pageNumbers = [
        currentPage - 2,
        currentPage - 1,
        currentPage,
        currentPage + 1,
        currentPage + 2,
      ];
    }

    return pageNumbers;
  }

  getRandomPosition(index: number, type: 'top' | 'left'): string {
    // Generate pseudo-random positions for map markers
    const seed = index * 17;
    if (type === 'top') {
      return `${10 + ((seed * 7) % 80)}%`;
    } else {
      return `${15 + ((seed * 13) % 70)}%`;
    }
  }

  // Mock data for demonstration
  private getMockListings(): PgListing[] {
    return [
      {
        id: '1',
        name: 'Cozy Single Room PG',
        location: 'Koramangala, Bangalore',
        price: 8000,
        rating: 4.8,
        reviewCount: 42,
        imageUrl:
          'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=2070&q=80',
        type: 'Single Sharing',
        amenities: ['WiFi', 'AC', 'TV', 'Washing Machine'],
        foodIncluded: true,
        gender: 'male',
      },
      {
        id: '2',
        name: 'Deluxe Double Sharing PG',
        location: 'HSR Layout, Bangalore',
        price: 12000,
        rating: 4.6,
        reviewCount: 28,
        imageUrl:
          'https://images.unsplash.com/photo-1540518614846-7eded433c457?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=2057&q=80',
        type: 'Double Sharing',
        amenities: ['WiFi', 'AC', 'TV', 'Gym', 'Parking'],
        foodIncluded: true,
        gender: 'female',
      },
      {
        id: '3',
        name: 'Premium Triple Sharing PG',
        location: 'Indiranagar, Bangalore',
        price: 10500,
        rating: 4.9,
        reviewCount: 56,
        imageUrl:
          'https://images.unsplash.com/photo-1560448204-603b3fc33ddc?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=2070&q=80',
        type: 'Triple Sharing',
        amenities: ['WiFi', 'AC', 'Fridge', 'CCTV', 'Power Backup'],
        foodIncluded: false,
        gender: 'unisex',
      },
      {
        id: '4',
        name: 'Economy Single Room',
        location: 'Electronic City, Bangalore',
        price: 7000,
        rating: 4.2,
        reviewCount: 34,
        imageUrl:
          'https://images.unsplash.com/photo-1513694203232-719a280e022f?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=2069&q=80',
        type: 'Single Sharing',
        amenities: ['WiFi', 'TV', 'Security Guard'],
        foodIncluded: true,
        gender: 'male',
      },
      {
        id: '5',
        name: 'Luxury PG Accommodation',
        location: 'Whitefield, Bangalore',
        price: 15000,
        rating: 4.7,
        reviewCount: 62,
        imageUrl:
          'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=2080&q=80',
        type: 'Double Sharing',
        amenities: ['WiFi', 'AC', 'TV', 'Gym', 'Swimming Pool', 'Parking'],
        foodIncluded: true,
        gender: 'female',
      },
      {
        id: '6',
        name: 'Affordable PG Stay',
        location: 'Marathahalli, Bangalore',
        price: 6500,
        rating: 4.0,
        reviewCount: 18,
        imageUrl:
          'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=2070&q=80',
        type: 'Triple Sharing',
        amenities: ['WiFi', 'Security Guard', 'Power Backup'],
        foodIncluded: false,
        gender: 'male',
      },
      {
        id: '7',
        name: 'Central City PG',
        location: 'MG Road, Bangalore',
        price: 13000,
        rating: 4.5,
        reviewCount: 45,
        imageUrl:
          'https://images.unsplash.com/photo-1493809842364-78817add7ffb?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=2070&q=80',
        type: 'Single Sharing',
        amenities: ['WiFi', 'AC', 'TV', 'Washing Machine', 'Fridge', 'Lift'],
        foodIncluded: true,
        gender: 'unisex',
      },
      {
        id: '8',
        name: 'Homely PG Accommodation',
        location: 'JP Nagar, Bangalore',
        price: 9500,
        rating: 4.4,
        reviewCount: 37,
        imageUrl:
          'https://images.unsplash.com/photo-1586023492125-27b2c045efd7?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=2158&q=80',
        type: 'Double Sharing',
        amenities: ['WiFi', 'TV', 'Fridge', 'Study Table'],
        foodIncluded: true,
        gender: 'female',
      },
      {
        id: '9',
        name: 'Budget PG Stay',
        location: 'Banashankari, Bangalore',
        price: 6000,
        rating: 3.9,
        reviewCount: 21,
        imageUrl:
          'https://images.unsplash.com/photo-1494203484021-3c454daf695d?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=2070&q=80',
        type: 'Multiple Sharing',
        amenities: ['WiFi', 'Power Backup'],
        foodIncluded: false,
        gender: 'male',
      },
    ];
  }

  // Toggle filter section expansion
  toggleSection(section: string): void {
    // If already expanded, just toggle it closed
    if (this.expandedSections[section]) {
      this.expandedSections[section] = false;
    } else {
      // Close all sections first
      Object.keys(this.expandedSections).forEach((key) => {
        this.expandedSections[key] = false;
      });

      // Then open the selected section
      this.expandedSections[section] = true;
    }
  }

  clearFilters() {
    // Reset all search parameters to default values
    this.searchParams = {
      location: this.searchParams.location,
      budget: '',
      roomType: '',
      gender: '',
      amenities: [],
      foodIncluded: false,
      rating: '',
    };

    // Reset amenity checkboxes
    this.amenityWifi = false;
    this.amenityAC = false;
    this.amenityTV = false;
    this.amenityGym = false;
    this.amenityWashing = false;
  }
}
