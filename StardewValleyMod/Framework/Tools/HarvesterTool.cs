using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using SpaceCore;
using SpaceShared;
using StardewValley;
using StardewValley.Tools;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValleyMod.Framework;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Net.Mail;
using StardewValley.Characters;
using SObject = StardewValley.Object;

namespace StardewValleyMod
{
    [XmlType("Mods_pegoons_HarvesterScythe")]
    public class HarvesterTool : BaseTool
    {
        // Texture for custom tool to use
        internal static Texture2D Texture;

        // Saves previous harvest spot
        private Vector2 LastGrowOrigin;

        public HarvesterTool()
        {
            this.Category = StardewValley.Object.toolCategory;
            this.Name = "Harvesting Scythe";
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

        public override Item getOne(){ return new HarvesterTool(); }

        protected override string loadDisplayName(){ return "Harvesting Scythe"; }

        protected override string loadDescription(){ return "Scythe for harvesting crops in a wide radius."; }

        public override int salePrice() { return 500; }

        public override string getDescription() { return this.loadDescription(); }

        // Source: spacechase0/BugNet/ButNetTool.cs L:56
        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            float num1 = 0.0f;
            if (MeleeWeapon.defenseCooldown > 0)
                num1 = MeleeWeapon.defenseCooldown / 1500f;
            float num2 = ModEntry.Instance.Helper.Reflection.GetField<float>(typeof(MeleeWeapon), "addedSwordScale").GetValue();
            if (!drawShadow)
                num2 = 0;
            spriteBatch.Draw(HarvesterTool.Texture, location + (this.type.Value == 1 ? new Vector2(Game1.tileSize * 2 / 3, Game1.tileSize / 3) :
                new Vector2(Game1.tileSize / 2, Game1.tileSize / 2)), new Rectangle(0, 0, 16, 16), Color.White * transparency, 0.0f, new Vector2(8f, 8f),
                Game1.pixelZoom * (scaleSize + num2), SpriteEffects.None, layerDepth);
            if (num1 <= 0.0 || drawShadow)
                return;
            spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X, (int)location.Y + (Game1.tileSize - (int)(num1 * (double)Game1.tileSize)),
                Game1.tileSize, (int)(num1 * (double)Game1.tileSize)), Color.Red * 0.66f);
        }

        public override void tickUpdate(GameTime time, Farmer who)
        {
            base.tickUpdate(time, who);
            if(who.FarmerSprite.isUsingWeapon())
            {
                
                Vector2 toolLoc = who.GetToolLocation(true);
                Vector2 a = Vector2.Zero, b = Vector2.Zero;
                Rectangle area = this.getAreaOfEffect((int)toolLoc.X, (int)toolLoc.Y, who.facingDirection, ref a, ref b, who.GetBoundingBox(),
                    who.FarmerSprite.currentAnimationIndex);

                Vector2 playerTile = Game1.player.getTileLocation();
                if(playerTile != this.LastGrowOrigin)
                {
                    //this.Grow(playerTile, radius: 3);
                    this.Harvest(playerTile, radius: 2);
                    this.LastGrowOrigin = playerTile;
                }
                
            }
        }

        /// <summary>Grow crops and trees around the given position.</summary>
        /// <param name="origin">The origin around which to grow crops and trees.</param>
        /// <param name="radius">The number of tiles in each direction to include, not counting the origin.</param>
        public void Harvest(Vector2 origin, int radius)
        {
            // get location
            GameLocation location = Game1.currentLocation;
            if (location == null)
                return;

            // check tile area
            foreach (Vector2 tile in GetTileArea(origin, radius))
            {
                // get target
                object? target = null;
                {
                    // terrain feature
                    if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrainFeature))
                    {
                        if (terrainFeature is HoeDirt dirt)
                            target = dirt;
                            //target = dirt.crop;
                        else if (terrainFeature is Bush or FruitTree or Tree)
                            target = terrainFeature;
                    }

                    // indoor pot
                    if (target == null && location.objects.TryGetValue(tile, out SObject obj) && obj is IndoorPot pot)
                    {
                        if (pot.hoeDirt.Value is { } dirt)
                            target = dirt;
                            //target = dirt.crop;

                        if (pot.bush.Value is { } bush)
                            target = bush;
                    }
                }

                switch(target)
                {
                    case HoeDirt dirt:
                        if (dirt.crop == null)
                            break;
                        
                        HoeDirt temp = (HoeDirt)target;
                        if(dirt.crop.harvest((int)tile.X, (int)tile.Y, dirt))
                        {
                            bool isScytheCrop = dirt.crop.harvestMethod.Value == Crop.sickleHarvest;
                            dirt.destroyCrop(tile, showAnimation: isScytheCrop, location);
                            if (!isScytheCrop && location is IslandLocation && Game1.random.NextDouble() < 0.05)
                                Game1.player.team.RequestLimitedNutDrops("IslandFarming", location, (int)tile.X * 64, (int)tile.Y * 64, 5);
                            break;
                        }
                        if (dirt.crop.hitWithHoe((int)tile.X, (int)tile.Y, location, dirt))
                        {
                            dirt.destroyCrop(tile, showAnimation: false, location);
                            break;
                        }
                        /*
                        for(int i = 0; i < 100 && temp.crop.fullyGrown.Value; i++)
                        {
                            if (temp.crop.harvest((int)tile.X, (int)tile.Y, temp))
                            {
                                bool isScytheCrop = dirt.crop.harvestMethod.Value == Crop.sickleHarvest;

                                dirt.destroyCrop(tile, showAnimation: isScytheCrop, location);
                                if (!isScytheCrop && location is IslandLocation && Game1.random.NextDouble() < 0.05)
                                    Game1.player.team.RequestLimitedNutDrops("IslandFarming", location, (int)tile.X * 64, (int)tile.Y * 64, 5);
                            }
                        }
                        */
                        break;

                    case Bush bush when bush.size.Value == Bush.greenTeaBush && bush.getAge() == Bush.daysToMatureGreenTeaBush:
                        bush.performUseAction(tile, location);
                        break;

                    case FruitTree fruitTree when !fruitTree.stump.Value && fruitTree.fruitsOnTree.Value > 0:
                        fruitTree.shake(tile, false, location);
                        break;

                }
            }
        }

        /*
            /// <summary>Grow crops and trees around the given position.</summary>
            /// <param name="origin">The origin around which to grow crops and trees.</param>
            /// <param name="radius">The number of tiles in each direction to include, not counting the origin.</param>
            public void Grow(Vector2 origin, int radius)
        {
            // get location
            GameLocation location = Game1.currentLocation;
            if (location == null)
                return;

            // check tile area
            foreach (Vector2 tile in GetTileArea(origin, radius))
            {
                // get target
                object? target = null;
                {
                    // terrain feature
                    if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrainFeature))
                    {
                        if (terrainFeature is HoeDirt dirt)
                            target = dirt.crop;
                        else if (terrainFeature is Bush or FruitTree or Tree)
                            target = terrainFeature;
                    }

                    // indoor pot
                    if (target == null && location.objects.TryGetValue(tile, out SObject obj) && obj is IndoorPot pot)
                    {
                        if (pot.hoeDirt.Value is { } dirt)
                            target = dirt.crop;

                        if (pot.bush.Value is { } bush)
                            target = bush;
                    }
                }

                // grow target
                switch (target)
                {
                    case Crop crop:
                        // grow crop using newDay to apply full logic like giant crops, wild seed randomization, etc
                        for (int i = 0; i < 100 && !crop.fullyGrown.Value; i++)
                            crop.newDay(HoeDirt.watered, HoeDirt.fertilizerHighQuality, (int)tile.X, (int)tile.Y, location);

                        // trigger regrowth logic for multi-harvest crops
                        crop.growCompletely();
                        break;

                    case Bush bush when bush.size.Value == Bush.greenTeaBush && bush.getAge() < Bush.daysToMatureGreenTeaBush:
                        bush.datePlanted.Value = (int)(Game1.stats.DaysPlayed - Bush.daysToMatureGreenTeaBush);
                        bush.dayUpdate(location, tile); // update source rect, grow tea leaves, etc
                        break;

                    case FruitTree fruitTree when !fruitTree.stump.Value && fruitTree.growthStage.Value < FruitTree.treeStage:
                        fruitTree.growthStage.Value = Tree.treeStage;
                        fruitTree.daysUntilMature.Value = 0;
                        break;

                    case Tree tree when !tree.stump.Value && tree.growthStage.Value < Tree.treeStage:
                        tree.growthStage.Value = Tree.treeStage;
                        break;
                }
            }
        }
        */

        /// <summary>Get the tile coordinates in a map area.</summary>
        /// <param name="origin">The center tile coordinate.</param>
        /// <param name="radius">The number of tiles in each direction to include, not counting the origin.</param>
        public static IEnumerable<Vector2> GetTileArea(Vector2 origin, int radius)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                    yield return new Vector2(origin.X + x, origin.Y + y);
            }
        }

        // Source: spacechase0/BugNet/BugNetTools.cs L:110
        public override void Draw(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, Rectangle sourceRect, int type, bool isOnSpecial)
        {
            var meleeWeaponCenter = new Vector2(1f, 15f);
            sourceRect = new Rectangle(0, 0, 16, 16);

            switch (facingDirection)
            {
                case 1:
                    switch (frameOfFarmerAnimation)
                    {
                        case 0:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 40f, (float)(playerPosition.Y - (double)Game1.tileSize + 8.0)), sourceRect, Color.White, -0.7853982f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - 1) / 10000f));
                            break;
                        case 1:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 56f, (float)(playerPosition.Y - (double)Game1.tileSize + 28.0)), sourceRect, Color.White, 0.0f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - 1) / 10000f));
                            break;
                        case 2:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + Game1.tileSize - Game1.pixelZoom, playerPosition.Y - 4 * Game1.pixelZoom), sourceRect, Color.White, 0.7853982f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - 1) / 10000f));
                            break;
                        case 3:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + Game1.tileSize - Game1.pixelZoom, playerPosition.Y - Game1.pixelZoom), sourceRect, Color.White, 1.570796f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 4:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + Game1.tileSize - 7 * Game1.pixelZoom, playerPosition.Y + Game1.pixelZoom), sourceRect, Color.White, 1.963495f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 5:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + Game1.tileSize - 12 * Game1.pixelZoom, playerPosition.Y + Game1.pixelZoom), sourceRect, Color.White, 2.356194f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 6:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + Game1.tileSize - 12 * Game1.pixelZoom, playerPosition.Y + Game1.pixelZoom), sourceRect, Color.White, 2.356194f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 7:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2((float)(playerPosition.X + (double)Game1.tileSize - 16.0), (float)(playerPosition.Y + (double)Game1.tileSize + 12.0)), sourceRect, Color.White, 1.963495f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                    }
                    break;

                case 3:
                    switch (frameOfFarmerAnimation)
                    {
                        case 0:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X - 4 * Game1.pixelZoom, playerPosition.Y - Game1.tileSize - 4 * Game1.pixelZoom), sourceRect, Color.White, 0.7853982f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (f.getStandingY() - 1) / 10000f));
                            break;
                        case 1:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X - 12 * Game1.pixelZoom, playerPosition.Y - Game1.tileSize + 5 * Game1.pixelZoom), sourceRect, Color.White, 0.0f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (f.getStandingY() - 1) / 10000f));
                            break;
                        case 2:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X - Game1.tileSize + 8 * Game1.pixelZoom, playerPosition.Y + 4 * Game1.pixelZoom), sourceRect, Color.White, -0.7853982f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (f.getStandingY() - 1) / 10000f));
                            break;
                        case 3:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + Game1.pixelZoom, playerPosition.Y + 11 * Game1.pixelZoom), sourceRect, Color.White, -1.570796f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 4:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 11 * Game1.pixelZoom, playerPosition.Y + 13 * Game1.pixelZoom), sourceRect, Color.White, -1.963495f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 5:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 20 * Game1.pixelZoom, playerPosition.Y + 10 * Game1.pixelZoom), sourceRect, Color.White, -2.356194f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 6:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 20 * Game1.pixelZoom, playerPosition.Y + 10 * Game1.pixelZoom), sourceRect, Color.White, -2.356194f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 7:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X - 44f, playerPosition.Y + 96f), sourceRect, Color.White, -5.105088f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipVertically, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                    }
                    break;

                case 0:
                    switch (frameOfFarmerAnimation)
                    {
                        case 0:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 32f), sourceRect, Color.White, -2.356194f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 1:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 48f), sourceRect, Color.White, -1.570796f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 2:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), sourceRect, Color.White, -3f * (float)Math.PI / 8f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 3:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), sourceRect, Color.White, -0.3926991f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 4:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2((float)(playerPosition.X + (double)Game1.tileSize - 8.0), playerPosition.Y - 40f), sourceRect, Color.White, 0.0f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 5:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + Game1.tileSize, playerPosition.Y - 40f), sourceRect, Color.White, 0.3926991f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 6:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + Game1.tileSize, playerPosition.Y - 40f), sourceRect, Color.White, 0.3926991f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 7:
                            spriteBatch.Draw(HarvesterTool.Texture, new Vector2((float)(playerPosition.X + (double)Game1.tileSize - 44.0), playerPosition.Y + Game1.tileSize), sourceRect, Color.White, -1.963495f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                    }
                    break;

                case 2:
                    {
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 56f, playerPosition.Y - 16f), sourceRect, Color.White, 0.3926991f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                            case 1:
                                spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 52f, playerPosition.Y - 8f), sourceRect, Color.White, 1.570796f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                            case 2:
                                spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 40f, playerPosition.Y), sourceRect, Color.White, 1.570796f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                            case 3:
                                spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 16f, playerPosition.Y + 4f), sourceRect, Color.White, 2.356194f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                            case 4:
                                spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 8f, playerPosition.Y + 8f), sourceRect, Color.White, 3.141593f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                            case 5:
                                spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 12f, playerPosition.Y), sourceRect, Color.White, 3.534292f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                            case 6:
                                spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 12f, playerPosition.Y), sourceRect, Color.White, 3.534292f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                            case 7:
                                spriteBatch.Draw(HarvesterTool.Texture, new Vector2(playerPosition.X + 44f, playerPosition.Y + Game1.tileSize), sourceRect, Color.White, -5.105088f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                        }
                        break;
                    }
            }
        }

    }
}