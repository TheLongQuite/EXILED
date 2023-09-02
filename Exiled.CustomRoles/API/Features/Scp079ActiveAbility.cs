﻿namespace Exiled.CustomRoles.API.Features
{
    using System;

    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using PlayerRoles;

    /// <summary>
    /// The base class for active (on-use) energy-using abilities for SCP-079.
    /// </summary>
    public abstract class Scp079ActiveAbility : ActiveAbility
    {
        /// <summary>
        /// Gets or sets minimal SCP-079 level to use this ability.
        /// </summary>
        public abstract byte MinRequiredLevel { get; set; }

        /// <summary>
        /// Gets or sets energy usage for ability.
        /// </summary>
        public abstract float EnergyUsage { get; set; }

        /// <inheritdoc/>
        public override bool CanUseAbility(Player player, out string response)
        {
            if (!player.Role.Is(out Scp079Role scp079Role))
            {
                response = "Вы не SCP-079, чтобы использовать эту команду.";
                return false;
            }

            if (scp079Role.Level < MinRequiredLevel)
            {
                var hint = CustomRoles.Instance!.Config.InsufficientLevelHint;
                response = string.Format(hint.Content, scp079Role.Level + 1, MinRequiredLevel + 1);
                if (hint.Show)
                    player.ShowHint(response, hint.Duration);
                return false;
            }

            if (scp079Role.Energy < EnergyUsage)
            {
                var hint = CustomRoles.Instance!.Config.InsufficientEnergyHint;
                response = string.Format(hint.Content, scp079Role.Energy, EnergyUsage);
                if (hint.Show)
                    player.ShowHint(response, hint.Duration);
                return false;
            }

            return base.CanUseAbility(player, out response);
        }

        /// <inheritdoc/>
        protected override void AbilityAdded(Player player)
        {
            if (player.Role.Type != RoleTypeId.Scp079)
                throw new Exception($"Unable to give {nameof(Scp079ActiveAbility)} to non-SCP079 player!");
        }

        /// <inheritdoc/>
        protected override void AbilityUsed(Player player)
        {
            if (player.Role.Is(out Scp079Role role))
                role.Energy -= EnergyUsage;
        }
    }
}