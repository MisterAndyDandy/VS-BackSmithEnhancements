using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace BlackSmithEnhancements.Behavior.Item
{
    internal class ItemBehaviorQuenching : CollectibleBehavior
    {
  
        private WorldInteraction[] _interactions;

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

            if (api is ICoreClientAPI)
            {
                _interactions = ObjectCacheUtil.GetOrCreate(api, "QuenchingInteractions", delegate
                {
                    var list = new List<ItemStack>();
                    foreach (var items in api.World.Collectibles)
                    {
                        if (api.World.GetBlock(items.Id)
                            is not BlockLiquidContainerBase containerBase)
                            continue;

                        if (containerBase is BlockBarrel or BlockBucket)
                            list.Add(new ItemStack(items));
                    }

                    return new WorldInteraction[]
                    {
                        new()
                        {
                            ActionLangCode = "heldhelp-quenching",
                            MouseButton = EnumMouseButton.Right,
                            Itemstacks = list.ToArray(),
                            GetMatchingStacks = (wi, _, _) => wi.Itemstacks
                        }
                    };
                });
            }
        }
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
        {
            if (inSlot.Inventory.Api.World is not { } world)
                return base.GetHeldInteractionHelp(inSlot, ref handling);

            return inSlot.Itemstack.Collectible.GetTemperature(world, inSlot.Itemstack) > 20.1f ?
                _interactions.Append(base.GetHeldInteractionHelp(inSlot, ref handling)) :
                base.GetHeldInteractionHelp(inSlot, ref handling);
        }
    }
}
