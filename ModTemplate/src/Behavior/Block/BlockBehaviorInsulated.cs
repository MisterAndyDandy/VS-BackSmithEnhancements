using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace BlackSmithEnhancements
{
    public class BlockEntityBehaviorInsulated : BlockEntityBehavior
    {
        BlockEntityGenericTypedContainer entityGenericTypedContainer;
        public BlockEntityBehaviorInsulated(BlockEntity blockentity) : base(blockentity)
        {
           entityGenericTypedContainer = blockentity as BlockEntityGenericTypedContainer;
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

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
      
            base.Initialize(api, properties);

            if (api != null)
            {
                api.World.RegisterGameTickListener(OnGameTick, 70);
            }
        }

        public void OnGameTick(float dt) {
           _ = dt;

            if (entityGenericTypedContainer != null)
            {
                if (entityGenericTypedContainer.Block.Code.FirstCodePart() == "chest")
                {
                    if (entityGenericTypedContainer.Block.Attributes["Insulated"][entityGenericTypedContainer.type].AsBool())
                    {
                        InventoryBase inventory = entityGenericTypedContainer.Inventory;
                        if (inventory != null)
                        {
                            if (inventory.Count > 0)
                            {
                                for (int i = 0; i < inventory.Count; i++)
                                {
                                    if (inventory[i].Empty)
                                    {
                                        continue;
                                    }

                                    ItemSlot itemSlot = inventory[i];

                                    if (itemSlot.Itemstack != null) { }

                                }
                            }
                        }
                    }
                }
            }
        }

        private static void Particles(IWorldAccessor world, Vec3d vec3, SimpleParticleProperties steam)
        {
            steam.MinPos = vec3;
            world.SpawnParticles(steam);
        }

    }
}
