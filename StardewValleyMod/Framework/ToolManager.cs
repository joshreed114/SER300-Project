using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewValleyMod.Framework
{
    public class ToolManager
    {
        /// <summary>The cheat implementations which should be notified of update ticks and saves.</summary>
        private readonly List<MeleeWeapon> ToolsWhichNeedUpdate = new();

        public IReflectionHelper Reflection;

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used via reflection")]
        public HarvesterTool harvesterTool = new HarvesterTool();

        // Construct an instance
        public ToolManager(IReflectionHelper reflection)
        {
            this.Reflection = reflection;
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUpdateTicked(UpdateTickedEventArgs e)
        {
            
        }
    }
}

