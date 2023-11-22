using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;

namespace BlackSmithEnhancements
{
    [HarmonyPatch(typeof(InventoryBase))]
    [HarmonyPatch("DropSlotIfHot")]
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

    [HarmonyPatch(typeof(BlockBarrel))]
    [HarmonyPatch("OnBlockInteractStart")]
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

    [HarmonyPatch(typeof(BlockEntityForge))]
    [HarmonyPatch("OnPlayerInteract")]
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
                        if (world.GetItem(heldItem.Id) is ItemBellow)
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
