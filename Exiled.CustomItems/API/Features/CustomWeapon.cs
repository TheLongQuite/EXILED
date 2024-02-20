// -----------------------------------------------------------------------
// <copyright file="CustomWeapon.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------
namespace Exiled.CustomItems.API.Features
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using AudioSystem.Models.SoundConfigs;
    using CustomPlayerEffects;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.DamageHandlers;
    using Exiled.API.Features.Items;
    using Exiled.API.Features.Pickups;
    using Exiled.Events.EventArgs.Item;
    using Exiled.Events.EventArgs.Player;
    using InventorySystem.Items.Firearms.Attachments;
    using InventorySystem.Items.Firearms.Attachments.Components;
    using InventorySystem.Items.Firearms.BasicMessages;
    using MEC;
    using PlayerRoles;
    using UnityEngine;

    /// <summary>
    ///     The Custom Weapon base class.
    /// </summary>
    public abstract class CustomWeapon : CustomItem
    {
        /// <inheritdoc />
        public override ItemType Type
        {
            get => base.Type;
            set
            {
                if (!value.IsWeapon(false) && value != ItemType.None)
                    throw new ArgumentOutOfRangeException($"{nameof(Type)}", value, "Invalid weapon type.");

                base.Type = value;
            }
        }
        /// <summary>
        ///     Gets or sets value indicating what <see cref="Attachment" />s the weapon will have.
        /// </summary>
        public virtual AttachmentName[] Attachments { get; set; } = { };
        /// <summary>
        ///     Gets or sets the weapon damage.
        /// </summary>
        public abstract float Damage { get; set; }
        /// <summary>
        ///     Gets or sets a value indicating how big of a clip the weapon will have.
        /// </summary>
        public virtual byte ClipSize { get; set; }
        /// <summary>
        ///     Gets or sets a value indicating how many ammo will be spent per shot.
        /// </summary>
        public virtual byte AmmoUsage { get; set; } = 1;
        /// <summary>
        ///     Gets or sets a value indicating whether firearm can be unloaded.
        /// </summary>
        public virtual bool CanUnload { get; set; } = true;
        /// <summary>
        ///     Gets or sets a value indicating whether firearm's attachments can be modified.
        /// </summary>
        public bool AllowAttachmentsChange { get; set; } = true;
        public LocalSoundConfig? ShotAudio { get; set; } = new();
        [Description("Кулдаун на выстрелы. Работает только при ClipSize > 1 и FireCooldown > 0. -1 для отключения.")]
        public float FireCooldown { get; set; } = -1;
        [Description("Сообщение при попытке перезарядить оружие под кулдауном. {0} - кулдаун из конфига")]
        public string WeaponNotReady { get; set; } = "Оружие ещё не готово к выстрелу! Оно может стрелять только раз в {0} секунд.";
        [Description("Множители урона в зависимости от брони и точки попадания. Словарь ТипБрони: (ЗонаПопадания: МножительУрона)")]
        public Dictionary<ItemType, Dictionary<HitboxType, float>> ArmorAndZoneDamageMultipliers { get; set; } = new()
        {
            [ItemType.None] = new Dictionary<HitboxType, float>
            {
                [HitboxType.Headshot] = 1,
                [HitboxType.Limb] = 1,
                [HitboxType.Body] = 1,
            },
            [ItemType.ArmorLight] = new Dictionary<HitboxType, float>
            {
                [HitboxType.Headshot] = 1,
            },
            [ItemType.ArmorCombat] = new Dictionary<HitboxType, float>
            {
                [HitboxType.Headshot] = 1,
            },
            [ItemType.ArmorHeavy] = new Dictionary<HitboxType, float>
            {
                [HitboxType.Headshot] = 1,
            },
        };
        [Description("Множители урона для ролей. Словарь RoleType: МножительУрона")]
        public Dictionary<RoleTypeId, float> RoleDamageMultipliers { get; set; } = new()
        {
            { RoleTypeId.Scp096, 1 },
            { RoleTypeId.Scp173, 1 },
        };
        private readonly HashSet<Player> cooldownedPlayers = new();

        /// <inheritdoc />
        public override Pickup? Spawn(Vector3 position, Player? previousOwner = null)
        {
            if (Item.Create(Type) is not Firearm firearm)
            {
                Log.Debug($"{nameof(Spawn)}: Item is not Firearm.");
                return null;
            }

            if (!Attachments.IsEmpty())
                firearm.AddAttachment(Attachments);

            if (firearm.Type != ItemType.GunShotgun)
            {
                firearm.Ammo = ClipSize;
                firearm.MaxAmmo = ClipSize;
            }

            Pickup? pickup = firearm.CreatePickup(position);

            if (pickup is null)
            {
                Log.Debug($"{nameof(Spawn)}: Pickup is null.");
                return null;
            }

            pickup.Weight = Weight;
            pickup.Scale = Scale;
            if (previousOwner is not null)
                pickup.PreviousOwner = previousOwner;

            TrackedSerials.Add(pickup.Serial);
            return pickup;
        }

        /// <inheritdoc />
        public override Pickup? Spawn(Vector3 position, Item item, Player? previousOwner = null)
        {
            if (item is Firearm firearm)
            {
                if (!Attachments.IsEmpty())
                    firearm.AddAttachment(Attachments);

                if (firearm.Type != ItemType.GunShotgun)
                {
                    firearm.Ammo = ClipSize;
                    firearm.MaxAmmo = ClipSize;
                }

                Log.Debug($"{nameof(Name)}.{nameof(Spawn)}: Spawning weapon with {firearm.Ammo} ammo.");

                Pickup pickup = firearm.CreatePickup(position);
                pickup.Scale = Scale;

                if (previousOwner is not null)
                    pickup.PreviousOwner = previousOwner;

                TrackedSerials.Add(pickup.Serial);
                return pickup;
            }

            return base.Spawn(position, item, previousOwner);
        }

        /// <inheritdoc />
        public override void Give(Player player, Item item, bool displayMessage = true)
        {
            item.Scale = Scale;

            if (item is Firearm firearm)
            {
                if (!Attachments.IsEmpty())
                    firearm.AddAttachment(Attachments);
                if (firearm.Type != ItemType.GunShotgun)
                {
                    firearm.Ammo = ClipSize;
                    firearm.MaxAmmo = ClipSize;
                }
            }

            player.AddItem(item);

            Log.Debug($"{nameof(Give)}: Adding {item.Serial} to tracker.");
            TrackedSerials.Add(item.Serial);

            OnAcquired(player, item, displayMessage);
        }

        /// <inheritdoc />
        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ReloadingWeapon += OnInternalReloading;
            Exiled.Events.Handlers.Player.Shooting += OnInternalShooting;
            Exiled.Events.Handlers.Player.Shot += OnInternalShot;
            Exiled.Events.Handlers.Player.Hurting += OnInternalHurting;
            Exiled.Events.Handlers.Player.UnloadingWeapon += OnInternalUnloading;
            Exiled.Events.Handlers.Item.ChangingAttachments += OnInternalChangingAttachments;

            base.SubscribeEvents();
        }

        /// <inheritdoc />
        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ReloadingWeapon -= OnInternalReloading;
            Exiled.Events.Handlers.Player.Shooting -= OnInternalShooting;
            Exiled.Events.Handlers.Player.Shot -= OnInternalShot;
            Exiled.Events.Handlers.Player.Hurting -= OnInternalHurting;
            Exiled.Events.Handlers.Player.UnloadingWeapon -= OnInternalUnloading;
            Exiled.Events.Handlers.Item.ChangingAttachments -= OnInternalChangingAttachments;

            base.UnsubscribeEvents();
        }

        /// <summary>
        ///     Handles reloading for custom weapons.
        /// </summary>
        /// <param name="ev"><see cref="ReloadingWeaponEventArgs" />.</param>
        protected virtual void OnReloading(ReloadingWeaponEventArgs ev)
        {
        }

        /// <summary>
        ///     Handles shooting for custom weapons.
        /// </summary>
        /// <param name="ev"><see cref="ShootingEventArgs" />.</param>
        protected virtual void OnShooting(ShootingEventArgs ev)
        {
        }

        /// <summary>
        ///     Handles shot for custom weapons.
        /// </summary>
        /// <param name="ev"><see cref="ShotEventArgs" />.</param>
        protected virtual void OnShot(ShotEventArgs ev)
        {
        }

        /// <summary>
        ///     Handles hurting for custom weapons.
        /// </summary>
        /// <param name="ev"><see cref="HurtingEventArgs" />.</param>
        protected virtual void OnHurting(HurtingEventArgs ev)
        {
        }

        /// <summary>
        ///     Handles unloading for custom weapons.
        /// </summary>
        /// <param name="ev"><see cref="HurtingEventArgs" />.</param>
        protected virtual void OnUnloading(UnloadingWeaponEventArgs ev)
        {
        }

        private void OnInternalChangingAttachments(ChangingAttachmentsEventArgs ev)
        {
            if (Check(ev.Player.CurrentItem) && !AllowAttachmentsChange)
                ev.IsAllowed = false;
        }

        private void OnInternalReloading(ReloadingWeaponEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            if (cooldownedPlayers.Contains(ev.Player))
            {
                ev.IsAllowed = false;
                ev.Player.ShowHint(string.Format(WeaponNotReady, FireCooldown));
                return;
            }

            Log.Debug($"{nameof(Name)}.{nameof(OnInternalReloading)}: Reloading weapon. Calling external reload event..");
            OnReloading(ev);

            Log.Debug($"{nameof(Name)}.{nameof(OnInternalReloading)}: External event ended. {ev.IsAllowed}");

            if (!ev.IsAllowed)
            {
                Log.Debug($"{nameof(Name)}.{nameof(OnInternalReloading)}: External event turned is allowed to false, returning.");
                return;
            }

            if (ev.Firearm.Type == ItemType.GunShotgun)
                return;

            Log.Debug($"{nameof(Name)}.{nameof(OnInternalReloading)}: Continuing with internal reload..");
            ev.IsAllowed = false;

            byte remainingClip = ev.Firearm.Ammo;

            if (remainingClip >= ClipSize)
            {
                Log.Debug($"{nameof(Name)}.{nameof(OnInternalReloading)}: remainingClip >= ClipSize, returning.");
                return;
            }

            Log.Debug($"{ev.Player.Nickname} ({ev.Player.UserId}) [{ev.Player.Role}] is reloading a {Name} ({Id}) [{Type} ({remainingClip}/{ClipSize})]!");

            AmmoType ammoType = ev.Firearm.AmmoType;

            if (!ev.Player.Ammo.ContainsKey(ammoType.GetItemType()))
            {
                Log.Debug($"{nameof(Name)}.{nameof(OnInternalReloading)}: {ev.Player.Nickname} does not have ammo to reload this weapon.");
                return;
            }

            byte amountToReload = (byte)Math.Min(ClipSize - remainingClip, ev.Player.Ammo[ammoType.GetItemType()]);

            if (amountToReload <= 0)
                return;

            ev.Player.Connection.Send(new RequestMessage(ev.Firearm.Serial, RequestType.Reload));
            ev.Player.ReferenceHub.playerEffectsController.GetEffect<Invisible>().Intensity = 0;

            ev.Player.Ammo[ammoType.GetItemType()] -= amountToReload;
            ev.Player.Inventory.SendAmmoNextFrame = true;

            ev.Firearm.Ammo = (byte)(ev.Firearm.Ammo + amountToReload);

            Log.Debug($"{ev.Player.Nickname} ({ev.Player.UserId}) [{ev.Player.Role}] reloaded a {Name} ({Id}) [{Type} ({ev.Firearm.Ammo}/{ClipSize})]!");
        }

        private void OnInternalShooting(ShootingEventArgs ev)
        {
            if (!Check(ev.Player))
                return;

            if (cooldownedPlayers.Contains(ev.Player))
            {
                ev.IsAllowed = false;
                Log.Debug($"Disallowed shot from cooldowned on {Name} player {ev.Player.Nickname}");
                return;
            }

            if (ev.Item.Type == ItemType.GunShotgun)
                return;

            Firearm firearm = ev.Firearm;
            if (firearm.Ammo < AmmoUsage - 1)
                ev.IsAllowed = false;
            else
                firearm.Ammo -= (byte)(AmmoUsage - 1);

            OnShooting(ev);
        }

        private void OnInternalShot(ShotEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            ShotAudio?.PlayPreset(ev.Player.Transform);
            OnShot(ev);
            if (FireCooldown <= 0 || ClipSize <= 1)
                return;

            cooldownedPlayers.Add(ev.Player);
            Firearm firearm = (Firearm)ev.Player.CurrentItem;
            byte recentAmmo = firearm.Ammo;
            firearm.Ammo = 0;
            Timing.CallDelayed(FireCooldown, () =>
            {
                cooldownedPlayers.Remove(ev.Player);
                firearm.Ammo = recentAmmo;
                Log.Debug($"Cooldown of {Name} removed from player {ev.Player.Nickname}");
            });
        }

        private void OnInternalHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker is null)
                return;

            if (ev.Player is null)
            {
                Log.Debug($"{Name}: {nameof(OnInternalHurting)}: target null");
                return;
            }

            if (!Check(ev.Attacker.CurrentItem))
            {
                Log.Debug($"{Name}: {nameof(OnInternalHurting)}: !Check()");
                return;
            }

            if (ev.Attacker == ev.Player)
            {
                Log.Debug($"{Name}: {nameof(OnInternalHurting)}: attacker == target");
                return;
            }

            if (ev.DamageHandler is null)
            {
                Log.Debug($"{Name}: {nameof(OnInternalHurting)}: Handler null");
                return;
            }

            if (!ev.DamageHandler.CustomBase.BaseIs(out FirearmDamageHandler firearmDamageHandler))
            {
                Log.Debug($"{Name}: {nameof(OnInternalHurting)}: Handler not firearm");
                return;
            }

            if (!Check(firearmDamageHandler.Item))
            {
                Log.Debug($"{Name}: {nameof(OnInternalHurting)}: type != type");
                return;
            }

            ev.Amount = Damage;
            if (ev.Player.IsHuman && ArmorAndZoneDamageMultipliers.TryGetValue(ev.Player.CurrentArmor?.Type ?? ItemType.None, out Dictionary<HitboxType, float> dic) &&
                dic.TryGetValue(firearmDamageHandler.Hitbox, out float multiplier))
            {
                Log.Debug($"{Name}: {nameof(OnInternalHurting)}: Found damage muptiplier for armor/hitbox {multiplier}");
                ev.Amount *= multiplier;
            }

            if (RoleDamageMultipliers.TryGetValue(ev.Player.Role.Type, out multiplier))
            {
                Log.Debug($"{Name}: {nameof(OnInternalHurting)}: Found damage muptiplier for target role: {multiplier}");
                ev.Amount *= multiplier;
            }

            OnHurting(ev);
        }

        private void OnInternalUnloading(UnloadingWeaponEventArgs ev)
        {
            if (!Check(ev.Firearm))
                return;

            ev.IsAllowed = CanUnload;

            OnUnloading(ev);
        }
    }
}