using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using System.Linq;
using System.Reflection;
using Vintagestory.GameContent;
using Vintagestory.API.Client;
using System.Collections.Generic;
using System.Drawing;

namespace BlackSmithEnhancements
{
    class BlackSmithEnhancementsCore : ModSystem
    {
        Harmony harmony = new Harmony("com.misterandydandy.black.smith.addons");

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterEntityBehaviorClass("entityparticles", typeof(EntityBehaviorParticles));

            api.RegisterItemClass("ItemBellow", typeof(ItemBellow));

            //api.RegisterBlockEntityBehaviorClass("Insulated", typeof(BlockEntityBehaviorInsulated));

            api.RegisterBlockBehaviorClass("Quenching", typeof(BlockBehaviorQuenching));


            api.RegisterCollectibleBehaviorClass("ItemQuenching", typeof(ItemBehaviorQuenching));


            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }


        public override void AssetsFinalize(ICoreAPI api)
        {
            base.AssetsLoaded(api);

            foreach (Block block in api.World.Blocks)
            {
                if (block is BlockBarrel or BlockBucket)
                {
                    block.BlockBehaviors = block.BlockBehaviors.Append(new BlockBehaviorQuenching(block));
                }

                //if (api.Side == EnumAppSide.Server)
                //{
                //    if (block is BlockGenericTypedContainer typedContainer)
                //    {
                //        BlockEntityGenericTypedContainer entityGenericTypedContainer = new BlockEntityGenericTypedContainer { Block = typedContainer };

                //        if (block.FirstCodePart() == "chest")
                //        {
                //            if (typedContainer.Attributes != null)
                //            {
                //                bool flag = typedContainer.Attributes["Insulated"][entityGenericTypedContainer.type].AsBool();
                //                if (flag)
                //                {
                //                    block.BlockBehaviors = block.BlockBehaviors.Append(new BlockBehaviorInsulated(typedContainer));
                //                }
                //            }
                //        }

                //    }
                //}
               
            }

            foreach (SmithingRecipe smithing in api.GetSmithingRecipes())
            {
                ItemStack itemStack = smithing.Output.ResolvedItemstack;

                if (itemStack.Collectible.HasBehavior<ItemBehaviorQuenching>())
                {
                    continue;
                }

                itemStack.Collectible.CollectibleBehaviors = itemStack.Collectible.CollectibleBehaviors.Append(new ItemBehaviorQuenching(itemStack.Collectible));

            }

            foreach (CollectibleObject colObj in api.World.Collectibles)
            {
                if (colObj.HasBehavior<ItemBehaviorQuenching>()) continue;

                bool flag = colObj.Attributes?.IsTrue("forgable") ?? false;

                if (colObj.Tool.HasValue || colObj is ItemIngot or ItemMetalPlate or ItemWorkItem or BlockSmeltedContainer || flag)
                {
                    colObj.CollectibleBehaviors = colObj.CollectibleBehaviors.Append(new ItemBehaviorQuenching(colObj));
                }

            }
        }

        public override void Dispose()
        {
            harmony.UnpatchAll(harmony.Id);
            base.Dispose();
        }

    }
}

//namespace QPClientTools
//{

//    public class LoadQPTools : ModSystem
//    {

//        private string Domain = "";
//        public override void StartPre(ICoreAPI api)
//        {
//            base.StartPre(api);

//            if (this.Mod.Info != null)
//            {
//                Domain = Mod.Info.ModID;
//            }

//            if (api is ICoreClientAPI clientAPI)
//            {
//                this.StartClient(clientAPI);
//            }

//        }


//        private void StartClient(ICoreClientAPI clientAPI)
//        {
//            clientAPI.ChatCommands.Create("qpt").RequiresPlayer().HandleWith(delegate (TextCommandCallingArgs args)
//            {

//                clientAPI.ShowChatMessage("Sub Commands:");
//                clientAPI.ShowChatMessage("/qpt imat " + Lang.Get(Domain + ":" + "chat-share-location"));
//                clientAPI.ShowChatMessage("/qpt this " + Lang.Get(Domain + ":" + "chat-share-helditem"));

//                return TextCommandResult.Success("", null);
//            }).BeginSubCommand("imat").HandleWith(delegate (TextCommandCallingArgs args)
//            {
//                string text = Lang.Get(Domain + ":" + "chat-share-loc-text");
//                Vec3i vec3i = args.Caller.Player.Entity.Pos.AsBlockPos.ToLocalPosition(clientAPI);
//                text = string.Concat(new string[]
//                {
//                    text,
//                    vec3i.X.ToString(),
//                    " ",
//                    vec3i.Y.ToString(),
//                    " ",
//                    vec3i.Z.ToString()
//                });
//                clientAPI.SendChatMessage(text, null);
//                return TextCommandResult.Success("", null);
//            }).EndSubCommand().BeginSubCommand("this").HandleWith(delegate (TextCommandCallingArgs args)
//            {
//                string text = "";
//                string textDomain = "";

