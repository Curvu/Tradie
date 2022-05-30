using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.Shared.Enums;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using ExileCore.PoEMemory.MemoryObjects;

namespace Tradie
{
    public partial class Core : BaseSettingsPlugin<Settings>
    {
        private readonly List<string> _whiteListedPaths = new List<string>
        {
                "Art/2DItems/Currency",
                "Art/2DItems/Divination",
                //TODO, maps are loading strange and causing too many draw issues
                "Art/2DItems/Maps",

        };

        private List<string> _initializedImageList = new List<string>();

        public override void Render()
        {
            var Area = GameController.Game.IngameState.Data.CurrentArea;
            if (!Area.IsTown && !Area.RawName.Contains("Hideout")) return;
            ShowNpcTradeItems();
            ShowPlayerTradeItems();
        }

        private void ShowNpcTradeItems()
        {
            if (!NpcTradeWindowVisible && Settings.UseCoreNPCTrade == false)
                return;

            SellWindowHideout npcTradingWindow = Settings.UseCoreNPCTrade ? GameController.Game.IngameState.IngameUi.SellWindowHideout
                :GetNpcTradeWindow() as SellWindowHideout
                ;

            if (npcTradingWindow == null || npcTradingWindow.IsVisible == false)
            {
                return;
            }

            var tradingItems = GetItemsInSellWindow(npcTradingWindow);



            var ourData = new ItemDisplay
            {
                Items = GetItemObjects(tradingItems.ourItems).OrderBy(item => item.Path),
                
                X = Settings.UseHorizon ? 
                    (int)npcTradingWindow.YourOffer.GetClientRect().TopLeft.X 
                    : Settings.YourItemStartingLocationX,

                Y = Settings.UseHorizon ? 
                    (int)npcTradingWindow.YourOffer.GetClientRect().TopLeft.Y - Settings.ImageSize 
                    : Settings.YourItemStartingLocationY,
                TextSize = Settings.TextSize,
                TextColor = Settings.YourItemTextColor,
                BackgroundColor = Settings.YourItemBackgroundColor,
                BackgroundTransparency = Settings.YourItemBackgroundColor.Value.A,
                ImageSize = Settings.ImageSize,
                Spacing = Settings.Spacing,
                LeftAlignment = Settings.YourItemsImageLeftOrRight,
                Ascending = Settings.YourItemsAscending
            };
            var theirData = new ItemDisplay
            {
                Items = GetItemObjects(tradingItems.theirItems).OrderBy(item => item.Path),
                X =Settings.UseHorizon ?  (int)npcTradingWindow.OtherOffer.GetClientRect().TopLeft.X
                    : Settings.TheirItemStartingLocationX,
                Y =Settings.UseHorizon ?  (int)npcTradingWindow.OtherOffer.GetClientRect().TopLeft.Y - Settings.ImageSize: Settings.TheirItemStartingLocationY,
                TextSize = Settings.TextSize,
                TextColor = Settings.TheirItemTextColor,
                BackgroundColor = Settings.TheirItemBackgroundColor,
                BackgroundTransparency = Settings.TheirItemBackgroundColor.Value.A,
                ImageSize = Settings.ImageSize,
                Spacing = Settings.Spacing,
                LeftAlignment = Settings.TheirItemsImageLeftOrRight,
                Ascending = Settings.TheirItemsAscending
            };
            if (ourData.Items.Any())
                DrawCurrency(ourData);
            if (theirData.Items.Any())
                DrawCurrency(theirData);
        }

