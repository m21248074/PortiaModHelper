namespace AddMoreItemsToShopEditor.Models
{
    public class ShopItemDisplayModel : ShopItem
    {
        public string Description { get; set; } = "No description available.";
        public string EnglishName { get; set; } = "Unknown";
        public string TargetLangName { get; set; } = "Unknown";
        public string StoreName { get; set; } = "Unknown Store";
        public string OpenTime { get; set; } = "-";

        public string FormattedName => $"{TargetLangName} ({EnglishName})";
    }
}
