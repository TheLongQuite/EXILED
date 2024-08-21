// -----------------------------------------------------------------------
// <copyright file="SendingRoleEventArgs.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Player
{
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using Exiled.Events.EventArgs.Interfaces;

    using PlayerRoles;

    /// <summary>
    /// Contains all information before a <see cref="API.Features.Player"/>'s role is sent to a client.
    /// </summary>
    public class SendingRoleEventArgs : IPlayerEvent
    {
        private RoleTypeId roleTypeId;

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
            ReferenceHub.TryGetHubNetID(target, out ReferenceHub targetHub);
            Target = targetHub;
            roleTypeId = roleType;
        }

        /// <summary>
        /// Gets the <see cref="API.Features.Player"/> on whose behalf the role change request is sent.
        /// </summary>
        public Player Player { get; }

        /// <summary>
        /// gets the <see cref="ReferenceHub"/> to whom the request is sent.
        /// </summary>
        public ReferenceHub Target { get; }

        /// <summary>
        /// Gets or sets the <see cref="RoleTypeId"/> that is sent to the <see cref="Target"/>.
        /// </summary>
        public RoleTypeId RoleType
        {
            get
            {
                return roleTypeId;
            }

            set
            {
                if (Player.Role is IAppearancedRole appearancedRole && RoleExtensions.TryGetRoleBase(value, out PlayerRoleBase roleBase) && appearancedRole.CheckAppearanceCompatibility(value, roleBase))
                {
                    roleTypeId = value;
                }
            }
        }
    }
}
