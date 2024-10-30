// -----------------------------------------------------------------------
// <copyright file="Registered.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles.Commands.Admin.List
{
    using System;
    using System.Linq;
    using System.Text;
    using API.Features;
    using CommandSystem;
    using Exiled.API.Features.Pools;
    using Permissions.Extensions;

    /// <inheritdoc />
    internal sealed class Registered : ICommand
    {
        /// <summary>
        /// The command aliases.
        /// </summary>
        public string[] Aliases { get; set; } = { "r", "reg" };

        /// <summary>
        /// The command description.
        /// </summary>
        public string Description { get; set; } = "Список всех кастомных ролей.";

        /// <summary>
        /// The message to display when the sender lacks permission.
        /// </summary>
        public string NoPermissionMessage { get; set; } = "Не хватает прав!";

        /// <summary>
        /// The message to display when there are no custom roles.
        /// </summary>
        public string NoCustomRolesMessage { get; set; } = "На сервере нет кастомных ролей.";

        /// <summary>
        /// The format for the custom roles list.
        /// </summary>
        public string CustomRolesListFormat { get; set; } = "[Кастомные роли ({0})]";

        /// <summary>
        /// The format for a single custom role.
        /// </summary>
        public string CustomRoleFormat { get; set; } = "[{0}. {1} ({2})]";

        /// <inheritdoc />
        public string Command { get; set; } = "registered";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission( "customroles.list.registered"))
            {
                response = NoPermissionMessage;
                return false;
            }

            if (CustomRole.Registered.Count == 0)
            {
                response = NoCustomRolesMessage;
                return false;
            }

            StringBuilder builder = StringBuilderPool.Pool.Get().AppendLine();

            builder.Append(string.Format(CustomRolesListFormat, CustomRole.Registered.Count));

            foreach (CustomRole role in CustomRole.Registered.OrderBy(r => r.Id))
                builder.Append(string.Format(CustomRoleFormat, role.Id, role.Name, role.Role)).AppendLine();

            response = StringBuilderPool.Pool.ToStringReturn(builder);
            return true;
        }
    }
}