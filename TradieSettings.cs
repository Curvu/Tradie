﻿using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace Tradie
{
    public class TradieSettings : ISettings
    {
        public TradieSettings()
        {
            Enable = new ToggleNode(false);
            ImageSize = new RangeNode<int>(32, 1, 78);
            TextSize = new RangeNode<int>(20, 1, 60);
            Spacing = new RangeNode<int>(3, 0, 20);

            // YourItemStartingLocationX = new RangeNode<int>(966, 0, (int)BasePlugin.API.GameController.Window.GetWindowRectangle().Width);
            YourItemStartingLocationX = new RangeNode<float>(966, 0, 1000);
            // YourItemStartingLocationY = new RangeNode<int>(863, 0, (int)BasePlugin.API.GameController.Window.GetWindowRectangle().Height);
            YourItemStartingLocationY = new RangeNode<float>(863, 0, 1000);
            YourItemsAscending = new ToggleNode(true);
            YourItemTextColor = Color.LightBlue;
            YourItemBackgroundColor = Color.Black;
            YourItemsImageLeftOrRight = new ToggleNode(true);

            // TheirItemStartingLocationX = new RangeNode<int>(966, 0, (int)BasePlugin.API.GameController.Window.GetWindowRectangle().Width);
            TheirItemStartingLocationX = new RangeNode<float>(966, 0, 1000);
            // TheirItemStartingLocationY = new RangeNode<int>(863, 0, (int)BasePlugin.API.GameController.Window.GetWindowRectangle().Height);
            TheirItemStartingLocationY = new RangeNode<float>(863, 0, 1000);
            TheirItemsAscending = new ToggleNode(false);
            TheirItemTextColor = Color.Red;
            TheirItemBackgroundColor = Color.Black;
            TheirItemsImageLeftOrRight = new ToggleNode(false);
        }

        [Menu("Image Size")]
        public RangeNode<int> ImageSize { get; set; }
        [Menu("Text Size")]
        public RangeNode<int> TextSize { get; set; }
        [Menu("Spacing", "Spacing between image and text")]
        public RangeNode<int> Spacing { get; set; }

        [Menu("Your Trade Items", 1001)]
        public EmptyNode Blank1 { get; set; }
        [Menu("X", 10011, 1001)]
        public RangeNode<float> YourItemStartingLocationX { get; set; }
        [Menu("Y", 10012, 1001)]
        public RangeNode<float> YourItemStartingLocationY { get; set; }
        [Menu("Ascending Order", 10013, 1001)]
        public ToggleNode YourItemsAscending { get; set; }
        [Menu("Currency Before Or After", 10014, 1001)]
        public ToggleNode YourItemsImageLeftOrRight { get; set; }
        [Menu("Text Color", 10015, 1001)]
        public ColorNode YourItemTextColor { get; set; }
        [Menu("Background Color", 10016, 1001)]
        public ColorNode YourItemBackgroundColor { get; set; }

        [Menu("Their Trade Items", 2001)]
        public EmptyNode Blank2 { get; set; }
        [Menu("X", 20011, 2001)]
        public RangeNode<float> TheirItemStartingLocationX { get; set; }
        [Menu("Y", 20012, 2001)]
        public RangeNode<float> TheirItemStartingLocationY { get; set; }
        [Menu("Ascending Order", 20013, 2001)]
        public ToggleNode TheirItemsAscending { get; set; }
        [Menu("Currency Before Or After", 20014, 2001)]
        public ToggleNode TheirItemsImageLeftOrRight { get; set; }
        [Menu("Text Color", 20015, 2001)]
        public ColorNode TheirItemTextColor { get; set; }
        [Menu("Background Color", 20016, 2001)]
        public ColorNode TheirItemBackgroundColor { get; set; }
        public ToggleNode Enable { get; set; }
    }
}