//                ItemSlot activeHotbarSlot = args.Caller.Player.InventoryManager.ActiveHotbarSlot;
//                if (activeHotbarSlot == null || activeHotbarSlot.Empty)
//                {
//                    return TextCommandResult.Error(Lang.Get(Domain + ":" + "chat-nothing-to-share"), "");
//                }
//                text += activeHotbarSlot.Itemstack.GetName();

//                textDomain += activeHotbarSlot.Itemstack.Collectible.Code.ShortDomain();

//                if (activeHotbarSlot.Itemstack.Collectible.Code.HasDomain())
//                {
//                    text = " [" + textDomain.UcFirst() + "] " + text;
//                }

//                if (activeHotbarSlot.StackSize > 1)
//                {
                
//                   text = text + " (x" + activeHotbarSlot.StackSize.ToString() + ")";
                    
//                }
//                clientAPI.SendChatMessage(text, null);
//                return TextCommandResult.Success("", null);
//            }).EndSubCommand();
//        }
//    }
//}



//namespace ModTemplate
//{
//    public class ModTemplateModSystem : ModSystem
//    {

//        public override void Start(ICoreAPI api)
//        {
//            base.Start(api);
//            //api.RegisterBlockBehaviorClass("UpgradeableFirePit", typeof(UpgradeableFirePit));
//            //api.RegisterCollectibleBehaviorClass("ImmersiveSawing", typeof(ImmersiveSawing));
//            //api.RegisterItemClass("UnstableWard", typeof(ItemUnstableWard));


//            api.RegisterBlockClass("BlockSafeFrame", typeof(BlockSafeFrame));
//        }
//    }


//    /* public class ImmersiveSawing : CollectibleBehavior
//    {
//        public ImmersiveSawing(CollectibleObject saw) : base(saw)
//        {
//        }

//        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
//        {
//            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
//            if (blockSel != null && this.IsLog(blockSel, byEntity))
//            {
//                handHandling = EnumHandHandling.PreventDefault;
//                if (byEntity.World.Side == EnumAppSide.Server)
//                {
//                    byEntity.WatchedAttributes.SetBool("didchop", false);
//                    byEntity.WatchedAttributes.SetBool("haschoppedblock", false);
//                }
//            }
//        }

//        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
//        {
//            base.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel, ref handling);
//            handling = EnumHandling.PreventDefault;
//            Block block = blockSel?.Block;
//            IPlayer player = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
//            if (blockSel != null && this.IsLog(blockSel, byEntity))
//            {
//                if (byEntity.World.Side == EnumAppSide.Client) {
//                    if (secondsUsed > 0.4f && !byEntity.WatchedAttributes.GetBool("didchop", false))
//                    {
//                        handling = EnumHandling.PreventDefault;
//                        byEntity.WatchedAttributes.SetBool("didchop", true);
//                        byEntity.WatchedAttributes.SetBool("haschoppedblock", true);
//                    }
//                }
//                if (byEntity.World.Side == EnumAppSide.Server && secondsUsed > 5f && !byEntity.WatchedAttributes.GetBool("haschoppedblock", false))
//                {
//                    int firewoodAmount = this.getFirewoodAmount(blockSel, byEntity);
//                    for (int i = 0; i < firewoodAmount; i++) { player.Entity.TryGiveItemStack(new ItemStack(byEntity.World.GetItem(new AssetLocation("plank-" + ReturnBlock(blockSel, byEntity).Variant["wood"])), 1)); }
//                    byEntity.Api.World.BlockAccessor.BreakBlock(blockSel.Position, player, 0);
//                    byEntity.Api.World.BlockAccessor.MarkBlockDirty(blockSel.Position);
//                    byEntity.WatchedAttributes.SetBool("haschoppedblock", true); 

//                }
//            }

//            return secondsUsed < 10f;
//        }


//        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason, ref EnumHandling handled)
//        {
//            base.OnHeldInteractCancel(secondsUsed, slot, byEntity, blockSel, entitySel, cancelReason, ref handled);
//            handled = EnumHandling.PreventDefault;
//            return true;
//        }


//        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
//        {
//            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel, ref handling);
//        }

//        private Block ReturnBlock(BlockSelection blockSel, EntityAgent byEntity) { 
//            return byEntity.World.BlockAccessor.GetBlock(blockSel.Position);
//        }

