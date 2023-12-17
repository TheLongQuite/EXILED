// -----------------------------------------------------------------------
// <copyright file="Give.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Pools;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using Exiled.Permissions.Extensions;
using RemoteAdmin;

namespace Exiled.CustomRoles.Commands.Admin
{
    /// <summary>
    /// The command to give a role to player(s).
    /// </summary>
    internal sealed class Give : ICommand
    {
        private Give()
        {
        }

        /// <summary>
        /// Gets the <see cref="Give"/> command instance.
        /// </summary>
        public static Give Instance { get; } = new();

        /// <inheritdoc/>
        public string Command { get; } = "give";

        /// <inheritdoc/>
        public string[] Aliases { get; } = { "g" };

        /// <inheritdoc/>
        public string Description { get; } = "Gives the specified custom role to the indicated player(s).";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customroles.give"))
            {
                response = "Не хватает прав!";
                return false;
            }

            if (arguments.Count == 0)
            {
                response = "give <Название/ID кастомной роли> [Никнейм/ID/SteamID игрока или all/*]";
                return false;
            }

            if (!CustomRole.TryGet(arguments.At(0), out CustomRole? role) || role is null)
            {
                response = $"Кастомная роль {arguments.At(0)} не найдена!";
                return false;
            }

            if (arguments.Count == 1)
            {
                if (sender is PlayerCommandSender playerCommandSender)
                {
                    Player player = Player.Get(playerCommandSender);

                    TryAddRole(player, role);
                    response = $"Кастомная роль {role.Name} дана игроку {player.Nickname}.";
                    return true;
                }

                response = "Игрок не найден.";
                return false;
            }

            string identifier = string.Join(" ", arguments.Skip(1));

            switch (identifier)
            {
                case "*":
                case "all":
                    List<Player> players = ListPool<Player>.Pool.Get(Player.List);

                    foreach (Player player in players)
                        TryAddRole(player, role);

                    response = $"Кастомная роль {role.Name} дана всем игрокам.";
                    ListPool<Player>.Pool.Return(players);
                    return true;
                default:
                    if (Player.Get(identifier) is not Player ply)
                    {
                        response = $"Игрок {identifier} не найден";
                        return false;
                    }

                    TryAddRole(ply, role);
                    response = $"Кастомная роль {role.Name} дана игроку {ply.Nickname}.";
                    return true;
            }
        }

        private void TryAddRole(Player player, CustomRole customRole)
        {
            foreach (var cRole in player.GetCustomRoles())
                cRole?.RemoveRole(player);

            customRole.AddRole(player, SpawnReason.ForceClass);
        }
    }
}
