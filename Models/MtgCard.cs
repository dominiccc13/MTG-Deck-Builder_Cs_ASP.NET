using System.Text.Json;

namespace mtg_deck_api.Models
{
    public class MtgCard
    {
        public string Name { get; set; } = string.Empty;
        public string? ManaCost { get; set; }
        public string? TypeLine { get; set; }
        public string? Power { get; set; }
        public string? Toughness { get; set; }
        public string? OracleText { get; set; }
        public string? Rarity { get; set; }
        public string? ImageUri { get; set; }
        static public string? GetImageUri(JsonElement face, JsonElement card)
        {
            if (face.TryGetProperty("image_uris", out var imageUri))
            {
                return imageUri.GetProperty("normal").ToString();
            }
            else if (card.TryGetProperty("image_uris", out var imageUri2))
            {
                return imageUri2.GetProperty("normal").ToString();
            }
            else
            {
                return null;
            }
        }
        static public MtgCard AddCard(JsonElement cardJson)
        {
            MtgCard card = new MtgCard();
            card.Name = cardJson.GetProperty("Name").ToString();
            card.ManaCost = (cardJson.TryGetProperty("ManaCost", out var manaCost)) ? manaCost.ToString() : null;
            card.TypeLine = (cardJson.TryGetProperty("TypeLine", out var typeLine)) ? typeLine.ToString() : null;
            card.Power = (cardJson.TryGetProperty("Power", out var power)) ? power.ToString() : null;
            card.Toughness = (cardJson.TryGetProperty("Toughness", out var toughness)) ? toughness.ToString() : null;
            card.OracleText = (cardJson.TryGetProperty("OracleText", out var oracleText)) ? oracleText.ToString() : null;
            card.Rarity = (cardJson.TryGetProperty("Rarity", out var rarity)) ? rarity.ToString() : null;
            card.ImageUri = (cardJson.TryGetProperty("ImageUri", out var imageUri)) ? imageUri.ToString() : null;
            return card;
        }
    }
}
