// -----------------------------------------------------------------------
// <copyright file="RoleInfo.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles.Commands.User
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using API;
    using API.Features;

    using CommandSystem;

    using Exiled.API.Features;

    using NorthwoodLib.Pools;

    /// <summary>
    ///     Handles the displaing of custom role info.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public class RoleInfo : ICommand
    {
        /// <inheritdoc />
        public string Command
        {
            get => "roleinfo";
        }

        /// <inheritdoc />
        public string[] Aliases { get; } = { "rinfo" };

        /// <inheritdoc />
        public string Description
        {
            get => "Даёт справку по вашей текущей особой роли и её способностях.";
        }

        /// <inheritdoc />
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null)
            {
                response = "Попробуйте позже";
                return false;
            }

            CustomRole? customRole = player.GetCustomRoles().FirstOrDefault();
            if (customRole == null)
            {
                response = "У вас нет особых ролей!";
                return false;
            }

            response = string.Format(CustomRoles.Instance!.Config.RoleInfoResponse, customRole.Name, customRole.Id, customRole.Description, GetAbilitiesInfo(player, customRole));
            return true;
        }

        internal static string GetAbilitiesInfo(Player player, CustomRole customRole, bool includePassive = true)
        {
            List<PassiveAbility> passiveAbilities = new();
            List<ActiveAbility> activeAbilities = new();
            List<Scp079ActiveAbility> scp079ActiveAbilities = new();
            StringBuilder? stringBuilder = StringBuilderPool.Shared.Rent();
            foreach (CustomAbility? ability in customRole.CustomAbilities)
            {
                switch (ability)
                {
                    case Scp079ActiveAbility scp079ActiveAbility:
                        if (!scp079ActiveAbility.IsAvailable(player))
                            continue;
                        scp079ActiveAbilities.Add(scp079ActiveAbility);
                        break;
                    case ActiveAbility activeAbility:
                        activeAbilities.Add(activeAbility);
                        break;
                    case PassiveAbility passiveAbility when includePassive:
                        passiveAbilities.Add(passiveAbility);
                        break;
                }
            }

            if (includePassive && passiveAbilities.Any())
            {
                stringBuilder.AppendFormat(CustomRoles.Instance!.Config.AbilityBlockFormat + '\n', "Пассивные способности:");
                for (int i = 0; i < passiveAbilities.Count; i++)
                {
                    stringBuilder.AppendFormat(CustomRoles.Instance!.Config.PassiveAbilityLineFormat + '\n', i + 1, passiveAbilities[i].Name, passiveAbilities[i].Description);
                }

                stringBuilder.AppendLine();
            }

            if (activeAbilities.Any() || scp079ActiveAbilities.Any())
            {
                stringBuilder.AppendFormat(CustomRoles.Instance!.Config.AbilityBlockFormat + '\n', "Активные способности:");
                for (int i = 0; i < activeAbilities.Count; i++)
                {
                    stringBuilder.AppendFormat(CustomRoles.Instance!.Config.ActiveAbilityLineFormat + '\n', i + 1, activeAbilities[i].Name, activeAbilities[i].Description, GetAbilityDuration(activeAbilities[i]), activeAbilities[i].Cooldown);
                }

                for (int i = 0; i < scp079ActiveAbilities.Count; i++)
                {
                    stringBuilder.AppendFormat(CustomRoles.Instance!.Config.Active079AbilityLineFormat + '\n', i + 1, scp079ActiveAbilities[i].Name, scp079ActiveAbilities[i].Description, GetAbilityDuration(scp079ActiveAbilities[i]),
                        scp079ActiveAbilities[i].Cooldown, scp079ActiveAbilities[i].MinRequiredLevel, scp079ActiveAbilities[i].EnergyUsage);
                }
            }

            return StringBuilderPool.Shared.ToStringReturn(stringBuilder);
        }

        private static string GetAbilityDuration(ActiveAbility activeAbility) => activeAbility.Duration <= 0 ? "Мгновенно" : activeAbility.Duration.ToString(CultureInfo.InvariantCulture);
    }
}