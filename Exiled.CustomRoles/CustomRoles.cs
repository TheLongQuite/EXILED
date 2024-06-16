// -----------------------------------------------------------------------
// <copyright file="CustomRoles.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles
{
    using Events;
    using Exiled.API.Features;

    using Player = Exiled.Events.Handlers.Player;

    /// <summary>
    ///     Handles all custom role API functions.
    /// </summary>
    public class CustomRoles : Plugin<Config>
    {
        private PlayerHandler playerHandler = null!;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomRoles" /> class.
        /// </summary>
        public CustomRoles()
        {
        }

        /// <summary>
        ///     Gets a static reference to the plugin's instance.
        /// </summary>
        public static CustomRoles? Instance { get; private set; }

        /// <inheritdoc />
        public override void OnEnabled()
        {
            Instance = this;
            playerHandler = new PlayerHandler();

            Player.ChangingRole += playerHandler.OnChangingRole;

            base.OnEnabled();
        }

        /// <inheritdoc />
        public override void OnDisabled()
        {
            Player.ChangingRole -= playerHandler.OnChangingRole;

            Instance = null;
            base.OnDisabled();
        }
    }
}