// -----------------------------------------------------------------------
// <copyright file="Tracked.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomItems.Commands.List
{
    using System;
    using System.Linq;
    using System.Text;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.API.Features.Pools;
    using Exiled.CustomItems.API.Features;
    using Exiled.Permissions.Extensions;
    using RemoteAdmin;

    /// <inheritdoc/>
    internal sealed class Tracked : ICommand
    {
        public string Command { get; set; } = "insideinventories";
        public string[] Aliases { get; set; } = { "ii", "inside", "inv", "inventories" };
        public string Description { get; set; } = "Получает все предметы которые лежат в инвенторях игроков.";
        public string NoPermissionMessage { get; set; } = "Не хватает прав!";
        public string NoCustomItemsMessage { get; set; } = "Кастомные предметы не найдены.";
        public string TitleFormat { get; set; } = "[Custom items inside inventories ({0})]";
        public string ItemFormat { get; set; } = "[{0}. {1} ({2}) {{ {3} }}]";
        public string SerialFormat { get; set; } = "{0}. ";
        public string NoOwnerMessage { get; set; } = "Никто";
        public string OwnerFormat { get; set; } = "{0} ({1}) ({2}) [{3}]";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customitems.list.insideinventories") && sender is PlayerCommandSender playerSender && !playerSender.FullPermissions)
            {
                response = NoPermissionMessage;
                return false;
            }

            StringBuilder message = StringBuilderPool.Pool.Get();

            int count = 0;

            foreach (CustomItem customItem in CustomItem.Registered)
            {
                if (customItem.TrackedSerials.Count == 0)
                    continue;

                message.AppendLine()
                    .AppendFormat(ItemFormat, customItem.Id, customItem.Name, customItem.Type, customItem.TrackedSerials.Count)
                    .AppendLine();

                count += customItem.TrackedSerials.Count;

                foreach (int insideInventory in customItem.TrackedSerials)
                {
                    Player owner = Player.List.FirstOrDefault(player => player.Inventory.UserInventory.Items.Any(item => item.Key == insideInventory));

                    message.AppendFormat(SerialFormat, insideInventory);

                    if (owner is null)
                        message.AppendLine(NoOwnerMessage);
                    else
                        message.AppendFormat(OwnerFormat, owner.Nickname, owner.UserId, owner.Id, owner.Role).AppendLine();
                }
            }

            if (message.Length == 0)
                message.Append(NoCustomItemsMessage);
            else
                message.Insert(0, Environment.NewLine + string.Format(TitleFormat, count) + Environment.NewLine);

            response = StringBuilderPool.Pool.ToStringReturn(message);
            return true;
        }
    }
}