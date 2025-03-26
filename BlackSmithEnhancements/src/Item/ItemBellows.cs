using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace BlackSmithEnhancements.Item
{
    internal class ItemBellow : Vintagestory.API.Common.Item
    {

        public float CooktimeBonus = 1;

        private WorldInteraction[] _interactions;

        private ItemSlot _inputSlot;

        public override void OnLoaded(ICoreAPI coreApi)
        {
     
            if (coreApi.Side != EnumAppSide.Client)
            {
                return;
            }

            _ = coreApi;

            _interactions = ObjectCacheUtil.GetOrCreate(coreApi, "bellowInteractions", delegate
            {
                var list = new List<ItemStack>();
                foreach (var items in coreApi.World.Collectibles)
                {
                    if (coreApi.World.GetBlock(items.Id) is BlockForge)
                    {
                        list.Add(new ItemStack(items));
                    }
                }

                return new WorldInteraction[]
                {
                        new()
                        {
                            ActionLangCode = "heldhelp-bellow",
                            MouseButton = EnumMouseButton.Right,
                            Itemstacks = list.ToArray(),
                            GetMatchingStacks = (wi, _, _) => wi.Itemstacks
                        }
                };
            });

        }

        private static SimpleParticleProperties InitializeSmokeEffect()
        {
            SimpleParticleProperties bellowSmoke;
            bellowSmoke = new SimpleParticleProperties(
                3, 6,
                ColorUtil.ToRgba(50, 248, 248, 255), // first alpha, second red, three green, four blue
                new Vec3d(),
                new Vec3d(),
                new Vec3f(-0.01f, 0.05f, -0.01f),
                new Vec3f(0.05f, 0.1f, 0.05f),
                2f,
                0.01f,
                0.2f,
                0.5f,
                EnumParticleModel.Quad
            )
            {
                AddPos = new Vec3d { X = 0, Y = 0, Z = 0 }.Set(0.1, 0.1, 0.1),
                OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -250f),
                SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -0.03f),
                VertexFlags = 100,
                WindAffected = false,
                WindAffectednes = 0.1f,
                ClimateColorMap = null,
                SelfPropelled = true
            };

            return bellowSmoke;
        }

        private static Vec3d GetVec3d(Entity byEntity)
        {
            var charView = 0.13f;
            var viewoffSet = 0f;

            if (byEntity.Api.World is IClientWorldAccessor world)
            {

                if (world.Player.CameraMode != EnumCameraMode.FirstPerson)
                {
                    charView = 0f;
                    return byEntity.Pos.XYZ.Add(0, byEntity.LocalEyePos.Y - 1.2f, 0).Ahead(1.5f, 3.2f - viewoffSet, byEntity.Pos.Yaw - viewoffSet).Ahead(charView, 0f, byEntity.Pos.Yaw + GameMath.PIHALF);

                }

                if (world.Player.CameraMode == EnumCameraMode.FirstPerson)
                {
                    viewoffSet =- 0.20f;
                    return byEntity.Pos.XYZ.Add(0, byEntity.LocalEyePos.Y - 0.6f, 0).Ahead(0.8f, byEntity.Pos.Pitch - viewoffSet, byEntity.Pos.Yaw - viewoffSet).Ahead(charView, 0f, byEntity.Pos.Yaw + GameMath.PIHALF);
                }
            }


            return new Vec3d(0f,0f,0f);
           
        }

        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            var itemSlot = capi.World.Player.InventoryManager.MouseItemSlot;

            if (itemSlot == renderinfo.InSlot)
            {
                if (itemstack.Attributes.HasAttribute("renderVariant"))
                {
                    itemSlot.Itemstack.TempAttributes.RemoveAttribute("renderVariant");

                    itemSlot.Itemstack.Attributes.SetInt("renderVariant", 0);
                    itemSlot.Itemstack.Attributes.RemoveAttribute("renderVariant");
                    capi.World.Player.InventoryManager.BroadcastHotbarSlot();
                    
                }
            }

            itemSlot = capi.World.Player.InventoryManager.ActiveHotbarSlot;

            if (renderinfo.InSlot != itemSlot) 
            {
                if (renderinfo.InSlot.Itemstack == null) return;

                if (renderinfo.InSlot.Itemstack.Attributes.HasAttribute("renderVariant"))
                {
                    renderinfo.InSlot.Itemstack.TempAttributes.RemoveAttribute("renderVariant");

                    renderinfo.InSlot.Itemstack.Attributes.SetInt("renderVariant", 0);
                    renderinfo.InSlot.Itemstack.Attributes.RemoveAttribute("renderVariant");

                }
            }
        }

        public override void OnModifiedInInventorySlot(IWorldAccessor world, ItemSlot slot, ItemStack extractedStack = null)
        {
            if(extractedStack != null && world.Api is ICoreClientAPI capi) 
            {
                if (capi.World is not null)
                {
                    extractedStack.TempAttributes.RemoveAttribute("renderVariant");
                }

                extractedStack.Attributes.SetInt("renderVariant", 0);
                extractedStack.Attributes.RemoveAttribute("renderVariant");
                capi.World?.Player?.InventoryManager.BroadcastHotbarSlot();

                capi.World?.Player?.Entity.TpAnimManager.StopAnimation("usebellow");
            }
            base.OnModifiedInInventorySlot(world, slot, extractedStack);
        }

        public override string GetHeldTpUseAnimation(ItemSlot activeHotbarSlot, Entity forEntity)
        {
            return "";
        }

        public override string GetHeldTpIdleAnimation(ItemSlot activeHotbarSlot, Entity forEntity, EnumHand hand)
        {
            if (!forEntity.AnimManager.IsAnimationActive("usebellow") ||
                ((EntityPlayer)forEntity).Controls.RightMouseDown)
            {
                return activeHotbarSlot.Itemstack.Collectible.HeldRightTpIdleAnimation;
            }

            if (forEntity.World is IClientWorldAccessor)
            {
                activeHotbarSlot.Itemstack.TempAttributes.RemoveAttribute("renderVariant");
            }

            activeHotbarSlot.Itemstack.Attributes.RemoveAttribute("renderVariant");
            ((EntityPlayer)forEntity).Player?.InventoryManager.BroadcastHotbarSlot();

            return activeHotbarSlot.Itemstack.Collectible.HeldRightTpIdleAnimation;
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);

            if (handling != EnumHandHandling.PreventDefault)
            {
                if (slot.Empty) {
                    return;
                }

                if (!byEntity.LeftHandItemSlot.Empty)
                {
                    var text = byEntity.LeftHandItemSlot.Itemstack.GetName().ToLower();
                    (api as ICoreClientAPI)?.TriggerIngameError(!byEntity.LeftHandItemSlot.Empty, "Requires both hands", Lang.Get("ingameerror-bellow-requires-bothhands-{0}", text.UcFirst()));
                    return;
                }

                if (byEntity.World is IClientWorldAccessor)
                {
                    slot.Itemstack.TempAttributes.SetInt("renderVariant", 1);
                }

                byEntity.Attributes.SetInt("startedBellowing", 1);
                byEntity.Attributes.SetInt("bellowCancel", 0);
                slot.Itemstack.Attributes.SetInt("renderVariant", 1);
                byEntity.AnimManager.StartAnimation("usebellow");
                handling = EnumHandHandling.PreventDefault;
            }
        }
        
        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            var num = GameMath.Clamp((int)Math.Ceiling(secondsUsed * 5f), 0, 2);
            var @int = slot.Itemstack.Attributes.GetInt("renderVariant");
            slot.Itemstack.TempAttributes.SetInt("renderVariant", num);
            slot.Itemstack.Attributes.SetInt("renderVariant", num);

            if (@int != num)
            {
                (byEntity as EntityPlayer)?.Player?.InventoryManager.BroadcastHotbarSlot();
            }

            return secondsUsed < 1.5f;
        }

        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            byEntity.Attributes.SetInt("startedBellowing", 0);
            byEntity.AnimManager.StopAnimation("usebellow");

            if (byEntity.World is IClientWorldAccessor)
            {
                slot.Itemstack.TempAttributes.RemoveAttribute("renderVariant");
            }

            slot.Itemstack.Attributes.SetInt("renderVariant", 0);
            (byEntity as EntityPlayer)?.Player?.InventoryManager.BroadcastHotbarSlot();

        

            if (cancelReason == EnumItemUseCancelReason.ReleasedMouse)
            {
                byEntity.Attributes.SetInt("bellowCancel", 1);
                return true;
              
            }

            return false;
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            var heldstack = slot.Itemstack;

            if (byEntity.Attributes.GetInt("bellowCancel") == 1)
            {
                return;
            }

            IPlayer dualCallByPlayer = null;
            if (byEntity is EntityPlayer byPlayer)
            {
                dualCallByPlayer = byEntity.World.PlayerByUid(byPlayer.PlayerUID);
            }

            if (api.World is IClientWorldAccessor)
            {
                heldstack.TempAttributes.SetInt("renderVariant", 1);
            }

            if (secondsUsed < 1f)
            {
                return;
            }

            heldstack.Attributes.SetInt("renderVariant", 0);
            byEntity.AnimManager.StopAnimation("usebellow");
            (byEntity as EntityPlayer)?.Player?.InventoryManager.BroadcastHotbarSlot();

            if (byEntity.World.Rand.Next(1, 80) < 5)
            {
                DamageItem(api.World, byEntity, (byEntity as EntityPlayer)?.Player.InventoryManager.ActiveHotbarSlot);
                //(api as ICoreClientAPI)?.TriggerChatMessage("Bellow been damage");
            }

            if (api.Side == EnumAppSide.Client)
            {
                PlaySound(byEntity.World.Api, byEntity, dualCallByPlayer, Attributes["sound"].AsString());
                var pos = GetVec3d(byEntity);
                var smokeHeld = InitializeSmokeEffect();
                smokeHeld.MinPos = pos.AddCopy(0, 0.3, 0);
                byEntity.World.SpawnParticles(smokeHeld);
            }

            byEntity.AnimManager.StartAnimation("finishbellow");
            // Add callback to stop animation in a second
            
            if (blockSel == null) return;

            if (api.World.BlockAccessor.GetBlockEntity(blockSel.Position)
                is BlockEntityForge blockEntityForge)
                BlowOnForge(blockEntityForge);

            if (api.World.BlockAccessor.GetBlockEntity(blockSel.Position)
                is BlockEntityFirepit blockEntityFirepit)
                BlowOnFirepit(blockEntityFirepit);
        }

        private void BlowOnForge(BlockEntityForge blockEntityForge) 
        {
            var forgeContents = blockEntityForge.Contents;

            if (!blockEntityForge.IsBurning)
            {
                (api as ICoreClientAPI)?.TriggerIngameError(blockEntityForge.IsBurning == false, "Lit the forge first", Lang.Get("ingameerror-forge-lit"));
                return;
            }

            if (forgeContents == null)
            {
                (api as ICoreClientAPI)?.TriggerIngameError(true, "Forge missing contents", Lang.Get("ingameerror-forge-empty"));
                return;
            }

            var temp = forgeContents.Collectible.GetTemperature(api.World, forgeContents);

            var tempDecrease = -forgeContents.StackSize * 2;

            var tempBoost = api.World.Rand.Next(15, 30) - tempDecrease;

            if (temp > 1100f) return;

            if (api.Side != EnumAppSide.Server) return;

            forgeContents.Collectible.SetTemperature(api.World, forgeContents, GameMath.Clamp(tempBoost + GameMath.Min(temp, 1100f), 0f, 1100f));
            blockEntityForge.MarkDirty(true);
        }

        private void BlowOnFirepit(BlockEntityFirepit blockEntityFirepit) {

            var world = api.World;

            if (world == null) return;

            _inputSlot = blockEntityFirepit.inputSlot;

            if (!blockEntityFirepit.IsBurning)
            {
                (api as ICoreClientAPI)?.TriggerIngameError(blockEntityFirepit.IsBurning == false, "Lit the firepit first", Lang.Get("ingameerror-firepit-lit"));
                return;
            }

            if (_inputSlot.Empty)
            {
                (api as ICoreClientAPI)?.TriggerIngameError(_inputSlot.Empty, "firepit empty", Lang.Get("ingameerror-firepit-empty"));
                return;
            }

            if (api.Side != EnumAppSide.Server)
            {
                return;
            }

            var cookingTime = blockEntityFirepit.inputStackCookingTime;

            var stackTemp = blockEntityFirepit.InputStackTemp;

            var fuelTemp = blockEntityFirepit.furnaceTemperature;

            switch (_inputSlot.Itemstack.Collectible)
            {
                case BlockSmeltingContainer:
                    CooktimeBonus = 3f;
                    break;
                case BlockCookingContainer:
                    CooktimeBonus = 1.25f;
                    break;
                case BlockSmeltedContainer:
                {
                    var min = GameMath.Min(stackTemp, fuelTemp); // always stay within the min value between stackTemp, fuelTemp... I think that what it does? :D
                    float random = api.World.Rand.Next(100); // < pick a number within 100
                    _inputSlot.Itemstack.Collectible.SetTemperature(world, _inputSlot.Itemstack, GameMath.Clamp(random + min, 0f, fuelTemp), false); // let set the temp to this.
                    break;
                }
            }

            if (cookingTime > 1)
            {
               
                blockEntityFirepit.inputStackCookingTime = cookingTime + CooktimeBonus;

                // burned things
                /*if (inputSlot.Itemstack.Collectible is not BlockSmeltedContainer or BlockCookingContainer or BlockSmeltingContainer)
                    {
                        if (api.World.Rand.Next(14, 100) < 15)
                        {
                            inputSlot.TakeOut(1);
                        }
                    }
                    */
            }
        }

        private void PlaySound(ICoreAPI coreApi, EntityAgent byEntity, IPlayer player, string name)
        {
            coreApi.World.PlaySoundAt(new AssetLocation(Code.Domain, name), byEntity, player, false, 2f);
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return _interactions.Append(base.GetHeldInteractionHelp(inSlot));
        }
    }
}
