// -----------------------------------------------------------------------
// <copyright file="InteractingDoorEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Player
{
    using API.Features;
    using Exiled.API.Features.Doors;
    using Interactables.Interobjects.DoorUtils;
    using Interfaces;

    /// <summary>
    /// Contains all information before a player interacts with a door.
    /// </summary>
    public class InteractingDoorEventArgs : IPlayerEvent, IDoorEvent, IDeniableEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractingDoorEventArgs" /> class.
        /// </summary>
        /// <param name="player">
        /// <inheritdoc cref="Player" />
        /// </param>
        /// <param name="door">
        /// <inheritdoc cref="Door" />
        /// </param>
        /// <param name="isAllowed">
        /// <inheritdoc cref="IsAllowed" />
        /// </param>
        /// <param name="canInteract">
        /// <inheritdoc cref="CanInteract" />
        /// </param>
        public InteractingDoorEventArgs(Player player, DoorVariant door, bool isAllowed = true, bool canInteract = true)
        {
            Player = player;
            Door = Door.Get(door);
            IsAllowed = isAllowed;
            CanInteract = canInteract;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the player can interact with the door.
        /// </summary>
        public bool CanInteract { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player can access the door.
        /// </summary>
        public bool IsAllowed { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="API.Features.Doors.Door" /> instance.
        /// </summary>
        public Door Door { get; set; }

        /// <summary>
        /// Gets the player who's interacting with the door.
        /// </summary>
        public Player Player { get; }
    }
}
