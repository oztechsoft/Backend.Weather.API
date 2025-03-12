# Weather API Backend Service

## Overview
This is a Web API that integrates with OpenWeatherMap API to provide weather data.

## Prerequisites
Ensure you have the following installed:
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Postman](https://www.postman.com/)

## Setup & Configuration

### 1. Clone the Repository
```sh
git clone <repository-url>
cd <repository-folder>
```

### 2. Build the Project
```sh
dotnet build
```

### 3. Run the API
```sh
dotnet run
```

### 4. Test the API
```sh
dotnet test
```

## Usage

### **1. Fetch Weather Data**
**Endpoint:** `GET /api/weather?city=London&country=uk&apiKey=api-key-1`

#### **Response Example:**
```json
{
  "description": "light rain"
}
```

### **2. Rate Limit Handling**
If the request limit (5 per hour per API key) is exceeded:
```json
{
  "Rate limit exceeded. Try again later."
}
```

## Notes
- Ensure your API key is passed as a query parameter.
- The API is rate-limited to **5 requests per hour per API key**.
- The service only returns the `description` field from the OpenWeatherMap response.
