# Justice.Dash.Server

Justice.Dash.Server is a .NET-based server application that provides various services including food and menu management, surveillance, and domicile-related functionalities.

## Project Structure

The project is organized into several key components:

### Controllers
- `DomicileController` - Handles domicile-related operations
- `FoodModifierController` - Manages food modifications
- `MenuController` - Handles menu-related operations
- `SurveillanceController` - Manages surveillance functionality

### Services
- `AiService` - Provides AI-related functionality
- `DomicileService` - Handles domicile business logic
- `FoodAndCoService` - Manages food-related operations
- `StateService` - Handles state management

### Data Models
- `BaseDataModel` - Base class for data models
- `FoodModifier` - Represents food modification data
- `MenuItem` - Represents menu item data
- `Photo` - Handles photo data
- `Surveillance` - Manages surveillance data

## Getting Started

1. Clone the repository
2. Open the solution in Visual Studio or your preferred IDE
3. Restore NuGet packages
4. Update the database using Entity Framework migrations
5. Run the application

## Database Migrations

The project uses Entity Framework Core for database management. Several migrations are available:
- Initial Create
- Add Photos Table
- Add Veganized Content
- Add Food Modifier
- Add Menu Updating Support
- Add Manually Modified Field

## Docker Support

The application includes Docker support with a Dockerfile for containerization.