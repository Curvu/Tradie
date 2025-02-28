namespace Tradie
{
    public class Item
    {
        public string ItemName { get; set; }
        public int Amount { get; set; }

        public Item()
        {
            
        }

        public Item(string itemName, int amount)
        {
            ItemName = itemName;
            Amount = amount;
        }
    }
}
