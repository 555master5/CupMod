﻿using CupMod.Entities;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;


namespace CupMod.Blocks
{
    public class BlockCup : BlockLiquidContainerTopOpened
    {
        private bool IsCurrentlyThrowing = false;

        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            if (GetCurrentLitres(itemslot.Itemstack) > 0 || byEntity.Controls.ShiftKey)
            {
                base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
                IsCurrentlyThrowing = false;
                return;
            }
            if (GetCurrentLitres(itemslot.Itemstack) == 0 && !byEntity.Controls.ShiftKey)
            {
                IsCurrentlyThrowing = true;
                byEntity.Attributes.SetInt("aiming", 1);
                byEntity.Attributes.SetInt("aimingCancel", 0);
                byEntity.StartAnimation("aim");
                handHandling = EnumHandHandling.PreventDefault;
                return;
            }
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            base.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel);
            if (secondsUsed >= 0.95f && IsCurrentlyThrowing == false)
            {
                base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);
            }

            if (IsCurrentlyThrowing)
            {
                base.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel);
                bool result = true;
                bool preventDefault = false;
                foreach (CollectibleBehavior behavior in CollectibleBehaviors)
                {
                    EnumHandling handled = EnumHandling.PassThrough;

                    bool behaviorResult = behavior.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel, ref handled);
                    if (handled != EnumHandling.PassThrough)
                    {
                        result &= behaviorResult;
                        preventDefault = true;
                    }

                    if (handled == EnumHandling.PreventSubsequent) return result;
                }
                if (preventDefault) return result;

                if (byEntity.Attributes.GetInt("aimingCancel") == 1) return false;

                if (byEntity.World is IClientWorldAccessor)
                {
                    ModelTransform tf = new ModelTransform();
                    tf.EnsureDefaultValues();

                    float offset = GameMath.Clamp(secondsUsed * 3, 0, 1.5f);

                    tf.Translation.Set(offset / 4f, offset / 2f, 0);
                    tf.Rotation.Set(0, 0, GameMath.Min(90, secondsUsed * 360 / 1.5f));

                    byEntity.Controls.UsingHeldItemTransformBefore = tf;
                }
                return true;
            }
            return true;
            
        }

        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            base.OnHeldInteractCancel(secondsUsed, slot, byEntity, blockSel, entitySel, cancelReason);
            if (IsCurrentlyThrowing)
            {
                IsCurrentlyThrowing = false;
                byEntity.Attributes.SetInt("aiming", 0);
                byEntity.StopAnimation("aim");

                if (cancelReason != EnumItemUseCancelReason.ReleasedMouse)
                {
                    byEntity.Attributes.SetInt("aimingCancel", 1);
                }

                return true;
            }
            return true;
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);
            if (IsCurrentlyThrowing)
            { 
                IsCurrentlyThrowing = false;
                bool preventDefault = false;

                foreach (CollectibleBehavior behavior in CollectibleBehaviors)
                {
                    EnumHandling handled = EnumHandling.PassThrough;

                    behavior.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel, ref handled);
                    if (handled != EnumHandling.PassThrough) preventDefault = true;

                    if (handled == EnumHandling.PreventSubsequent) return;
                }

                if (preventDefault) return;

                if (byEntity.Attributes.GetInt("aimingCancel") == 1) return;

                byEntity.Attributes.SetInt("aiming", 0);
                byEntity.StopAnimation("aim");

                if (secondsUsed < 0.35f) return;

                float damage = 1;
                ItemStack stack = slot.TakeOut(1);
                //Used for glass and clay types
                string cup_color = stack.Collectible.Variant["color"];
                //Used for flagons
                string cup_wood = stack.Collectible.Variant["wood"];
                string cup_metal = stack.Collectible.Variant["metal"];

                string cup_type = stack.Collectible.Code.FirstCodePart();
                Console.WriteLine(cup_type);
                slot.MarkDirty();

                IPlayer byPlayer = null;
                if (byEntity is EntityPlayer) byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
                byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/player/throw"), byEntity, byPlayer, false, 8);

                //Cup entity is created - uses base name of cup type (claycup, wineglass, etc) and color to create entity
                EntityProperties type;
                switch (cup_type)
                {
                    case "claycup":
                        type = byEntity.World.GetEntityType(new AssetLocation("cupmod", $"throwncup-{cup_color}"));
                        break;
                    case "claymug":
                        type = byEntity.World.GetEntityType(new AssetLocation("cupmod", $"thrownmug-{cup_color}"));
                        break;
                    case "wineglass":
                        type = byEntity.World.GetEntityType(new AssetLocation("cupmod", $"thrownwineglass-{cup_color}"));
                        break;
                    case "flagon":
                        type = byEntity.World.GetEntityType(new AssetLocation("cupmod", $"thrownflagon-{cup_wood}-{cup_metal}"));
                        break;
                    default:
                        type = byEntity.World.GetEntityType(new AssetLocation("cupmod", $"throwncup-{cup_color}"));
                        break;
                }
                Entity entity = byEntity.World.ClassRegistry.CreateEntity(type);
                ((EntityThrownCup)entity).HorizontalImpactBreakChance = stack.Collectible.Attributes["breakchance"].AsFloat(0f);
                Console.WriteLine("[Cup Mod] Break chance set as " + stack.Collectible.Attributes["breakchance"].AsFloat(0f));

                ((EntityThrownCup)entity).FiredBy = byEntity;
                ((EntityThrownCup)entity).Damage = damage;
                ((EntityThrownCup)entity).ProjectileStack = stack;

                float acc = 1 - byEntity.Attributes.GetFloat("aimingAccuracy", 0);
                double rndpitch = byEntity.WatchedAttributes.GetDouble("aimingRandPitch", 1) * acc * 0.75;
                double rndyaw = byEntity.WatchedAttributes.GetDouble("aimingRandYaw", 1) * acc * 0.75;

                Vec3d pos = byEntity.ServerPos.XYZ.Add(0, byEntity.LocalEyePos.Y, 0);
                Vec3d aheadPos = pos.AheadCopy(1, byEntity.ServerPos.Pitch + rndpitch, byEntity.ServerPos.Yaw + rndyaw);
                Vec3d velocity = (aheadPos - pos) * 0.5;

                entity.ServerPos.SetPosWithDimension(
                    byEntity.ServerPos.BehindCopy(0.21).XYZ.Add(0, byEntity.LocalEyePos.Y, 0)
                );

                entity.ServerPos.Motion.Set(velocity);

                entity.Pos.SetFrom(entity.ServerPos);
                entity.World = byEntity.World;

                byEntity.World.SpawnEntity(entity);
                byEntity.StartAnimation("throw");

                //byEntity.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(2f);
            }

        }
    }
}