// -----------------------------------------------------------------------
// <copyright file="RemoteAdmin.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Commands.Reload
{
    using System;

    using CommandSystem;

    using Exiled.Permissions.Extensions;

    using Loader;

    /// <summary>
    /// The reload remoteadmin command.
    /// </summary>
    public class RemoteAdmin : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "remoteadmin";

        /// <inheritdoc/>
        public string[] Aliases { get; } = new[] { "ra" };

        /// <inheritdoc/>
        public string Description { get; set; } = "Reloads remote admin configs.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ee.reloadremoteadmin"))
            {
                response = "You can't reload remote admin configs, you don't have \"ee.reloadremoteadmin\" permission.";
                return false;
            }

            ConfigManager.ReloadRemoteAdmin();

            Handlers.Server.OnReloadedRA();

            response = "Remote admin configs reloaded.";
            return true;
        }
    }
}