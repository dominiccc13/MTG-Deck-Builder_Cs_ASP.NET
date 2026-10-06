using Microsoft.AspNetCore.Mvc;
using mtg_deck_api.Models;
using System.Text.Json;

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
                        card.ManaCost = cardFace.GetProperty("mana_cost").ToString();
                        card.TypeLine = cardFace.GetProperty("type_line").ToString();
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