//        private bool IsLog(BlockSelection blockSel, EntityAgent byEntity)
//        {
//            return byEntity.World.BlockAccessor.GetBlock(blockSel.Position).Code.Path.StartsWith("log-placed") || byEntity.World.BlockAccessor.GetBlock(blockSel.Position).Code.Path.StartsWith("logsection-placed") || byEntity.World.BlockAccessor.GetBlock(blockSel.Position).Code.Path.StartsWith("debarkedlog");
//        }

//        private int getFirewoodAmount(BlockSelection blockSel, EntityAgent byEntity)
//        {
//            string path = byEntity.World.BlockAccessor.GetBlock(blockSel.Position).Code.Path;
//            if (!path.Contains("kapok") && !path.Contains("purpleheart") && !path.Contains("ebony"))
//            {
//                return 4;
//            }
//            return 3;
//        }
//    }

//    public class ItemUnstableWard : Item {

//        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
//        {
//            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);

//            if (blockSel != null)
//            {
//                if (byEntity.Controls.ShiftKey)
//                {
//                    handling = EnumHandHandling.PreventDefault;
//                }
//            }
//        }

//        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
//        {
//            base.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel);

//            if (blockSel != null) {
//                if (byEntity.World.Side == EnumAppSide.Client) 
//                { 

//                }
//                if (byEntity.World.Side == EnumAppSide.Server) 
//                {
//                    int id = 0;
//                    foreach (Block blockType in byEntity.World.Blocks)
//                    {

//                        if(blockType == null) {  continue; }

//                        if(blockType.Code == null) { continue; }

//                        if (1 > api.World.Rand.Next(500))
//                        {
//                            if (IsLog(blockType) || IsRock(blockType) || IsOre(blockType))
//                            {
//                                id = blockType.Id;
//                                break;
//                            }

//                        }
//                    }

//                    Block block = api.World.GetBlock(id);
//                    if (block == null) return false;

//                    if (secondsUsed > 0.3f)
//                    {

//                        api.World.BlockAccessor.ExchangeBlock(block.Id, blockSel.Position);
//                        api.World.BlockAccessor.MarkBlockDirty(blockSel.Position);
//                        api.World.BlockAccessor.MarkBlockEntityDirty(blockSel.Position);
//                    }
//                }
//            }

//            return secondsUsed < 1f;
//        }

//        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
//        {
//            base.OnHeldInteractCancel(secondsUsed, slot, byEntity, blockSel, entitySel, cancelReason);

//            if (cancelReason == EnumItemUseCancelReason.MovedAway) return true;

//            return false;
//        }

//        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
//        {
//            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);
//        }

//        private bool IsRock(Block blockType) {
//            return blockType.Code.Path.StartsWith("rock");
//        }

//        private bool IsOre(Block blockType) {
//            return blockType.Code.Path.StartsWith("ore");
//        }

//        private bool IsLog(Block blockType)
//        {
//            return blockType.Code.Path.StartsWith("log-placed") || blockType.Code.Path.StartsWith("logsection-placed") || blockType.Code.Path.StartsWith("debarkedlog");
//        }

//    } */


//    public class BlockSafe : BlockGenericTypedContainer { 

//    }

//    public class BlockSafeFrame : Block, IMultiBlockColSelBoxes
//    {

//        public string BlockName { get; set; }

//        private Cuboidf[] mirroredColBox;

//        public override void OnLoaded(ICoreAPI api)
//        {
//            base.OnLoaded(api);
//            mirroredColBox = new Cuboidf[1] { CollisionBoxes[0].RotatedCopy(0f, 180f, 0f, new Vec3d(0.5, 0.5, 0.5)) };
//        }

//        public Cuboidf[] MBGetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos, Vec3i offset)
//        {
//            return mirroredColBox;
//        }

//        public Cuboidf[] MBGetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos, Vec3i offset)
//        {
//            return mirroredColBox;
//        }


//        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
//        {
//            if (blockSel != null) 
//            {
//                if(blockSel.Block == null) return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);


//                if (itemstack.Block.Variant["parts"] != null)
//                {
//                    if (blockSel.Block.Variant["parts"] != null)
//                    {
//                        if (blockSel.Block.Variant["parts"].StartsWith("frame"))
//                        {
//                            return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
//                        }
//                    }
//                }
//            }

//            return false;
//        }

//        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
//        {
//            return true;
//        }

//        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
//        {

//            BlockPos blockPos = blockSel.Position;

//            if (blockPos != null)
//            {

//                if (world.Side == EnumAppSide.Client)
//                {


//                }

//                if (world.Side == EnumAppSide.Server)
//                {

