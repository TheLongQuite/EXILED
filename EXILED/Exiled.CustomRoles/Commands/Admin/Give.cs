// -----------------------------------------------------------------------
// <copyright file="Give.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles.Commands.Admin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using API;
    using API.Features;
    using CommandSystem;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Pools;
    using Permissions.Extensions;
    using PlayerRoles;
    using RemoteAdmin;

    /// <summary>
    ///     The command to give a role to player(s).
    /// </summary>
    internal sealed class Give : ICommand
    {
        public string Command { get; set; } = "give";

        public string[] Aliases { get; set; } = { "g" };

        public string Description { get; set; } = "Gives the specified custom role to the indicated player(s).";

        private string NoPermissionMessage { get; set; } = "Не хватает прав!";
        private string NoRoleFoundMessage { get; set; } = "Кастомная роль {0} не найдена!";
        private string PlayerNotFoundMessage { get; set; } = "Игрок не найден.";
        private string RoleGivenMessage { get; set; } = "Кастомная роль {0} дана игроку {1}.";
        private string AllPlayersRoleGivenMessage { get; set; } = "Кастомная роль {0} дана всем игрокам.";
        private string PlayerNotFoundErrorMessage { get; set; } = "Игрок {0} не найден";
        private string UsageMessage { get; set; } = "{0} <Название/ID кастомной роли> [Никнейм/ID/SteamID игрока или all/*]";

        /// <inheritdoc />
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customroles.give"))
            {
                response = NoPermissionMessage;
                return false;
            }

            if (arguments.Count == 0)
            {
                response = string.Format(UsageMessage, Command);
                return false;
            }

            if (!CustomRole.TryGet(arguments.At(0), out CustomRole? role) || role is null)
            {
                response = string.Format(NoRoleFoundMessage, arguments.At(0));
                return false;
            }

            if (arguments.Count == 1)
            {
                if (sender is PlayerCommandSender playerCommandSender)
                {
                    Player player = Player.Get(playerCommandSender);

                    TryAddRole(player, role);
                    response = string.Format(RoleGivenMessage, role.Name, player.Nickname);
                    return true;
                }

                response = PlayerNotFoundMessage;
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

                    response = string.Format(AllPlayersRoleGivenMessage, role.Name);
                    ListPool<Player>.Pool.Return(players);
                    return true;
                default:
                    if (Player.Get(identifier) is not { } ply)
                    {
                        response = string.Format(PlayerNotFoundErrorMessage, identifier);
                        return false;
                    }

                    TryAddRole(ply, role);
                    response = string.Format(RoleGivenMessage, role.Name, ply.Nickname);
                    return true;
            }
        }

        private void TryAddRole(Player player, CustomRole customRole)
        {
            player.GetCustomRole()?.RemoveRole(player);

            customRole.AddRole(player, SpawnReason.ForceClass, RoleSpawnFlags.All);
        }
    }
}