using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Client;
using System.Collections.Generic;

namespace BlackSmithEnhancements
{
    [HarmonyPatch(typeof(InventoryBase), "DropSlotIfHot")]
    public class Player_DropSlotIfHot_Patch
    {
         // Blacksmith Gloves by Arahvin.  Fixed by me //
        //[https://mods.vintagestory.at/show/mod/6581]//

        [HarmonyPrefix]

        public static bool Gear_Has_Heat_Resistant(ItemSlot slot, IPlayer player)
        {
            if (slot.Empty)
            {
                return false;
            }
            if (player != null && player.WorldData.CurrentGameMode == EnumGameMode.Creative)
            {
                return false;
            }
            if (player.Entity == null || player.Entity.GearInventory == null)
            {
                return true;
            }
            foreach (ItemSlot itemSlot in player.Entity.GearInventory)
            {
                if (itemSlot.BackgroundIcon == "gloves")
                {
                    if (!itemSlot.Empty)
                    {
                        ItemStack itemstack = itemSlot.Itemstack;
                        bool? flag;
                        if (itemstack == null)
                        {
                            flag = null;
                        }
                        else
                        {
                            JsonObject attributes = itemstack.Collectible.Attributes;
                            flag = ((attributes != null) ? new bool?(attributes.IsTrue("heatResistant")) : null);
                        }
                        bool? flag2 = flag;
                        if (flag2.Value)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BlockBarrel), "OnBlockInteractStart")]
    public class OnBlockInteractStart_Patch
    {

        [HarmonyPrefix]
        public static bool BlockBarrel_OnBlockInteractStart_Patch(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (blockSel != null && world != null)
            {
                if (byPlayer.Entity.Controls.ShiftKey == false && !byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                {
                    Item heldItem = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack?.Item;

                    if (heldItem != null)
                    {
                        if (heldItem.HasBehavior<ItemBehaviorQuenching>())
                        {
                            return true;
                        }
                    }
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(BlockEntityForge), "OnPlayerInteract")]
    public class OnPlayerInteract_Patch
    {

        [HarmonyPrefix]
        public static bool BlockEntityForge_OnPlayerInteract_Patch(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (blockSel != null && world != null) {
                if (!byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                {
                    Item heldItem = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Item;

                    if (heldItem != null)
                    {
                        if (heldItem is ItemBellow)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
    // to remove unwanted types
    //[HarmonyPatch(typeof(GuiHandbookItemStackPage), "RenderListEntryTo", MethodType.Normal)]
    //public class Class_GuiHandbook_Patch
    //{

    //    static void Prefix(GuiHandbookItemStackPage __instance, ICoreClientAPI capi, float dt, double x, double y, double cellWidth, double cellHeight)
    //    {

    //        ItemStack itemStack = null;

    //        if (__instance != null) 
    //        {
    //            itemStack = __instance.Stack;

    //            if (itemStack.Collectible.Variant == null) return;

    //            ICollection<string> types = itemStack.Collectible.Variant.Keys;

    //            ICollection<string> typesof = itemStack.Collectible.Variant.Values;

    //            if (types == null) return;


    //            if (typesof.Contains("oak") || typesof.Contains("granite"))
    //            {
    //                __instance.dummySlot = new DummySlot(itemStack);
    //                __instance.Recompose(capi);
    //            }
    //            else { __instance.Visible = false; }
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(Block), "OnEntityInside")]
    public class OnEntityInside_Patch
    {

        [HarmonyPrefix]
        public static bool BlockForge_OnEntityInside_Patch(IWorldAccessor world, Entity entity, BlockPos pos)
        {
            if (world == null) return false;

            if (entity == null || entity is not EntityPlayer entityPlayer) return false;

            if (pos == null) return false;

            if (world.Rand.NextDouble() < 0.05 && (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityForge blockEntityForge))
            {

                if(entityPlayer?.Player.WorldData.CurrentGameMode != EnumGameMode.Survival) return false;

                if (blockEntityForge.IsBurning && entityPlayer.Pos.AsBlockPos.UpCopy(1) == blockEntityForge.Pos.UpCopy(1))
                {
                    entity.ReceiveDamage(new DamageSource
                    {
                        Source = EnumDamageSource.Block,
                        SourceBlock = blockEntityForge.Block,
                        Type = EnumDamageType.Fire,
                        SourcePos = pos.ToVec3d()
                    }, 0.5f);
                }
         
            }
           
            return true;
        }
    }

    [HarmonyPatch(typeof(BlockEntityFirepit), "OnPlayerRightClick")]
    public class OnPlayerRightClick_Patch
    {

        [HarmonyPrefix]
        public static bool BlockEntityFirepit(IPlayer byPlayer, BlockSelection blockSel)
        {
            if (blockSel != null)
            {
                if (!byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                {
                    Item heldItem = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Item;

                    if (heldItem != null)
                    {
                        if (heldItem is ItemBellow)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }

}
