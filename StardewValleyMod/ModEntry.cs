using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace StardewTest
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        // ***PUBLIC***
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            helper.Events.GameLoop.DayStarted += this.OnDayStart;

            helper.Events.Player.InventoryChanged += this.OnInventoryChange;
        }

        // ***PRIVATE***
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);

            SButtonState state = this.Helper.Input.GetState(SButton.OemTilde);
            if (state == SButtonState.Pressed)
            {
                Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(72, 1, false, 1000, 0));
                this.Monitor.Log($"Item added to inventory");
            }
        }

        private void OnDayStart(object sender, DayStartedEventArgs e)
        {
            this.Monitor.Log($"{Game1.player.Name} started a new day!", LogLevel.Debug);
        }

        private void OnInventoryChange(object sender, InventoryChangedEventArgs e)
        {
            if (e.Added != null)
                this.Monitor.Log($"{e.Player} added {e.QuantityChanged} {e.Added}", LogLevel.Debug);
            else if (e.Removed != null)
                this.Monitor.Log($"{e.Player} removed {e.QuantityChanged} {e.Added}", LogLevel.Debug);
        }

    }
}