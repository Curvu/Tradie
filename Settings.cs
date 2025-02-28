using Color = System.Drawing.Color;

using ExileCore2.Shared.Attributes;
using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;

namespace Tradie
{
    public class Settings : ISettings
    {
        public Settings()
        {
            ItemTextColor = Color.LightBlue;
            ItemBackgroundColor = Color.Black;
        }

        [Menu("Image Size")]
        public RangeNode<int> ImageSize { get; set; } = new RangeNode<int>(32, 1, 78);

        [Menu("Text Size")]
        public RangeNode<int> TextSize { get; set; } = new RangeNode<int>(20, 1, 60);

        [Menu("Spacing", "Spacing between image and text")]
        public RangeNode<int> Spacing { get; set; } = new RangeNode<int>(3, 0, 20);

        [Menu("Item Text Color")]
        public ColorNode ItemTextColor { get; set; }

        [Menu("Item Background Color")]
        public ColorNode ItemBackgroundColor { get; set; }

        [Menu("Hover Delay", "Delay in ms in between each hover")]
        public RangeNode<int> HoverDelay { get; set; } = new RangeNode<int>(100, 0, 1000);

        public ToggleNode Enable { get; set; } = new ToggleNode(true);
    }
}