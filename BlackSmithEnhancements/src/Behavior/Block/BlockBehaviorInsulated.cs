using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace BlackSmithEnhancements.Behavior.Block
{
    public class BlockEntityBehaviorInsulated : BlockEntityBehavior
    {
        BlockEntityGenericTypedContainer blockEntityGenericTypedContainer;

        public double nowHours;

        public double hourDiff;

        public double lastUpdateHours;

        private float lastUpdateTemp;

        public BlockEntityBehaviorInsulated(BlockEntity blockentity) : base(blockentity)
        {

            nowHours = 0;

            lastUpdateHours = 0;

            hourDiff = 0;
        }
    
        public static SimpleParticleProperties InitializeSmokeEffect()
        {
            SimpleParticleProperties smoke;
            smoke = new SimpleParticleProperties(
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

            return smoke;
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);

            if (Pos != null)
            {
                blockEntityGenericTypedContainer = (BlockEntityGenericTypedContainer)Api.World.BlockAccessor.GetBlockEntity(Pos);

                if (blockEntityGenericTypedContainer != null)
                {
                    if (blockEntityGenericTypedContainer.Block.Attributes["Insulated"][blockEntityGenericTypedContainer.type].AsBool(false) == true)
                    {
                        api.World.RegisterGameTickListener(OnGameTick, 100);

                        api.World.RegisterGameTickListener(OnSlowTick, 1000);
                    }
                }
            }
        }

        private void OnGameTick(float dt) 
        {
            ItemStack itemStack = GetHeatStack(blockEntityGenericTypedContainer.Inventory);

            if (itemStack == null) return;

            float temp = itemStack.Collectible.GetTemperature(Api.World, itemStack);

            if (20.3f > temp) return;

            if (itemStack.Attributes["temperature"] is not ITreeAttribute attr) return;

            nowHours = Api.World.Calendar.TotalHours;

            if (nowHours < 0) { nowHours = 0; }

            lastUpdateHours = attr.GetDouble("temperatureLastUpdate");

            hourDiff = nowHours - lastUpdateHours;

            double tempDiff = (temp / lastUpdateHours / temp) + (-hourDiff * 2 * 8f);

            itemStack.Collectible.SetTemperature(Api.World, itemStack, (float)GameMath.Clamp(tempDiff + GameMath.Min(temp, 1100f), 0f, 1100f), false);

            lastUpdateTemp = temp;

            blockEntityGenericTypedContainer.MarkDirty();

        }

        private void OnSlowTick(float dt)
        {
            if (blockEntityGenericTypedContainer.Block != null)
            {
                var Smoke = InitializeSmokeEffect();
                Smoke.AddPos = Pos.ToVec3d().AddCopy(0, 0.5f, 0);
                Smoke.MinPos = Pos.ToVec3d();  
                Api.World.SpawnParticles(Smoke);
            }

        }

        private ItemStack GetHeatStack(InventoryBase inv) 
        {
            for (int i = 0; i < inv.Count; i++) {

                ItemSlot itemSlot = inv[i];

                if (itemSlot.Empty)
                {
                    continue;
                }

                return itemSlot.Itemstack;
            }

            return null;
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            dsc.AppendFormat("Contents - temperature: " + (int)lastUpdateTemp, 20.3f > lastUpdateTemp);
            dsc.AppendLine();
            base.GetBlockInfo(forPlayer, dsc);

        }

    }
}
