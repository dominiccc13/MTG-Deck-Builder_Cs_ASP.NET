## Project Overview

This is a web application for searching Magic The Gathering cards and building decks. It was built with the ASP.NET Core framework to serve static pages and a dynamic controller for searching cards with various properties. It utilizes the Scryfall API.

## Backend Architecture

The backend is built with ASP.NET Core and handles card search operations through a dedicated controller. It queries the Scryfall API, parses the JSON response using System.Text.Json, and normalizes both standard single-faced cards and complex double-faced cards into a unified format.

### Models and Controllers

The MtgCard model stores core card properties including name, mana cost, type line, power, toughness, oracle text, rarity, and image URI. The CardsController exposes an endpoint at api/cards/search which accepts query parameters for card name, set, colors, and mana cost.

## Frontend Integration

The user interface is built using standard HTML, CSS, and vanilla JavaScript files located in the wwwroot directory. ASP.NET Core static files middleware serves these frontend files directly from the application base URL, allowing the client interface and the API to run on the same origin.

## Setup and Running

To run the project, open the solution in Visual Studio or use the .NET CLI. Ensure the application is started using the https launch profile configured in launchSettings.json. Once running, open a web browser and navigate to the application root URL to access the card search and deck builder interface.
