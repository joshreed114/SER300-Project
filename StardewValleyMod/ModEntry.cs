using System;
using System.Collections.Generic;
using Netcode;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValleyMod.Framework;
using System.IO;

namespace StardewValleyMod
{
    public interface IJsonAssetsApi
    {
        int GetObjectId(string name);
        void LoadAssets(string path);
    }

    public class ModEntry : Mod
    {
        /******
         * Fields
         *****/
        private IJsonAssetsApi jsonAssets;
        private ISpaceCoreApi spaceCore;

        // TODO: unused?
        private ToolManager ToolManager;

        // TESTING
        // Custom Object IDs 
        int kiwiID;
        int kiwiSeedsID;


        /******
         * Accessors
         ******/
        public static Mod Instance;


        /*******
         * Public Methods
        *******/
        public override void Entry(IModHelper helper)
        {
            ModEntry.Instance = this;

            this.ToolManager = new ToolManager(this.Helper.Reflection);

            //TODO add config file ?
            //this.Config = helper.ReadConfig<ModConfig>();

            // hook events
            IModEvents events = helper.Events;

            events.GameLoop.GameLaunched += this.OnGameLaunched;
            events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            events.Input.ButtonPressed += this.OnButtonPressed;
            events.Display.MenuChanged += this.OnMenuChanged;
            events.GameLoop.UpdateTicked += this.OnUpdateTicked;

            // Loading Textures

            // Harvester Tool
            //HarvesterTool.Texture = helper.ModContent.Load<IRawTextureData>("assets/harvesterscythe.png"); // IRawTextureData
            HarvesterTool.Texture = helper.ModContent.Load<Texture2D>("assets/harvesterscythe.png"); // Texture2D

            // Grow Tool
            //GrowTool.Texture = helper.ModContent.Load<IRawTextureData>("assets/growwand.png"); // IRawTextureData
            GrowTool.Texture = helper.ModContent.Load<Texture2D>("assets/growwand.png"); // Texture2D
        }


        /*******
         * Private Methods
        *******/

        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            //  Gets APIs from other Frameworks/Mods
            jsonAssets = this.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            spaceCore = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");

            // Register Xml Serializer Types for custom tools
            spaceCore.RegisterSerializerType(typeof(HarvesterTool));
            spaceCore.RegisterSerializerType(typeof(GrowTool));

            if (jsonAssets != null)
                jsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets", "json-assets"));
            else
                Monitor.Log("Can't load Json Assets API, which is needed for test mod to function", LogLevel.Error);
        }

        /// <inheritdoc cref="IDisplayEvents.MenuChanged">
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // Adds custom tools to Pierre's shop
            if (e.NewMenu is not ShopMenu { portraitPerson: { Name: "Pierre" } } pierreMenu)
                return;

            Monitor.Log("Adding Harvester Tool to Pierre's shop.");

            var forSalePierre = pierreMenu.forSale;
            var itemPriceAndStockPierre = pierreMenu.itemPriceAndStock;

            var harvestingTool = new HarvesterTool();
            var growTool = new GrowTool();

            forSalePierre.Add(harvestingTool);
            itemPriceAndStockPierre.Add(harvestingTool, new[] { 5000, 1 });
            forSalePierre.Add(growTool);
            itemPriceAndStockPierre.Add(growTool, new[] { 10000, 1 });
        }

        /// <inheritdoc cref="IGameLoopEvents.UpdateTicked"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            this.ToolManager.OnUpdateTicked(e);
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // Testing
            // Use to test if custom objects are added using json assets api

            // Logs a warning if items are not correctly loaded into the game
            if (jsonAssets != null)
            {
                kiwiID = jsonAssets.GetObjectId("Kiwi");
                kiwiSeedsID = jsonAssets.GetObjectId("Kiwi Seeds");

                if (kiwiID == -1)
                {
                    Monitor.Log("Can't get ID for Kiwi Item", LogLevel.Warn);
                }
                else
                {
                    //Monitor.Log($"Test item ID is {kiwiID}.", LogLevel.Info);
                }
                if (kiwiSeedsID == -1)
                {
                    Monitor.Log("Can't get ID for Kiwi Seeds", LogLevel.Warn);
                }
                else
                {
                    //Monitor.Log($"Test item ID is {kiwiSeedsID}.", LogLevel.Info);
                }
            }
        }

        // Test method for button events
        /// <inheritdoc cref="IGameLoopEvents.ButtonPressed"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // Testing

            /*
            // print button presses to the console window
            //this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);

            // if the '~' key is pressed, adds a kiwi to the players inventory
            SButtonState state = this.Helper.Input.GetState(SButton.OemTilde);
            if (state == SButtonState.Pressed)
            {
                Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(kiwiID, 1, false, 1000, 0));
                this.Monitor.Log($"Item added to inventory");
            }
            */
        }

        /*******
         * Helper Methods
        *******/
    }
}