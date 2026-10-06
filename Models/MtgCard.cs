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
    }
}
