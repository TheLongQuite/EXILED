// -----------------------------------------------------------------------
// <copyright file="SendingRoleEventArgs.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Player
{
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Interfaces;

    using PlayerRoles;

    /// <summary>
    /// Contains all information before a <see cref="API.Features.Player"/>'s role is sent to a client.
    /// </summary>
    public class SendingRoleEventArgs : IPlayerEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendingRoleEventArgs" /> class.
        /// </summary>
        /// <param name="player">
        /// <inheritdoc cref="Player" />
        /// </param>
        /// <param name="target">
        /// <inheritdoc cref="Target" />
        /// </param>
        /// <param name="roleType">
        /// <inheritdoc cref="RoleType" />
        /// </param>
        public SendingRoleEventArgs(Player player, uint target, RoleTypeId roleType)
        {
            Player = player;
            Target = Player.Get(target);
            RoleType = roleType;
        }

        /// <summary>
        /// Gets the <see cref="API.Features.Player"/> on whose behalf the role change request is sent.
        /// </summary>
        public Player Player { get; }

        /// <summary>
        /// gets the <see cref="API.Features.Player"/> to whom the request is sent.
        /// </summary>
        public Player Target { get; }

        /// <summary>
        /// Gets the <see cref="RoleTypeId"/> that is sent to the <see cref="Target"/>.
        /// </summary>
        public RoleTypeId RoleType { get; }
    }
}
