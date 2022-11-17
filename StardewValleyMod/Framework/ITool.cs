using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace StardewValleyMod.Framework
{
    internal interface ITool
    {
        public virtual Item getOne() { return new BaseTool(); }

        protected virtual string loadDisplayName() { return ""; }

        protected virtual string loadDescription() { return ""; }

        public virtual int salePrice() { return -1; }

        public virtual string getDescription() { return ""; }

        public virtual void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth,
            StackDrawType drawStackNumber, Color color, bool drawShadow)
        { }

        public virtual void tickUpdate(GameTime time, Farmer who) { }

        public virtual void Draw(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition,
            Farmer f, Rectangle sourceRect, int type, bool isOnSpecial)
        { }

        /// <summary>Handle a game update if <see cref="OnSaveLoaded"/> indicated updates were needed.</summary>
        /// <param name="context">The cheat context.</param>
        /// <param name="e">The update event arguments.</param>
        public virtual void OnUpdated(UpdateTickedEventArgs e) { }
    }
}

