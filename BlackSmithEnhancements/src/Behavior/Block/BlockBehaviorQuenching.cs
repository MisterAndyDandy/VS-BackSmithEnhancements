using System;
using BlackSmithEnhancements.Behavior.Item;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace BlackSmithEnhancements.Behavior.Block
{
    public class BlockBehaviorQuenching : BlockBehavior
    {
        private long secondPasted = 360;

        public BlockBehaviorQuenching(Vintagestory.API.Common.Block block) : base(block)
        {
        }
    
        public static SimpleParticleProperties InitializeSteamEffect()
        {
            SimpleParticleProperties steam;
            steam = new SimpleParticleProperties(
                8, 16,
                ColorUtil.ToRgba(50, 248, 248, 255), // first alpha, second red, three green, four blue
                new Vec3d(),
                new Vec3d(),
                new Vec3f(0.1f, 0.1f, 0.1f),
                new Vec3f(0.2f, 0.3f, 0.2f),
                1f,
                0.01f,
                0.2f,
                0.8f,
                EnumParticleModel.Quad
            )
            {
                AddPos = new Vec3d() { }.Set(0f, 0.2f, 0f),
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
            base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);

            if (blockSel != null && !byPlayer.Entity.RightHandItemSlot.Empty)
            {
                if (block.GetBlockEntity<BlockEntityLiquidContainer>(blockSel) is BlockEntityLiquidContainer entityLiquidContainer)
                {
                    ItemStack heldStack = byPlayer.Entity.RightHandItemSlot.Itemstack;
                    float temp = heldStack.Collectible.GetTemperature(world, heldStack);

                    if (heldStack.Collectible.HasTemperature(heldStack) && temp > 20.1f)
                    {
                        if (heldStack.Collectible.HasBehavior<ItemBehaviorQuenching>())
                        {
                                Quenching(world, heldStack, byPlayer, blockSel, (BlockLiquidContainerBase)entityLiquidContainer.Block, entityLiquidContainer, (float)heldStack.Collectible.GetTemperature(world, heldStack));
                                heldStack.Collectible.HeldTpUseAnimation = "interactstatic";
                                secondPasted = world.Calendar.ElapsedSeconds;
                                handling = EnumHandling.Handled;
                                return true;
                            
                        }
                    };
                }
            }

            return false;
        }
    
        public void Quenching(IWorldAccessor world, ItemStack heldStack, IPlayer byPlayer, BlockSelection blockSel, BlockLiquidContainerBase containerBase, BlockEntityLiquidContainer entityLiquidContainer, float temp)
        {
            long elapsedSeconds = world.Calendar.ElapsedSeconds - secondPasted;

            ItemStack contentStacks = IsContentWater(entityLiquidContainer.GetNonEmptyContentStacks());

            if (contentStacks == null) return;

            if (containerBase.GetCurrentLitres(contentStacks) > containerBase.GetContentProps(blockSel.Position).ItemsPerLitre) return;

            if (elapsedSeconds > 100)
            {
                if (world.Side == EnumAppSide.Client)
                {
                    world.PlaySoundAt(new AssetLocation("sounds/sizzle"), blockSel.FullPosition.X, blockSel.FullPosition.Y, blockSel.FullPosition.Z, byPlayer.Entity.World.PlayerByUid(byPlayer.Entity.PlayerUID), 1f, 4f, 0.8f);
                    Particles(world, new Vec3d(entityLiquidContainer.Pos.X + 0.5f, entityLiquidContainer.Pos.Y + 0.25f, entityLiquidContainer.Pos.Z + 0.5f), InitializeSteamEffect(), InitializeWaterSplashEffect());
                }
            }
            //containerBase.TryTakeContent(blockSel.Position, (int)Math.Ceiling(0.05f * BlockLiquidContainerBase.GetContainableProps(contentStacks).ItemsPerLitre)); will do something later
            heldStack.Collectible.SetTemperature(world, heldStack, GameMath.Max(0, temp - Math.Max(0f, GameMath.Max(0f, world.Rand.Next(10, 100)))), true);

        }

        private static ItemStack IsContentWater(ItemStack[] contentStacks) {
            if (contentStacks.Length != 0)
            {
                string isWater;

                ItemStack itemStack = contentStacks[0] ?? contentStacks[1];

                if (itemStack == null) return null;

                if (itemStack.Collectible.IsLiquid()) {

                    isWater = itemStack.Collectible.FirstCodePart();

                    if (isWater != "waterportion") return null;

                    return itemStack;
                }

                if (itemStack != null && !itemStack.Collectible.IsLiquid()) return null;

            };

            return null;
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
