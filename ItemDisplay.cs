using System.Collections.Generic;

namespace Tradie
{
    public class ItemDisplay
    {
        public IEnumerable<Item> Items { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public ItemDisplay()
        {

        }

        public ItemDisplay(IEnumerable<Item> items, int x, int y)
        {
            Items = items;
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"ItemDisplay: X={X}, Y={Y}";
        }
    }
}
