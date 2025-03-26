using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace BlackSmithEnhancements.Behavior.Item
{
    internal class ItemBehaviorQuenching : CollectibleBehavior
    {
  
        private WorldInteraction[] interactions;

        public ItemBehaviorQuenching(CollectibleObject collObj) : base(collObj)
        {
        }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            if (api.Side != EnumAppSide.Client)
            {
                return;
            }

            if (api is ICoreClientAPI capi)
            {
                interactions = ObjectCacheUtil.GetOrCreate(api, "QuenchingInteractions", delegate
                {
                    List<ItemStack> list = new List<ItemStack>();
                    foreach (CollectibleObject items in api.World.Collectibles)
                    {
                        if (api.World.GetBlock(items.Id) is BlockLiquidContainerBase containerBase)
                        {
                            if (containerBase is BlockBarrel || containerBase is BlockBucket)
                            {
                                list.Add(new ItemStack(items));
                            }
                        }

                    }

                    return new WorldInteraction[1]
                    {
                        new WorldInteraction
                        {
                            ActionLangCode = "heldhelp-quenching",
                            MouseButton = EnumMouseButton.Right,
                            Itemstacks = list.ToArray(),
                            GetMatchingStacks = delegate(WorldInteraction wi, BlockSelection bs, EntitySelection es) {
                                return wi.Itemstacks;
                            }
                        }
                    };
                });
            }
        }
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
        {
            if (inSlot.Inventory.Api.World is IWorldAccessor world)
            {
                if (inSlot.Itemstack.Collectible.GetTemperature(world, inSlot.Itemstack) > 20.1f)
                {
                    return interactions.Append(base.GetHeldInteractionHelp(inSlot, ref handling));
                }
            }

            return base.GetHeldInteractionHelp(inSlot, ref handling);
        }
    }
}
