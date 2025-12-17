# API Documentation

This document provides comprehensive documentation for the Justice Dash Server API, specifically covering menu items and food modifiers.

## Table of Contents
- [Menu API](#menu-api)
  - [Get Menu Items](#get-menu-items)
  - [Regenerate Menu Item](#regenerate-menu-item)
  - [Update Menu Item](#update-menu-item)
- [Food Modifier API](#food-modifier-api)
  - [Get All Food Modifiers](#get-all-food-modifiers)
  - [Get Single Food Modifier](#get-single-food-modifier)
  - [Create Food Modifier](#create-food-modifier)
  - [Update Food Modifier](#update-food-modifier)
  - [Delete Food Modifier](#delete-food-modifier)
- [Data Models](#data-models)

---

## Menu API

The Menu API allows you to retrieve, update, and manage menu items in the system. Menu items represent food items scheduled for specific dates, including their original and veganized versions.

### Get Menu Items

Retrieves menu items from the database.

**Endpoint:** `GET /Menu`

**Query Parameters:**
- `full` (boolean, optional): 
  - `true`: Returns all menu items from the database
  - `false` (default): Returns only menu items from today onwards

**Response:** Array of MenuItem objects

**Example Request:**
```http
GET /Menu?full=false
```

**Example Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "date": "2024-03-15",
    "day": "Friday",
    "weekNumber": 11,
    "foodName": "Spaghetti Carbonara",
    "correctedFoodName": "Spaghetti Carbonara",
    "veganizedFoodName": "Vegan Spaghetti Carbonara",
    "description": "A classic Italian pasta dish",
    "veganizedDescription": "A plant-based version of the classic Italian pasta dish",
    "recipe": "...",
    "foodContents": ["pasta", "eggs", "bacon", "parmesan"],
    "image": {
      "path": "https://example.com/images/carbonara.jpg"
    },
    "veganizedImage": {
      "path": "https://example.com/images/vegan-carbonara.jpg"
    },
    "foodModifier": null,
    "manuallyModified": false,
    "needsNameCorrection": false,
    "needsVeganization": false,
    "needsDescription": false,
    "needsVeganDescription": false,
    "needsRecipeGeneration": false,
    "needsFoodContents": false,
    "needsImageRegeneration": false,
    "needsVeganImageRegeneration": false
  }
]
```

---

### Regenerate Menu Item

Triggers regeneration of a menu item for a specific date. This marks the item for image regeneration and triggers AI tasks.

**Endpoint:** `GET /Menu/regen/{date}`

**Path Parameters:**
- `date` (string, required): The date of the menu item to regenerate (format: YYYY-MM-DD)

**Response:** Status code 200 with the number of affected records

**Example Request:**
```http
GET /Menu/regen/2024-03-15
```

**Example Response:**
```json
1
```

---

### Update Menu Item

Updates a menu item for a specific date with new information. This endpoint allows both manual edits and triggering regeneration of specific components.

**Endpoint:** `PUT /Menu/{date}`

**Path Parameters:**
- `date` (string, required): The date of the menu item to update (format: YYYY-MM-DD)

**Request Body:** MenuItemUpdate object

```json
{
  "foodName": "string (optional)",
  "correctedFoodName": "string (optional)",
  "veganizedFoodName": "string (optional)",
  "description": "string (optional)",
  "veganizedDescription": "string (optional)",
  "recipe": "string (optional)",
  "foodModifierId": "guid (optional)",
  "regenerateImages": "boolean",
  "regenerateDescriptions": "boolean",
  "regenerateNames": "boolean",
  "regenerateRecipe": "boolean",
  "regenerateFoodContents": "boolean"
}
```

**Field Descriptions:**

**Manual Override Fields** (setting these prevents auto-regeneration of that field):
- `foodName`: Updates the original food name and marks item as manually modified. Triggers regeneration of all dependent fields.
- `correctedFoodName`: Sets the corrected food name without triggering regeneration
- `veganizedFoodName`: Sets the veganized food name without triggering regeneration
- `description`: Sets the description without triggering regeneration
- `veganizedDescription`: Sets the veganized description without triggering regeneration
- `recipe`: Sets the recipe without triggering regeneration
- `foodModifierId`: Associates a food modifier with the menu item. Use empty GUID (00000000-0000-0000-0000-000000000000) to remove the modifier. Triggers image regeneration.

**Regeneration Flags** (explicitly request regeneration):
- `regenerateImages`: Marks the item for image regeneration
- `regenerateDescriptions`: Marks the item for description regeneration
- `regenerateNames`: Marks the item for name correction regeneration
- `regenerateRecipe`: Marks the item for recipe regeneration
- `regenerateFoodContents`: Marks the item for food contents regeneration

**Response:** The updated MenuItem object (200 OK) or NotFound (404) if the menu item doesn't exist

**Example Request:**
```http
PUT /Menu/2024-03-15
Content-Type: application/json

{
  "description": "A delicious classic Italian pasta dish with eggs and bacon",
  "regenerateImages": true
}
```

**Example Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "date": "2024-03-15",
  "day": "Friday",
  "weekNumber": 11,
  "foodName": "Spaghetti Carbonara",
  "description": "A delicious classic Italian pasta dish with eggs and bacon",
  "needsImageRegeneration": true,
  ...
}
```

---

## Food Modifier API

Food modifiers are special attributes that can be applied to menu items to modify their presentation or style (e.g., "heavily colored blue", "Pixar style", "served in space").

### Get All Food Modifiers

Retrieves all food modifiers from the database.

**Endpoint:** `GET /FoodModifier`

**Response:** Array of FoodModifier objects

**Example Request:**
```http
GET /FoodModifier
```

**Example Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Blue Theme",
    "description": "The food is heavily colored blue."
  },
  {
    "id": "7b8c9d0e-1234-5678-90ab-cdef12345678",
    "title": "Pixar Style",
    "description": "The plate is presented in the style of Pixar."
  }
]
```

---

### Get Single Food Modifier

Retrieves a specific food modifier by its ID.

**Endpoint:** `GET /FoodModifier/{id}`

**Path Parameters:**
- `id` (guid, required): The unique identifier of the food modifier

**Response:** FoodModifier object (200 OK) or NotFound (404)

**Example Request:**
```http
GET /FoodModifier/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Example Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Blue Theme",
  "description": "The food is heavily colored blue."
}
```

---

### Create Food Modifier

Creates a new food modifier.

**Endpoint:** `POST /FoodModifier`

**Request Body:** FoodModifier object

```json
{
  "title": "string (required)",
  "description": "string (required)"
}
```

**Response:** The created FoodModifier object with status 201 (Created)

**Example Request:**
```http
POST /FoodModifier
Content-Type: application/json

{
  "title": "Underwater Theme",
  "description": "The food is served deep down in the ocean."
}
```

**Example Response:**
```http
Status: 201 Created
Location: /FoodModifier/9a1b2c3d-4e5f-6789-0abc-def123456789

{
  "id": "9a1b2c3d-4e5f-6789-0abc-def123456789",
  "title": "Underwater Theme",
  "description": "The food is served deep down in the ocean."
}
```

---

### Update Food Modifier

Updates an existing food modifier. When the description is changed, all menu items using this modifier are marked for image regeneration.

**Endpoint:** `PUT /FoodModifier/{id}`

**Path Parameters:**
- `id` (guid, required): The unique identifier of the food modifier to update

**Request Body:** FoodModifierUpdate object

```json
{
  "title": "string (required)",
  "description": "string (required)"
}
```

**Response:** The updated FoodModifier object (200 OK) or NotFound (404)

**Example Request:**
```http
PUT /FoodModifier/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json

{
  "title": "Deep Blue Theme",
  "description": "The food is heavily colored in a deep ocean blue."
}
```

**Example Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Deep Blue Theme",
  "description": "The food is heavily colored in a deep ocean blue."
}
```

---

### Delete Food Modifier

Deletes a food modifier. All menu items using this modifier will have it removed and marked for image regeneration.

**Endpoint:** `DELETE /FoodModifier/{id}`

**Path Parameters:**
- `id` (guid, required): The unique identifier of the food modifier to delete

**Response:** NoContent (204) or NotFound (404)

**Example Request:**
```http
DELETE /FoodModifier/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Example Response:**
```http
Status: 204 No Content
```

---

## Data Models

### MenuItem

Represents a menu item in the system, including its original and modified versions.

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `id` | guid | Unique identifier for the menu item |
| `date` | DateOnly | The date this menu item is scheduled for |
| `day` | string | The day of the week this menu item is for |
| `weekNumber` | integer | The week number in the year for this menu item |
| `foodName` | string | The original name of the food item |
| `correctedFoodName` | string (nullable) | The corrected/normalized name of the food item |
| `veganizedFoodName` | string (nullable) | The veganized version of the food name |
| `description` | string (nullable) | Description of the original food item |
| `veganizedDescription` | string (nullable) | Description of the veganized version |
| `recipe` | string (nullable) | Recipe for the food item |
| `foodContents` | array of strings | List of ingredients or contents in the food item |
| `image` | Image (nullable) | The image of the original food item |
| `veganizedImage` | Image (nullable) | The image of the veganized version |
| `foodModifier` | FoodModifier (nullable) | Optional modifier affecting the food's presentation |
| `manuallyModified` | boolean | Indicates if the item has been manually modified |
| `needsNameCorrection` | boolean | Flag indicating if name correction is needed |
| `needsVeganization` | boolean | Flag indicating if veganization is needed |
| `needsDescription` | boolean | Flag indicating if description generation is needed |
| `needsVeganDescription` | boolean | Flag indicating if vegan description generation is needed |
| `needsRecipeGeneration` | boolean | Flag indicating if recipe generation is needed |
| `needsFoodContents` | boolean | Flag indicating if food contents extraction is needed |
| `needsImageRegeneration` | boolean | Flag indicating if image regeneration is needed |
| `needsVeganImageRegeneration` | boolean | Flag indicating if vegan image regeneration is needed |
| `foodDisplayName` | string (computed) | Gets the display name, using corrected name if available, otherwise the original name |

---

### FoodModifier

Represents a modifier that can be applied to menu items to change their presentation style.

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `id` | guid | Unique identifier for the food modifier |
| `title` | string | The display title of the modifier |
| `description` | string | The description that affects how the food is presented (used in AI generation) |

**Common Food Modifier Descriptions:**

The system supports various predefined modifiers including:

**Color Themes:**
- "The food is heavily colored blue/red/green/yellow/purple/white/black."

**Style Themes:**
- "The plate is presented in the style of Pixar/Dream Works/Disney."
- "The plate is presented in the style of an old analog VHS film."
- "The plate is presented in the style an image from the 18th century."

**Setting Themes:**
- "The plate is presented in the middle of the night."
- "The food is served on a lush forest floor."
- "The food is served deep down in the ocean."
- "The food is served in space."

**State Modifiers:**
- "The food is raw."
- "The food is burnt."
- "The food is spread across the table."
- "The food is made of rainbows and sprinkles."

**Plating Styles:**
- "The plating is weird."
- "The plating is very traditional."
- "The plating is upside down."
- "The plating is like a fine dining restaurant."
- "The plating is like a hearthy warm inn."

**Regional/Temporal:**
- "The food is from america/south america/africa/asia/antarctica."
- "The food is from the far future."
- "The food is from fantasy land."
- "The food is from post apocalyptic future."

---

### MenuItemUpdate

Model used for updating menu items.

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `foodName` | string (optional) | Updates the original food name |
| `correctedFoodName` | string (optional) | Updates the corrected food name |
| `veganizedFoodName` | string (optional) | Updates the veganized food name |
| `description` | string (optional) | Updates the description |
| `veganizedDescription` | string (optional) | Updates the veganized description |
| `recipe` | string (optional) | Updates the recipe |
| `foodModifierId` | guid (optional) | Associates or removes a food modifier |
| `regenerateImages` | boolean | Request image regeneration |
| `regenerateDescriptions` | boolean | Request description regeneration |
| `regenerateNames` | boolean | Request name correction regeneration |
| `regenerateRecipe` | boolean | Request recipe regeneration |
| `regenerateFoodContents` | boolean | Request food contents regeneration |

---

### FoodModifierUpdate

Model used for updating food modifiers.

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `title` | string (required) | The display title of the modifier |
| `description` | string (required) | The description that affects how the food is presented |

---

### Image

Represents an image associated with a menu item.

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `path` | string | The full URL path to the image (automatically constructed from BaseUrl config) |

---

## Error Responses

All endpoints may return the following error responses:

**400 Bad Request**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "field": ["Error message"]
  }
}
```

**404 Not Found**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404
}
```

**500 Internal Server Error**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500
}
```

---

## Notes

- All dates should be provided in ISO 8601 format (YYYY-MM-DD)
- GUIDs should be provided in standard format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- Image URLs are automatically constructed using the `BaseUrl` configuration value
- Changing a menu item's `foodName` will trigger regeneration of all dependent fields
- Modifying a food modifier's description will trigger image regeneration for all menu items using that modifier
- Deleting a food modifier will remove it from all associated menu items and trigger their image regeneration
