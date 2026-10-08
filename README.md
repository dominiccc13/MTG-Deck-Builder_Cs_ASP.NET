# MTG Deck Builder

A simple web app for searching Magic The Gathering cards and saving them into decks.

## What it does

- Search for cards by name
- Add cards to a deck
- Save decks under a username
- Load a user's saved decks

## Built with

- C# and ASP.NET Core (backend API)
- HTML, CSS, and JavaScript (frontend)
- Scryfall API for card data

## How to run

1. Install the .NET SDK.
2. Clone this repository.
3. In the project folder, run:

```
dotnet run --launch-profile https
```

4. Open https://localhost:7259 in your browser.

## Notes

This is a learning project, so it is intentionally simple. Decks are stored in a local JSON file, and there is no login system.

## Credits

Card data and images are provided by [Scryfall](https://scryfall.com/docs/api). This is an unofficial fan project and is not affiliated with or endorsed by Wizards of the Coast.