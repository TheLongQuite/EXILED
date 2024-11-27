// -----------------------------------------------------------------------
// <copyright file="Disable.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Commands.PluginManager
{
    using System;

    using API.Interfaces;
    using CommandSystem;
    using Exiled.Permissions.Extensions;
    using RemoteAdmin;

    /// <summary>
    /// The command to disable a plugin.
    /// </summary>
    public sealed class Disable : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "disable";

        /// <inheritdoc/>
        public string[] Aliases { get; } = { "ds", "dis" };

        /// <inheritdoc/>
        public string Description { get; set; } = "Disable a plugin.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            const string perm = "pm.disable";

            if (!sender.CheckPermission(perm) && sender is PlayerCommandSender playerSender && !playerSender.FullPermissions)
            {
                response = $"You can't disable a plugin, you don't have \"{perm}\" permissions.";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Please, use: pluginmanager disable <pluginname>";
                return false;
            }

            IPlugin<IConfig> plugin = Loader.Loader.GetPlugin(arguments.At(0));
            if (plugin is null)
            {
                response = "Plugin not enabled or not found.";
                return false;
            }

            plugin.OnUnregisteringCommands();
            plugin.OnDisabled();
            Loader.Loader.Plugins.Remove(plugin);
            Loader.Loader.Locations.Remove(plugin.Assembly);
            response = $"Plugin {plugin.Name} has been disabled!";
            return true;
        }
    }
}
