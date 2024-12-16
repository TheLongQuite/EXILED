// -----------------------------------------------------------------------
// <copyright file="ChangedIntoGrenadeEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Map
{
    using Exiled.API.Features.Pickups;
    using Exiled.API.Features.Pickups.Projectiles;
    using Exiled.Events.EventArgs.Interfaces;
    using InventorySystem.Items.ThrowableProjectiles;

    /// <summary>
    /// Contains all information for when the server is turned a pickup into a live grenade.
    /// </summary>
    public class ChangedIntoGrenadeEventArgs : IExiledEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangedIntoGrenadeEventArgs"/> class.
        /// </summary>
        /// <param name="pickup">The <see cref="API.Features.Pickups.Pickup"/> being changed.</param>
        /// <param name="projectile">The <see cref="TimeGrenadeProjectile"/>.</param>
        public ChangedIntoGrenadeEventArgs(TimedGrenadePickup pickup, ThrownProjectile projectile)
        {
            Pickup = API.Features.Pickups.Pickup.Get<GrenadePickup>(pickup);
            Projectile = (Projectile)API.Features.Pickups.Pickup.Get(projectile);
        }

        /// <summary>
        /// Gets a value indicating the pickup that changed into a grenade.
        /// </summary>
        public GrenadePickup Pickup { get; }

        /// <summary>
        /// Gets a value indicating the projectile that spawned.
        /// </summary>
        // TODO: Make that TimedGrenadeProjectile
        public Projectile Projectile { get; }

        /// <summary>
        /// Gets or sets a value indicating how long the fuse of the changed grenade will be.
        /// </summary>
        [Obsolete("Use Projectile.Is(Projectile, out TimeGrenadeProjectile timeGrenadeProjectile) ? timeGrenadeProjectile.FuseTime : 0 instead of this", true)]
        public double FuseTime { get; set; }
    }
}
