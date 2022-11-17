using System;
using System.Collections.Generic;
using Netcode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewModdingAPI.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;
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

        //TODO add config
        //this.Config = helper.ReadConfig<ModConfig>(); // ?

        // hook events
        IModEvents events = helper.Events;

            events.GameLoop.GameLaunched += this.OnGameLaunched;
            events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            events.Input.ButtonPressed += this.OnButtonPressed;
            events.Player.InventoryChanged += this.OnInventoryChange;
            events.Display.MenuChanged += this.OnMenuChanged;
            events.GameLoop.UpdateTicked += this.OnUpdateTicked;

            // * Harvester Tool
            //HarvesterTool.Texture = helper.ModContent.Load<IRawTextureData>("assets/harvesterscythe.png"); // IRawTextureData
            HarvesterTool.Texture = helper.ModContent.Load<Texture2D>("assets/harvesterscythe.png"); // Texture2D
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

            spaceCore.RegisterSerializerType(typeof(HarvesterTool));

            if (jsonAssets != null)
                jsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets", "json-assets"));
            else
                Monitor.Log("Can't load Json Assets API, which is needed for test mod to function", LogLevel.Error);
        }

        // Adds custom tool to Pierre's shop
        /// <inheritdoc cref="IDisplayEvents.MenuChanged">
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is not ShopMenu { portraitPerson: { Name: "Pierre" } } pierreMenu)
                return;

            Monitor.Log("Adding Harvester Tool to Pierre's shop.");

            var forSale = pierreMenu.forSale;
            var itemPriceAndStock = pierreMenu.itemPriceAndStock;

            var tool = new HarvesterTool();
            forSale.Add(tool);
            itemPriceAndStock.Add(tool, new[] { 3000, 1 });
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
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
                    Monitor.Log($"Test item ID is {kiwiID}.", LogLevel.Info);
                }
                if (kiwiSeedsID == -1)
                {
                    Monitor.Log("Can't get ID for Kiwi Seeds", LogLevel.Warn);
                }
                else
                {
                    Monitor.Log($"Test item ID is {kiwiSeedsID}.", LogLevel.Info);
                }
            }
        }

        // Test method for tick updates
        /// <inheritdoc cref="IGameLoopEvents.UpdateTicked"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            this.ToolManager.OnUpdateTicked(e);
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

            // print button presses to the console window
            //this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);

            // if the '~' key is pressed, adds a kiwi to the players inventory
            SButtonState state = this.Helper.Input.GetState(SButton.OemTilde);
            if (state == SButtonState.Pressed)
            {
                Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(kiwiID, 1, false, 1000, 0));
                this.Monitor.Log($"Item added to inventory");
            }
        }

        // Test Method for SMAPI logging
        /// <inheritdoc cref="IGameLoopEvents.InventoryChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnInventoryChange(object sender, InventoryChangedEventArgs e)
        {
            // displays items added
            if (e.Added != null)
                this.Monitor.Log($"{e.Player} added {e.QuantityChanged} {e.Added}", LogLevel.Debug);
            // displays items removed
            else if (e.Removed != null)
                this.Monitor.Log($"{e.Player} removed {e.QuantityChanged} {e.Removed}", LogLevel.Debug);
        }

        /*******
         * Helper Methods
        *******/
    }
}