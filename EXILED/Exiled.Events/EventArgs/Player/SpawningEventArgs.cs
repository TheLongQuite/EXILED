// -----------------------------------------------------------------------
// <copyright file="SpawningEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Player
{
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using Exiled.Events.EventArgs.Interfaces;
    using PlayerRoles;

    using UnityEngine;

    /// <summary>
    /// Contains all information before spawning a player.
    /// </summary>
    public class SpawningEventArgs : IPlayerEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpawningEventArgs" /> class.
        /// </summary>
        /// <param name="player">
        /// <inheritdoc cref="Player" />
        /// </param>
        /// <param name="position">
        /// <inheritdoc cref="Position" />
        /// </param>
        /// <param name="rotation">
        /// <inheritdoc cref="HorizontalRotation" />
        /// </param>
        public SpawningEventArgs(Player player, Vector3 position, float rotation)
        {
            Player = player;
            Position = position;
            HorizontalRotation = rotation;
        }

        /// <summary>
        /// Gets the spawning <see cref="Player"/>.
        /// </summary>
        public Player Player { get; }

        /// <summary>
        /// Gets or sets the <see cref="Player"/>'s spawning position.
        /// </summary>
        /// <remarks>
        /// Position will apply only for <see cref="FpcRole"/>.
        /// </remarks>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Player"/>'s spawning rotation.
        /// </summary>
        /// <remarks>
        /// Rotation will apply only for <see cref="FpcRole"/>.
        /// </remarks>
        public float HorizontalRotation { get; set; }
    }
}