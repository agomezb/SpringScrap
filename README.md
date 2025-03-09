# Overview

SpringScrap is a .NET tool to fetch configuration from Spring Cloud Config Server and save it as a JSON file.

### Features
- Connects to a Spring Cloud Config server.
- Fetches configuration data for a specified service.
- Generates a JSON file (default: appsettings.spring.json) with the fetched configuration.

### Prerequisites
- .NET SDK 8.0 or later
- Access to a Spring Cloud Config server
- Set SPRING_SCRAP_URI environment variable with spring cloud server url

## Installation

To install the tool globally, use the following command:

```
dotnet tool install --global SpringScrap
```

## Usage
Once installed, you can use the springscrap command to fetch configuration.

```
springscrap --service <service-name> --env <env-name>
```

## Command Line Options
- --service or -s: Service name to get configuration (required).
- --environment or -e: Environment name to get configuration (required).
- --output or -o: Name of the output JSON file (default: appsettings.spring.json).

## Example
```
springscrap -s my-service --environment production --output config.json
```
This command will fetch the configuration for my-service in the production environment and save it to config.json.

### License
This project is licensed under the MIT License. See the LICENSE file for details.