//                    if (byPlayer.InventoryManager.ActiveHotbarSlot.Empty) return base.OnBlockInteractStep(secondsUsed, world, byPlayer, blockSel);

//                    if (IsFacingPLayer(blockSel))
//                    {
//                        if (DoesItMatch(byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack))
//                        {
//                            string firstPart = Code.FirstCodePart();

//                            if (api.World.GetBlock(new AssetLocation(Code.Domain, firstPart + "-" + blockSel.Face.Code)) is BlockGenericTypedContainer BlockGenericTyped) // add a other way around finding the right block.
//                            {
//                                if (new ItemStack(BlockGenericTyped) is ItemStack itemStack)
//                                {

//                                    /// <summary>
//                                    ///  DoPlaceBlock to get the entity because without it we can't get the EntityTypedContainer.type
//                                    /// </summary>

//                                    if (BlockGenericTyped.DoPlaceBlock(world, byPlayer, blockSel, itemStack))
//                                    {
//                                        if (itemStack.Attributes != null)
//                                        {
//                                            if (api.World.BlockAccessor.GetBlockEntity(blockPos) is BlockEntityGenericTypedContainer EntityTypedContainer)
//                                            {
//                                                string @string = itemStack.Attributes.GetAsString("types", "normal-generic");

//                                                foreach (var type in Variant)
//                                                {

//                                                    if (type.Key == "types")
//                                                    {
//                                                        if (type.Value == @string) continue;

//                                                        @string = type.Value;
//                                                    }

//                                                }

//                                                if (@string != EntityTypedContainer.type)
//                                                {
//                                                    EntityTypedContainer.type = @string;
//                                                    EntityTypedContainer.MarkDirty();
//                                                }
//                                            }
//                                        }
//                                    }
//                                }
//                            };

//                            return false;
//                        }
//                    }
//                }
//            }

//            return true;
//        }

//        /// <summary>
//        /// Does the heldstack match the placed block if so return true and add the door to safe.
//        /// </summary>
//        /// <param name="heldStack"></param>
//        /// <returns></returns>

//        private bool DoesItMatch(ItemStack heldStack) {

//            if (heldStack.Block.Code.FirstCodePart() == base.Code.FirstCodePart())
//            {
//                if (heldStack.Block.Variant["parts"] != null)
//                {
//                    if (heldStack.Block.Variant["parts"].StartsWith("door") == true && base.Variant["parts"].StartsWith("frame"))
//                    {
//                        if (heldStack.Block.Variant["types"] == base.Variant["types"])
//                        {
//                            return true;
//                        }
//                    }

//                }
//            }
//            return false;
//        }

//        /// <summary>
//        /// Is the player facing the right part of the block if so return true
//        /// </summary>
//        /// <param name="blockSel"></param>
//        /// <returns></returns>

//        private bool IsFacingPLayer(BlockSelection blockSel)
//        {
//            return blockSel.Face.Code switch
//            {
//                "north" => Shape.rotateY == 180,
//                "east" => Shape.rotateY == 90,
//                "west" => Shape.rotateY == 270,
//                "south" => Shape.rotateY == 0,
//                _ => false
//            };
//        }
//    }


//  /*  public class UpgradeableFirePit : BlockBehavior
//    {
//        public UpgradeableFirePit(Block block) : base(block)
//        {
//        }

//        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
//        {
//            base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);

//            if (blockSel != null)
//            {
//                if (byPlayer.Entity.Controls.ShiftKey)
//                {
//                    if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityFirepit entityFirepit)
//                    {
//                        BlockFirepit upgradedFirePit = world.GetBlock(new AssetLocation("mymodid", "improvisefirepit-cold")) as BlockFirepit;

//                        if (upgradedFirePit == null)
//                        {
//                            world.Logger.Chat("can't find upgradedfirepit");
//                            handling = EnumHandling.PassThrough;
//                            return false;
//                        }

//                        if (entityFirepit.Block.Code.EndVariant() == "lit")
//                        {
//                            handling = EnumHandling.PassThrough;
//                            return false;
//                        }

//                        if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Item.Code.FirstCodePart() == "upgradekit") {
//                            if (!(entityFirepit.Block as BlockFirepit).TryConstruct(world, blockSel.Position, collObj, byPlayer))
//                            {
//                                if (entityFirepit.OnPlayerRightClick(byPlayer, blockSel))
//                                {
//                                    byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(1);
//                                    world.BlockAccessor.ExchangeBlock(upgradedFirePit.Id, blockSel.Position);
//                                    world.BlockAccessor.MarkBlockDirty(blockSel.Position);
//                                    world.BlockAccessor.MarkBlockEntityDirty(blockSel.Position);
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            return true;
//        }
//    }
//  */
//}
