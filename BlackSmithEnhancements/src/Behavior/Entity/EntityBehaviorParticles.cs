using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace BlackSmithEnhancements.Behavior.Entity
{
    public class EntityBehaviorParticles : EntityBehavior
    {
        private EntityPlayer _entityPlayer;

        public EntityBehaviorParticles(Vintagestory.API.Common.Entities.Entity entity) : base(entity)
        {
        }

        public static SimpleParticleProperties InitializeSteamEffect()
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

        public override void Initialize(EntityProperties properties, JsonObject attributes)
        {
            base.Initialize(properties, attributes);

            _entityPlayer = entity as EntityPlayer;
        }

        public override void OnGameTick(float deltaTime)
        {
            if (_entityPlayer is { Player: not null })
            {
                if (!_entityPlayer.LeftHandItemSlot.Empty)
                {
                    SpawnWearableParticles();
                }
            }
        }

        protected virtual void SpawnWearableParticles()
        {
            var coreClientApi = entity.Api as ICoreClientAPI;
            if (coreClientApi == null) return;
            var entityPos = coreClientApi.World.Player.Entity.EntityId == entity.EntityId ? entity.Pos : entity.ServerPos;
            var num = (float)Math.Sqrt(entity.Pos.Motion.X * entity.Pos.Motion.X + entity.Pos.Motion.Z * entity.Pos.Motion.Z);
            if (!(entity.Api.World.Rand.NextDouble() < 10f * num)) return;
            var rand = coreClientApi.World.Rand;
            var velocity = new Vec3f(1f - 2f * (float)rand.NextDouble() + GameMath.Clamp((float)entity.Pos.Motion.X * 15f, -5f, 5f), 0.5f + 3.5f * (float)rand.NextDouble(), 1f - 2f * (float)rand.NextDouble() + GameMath.Clamp((float)entity.Pos.Motion.Z * 15f, -5f, 5f));
            var radius = Math.Min(entity.SelectionBox.XSize, entity.SelectionBox.ZSize) * 0.9f;
            entity.World.SpawnCubeParticles(entityPos.AsBlockPos, entityPos.XYZ.Add(0.0, 0.0, 0.0), radius, 2 + (int)(rand.NextDouble() * num * 5.0), 0.5f + (float)rand.NextDouble() * 0.5f, null, velocity);
        }

        private static void Particles(IWorldAccessor world, Vec3d vec3, SimpleParticleProperties smoke)
        {
            smoke.MinPos = vec3;
            world.SpawnParticles(smoke);
        }


        public override string PropertyName()
        {
            return "entityparticles";
        }
    }
}
