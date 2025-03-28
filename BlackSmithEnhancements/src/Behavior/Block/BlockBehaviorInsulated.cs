using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace BlackSmithEnhancements.Behavior.Block
{
    public class BlockEntityBehaviorInsulated : BlockEntityBehavior
    {
        private BlockEntityGenericTypedContainer _blockEntityGenericTypedContainer;

        public double NowHours;

        public double HourDiff;

        public double LastUpdateHours;

        private float _lastUpdateTemp;

        public BlockEntityBehaviorInsulated(BlockEntity blockentity) : base(blockentity)
        {

            NowHours = 0;

            LastUpdateHours = 0;

            HourDiff = 0;
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

            return smoke;
        }

        public override void Initialize(ICoreAPI api, JsonObject jsonProperties)
        {
            base.Initialize(api, jsonProperties);
            if (Pos == null) return;
            _blockEntityGenericTypedContainer = (BlockEntityGenericTypedContainer)Api.World.BlockAccessor.GetBlockEntity(Pos);
            if (_blockEntityGenericTypedContainer == null) return;
            if (!_blockEntityGenericTypedContainer.Block.Attributes["Insulated"][
                    _blockEntityGenericTypedContainer.type].AsBool())
                return;
            api.World.RegisterGameTickListener(OnGameTick, 100);
            api.World.RegisterGameTickListener(OnSlowTick, 1000);
        }

        private void OnGameTick(float dt) 
        {
            var itemStack = GetHeatStack(_blockEntityGenericTypedContainer.Inventory);

            if (itemStack == null) return;

            var temp = itemStack.Collectible.GetTemperature(Api.World, itemStack);

            if (20.3f > temp) return;

            if (itemStack.Attributes["temperature"] is not ITreeAttribute attr) return;

            NowHours = Api.World.Calendar.TotalHours;

            if (NowHours < 0) { NowHours = 0; }

            LastUpdateHours = attr.GetDouble("temperatureLastUpdate");

            HourDiff = NowHours - LastUpdateHours;

            var tempDiff = temp / LastUpdateHours / temp + -HourDiff * 2 * 8f;

            itemStack.Collectible.SetTemperature(Api.World, itemStack, (float)GameMath.Clamp(tempDiff + GameMath.Min(temp, 1100f), 0f, 1100f), false);

            _lastUpdateTemp = temp;

            _blockEntityGenericTypedContainer.MarkDirty();

        }

        private void OnSlowTick(float dt)
        {
            if (_blockEntityGenericTypedContainer.Block != null)
            {
                var smoke = InitializeSmokeEffect();
                smoke.AddPos = Pos.ToVec3d().AddCopy(0, 0.5f, 0);
                smoke.MinPos = Pos.ToVec3d();  
                Api.World.SpawnParticles(smoke);
            }

        }

        private ItemStack GetHeatStack(InventoryBase inv) 
        {
            for (var i = 0; i < inv.Count; i++) {

                var itemSlot = inv[i];

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
            dsc.AppendFormat("Contents - temperature: " + (int)_lastUpdateTemp, 20.3f > _lastUpdateTemp);
            dsc.AppendLine();
            base.GetBlockInfo(forPlayer, dsc);

        }

    }
}
