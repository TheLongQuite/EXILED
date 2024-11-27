// -----------------------------------------------------------------------
// <copyright file="ShootingEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Player
{
    using API.Features;

    using Exiled.API.Features.Items;

    using Interfaces;

    using InventorySystem.Items.Firearms.BasicMessages;

    using RelativePositioning;

    using UnityEngine;

    using BaseFirearm = InventorySystem.Items.Firearms.Firearm;

    /// <summary>
    /// Contains all information before a player fires a weapon.
    /// </summary>
    public class ShootingEventArgs : IPlayerEvent, IDeniableEvent, IFirearmEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShootingEventArgs" /> class.
        /// </summary>
        /// <param name="shooter">
        /// <inheritdoc cref="Player" />
        /// </param>
        /// <param name="firearm">
        /// <inheritdoc cref="Firearm" />
        /// </param>
        public ShootingEventArgs(Player shooter, BaseFirearm firearm)
        {
            Player = shooter;
            Firearm = Item.Get(firearm).As<Firearm>();

            // ShotMessage = msg;
        }

        /// <summary>
        /// Gets the player who's shooting.
        /// </summary>
        public Player Player { get; }

        /// <summary>
        /// Gets the target <see cref="API.Features.Items.Firearm" />.
        /// </summary>
        public Firearm Firearm { get; }

        /// <inheritdoc/>
        public Item Item => Firearm;

        /*
        /// <summary>
        /// Gets or sets the <see cref="ShotMessage" /> for the event.
        /// </summary>
        public ShotMessage ShotMessage { get; set; }

        /// <summary>
        /// Gets or sets the position of the shot.
        /// </summary>
        public Vector3 ShotPosition
        {
            get => ShotMessage.TargetPosition.Position;
            set
            {
                ShotMessage msg = ShotMessage;
                ShotMessage = new ShotMessage
                {
                    ShooterPosition = msg.ShooterPosition,
                    ShooterCameraRotation = msg.ShooterCameraRotation,
                    ShooterWeaponSerial = msg.ShooterWeaponSerial,
                    TargetPosition = new RelativePosition(value),
                    TargetRotation = msg.TargetRotation,
                    TargetNetId = msg.TargetNetId,
                };
            }
        }

        /// <summary>
        /// Gets or sets the netId of the target of the shot.
        /// </summary>
        public uint TargetNetId
        {
            get => ShotMessage.TargetNetId;
            set
            {
                ShotMessage msg = ShotMessage;
                ShotMessage = new ShotMessage
                {
                    ShooterPosition = msg.ShooterPosition,
                    ShooterCameraRotation = msg.ShooterCameraRotation,
                    ShooterWeaponSerial = msg.ShooterWeaponSerial,
                    TargetPosition = msg.TargetPosition,
                    TargetRotation = msg.TargetRotation,
                    TargetNetId = value,
                };
            }
        }
        */

        /// <summary>
        /// Gets or sets a value indicating whether the shot can be fired.
        /// </summary>
        public bool IsAllowed { get; set; } = true;
    }
}
