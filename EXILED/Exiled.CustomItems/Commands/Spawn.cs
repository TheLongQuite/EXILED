// -----------------------------------------------------------------------
// <copyright file="Spawn.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomItems.Commands
{
    using System;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.CustomItems.API.Features;
    using Exiled.Permissions.Extensions;
    using UnityEngine;

    /// <summary>
    /// The command to spawn a specific item.
    /// </summary>
    internal sealed class Spawn : ICommand
    {
        public string InsufficientPermissionsMessage { get; set; } = "Не хватает прав!";
        public string InvalidArgumentsMessage { get; set; } = "spawn [Название/ID кастомного предмета] [Никнейм/SteamID игока]\nspawn [Название/ID кастомного предмета] [X] [Y] [Z]";
        public string InvalidCustomItemMessage { get; set; } = " {0} is not a valid custom item.";
        public string PlayerIsDeadMessage { get; set; } = "Игрок мертв!";
        public string InvalidCoordinatesMessage { get; set; } = "Невозможно получить координату (попробуй писать через , а не .)";
        public string UnableToFindLocationMessage { get; set; } = "Невозможно найти локацию для спавна.";
        public string SpawnSuccessMessage { get; set; } = "{0} ({1}) заспавнился на позиции {2}.";

        /// <inheritdoc/>
        public string Command { get; set; } = "spawn";

        /// <inheritdoc/>
        public string[] Aliases { get; set; } = { "sp" };

        /// <inheritdoc/>
        public string Description { get; set; } = "Спавнит кастомный предмет.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customitems.spawn"))
            {
                response = InsufficientPermissionsMessage;
                return false;
            }

            if (arguments.Count < 2)
            {
                response = InvalidArgumentsMessage;
                return false;
            }

            if (!CustomItem.TryGet(arguments.At(0), out CustomItem? item))
            {
                response = string.Format(InvalidCustomItemMessage, arguments.At(0));
                return false;
            }

            Vector3 position;

            if (Player.Get(arguments.At(1)) is Player player)
            {
                if (player.IsDead)
                {
                    response = PlayerIsDeadMessage;
                    return false;
                }

                position = player.Position;
            }
            else if (arguments.Count > 3)
            {
                if (!float.TryParse(arguments.At(1), out float x) || !float.TryParse(arguments.At(2), out float y) || !float.TryParse(arguments.At(3), out float z))
                {
                    response = InvalidCoordinatesMessage;
                    return false;
                }

                position = new Vector3(x, y, z);
            }
            else
            {
                response = UnableToFindLocationMessage;
                return false;
            }

            item?.Spawn(position);

            response = string.Format(SpawnSuccessMessage, item?.Name, item?.Type, position);
            return true;
        }
    }
}