        private void ShowPlayerTradeItems()
        {
            

            if (!TradingWindowVisible && Settings.UseCoreYourItems == false)
                return;

            TradeWindow tradingWindow = Settings.UseCoreYourItems ? GameController.Game.IngameState.IngameUi.TradeWindow
                :GetTradingWindow() as TradeWindow 
                ;
            if (tradingWindow == null || tradingWindow.IsVisible == false)
            {
                return;
            }

            var tradingItems = GetItemsInTradingWindow(tradingWindow);
            var ourData = new ItemDisplay
            {
                Items = GetItemObjects(tradingItems.ourItems).OrderBy(item => item.Path),
                X =Settings.UseHorizon ? (int)tradingWindow.OtherOfferElement.GetClientRect().TopLeft.X
                    :  Settings.YourItemStartingLocationX,
                Y =Settings.UseHorizon ? (int)tradingWindow.OtherOfferElement.GetClientRect().TopLeft.Y - Settings.ImageSize
                    :   Settings.YourItemStartingLocationY,
                TextSize = Settings.TextSize,
                TextColor = Settings.YourItemTextColor,
                BackgroundColor = Settings.YourItemBackgroundColor,
                BackgroundTransparency = Settings.YourItemBackgroundColor.Value.A,
                ImageSize = Settings.ImageSize,
                Spacing = Settings.Spacing,
                LeftAlignment = Settings.YourItemsImageLeftOrRight,
                Ascending = Settings.YourItemsAscending
            };
            var theirData = new ItemDisplay
            {
                Items = GetItemObjects(tradingItems.theirItems).OrderBy(item => item.Path),
                X =Settings.UseHorizon ? (int)tradingWindow.YourOfferElement.GetClientRect().TopLeft.X
                    :   Settings.TheirItemStartingLocationX,
                Y =Settings.UseHorizon ? (int)tradingWindow.YourOfferElement.GetClientRect().TopLeft.Y - Settings.ImageSize
                    :   Settings.TheirItemStartingLocationY,
                TextSize = Settings.TextSize,
                TextColor = Settings.TheirItemTextColor,
                BackgroundColor = Settings.TheirItemBackgroundColor,
                BackgroundTransparency = Settings.TheirItemBackgroundColor.Value.A,
                ImageSize = Settings.ImageSize,
                Spacing = Settings.Spacing,
                LeftAlignment = Settings.TheirItemsImageLeftOrRight,
                Ascending = Settings.TheirItemsAscending
            };
            if (ourData.Items.Any())
                DrawCurrency(ourData);
            if (theirData.Items.Any())
                DrawCurrency(theirData);
        }

        private void DrawCurrency(ItemDisplay data)
        {
            const string symbol = "-";
            var counter = 0;
            var newColor = data.BackgroundColor;
            newColor.A = (byte)data.BackgroundTransparency;
            var maxCount = data.Items.Max(i => i.Amount);
            var lengthText = Graphics.MeasureText(symbol + " " + maxCount, data.TextSize).X;

            if (Settings.UseHorizon)
            {
                var background = new RectangleF(data.X, data.Y,
                    (data.ImageSize + data.Spacing * 2 + lengthText) * data.Items.Count(),
                    data.ImageSize);

                Graphics.DrawBox(background, newColor);

                foreach (var ourItem in data.Items)
                {
                    var imageBox = new RectangleF(
                        data.X + counter * (data.ImageSize + data.Spacing * 2 + lengthText),
                        data.Y,
                        data.ImageSize, data.ImageSize);

                    Graphics.DrawImageGui(ourItem.Path, imageBox, new RectangleF(0, 0, 1, 1));

                    Graphics.DrawText($"{symbol} {ourItem.Amount}",
                        new Vector2(
                            imageBox.Center.X + imageBox.Width / 2 + data.Spacing / 2,
                            imageBox.Center.Y - data.TextSize / 2 - 3)
                        , data.TextColor,
                        FontAlign.Left);
                    counter++;
                }
            }
            else
            {
                var background = new RectangleF(data.LeftAlignment ? data.X : data.X + data.ImageSize, data.Y,
                    data.LeftAlignment
                        ? +data.ImageSize + data.Spacing + 3 +
                          Graphics.MeasureText(symbol + " " + maxCount, data.TextSize).X
                        : -data.ImageSize - data.Spacing - 3 -
                          Graphics.MeasureText(symbol + " " + maxCount, data.TextSize).X,
                    data.Ascending ? -data.ImageSize * data.Items.Count() : data.ImageSize * data.Items.Count());

                Graphics.DrawBox(background, newColor);

                foreach (var ourItem in data.Items)
                {
                    counter++;
                    var imageBox = new RectangleF(data.X,
                        data.Ascending
                            ? data.Y - counter * data.ImageSize
                            : data.Y - data.ImageSize + counter * data.ImageSize,
                        data.ImageSize, data.ImageSize);

                    Graphics.DrawImageGui(ourItem.Path, imageBox, new RectangleF(0, 0, 1, 1));

                    Graphics.DrawText(data.LeftAlignment ? $"{symbol} {ourItem.Amount}" : $"{ourItem.Amount} {symbol}",
                        new Vector2(data.LeftAlignment ? data.X + data.ImageSize + data.Spacing : data.X - data.Spacing,
                            imageBox.Center.Y - data.TextSize / 2 - 3), data.TextColor,
                        data.LeftAlignment ? FontAlign.Left : FontAlign.Right);
                }
            }
        }

