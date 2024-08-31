// -----------------------------------------------------------------------
// <copyright file="TimedThrowable.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Items
{
    using Exiled.API.Features.Pickups;
    using Exiled.API.Features.Pickups.Projectiles;
    using Exiled.API.Interfaces;

    using InventorySystem.Items.Pickups;
    using InventorySystem.Items.ThrowableProjectiles;

    using UnityEngine;

    /// <summary>
    /// A wrapper class for throwable items that have fuseTime.
    /// </summary>
    public class TimedThrowable : Throwable, IWrapper<ThrowableItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimedThrowable"/> class.
        /// </summary>
        /// <param name="itemBase">The base <see cref="ThrowableItem"/> class.</param>
        public TimedThrowable(ThrowableItem itemBase)
            : base(itemBase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedThrowable"/> class.
        /// </summary>
        /// <param name="type">The <see cref="ItemType"/> of the throwable item.</param>
        /// <param name="player">The owner of the throwable item. Leave <see langword="null"/> for no owner.</param>
        /// <remarks>The player parameter will always need to be defined if this throwable is custom using Exiled.CustomItems.</remarks>
        internal TimedThrowable(ItemType type, Player player = null)
            : base(type, player)
        {
        }

        /// <summary>
        /// Gets or sets how long the fuse will last.
        /// </summary>
        public float FuseTime { get; set; }

        /// <summary>
        /// Clones current <see cref="Throwable"/> object.
        /// </summary>
        /// <returns> New <see cref="Throwable"/> object. </returns>
        public override Item Clone() => new Throwable(Type)
        {
            PinPullTime = PinPullTime,
            Repickable = Repickable,
        };

        /// <summary>
        /// Returns the Throwable in a human readable format.
        /// </summary>
        /// <returns>A string containing Throwable-related data.</returns>
        public override string ToString() => $"{Type} ({Serial}) [{Weight}] *{Scale}* |{PinPullTime}|";

        /// <inheritdoc/>
        protected override void InitializeProperties(ThrowableItem throwable)
        {
            base.InitializeProperties(throwable);
            if (throwable.Projectile is TimeGrenade timeGrenade)
            {
                FuseTime = timeGrenade._fuseTime;
            }
        }
    }
}
