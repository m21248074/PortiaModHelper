namespace AddMoreItemsToShopEditor.Models
{
    public class GameItem
    {
        public int ID { get; set; }
        public string EnglishName { get; set; } = string.Empty;
        public string TargetLangName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int BuyPrice { get; set; }
        public string SellPrice { get; set; } = string.Empty;
        public int StackNumber { get; set; }

        public string FormattedName => $"{TargetLangName} ({EnglishName})";
    }
}