        private bool TradingWindowVisible
        {
            get
            {
                var windowElement = GameController.IngameState.IngameUi.GetChildAtIndex(Settings.PlayerTradeIndex.Value).GetChildAtIndex(3);
                return windowElement != null && windowElement.IsVisible;
            }
        }

        private Element GetTradingWindow()
        {
            try
            {
                return GameController.IngameState.IngameUi.GetChildAtIndex(Settings.PlayerTradeIndex.Value).GetChildAtIndex(3).GetChildAtIndex(1).GetChildAtIndex(0).GetChildAtIndex(0);
            }
            catch
            {
                return null;
            }
        }

        private bool NpcTradeWindowVisible
        {
            get
            {
                var windowElement = GameController.IngameState.IngameUi.GetChildAtIndex(Settings.NPCTradeIndex.Value).GetChildAtIndex(3);
                return windowElement != null && windowElement.IsVisible;
            }
        }

        private Element GetNpcTradeWindow()
        {
            try
            {
                return GameController.IngameState.IngameUi.GetChildAtIndex(Settings.NPCTradeIndex.Value).GetChildAtIndex(3);
            }
            catch
            {
                return null;
            }
        }

        private (List<NormalInventoryItem> ourItems, List<NormalInventoryItem> theirItems) GetItemsInSellWindow(SellWindowHideout tradingWindow)
        {
            var ourItems = new List<NormalInventoryItem>();
            var theirItems = new List<NormalInventoryItem>();
            if (tradingWindow.ChildCount < 2)
            {
                return (ourItems, theirItems);
            }

            var ourItemsElement = tradingWindow.YourOffer;
            var theirItemsElement = tradingWindow.OtherOffer;


            // We are skipping the first, since it's a Element ("Place items you want to trade here") that we don't need.
            // 
            // skipping the first item as its a strange object added after 3.3 Incursion
            foreach (var ourElement in ourItemsElement.Children.Skip(1))
            {
                var normalInventoryItem = ourElement.AsObject<NormalInventoryItem>();
                if (normalInventoryItem == null)
                {
                    LogMessage("Tradie: OurItem was null!", 5);
                    throw new Exception("Tradie: OurItem was null!");
                }

                ourItems.Add(normalInventoryItem);
            }

            // skipping the first item as its a strange object added after 3.3 Incursion
            foreach (var theirElement in theirItemsElement.Children.Skip(1))
            {
                var normalInventoryItem = theirElement.AsObject<NormalInventoryItem>();
                if (normalInventoryItem == null)
                {
                    LogMessage("Tradie: OurItem was null!", 5);
                    throw new Exception("Tradie: OurItem was null!");
                }

                theirItems.Add(normalInventoryItem);
            }

            return (ourItems, theirItems);
        }
        
