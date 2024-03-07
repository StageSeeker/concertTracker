# Concert Tracker

## TeckStack
- C# .Net ASP Core
- MongoDB for database connections.
- Swagger UI for development.

## Features
### Concert discovery
- Search for concerts by artist, genre, location, date, etc.
- Utilize SeatGeek API gather concert data.
- Display relevant information like artist lineup, venue details, ticket prices, reviews, etc.

### Watchlist management
- Allow users to create and manage personalized watchlists of upcoming concerts they're interested in.
- Provide options to set reminders, track ticket purchases, and share watchlists with friends.

### Attendance tracking
- Enable users to mark concerts they've attended, potentially logging notes, pictures, or memories.

## Installation
Install the following:
- IDE such as Visual Studio Code (VS Code) or Visual Studio
- VS Code C# extensions: C# Extension, C# Dev Kit, .NET install tools.
- Install .NET: .NET 7.0+ runtime and sdk.

### Setup
- Clone this repositiory
- Open up project in your IDE of choice(VS Code).

### Build & Run Application
***Trust HTTP certificate***
``` bash
dotnet dev-certs https --trust
```
***Build Command***
``` bash
dotnet build
```
***Run Command***
``` bash
dotnet run --launch-profile https
```

***Start the server with .NET Hot Reload (to be able to make changes to the server without restarting):***
``` bash
dotnet watch run --launch-profile https
```
## Testing
Test the API Endpoints by:
- First Authenticating with the https://localhost/7290/login
- This will redirect you to https://localhost/7290/swagger
- All Endpoints will be listed on Swagger. 

## API Documnetation
View StageSeeker API Documentation here:
https://app.swaggerhub.com/apis-docs/LavonSampson/StageSeeker/1.5

