using Tradie.Utils;

using ExileCore2;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.Elements.InventoryElements;
using ExileCore2.Shared;
using ExileCore2.Shared.Enums;
using Graphics = ExileCore2.Graphics;
using RectangleF = ExileCore2.Shared.RectangleF;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using Color = System.Drawing.Color;
using Vector2 = System.Numerics.Vector2;
using ImGuiNET;

namespace Tradie
{
    public partial class Core : BaseSettingsPlugin<Settings>
    {
        private readonly Dictionary<string, string> _whiteListedPaths = new Dictionary<string, string>
        {
            { "Metadata/Items/Currency/CurrencyAddModToRare", "CurrencyAddModToRare.png" },
            { "Metadata/Items/Currency/CurrencyRerollRare", "CurrencyRerollRare.png" },
            { "Metadata/Items/Currency/CurrencyModValues", "CurrencyModValues.png" },
            { "Metadata/Items/Currency/CurrencyDuplicate", "CurrencyDuplicate.png" }
        };

        private const int btnMillis = 500;
        private long lastBtnPress = 0;

        public override bool Initialise()
        {
            base.Initialise();
            Name = "Tradie";

            var eyeBtn = Path.Combine(DirectoryFullName, "images\\eye.png").Replace('\\', '/');
            Graphics.InitImage(eyeBtn, false);

            // Load all images
            foreach (var path in _whiteListedPaths.Values)
            {
                var fullPath = Path.Combine(DirectoryFullName, "images\\" + path).Replace('\\', '/');
                Graphics.InitImage(fullPath, false);
            }

            return true;
        }

        public override void Render()
        {
            var Area = GameController.Area.CurrentArea;
            if (!Area.IsTown && !Area.IsHideout) return;
            if (!TradingWindowVisible) return;

            const float buttonSize = 37;
            var buttonPos = GetTradingWindow().GetChildAtIndex(5).GetClientRect().BottomRight + new Vector2(20, -buttonSize-2);
            var buttonRect = new RectangleF(buttonPos.X, buttonPos.Y, buttonSize, buttonSize);
            Graphics.DrawImage("eye.png", buttonRect);
            if(IsButtonPressed(buttonRect))
            {
                HoverAllItems();
            }

            ShowPlayerTradeItems();
        }

        private void HoverAllItems()
        {
            var tradingWindow = GetTradingWindow();
            if (tradingWindow == null || tradingWindow.IsVisible == false) return;

            var items = GetItemsInTradingWindow(tradingWindow).theirItems;
            foreach (var item in items)
            {
                var itemPos = item.GetClientRect().Center;
                Mouse.moveMouse(itemPos + WindowOffset);
                Thread.Sleep(Settings.HoverDelay);
            }
        }

        private void ShowPlayerTradeItems()
        {
            if (!TradingWindowVisible) return;
            Element tradingWindow = GetTradingWindow();
            if (tradingWindow == null || tradingWindow.IsVisible == false) return;

            var tradingItems = GetItemsInTradingWindow(tradingWindow);
            var ourData = new ItemDisplay
            {
                Items = GetItemObjects(tradingItems.ourItems),
                X = (int)MyTradeWindow.GetClientRect().BottomRight.X,
                Y = (int)MyTradeWindow.GetClientRect().BottomRight.Y,
            };
            var theirData = new ItemDisplay
            {
                Items = GetItemObjects(tradingItems.theirItems),
                X = (int)TheirTradeWindow.GetClientRect().BottomRight.X,
                Y = (int)TheirTradeWindow.GetClientRect().BottomRight.Y,
            };
            DrawCurrency(ourData);
            DrawCurrency(theirData);
        }

