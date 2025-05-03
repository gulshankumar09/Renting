# Renting API Postman Collection

This directory contains Postman collection files for testing the Renting API. The collection includes all API endpoints organized by functionality.

## Files

- `RentingAPI.postman_collection.json` - Main collection file
- `UserProfile.json` - User profile endpoints
- `Roles.json` - Role management endpoints
- `Documents.json` - Document management endpoints
- `UserPreferences.json` - User preferences endpoints
- `UserActivity.json` - User activity endpoints
- `RentingAPI.postman_environment.json` - Environment variables

## Setup Instructions

### Import the Collection and Environment

1. Open Postman
2. Click on "Import" in the top left corner
3. Drag and drop all the JSON files from this directory, or use the file browser to select them
4. The main collection and environment should now be imported

### Set Up Environment Variables

1. In Postman, click on the environments dropdown in the top right corner
2. Select "Renting API Environment"
3. Update the following variables:
   - `baseUrl`: Set to your API URL (default: `http://localhost:5000`)
   - `userEmail`: Set to a valid user email
   - `userPassword`: Set to the corresponding user password

### Using the Collection

1. Start with the Authentication requests:
   - Use the "Register" request to create a new user (if needed)
   - Use the "Login" request to authenticate and automatically set the auth token
2. After successful login, the following variables will be automatically set:

   - `authToken`: JWT for authentication
   - `refreshToken`: Token for refreshing the JWT

3. Other requests are organized in folders by functionality:
   - User Profile: Manage user profiles
   - Roles: Manage roles (admin only)
   - Documents: Upload and manage documents
   - User Preferences: Manage user preferences
   - User Activity: View user activities

### Testing Flow

A typical testing flow might be:

1. Register a new user or login with existing credentials
2. Get the user profile (sets the `userProfileId` variable)
3. Update the user profile
4. Get user preferences
5. Update user preferences
6. Upload a document (sets the `documentId` variable)
7. View the uploaded document
8. Update the document
9. View user activities

### Admin Operations

For admin operations, you need to login with an admin account. Then you can:

1. Manage roles
2. View and manage other users' data
3. Verify documents

## Notes

- The collection includes test scripts to automatically set environment variables
- Bearer token authentication is set up for all requests requiring authentication
- For file upload requests, you'll need to select an actual file on your system

## Troubleshooting

- If you get 401 Unauthorized errors, your token may have expired. Run the "Login" or "Refresh Token" request
- For 403 Forbidden errors, you may not have the required permissions (e.g., admin role)
- Check the API server logs for details on any server-side errors