        private (List<NormalInventoryItem> ourItems, List<NormalInventoryItem> theirItems) GetItemsInTradingWindow(TradeWindow tradingWindow)
        {
            var ourItems = new List<NormalInventoryItem>();
            var theirItems = new List<NormalInventoryItem>();
            if (tradingWindow.ChildCount < 2)
            {
                return (ourItems, theirItems);
            }

            var ourItemsElement = tradingWindow.OtherOfferElement;
            var theirItemsElement = tradingWindow.YourOfferElement;


            // We are skipping the first, since it's a Element ("Place items you want to trade here") that we don't need.
            // 
            // skipping the first item as its a strange object added after 3.3 Incursion
            foreach (var ourElement in ourItemsElement.Children.Skip(1))
            {
                var normalInventoryItem = ourElement.AsObject<NormalInventoryItem>();
                if (normalInventoryItem == null)
                {
                    LogMessage("Tradie: OurItem was null!", 5);
                    throw new Exception("Tradie: OurItem was null!");
                }

                ourItems.Add(normalInventoryItem);
            }

            // skipping the first item as its a strange object added after 3.3 Incursion
            foreach (var theirElement in theirItemsElement.Children.Skip(1))
            {
                var normalInventoryItem = theirElement.AsObject<NormalInventoryItem>();
                if (normalInventoryItem == null)
                {
                    LogMessage("Tradie: OurItem was null!", 5);
                    throw new Exception("Tradie: OurItem was null!");
                }

                theirItems.Add(normalInventoryItem);
            }

            return (ourItems, theirItems);
        }

        private bool IsWhitelisted(string metaData) => _whiteListedPaths.Any(metaData.Contains);

        private IEnumerable<Item> GetItemObjects(IEnumerable<NormalInventoryItem> normalInventoryItems)
        {
            var items = new List<Item>();
            foreach (var normalInventoryItem in normalInventoryItems)
                try
                {
                    if (normalInventoryItem.Item == null || normalInventoryItem.Item.Address < 1) continue;
                    var metaData = normalInventoryItem.Item.GetComponent<RenderItem>().ResourcePath;
                    if (metaData.Equals("")) continue;
                    if (!IsWhitelisted(metaData))
                        continue;
                    var stack = normalInventoryItem.Item.GetComponent<Stack>();
                    var amount = stack?.Info == null ? 1 : stack.Size;
                    var name = GetImagePath(metaData, normalInventoryItem);
                    var found = false;
                    foreach (var item in items)
                        if (item.ItemName.Equals(name))
                        {
                            item.Amount += amount;
                            found = true;
                            break;
                        }

                    if (found) continue;
                    var path = GetImagePath(metaData, normalInventoryItem);
                    items.Add(new Item(name, amount, path));
                }
                catch
                {
                    LogError("Tradie: Sometime went wrong in GetItemObjects() for a brief moment", 5);
                }

            return items;
        }

        private string GetImagePath(string metadata, NormalInventoryItem invItem)
        {
            metadata = metadata.Replace(".dds", ".png");
            var url = $"http://webcdn.pathofexile.com/image/{metadata}";
            var metadataPath = metadata.Replace('/', '\\');
            var fullPath = $"{DirectoryFullName}\\images\\{metadataPath}";

            /////////////////////////// Yucky Map bits ///////////////////////////////
            if (invItem.Item.HasComponent<Map>())
            {
                var isShapedMap = 0;
                var mapTier = invItem.Item.GetComponent<Map>().Tier;
                foreach (var itemList in invItem.Item.GetComponent<Mods>().ItemMods)
                {
                    if (!itemList.RawName.Contains("MapShaped")) continue;
                    isShapedMap = 1;
                    mapTier += 5;
                }

                url = $"http://webcdn.pathofexile.com/image/{metadata}?mn=1&mr={isShapedMap}&mt={mapTier}";
                fullPath = $"{DirectoryFullName}\\images\\{metadataPath.Replace(".png", "")}_{mapTier}_{isShapedMap}.png";
            }
            //

            if (File.Exists(fullPath))
            {
                if (_initializedImageList.All(x => x != fullPath))
                {
                    Graphics.InitImage($"{fullPath}", false);
                    _initializedImageList.Add(fullPath);
                }

                return fullPath;
            }

            var path = fullPath.Substring(0, fullPath.LastIndexOf('\\'));
            Directory.CreateDirectory(path);
            using (var client = new WebClient())
            {
                client.DownloadFile(new Uri(url), fullPath);
            }

            if (_initializedImageList.All(x => x != fullPath))
            {
                Graphics.InitImage($"{fullPath}", false);
                _initializedImageList.Add(fullPath);
            }

            return fullPath;
        }
    }
}