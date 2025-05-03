#!/bin/bash

# This script merges all separate JSON files into the main Postman collection file

# Exit on any error
set -e

# Current directory
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Create a temporary file for manipulating the JSON
TEMP_FILE="$DIR/temp_collection.json"

# Start with the base collection
cp "$DIR/RentingAPI.postman_collection.json" "$TEMP_FILE"

# Function to merge a JSON file into the main collection
merge_file() {
  local file="$1"
  local filename=$(basename "$file" .json)
  
  if [ -f "$file" ] && [ "$filename" != "RentingAPI.postman_collection" ] && [ "$filename" != "RentingAPI.postman_environment" ]; then
    echo "Merging $filename..."
    
    # Use jq to merge the file contents into the collection's items array
    jq --argjson newitem "$(cat "$file")" '.item += [$newitem]' "$TEMP_FILE" > "$TEMP_FILE.new"
    mv "$TEMP_FILE.new" "$TEMP_FILE"
  fi
}

# Merge each JSON file
for file in "$DIR"/*.json; do
  if [ "$(basename "$file")" != "RentingAPI.postman_collection.json" ] && \
     [ "$(basename "$file")" != "RentingAPI.postman_environment.json" ] && \
     [ "$(basename "$file")" != "temp_collection.json" ]; then
    merge_file "$file"
  fi
done

# Move the merged file to the final destination
mv "$TEMP_FILE" "$DIR/RentingAPI.postman_collection.json"

echo "Successfully merged all files into RentingAPI.postman_collection.json" 