using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace BlackSmithEnhancements
{
    public class BlockEntityBehaviorInsulated : BlockEntityBehavior
    {
        private readonly BlockEntityGenericTypedContainer entityGenericTypedContainer;

        public double nowHours;

        public double hourDiff;

        public double lastUpdateHours;

        private float lastUpdateTemp;

        public BlockEntityBehaviorInsulated(BlockEntity blockentity) : base(blockentity)
        {
            entityGenericTypedContainer = blockentity as BlockEntityGenericTypedContainer;

            nowHours = 0;

            lastUpdateHours = 0;

            hourDiff = 0;
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

            if (entityGenericTypedContainer == null) return;

            api.World.RegisterGameTickListener(OnGameTick, 100);
        }

        public void OnGameTick(float dt) 
        {
            if (entityGenericTypedContainer.Block.Code.FirstCodePart() != "chest") return;

            if (entityGenericTypedContainer.Block.Attributes["Insulated"][entityGenericTypedContainer.type].AsBool())
            {
                for (int i = 0; i < entityGenericTypedContainer.Inventory.Count; i++)
                {
                    ItemSlot itemSlot = entityGenericTypedContainer.Inventory[i];

                    if (itemSlot.Empty)
                    {
                        continue;
                    }

                    ItemStack itemStack = itemSlot.Itemstack;

                    float temp = itemStack.Collectible.GetTemperature(Api.World, itemStack);

                    if (temp < 20f) continue;

                    if (itemStack.Attributes["temperature"] is not ITreeAttribute attr) return;

                    nowHours = Api.World.Calendar.TotalHours;

                    if(nowHours < 0) { nowHours = 0; }

                    lastUpdateHours = attr.GetDouble("temperatureLastUpdate");

                    hourDiff = nowHours - lastUpdateHours;

                    double tempDiff = (temp / lastUpdateHours / temp) + ( -hourDiff * 2 * 10f);

                    itemStack.Collectible.SetTemperature(Api.World, itemStack, (float)GameMath.Clamp(tempDiff + GameMath.Min(temp, 1100f), 0f, 1100f), false);

                    lastUpdateTemp = temp;

                    entityGenericTypedContainer.MarkDirty();
                }
            }
        }


        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            dsc.AppendFormat("Contents - temperature: " + (int)lastUpdateTemp, 20.3f > lastUpdateTemp);
            dsc.AppendLine();
            base.GetBlockInfo(forPlayer, dsc);

        }

    }
}
