namespace AddMoreItemsToShopEditor.Models
{
    class GameItem
    {
        public int ID { get; set; }
        public string EnglishName { get; set; }
        public string TargetLangName { get; set; }
        public string Description { get; set; }
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public int StackNumber { get; set; }
    }
}
