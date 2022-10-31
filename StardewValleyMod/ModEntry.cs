using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewModdingAPI.Framework;
using StardewValley;
using StardewValley.Monsters;
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
        // JSONAssets API instance
        private IJsonAssetsApi JsonAssets;

        // Custom Object IDs 
        int kiwiID;
        int kiwiSeedsID;

        /*******
         * Public Methods
        *******/

        public override void Entry(IModHelper helper)
        {
            //TODO add config?
            //this.Config = helper.ReadConfig<ModConfig>();

            // hook events
            IModEvents events = helper.Events;

            events.GameLoop.GameLaunched += this.OnGameLaunched;

            events.GameLoop.SaveLoaded += this.OnSaveLoaded;

            events.Input.ButtonPressed += this.OnButtonPressed;

            events.Player.InventoryChanged += this.OnInventoryChange;

            events.GameLoop.UpdateTicked += this.OnUpdateTicked;

            //events.Player.Warped += this.OnWarped;

            // * Harvester Tool -- Buy from Clint? *

            //

        }

        /*******
         * Private Methods
        *******/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            JsonAssets = this.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (JsonAssets == null)
            {
                Monitor.Log("Can't load Json Assets API, which is needed for test mod to function", LogLevel.Error);
            }
            else
            {
                JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets", "json-assets"));
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (JsonAssets != null)
            {
                kiwiID = JsonAssets.GetObjectId("Kiwi");
                kiwiSeedsID = JsonAssets.GetObjectId("Kiwi Seeds");
                
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

        /// <inheritdoc cref="IGameLoopEvents.UpdateTicked"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {

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

        // Helper Methods
        

    }
}