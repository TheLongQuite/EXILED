// -----------------------------------------------------------------------
// <copyright file="CustomGrenade.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------
namespace Exiled.CustomItems.API.Features
{
    using System;

    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.API.Features.Pickups;
    using Exiled.API.Features.Pickups.Projectiles;
    using Exiled.API.Features.Roles;
    using Exiled.Events.EventArgs.Map;
    using Exiled.Events.EventArgs.Player;
    using Footprinting;
    using InventorySystem.Items;
    using InventorySystem.Items.Pickups;
    using InventorySystem.Items.ThrowableProjectiles;
    using Mirror;
    using UnityEngine;
    using Map = Exiled.Events.Handlers.Map;
    using Object = UnityEngine.Object;

    /// <summary>
    ///     The Custom Grenade base class.
    /// </summary>
    public abstract class CustomGrenade : CustomItem
    {
        /// <summary>
        ///     Gets or sets the <see cref="ItemType" /> to use for this item.
        /// </summary>
        public override ItemType Type
        {
            get => base.Type;
            set
            {
                if (!value.IsThrowable() && value != ItemType.None)
                    throw new ArgumentOutOfRangeException("Type", value, "Invalid grenade type.");

                base.Type = value;
            }
        }
        /// <summary>
        ///     Gets or sets a value indicating whether or not the grenade should explode immediately when contacting any surface.
        /// </summary>
        public abstract bool ExplodeOnCollision { get; set; }
        /// <summary>
        ///     Gets or sets a value indicating how long grenate will not detonate when contacting any surface is you use
        ///     <see cref="ExplodeOnCollision" />.
        /// </summary>
        public virtual float ExplodeOnCollisionFuseTime { get; set; } = 0.15f;
        /// <summary>
        ///     Gets or sets a value indicating how long the grenade's fuse time should be.
        /// </summary>
        public abstract float FuseTime { get; set; }

        /// <inheritdoc />
        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ThrowingRequest += OnInternalThrowingRequest;
            Exiled.Events.Handlers.Player.ThrownProjectile += OnInternalThrownProjectile;
            Map.ExplodingGrenade += OnInternalExplodingGrenade;
            Map.ChangedIntoGrenade += OnInternalChangedIntoGrenade;

            base.SubscribeEvents();
        }

        /// <inheritdoc />
        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ThrowingRequest -= OnInternalThrowingRequest;
            Exiled.Events.Handlers.Player.ThrownProjectile -= OnInternalThrownProjectile;
            Map.ExplodingGrenade -= OnInternalExplodingGrenade;
            Map.ChangedIntoGrenade -= OnInternalChangedIntoGrenade;

