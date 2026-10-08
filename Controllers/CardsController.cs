using Microsoft.AspNetCore.Mvc;
using mtg_deck_api.Models;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace mtg_deck_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardsController : ControllerBase
    {
        private IHttpClientFactory _httpClientFactory;
        public CardsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("decks/{username}")]
        public async Task<IActionResult> GetDecks(string username)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(baseDir, "Data", "Decks.json");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
            JsonObject rootObject = JsonNode.Parse(jsonContent)?.AsObject() ?? new JsonObject();

            if (rootObject.TryGetPropertyValue(username, out var userNode))
            {
                return Ok(userNode);
            }

            return NotFound();
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveDeck([FromBody] JsonElement requestData)
        {
            string deckName = requestData.GetProperty("deckName").ToString();
            List<MtgCard> cards = requestData.GetProperty("deckCards").EnumerateArray()
                .Select(card => MtgCard.AddCard(card))
                .ToList();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(baseDir, "Data", "Decks.json");

            JsonObject rootObject;
            if (System.IO.File.Exists(filePath))
            {
                string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
                rootObject = JsonNode.Parse(jsonContent)?.AsObject() ?? new JsonObject();
            }
            else
            {
                rootObject = new JsonObject();
            }

            JsonArray userDecksArray;
            if (rootObject.TryGetPropertyValue(requestData.GetProperty("username").ToString(), out var existingNode) && existingNode is JsonArray jsonArray)
            {
                userDecksArray = jsonArray;
            }
            else
            {
                userDecksArray = new JsonArray();
                rootObject[requestData.GetProperty("username").ToString()] = userDecksArray;
            }

            JsonObject cardsObject = new JsonObject();
            foreach (var card in cards)
            {
                JsonObject cardDetails = new JsonObject
                {
                    ["ManaCost"] = card.ManaCost,
                    ["TypeLine"] = card.TypeLine,
                    ["Power"] = card.Power,
                    ["Toughness"] = card.Toughness,
                    ["OracleText"] = card.OracleText,
                    ["Rarity"] = card.Rarity,
                    ["ImageUri"] = card.ImageUri
                };
                cardsObject[card.Name ?? "UnknownCard"] = cardDetails;
            }

            JsonObject newDeckEntry = new JsonObject
            {
                [requestData.GetProperty("deckName").ToString()] = cardsObject
            };
            userDecksArray.Add(newDeckEntry);

            var options = new JsonSerializerOptions { WriteIndented = true };
            await System.IO.File.WriteAllTextAsync(filePath, rootObject.ToJsonString(options));
            return Ok(new { message = "Deck saved successfully." });
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetCard([FromQuery] string? name, string? set, string? colors, string? cost)
        {
            string nameQuery = (name != null) ? $"name%3D{Uri.EscapeDataString(name)}" : "";
            string setQuery = (set != null) ? $"e%3A{Uri.EscapeDataString(set)}" : "";
            string colorQuery = (colors != null) ? $"c%3A{Uri.EscapeDataString(colors)}" : "";
            string costQuery = (cost != null) ? $"mv%3D{Uri.EscapeDataString(cost)}" : "";
            if (name != null && (set != null || colors != null || cost != null)) nameQuery += "+";
            if (set != null && (colors != null || cost != null)) setQuery += "+";
            if (colors != null && (cost != null)) colorQuery += "+";

            var client = _httpClientFactory.CreateClient("ScryfallClient");
            var response = await client.GetAsync($"https://api.scryfall.com/cards/search?q={nameQuery + setQuery + colorQuery + costQuery}");
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Ok(new List<MtgCard>());
            }
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "Card search service is unavailable. Please try again later." });
            }
            
            var data = await response.Content.ReadAsStringAsync();
            JsonElement json = JsonDocument.Parse(data).RootElement;
            JsonElement cardsJson = json.GetProperty("data");
            List<MtgCard> cards = new List<MtgCard>();

            foreach (var cardJson in cardsJson.EnumerateArray())
            {
                if (cardJson.TryGetProperty("card_faces", out var cardFaces))
                {
                    foreach (var cardFace in cardFaces.EnumerateArray())
                    {
                        MtgCard card = new MtgCard();
                        card.Name = cardFace.GetProperty("name").ToString();
                        card.ManaCost = (cardFace.TryGetProperty("mana_cost", out var manaCost)) ? manaCost.ToString() : null;
                        card.TypeLine = (cardFace.TryGetProperty("type_line", out var typeLine)) ? typeLine.ToString() : null;
                        card.Power = (cardFace.TryGetProperty("power", out var power)) ? power.ToString() : null;
                        card.Toughness = (cardFace.TryGetProperty("toughness", out var toughness)) ? toughness.ToString() : null;
                        card.OracleText = cardFace.GetProperty("oracle_text").ToString();
                        card.Rarity = (cardFace.TryGetProperty("rarity", out var rarity)) ? rarity.ToString() : cardJson.GetProperty("rarity").ToString();
                        card.ImageUri = MtgCard.GetImageUri(cardFace, cardJson);
                        cards.Add(card);
                    }
                }
                else
                {
                    MtgCard card = new MtgCard();
                    card.Name = cardJson.GetProperty("name").ToString();
                    card.ManaCost = cardJson.GetProperty("mana_cost").ToString();
                    card.TypeLine = cardJson.GetProperty("type_line").ToString();
                    card.Power = (cardJson.TryGetProperty("power", out var power)) ? power.ToString() : null;
                    card.Toughness = (cardJson.TryGetProperty("toughness", out var toughness)) ? toughness.ToString() : null;
                    card.OracleText = cardJson.GetProperty("oracle_text").ToString();
                    card.Rarity = (cardJson.TryGetProperty("rarity", out var rarity)) ? rarity.ToString() : null;
                    card.ImageUri = cardJson.GetProperty("image_uris").GetProperty("normal").ToString();
                    cards.Add(card);
                }
            }

            return Ok(cards);
        }
    }
}
