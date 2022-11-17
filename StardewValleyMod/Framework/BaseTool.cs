using System;
using SpaceCore;
using SpaceShared;
using StardewValley;
using StardewValley.Tools;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SObject = StardewValley.Object;

namespace StardewValleyMod.Framework
{
    public class BaseTool : MeleeWeapon, ICustomWeaponDraw, ITool
    {
        public BaseTool()
        {
            this.Category = StardewValley.Object.toolCategory;
            this.Name = "???";
            this.ParentSheetIndex = MeleeWeapon.scythe;

            this.minDamage.Value = 1;
            this.maxDamage.Value = 1;
            this.knockback.Value = 1;
            this.speed.Value = 0;
            this.addedPrecision.Value = 0;
            this.addedDefense.Value = 0;
            this.type.Value = 3; // ?
            this.addedAreaOfEffect.Value = 0;
            this.critChance.Value = 0.02f;
            this.critMultiplier.Value = 1;

            this.Stack = 1;
        }

        public override Item getOne() { return new BaseTool(); }

        protected override string loadDisplayName() { return ""; }

        protected override string loadDescription() { return ""; }

        public override int salePrice() { return -1; }

        public override string getDescription() { return "";  }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth,
            StackDrawType drawStackNumber, Color color, bool drawShadow) { }

        public override void tickUpdate(GameTime time, Farmer who) { }

        public virtual void Draw(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition,
            Farmer f, Rectangle sourceRect, int type, bool isOnSpecial) { }

        // TODO: Add
        public virtual void OnUpdated(UpdateTickedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
