namespace AddMoreItemsToShopEditor.Models
{
    public class ShopItem
    {
        public int ID { get; set; }
        public int count { get; set; }
        public int currency { get; set; }
        public int requireMission { get; set; }
        public int storeId { get; set; }
        public double chance { get; set; }
    }
}