            base.UnsubscribeEvents();
        }

        /// <summary>
        ///     Throw the CustomGrenade object.
        /// </summary>
        /// <param name="position">The <see cref="Vector3" />position to throw at.</param>
        /// <param name="force">The amount of force to throw with.</param>
        /// <param name="weight">The <see cref="float" />Weight of the Grenade.</param>
        /// <param name="fuseTime">The <see cref="float" />FuseTime of the grenade.</param>
        /// <param name="grenadeType">The <see cref="ItemType" />of the grenade to spawn.</param>
        /// <param name="player">The <see cref="Player" /> to count as the thrower of the grenade.</param>
        /// <returns>The <see cref="Pickup" /> spawned.</returns>
        public virtual Pickup Throw(Vector3 position, float force, float weight, float fuseTime = 3f, ItemType grenadeType = ItemType.GrenadeHE, Player? player = null)
        {
            if (player is null)
                player = Server.Host;

            player.Role.Is(out FpcRole fpcRole);
            Vector3 velocity = fpcRole.FirstPersonController.FpcModule.Motor.Velocity;

            Throwable throwable = (Throwable)Item.Create(grenadeType, player);

            ThrownProjectile thrownProjectile = Object.Instantiate(throwable.Base.Projectile, position, throwable.Owner.CameraTransform.rotation);

            PickupSyncInfo newInfo = new()
            {
                ItemId = throwable.Type,
                Locked = !throwable.Base._repickupable,
                Serial = ItemSerialGenerator.GenerateNext(),
                WeightKg = weight,
            };
            if (thrownProjectile is TimeGrenade time)
                time._fuseTime = fuseTime;
            thrownProjectile.NetworkInfo = newInfo;
            thrownProjectile.PreviousOwner = new Footprint(throwable.Owner.ReferenceHub);
            NetworkServer.Spawn(thrownProjectile.gameObject);
            thrownProjectile.InfoReceivedHook(default, newInfo);
            if (thrownProjectile.TryGetComponent(out Rigidbody component))
                throwable.Base.PropelBody(component, throwable.Base.FullThrowSettings.StartTorque, ThrowableNetworkHandler.GetLimitedVelocity(velocity), force, throwable.Base.FullThrowSettings.UpwardsFactor);
            thrownProjectile.ServerActivate();

            return Pickup.Get(thrownProjectile);
        }

        /// <summary>
        ///     Checks to see if the grenade is a custom grenade.
        /// </summary>
        /// <param name="grenade">The <see cref="Projectile">grenade</see> to check.</param>
        /// <returns>True if it is a custom grenade.</returns>
        public virtual bool Check(Projectile grenade) => grenade != null && TrackedSerials.Contains(grenade.Serial);

        /// <summary>
        ///     Handles tracking thrown requests by custom grenades.
        /// </summary>
        /// <param name="ev"><see cref="ThrowingRequestEventArgs" />.</param>
        protected virtual void OnThrowingRequest(ThrowingRequestEventArgs ev)
        {
        }

        /// <summary>
        ///     Handles tracking thrown custom grenades.
        /// </summary>
        /// <param name="ev"><see cref="ThrownProjectileEventArgs" />.</param>
        protected virtual void OnThrownProjectile(ThrownProjectileEventArgs ev)
        {
        }

        /// <summary>
        ///     Handles tracking exploded custom grenades.
        /// </summary>
        /// <param name="ev"><see cref="ExplodingGrenadeEventArgs" />.</param>
        protected virtual void OnExploding(ExplodingGrenadeEventArgs ev)
        {
        }

        /// <summary>
        ///     Handles the tracking of custom grenade pickups that are changed into live grenades by a frag grenade explosion.
        /// </summary>
        /// <param name="ev"><see cref="ChangedIntoGrenadeEventArgs" />.</param>
        protected virtual void OnChangedIntoGrenade(ChangedIntoGrenadeEventArgs ev)
        {
        }

        private void OnInternalThrowingRequest(ThrowingRequestEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            Log.Debug($"{ev.Player.Nickname} is requesting throw of {Name}!");

            OnThrowingRequest(ev);
        }

        private void OnInternalThrownProjectile(ThrownProjectileEventArgs ev)
        {
            if (!Check(ev.Throwable))
                return;

            OnThrownProjectile(ev);
            Log.Debug($"{ev.Player.Nickname} has thrown a {Name} ({FuseTime}) {ev.Player.Items.ToString(true)}!");

            if (ev.Projectile is TimeGrenadeProjectile timeGrenade)
                timeGrenade.FuseTime = FuseTime;

            if (ExplodeOnCollision)
            {
                ev.Projectile.GameObject.AttachActionOnCollision(
                    () =>
                {
                    if (ev.Projectile is TimeGrenadeProjectile grenadeProjectile)
                    {
                        grenadeProjectile.FuseTime = 0.1f;
                        ev.Projectile.PreviousOwner?.RemoveItem(ev.Projectile.Serial); // фикс наличия предмета в инвентаре на момент смерти от него же
                    }
                }, Server.Host,
                    ExplodeOnCollisionFuseTime);
            }
        }

        private void OnInternalExplodingGrenade(ExplodingGrenadeEventArgs ev)
        {
            if (Check(ev.Projectile))
            {
                OnExploding(ev);
                Log.Debug($"A {Name} is exploding! IsAllowed: {ev.IsAllowed}");
            }
        }

        private void OnInternalChangedIntoGrenade(ChangedIntoGrenadeEventArgs ev)
        {
            if (!Check(ev.Pickup))
                return;

            if (ev.Projectile is TimeGrenadeProjectile timedGrenade)
                timedGrenade.FuseTime = FuseTime;

            OnChangedIntoGrenade(ev);
            Log.Debug($"A {Name} ChangedIntoGrenade");

            if (ExplodeOnCollision)
                ev.Projectile.GameObject.AttachActionOnCollision(() => ((EffectGrenade)ev.Projectile.Base).TargetTime = 0.1f, ev.Pickup.PreviousOwner ?? Server.Host, ExplodeOnCollisionFuseTime);
        }
    }
}