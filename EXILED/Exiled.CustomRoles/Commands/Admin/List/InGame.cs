// -----------------------------------------------------------------------
// <copyright file="InGame.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles.Commands.Admin.List
{
    using System;
    using System.Text;
    using API.Features;
    using CommandSystem;
    using Exiled.API.Features;
    using NorthwoodLib.Pools;
    using Permissions.Extensions;

    /// <inheritdoc />
    internal sealed class InGame : ICommand
    {
        /// <inheritdoc />
        public string Command { get; } = "ingame";

        /// <inheritdoc />
        public string[] Aliases { get; } = { "ig", "alife" };

        /// <inheritdoc />
        public string Description { get; set; } = "Получает все кастомные роли которые сейчас учавствуют в раунде.";

        private string Permission { get; set; } = "customroles.list.ingame";
        private string NoPermissionMessage { get; set; } = "Не хватает прав!";
        private string NoCustomRolesMessage { get; set; } = "Кастомные роли не найдены.";
        private string CustomRolesHeaderFormat { get; set; } = "[Текущие живые кастомные роли: ({0})]{1}";
        private string CustomRoleFormat { get; set; } = "[{0}. {1} ({2}) {{ {3} }}]{1}";
        private string PlayerFormat { get; set; } = "{0} ({1}) ({2}) [{3}]";

        /// <inheritdoc />
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission(Permission))
            {
                response = NoPermissionMessage;
                return false;
            }

            StringBuilder message = StringBuilderPool.Shared.Rent();

            int count = 0;

            foreach (CustomRole customRole in CustomRole.Registered)
            {
                if (customRole.TrackedPlayers.Count == 0)
                    continue;

                message.AppendLine()
                    .AppendFormat(CustomRoleFormat, customRole.Id, customRole.Name, customRole.Role, customRole.TrackedPlayers.Count)
                    .AppendLine();

                count += customRole.TrackedPlayers.Count;

                foreach (Player owner in customRole.TrackedPlayers)
                {
                    message.AppendFormat(PlayerFormat, owner.Nickname, owner.UserId, owner.Id, owner.Role.Type)
                        .AppendLine();
                }
            }

            if (message.Length == 0)
                message.Append(NoCustomRolesMessage);
            else
                message.Insert(0, string.Format(CustomRolesHeaderFormat, count, Environment.NewLine));

            response = StringBuilderPool.Shared.ToStringReturn(message);
            return true;
        }
    }
}