        private void DrawCurrency(ItemDisplay data)
        {
            var counter = 0;
            var newColor = (Color) Settings.ItemBackgroundColor;
            newColor = Color.FromArgb(Settings.ItemBackgroundColor.Value.A, newColor.R, newColor.G, newColor.B);
            if (data.Items == null || !data.Items.Any()) return;
            var maxCount = data.Items.Max(i => i.Amount);

            var background = new RectangleF(data.X, data.Y, Settings.ImageSize + Settings.Spacing + 3 + Graphics.MeasureText("- " + maxCount, Settings.TextSize).X, -Settings.ImageSize * data.Items.Count());

            Graphics.DrawBox(background, newColor);

            foreach (var item in data.Items)
            {
                counter++;
                var imageBox = new RectangleF(data.X, data.Y - counter * Settings.ImageSize, Settings.ImageSize, Settings.ImageSize);
                Graphics.DrawImage(_whiteListedPaths[item.ItemName], imageBox);
                Graphics.DrawText($"- {item.Amount}", new Vector2(data.X + Settings.ImageSize + Settings.Spacing, imageBox.Center.Y - Settings.TextSize / 2 - 3), Settings.ItemTextColor, FontAlign.Left);
            }
        }

        private (List<NormalInventoryItem> ourItems, List<NormalInventoryItem> theirItems) GetItemsInTradingWindow(Element tradingWindow)
        {
            var ourItems = new List<NormalInventoryItem>();
            var theirItems = new List<NormalInventoryItem>();
            if (tradingWindow.ChildCount < 2) return (ourItems, theirItems);

            ourItems = GetItemsInList(tradingWindow.Children[0]);
            theirItems = GetItemsInList(tradingWindow.Children[1]);

            return (ourItems, theirItems);
        }

        private List<NormalInventoryItem> GetItemsInList(Element listElement)
        {
            var items = new List<NormalInventoryItem>();
            foreach (var itemElement in listElement.Children.Skip(1))
            {
                var normalInventoryItem = itemElement.AsObject<NormalInventoryItem>();
                if (normalInventoryItem == null)
                {
                    LogMessage("Tradie: Item was null!", 5);
                    throw new Exception("Tradie: Item was null!");
                }

                items.Add(normalInventoryItem);
            }
            return items;
        }

        private IEnumerable<Item> GetItemObjects(IEnumerable<NormalInventoryItem> normalInventoryItems)
        {
            var items = new List<Item>();
            foreach (var normalInventoryItem in normalInventoryItems)
                try
                {
                    if (normalInventoryItem?.Text == "Place items you want to trade here" || normalInventoryItem.Item == null || normalInventoryItem.Item.Address < 1) continue;
                    var metaData = normalInventoryItem.Item.Path;
                    if (metaData.Equals("") || !_whiteListedPaths.ContainsKey(metaData)) continue;

                    var stack = normalInventoryItem.Item.GetComponent<Stack>();
                    var amount = stack?.Info == null ? 1 : stack.Size;
                    var found = false;
                    foreach (var item in items) {
                        if (item.ItemName.Equals(metaData))
                        {
                            item.Amount += amount;
                            found = true;
                            break;
                        }
                    }

                    if (found) continue;
                    items.Add(new Item(metaData, amount));
                }
                catch(Exception e)
                {
                    LogError("Tradie: Sometime went wrong in GetItemObjects() for a brief moment", 5);
                    LogError(e.ToString(), 5);
                }

            return items;
        }

        private bool IsButtonPressed(RectangleF buttonRect)
        {
            if (Control.MouseButtons == MouseButtons.Left && !ImGui.GetIO().WantCaptureMouse)
            {
                if (buttonRect.Contains(Mouse.GetCursorPosition() - WindowOffset))
                {
                    if (lastBtnPress == 0 || lastBtnPress + btnMillis < DateTimeOffset.Now.ToUnixTimeMilliseconds())
                    {
                        lastBtnPress = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TradingWindowVisible
        {
            get
            {
                var windowElement = GameController.IngameState.IngameUi.TradeWindow;
                return windowElement != null && windowElement.IsVisible;
            }
        }

        private Element GetTradingWindow()
        {
            try
            {
                return GameController.IngameState.IngameUi.TradeWindow.GetChildAtIndex(3).GetChildAtIndex(1).GetChildAtIndex(0).GetChildAtIndex(0);
            }
            catch
            {
                return null;
            }
        }

        private Element MyTradeWindow => GetTradingWindow()?.Children[0];
        private Element TheirTradeWindow => GetTradingWindow()?.Children[1];
        private Vector2 WindowOffset => GameController.Window.GetWindowRectangleTimeCache.TopLeft;
    }
}