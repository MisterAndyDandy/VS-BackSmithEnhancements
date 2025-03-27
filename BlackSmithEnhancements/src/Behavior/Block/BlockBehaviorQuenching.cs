using System;
using BlackSmithEnhancements.Behavior.Item;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace BlackSmithEnhancements.Behavior.Block
{
    public class BlockBehaviorQuenching : BlockBehavior
    {
        private long _lastPlaySizzleSoundMilliseconds;

        public BlockBehaviorQuenching(Vintagestory.API.Common.Block block) : base(block)
        {
        }
    
        public static SimpleParticleProperties InitializeSteamEffect(float intensity = 1)
        {
            SimpleParticleProperties steam;
            steam = new SimpleParticleProperties(
                8, 16,
                ColorUtil.ToRgba(50, 248, 248, 255), // first alpha, second red, three green, four blue
                new Vec3d(),
                new Vec3d(),
                new Vec3f(0.1f, 0.1f, 0.1f) * intensity,
                new Vec3f(0.2f, 0.3f, 0.2f) * intensity,
                1f,
                0.01f,
                0.2f * intensity,
                0.8f * intensity,
                EnumParticleModel.Quad
            )
            {
                AddPos = new Vec3d { X = 0, Y = 0, Z = 0 }.Set(0f, 0.2f, 0f),
                OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -250f),
                SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -0.03f),
                AddVelocity = new Vec3f(0.5f, 2f, 0.5f),
                VertexFlags = 100,
                ClimateColorMap = null,
                WindAffected = true,
                WindAffectednes = 0.1f,
                SelfPropelled = true
            };

            return steam;
        }

        public static WaterSplashParticles InitializeWaterSplashEffect()
        {
            WaterSplashParticles waterSplash;
            waterSplash = new WaterSplashParticles();
            return waterSplash;
        }

        //public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        //{
           
        //    if (blockSel != null || byEntity.Controls.ShiftKey)
        //    {
        //        IWorldAccessor world = byEntity.World;

        //        if (world == null) return;

        //        IPlayer byPlayer = world.PlayerByUid((byEntity as EntityPlayer)?.PlayerUID);

        //        if (byPlayer == null) return;


        //        long elapsedSeconds = world.Calendar.ElapsedSeconds - secondPasted;



        //        if (block.GetBlockEntity<BlockEntityGroundStorage>(blockSel) is BlockEntityGroundStorage groundStorage)
        //        {
        //            ItemStack blockStack = new ItemStack(groundStorage.Block);

        //            if (blockStack == null) return;

        //            float temp = blockStack.Collectible.GetTemperature(world, blockStack);

        //            if (blockStack.Collectible.HasTemperature(blockStack) && temp > 20.1f)
        //            {

        //                if (elapsedSeconds > 100)
        //                {
        //                    if (world.Side == EnumAppSide.Client)
        //                    {
        //                        world.PlaySoundAt(new AssetLocation("sounds/sizzle"), blockSel.FullPosition.X, blockSel.FullPosition.Y, blockSel.FullPosition.Z, byPlayer.Entity.World.PlayerByUid(byPlayer.Entity.PlayerUID), 1f, 4f, 0.8f);
        //                        Particles(world, new Vec3d(groundStorage.Pos.X + 0.5f, groundStorage.Pos.Y + 0.25f, groundStorage.Pos.Z + 0.5f), InitializeSteamEffect(), InitializeWaterSplashEffect());
        //                    }
        //                }

        //                blockStack.Collectible.SetTemperature(world, blockStack, GameMath.Max(0, temp - Math.Max(0f, GameMath.Max(0f, world.Rand.Next(10, 100)))), true);
        //                slot.Itemstack.Collectible.HeldTpUseAnimation = "interactstatic";
        //                secondPasted = world.Calendar.ElapsedSeconds;
        //                handling = EnumHandling.Handled;
        //            };
        //        }
        //    }
        //}


        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            if (blockSel == null || byPlayer.Entity.RightHandItemSlot.Empty)
                return false;
            if (block.GetBlockEntity<BlockEntityLiquidContainer>(blockSel) is not { } beLiquidContainer)
                return false;
            var heldStack = byPlayer.Entity.RightHandItemSlot.Itemstack;
            var temp = heldStack.Collectible.GetTemperature(world, heldStack);
            if (temp < 20.1f) return false;
            if (!heldStack.Collectible.HasBehavior<ItemBehaviorQuenching>()) return false;
            handling = EnumHandling.PreventDefault;
            if (world.ElapsedMilliseconds - _lastPlaySizzleSoundMilliseconds < 5000) return true;
            _lastPlaySizzleSoundMilliseconds = world.ElapsedMilliseconds;
            var volume = temp / 1500;
            world.PlaySoundAt(new AssetLocation("sounds/sizzle"), blockSel.FullPosition.X, blockSel.FullPosition.Y, blockSel.FullPosition.Z, byPlayer, 1f, 4f, volume);
            return true;
        }
        
        public override bool OnBlockInteractStep(
            float secondsUsed,
        IWorldAccessor world,
            IPlayer byPlayer,
        BlockSelection blockSel,
        ref EnumHandling handling)
        {
            handling = EnumHandling.PreventDefault;
            return false;
        }

        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel,
            ref EnumHandling handling)
        {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is not BlockEntityLiquidContainer beLiquidContainer) return;
            handling = EnumHandling.Handled;
            var heldStack = byPlayer.Entity.RightHandItemSlot.Itemstack;
            QuenchStack(world, heldStack, byPlayer, blockSel, beLiquidContainer);
        }

        public void QuenchStack(IWorldAccessor world, ItemStack heldStack, IPlayer byPlayer, BlockSelection blockSel, BlockEntityLiquidContainer beLiquidContainer)
        {
            var contentStacks = IsContentWater(beLiquidContainer.GetNonEmptyContentStacks());

            if (contentStacks == null) return;

            var containerBase = (BlockLiquidContainerBase)beLiquidContainer.Block;
            if (containerBase.GetCurrentLitres(contentStacks) > containerBase.GetContentProps(blockSel.Position).ItemsPerLitre) return;

            var temp = heldStack.Collectible.GetTemperature(world, heldStack);
            
            var newTemp = GameMath.Max(20, temp - Math.Max(0f, GameMath.Max(0f, world.Rand.Next(10, 100))));
            heldStack.Collectible.SetTemperature(world, heldStack, newTemp);
            
            // Evaporate water
            if (temp < 100f) return;
            var tempDiff = temp - newTemp;
            if (tempDiff <= 0) return;
            var evaporateCount = (int)Math.Ceiling(tempDiff/200 * 0.05f * BlockLiquidContainerBase.GetContainableProps(contentStacks).ItemsPerLitre);
            containerBase.TryTakeContent(blockSel.Position, evaporateCount);
            var intensity = temp / 1500;
            Particles(world, new Vec3d(beLiquidContainer.Pos.X + 0.5f, beLiquidContainer.Pos.Y + 0.25f, beLiquidContainer.Pos.Z + 0.5f), InitializeSteamEffect(intensity), InitializeWaterSplashEffect());
            world.PlaySoundAt(new AssetLocation("sounds/pourmetal"), blockSel.FullPosition.X, blockSel.FullPosition.Y, blockSel.FullPosition.Z, byPlayer, 1.5f, 4f, 0.8f*intensity);
        }

        private static ItemStack IsContentWater(ItemStack[] contentStacks) {
            if (contentStacks.Length == 0)
                return null;
            var itemStack = contentStacks.Length > 1 ? contentStacks[0] ?? contentStacks[1] : contentStacks[0];
            if (itemStack == null) return null;
            if (!itemStack.Collectible.IsLiquid()) return null;

            // Check for waterportion instead for compatibility with BalancedThirst and Hydrate or Diedrate or similar mods
            var isWater = itemStack.Collectible.Code.ToString().Contains("waterportion");
            return isWater ? itemStack : null;
        }

        private static void Particles(IWorldAccessor world, Vec3d vec3, SimpleParticleProperties steam, WaterSplashParticles waterSplash)
        {
            waterSplash.BasePos.Set(vec3);
            waterSplash.AddVelocity.Set(0, 0, 0);
            waterSplash.QuantityMul = 1f;
            world.SpawnParticles(waterSplash);
            steam.MinPos = vec3;
            world.SpawnParticles(steam);
        }

    }
}
