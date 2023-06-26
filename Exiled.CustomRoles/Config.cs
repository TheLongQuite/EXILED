// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles
{
    using System.ComponentModel;

    using Exiled.API.Features;
    using Exiled.API.Interfaces;
    using Exiled.CustomRoles.API.Features;

    /// <summary>
    /// The plugin's config.
    /// </summary>
    public class Config : IConfig
    {
        /// <inheritdoc/>
        [Description("Whether or not the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether debug messages should be printed to the console.
        /// </summary>
        /// <returns><see cref="bool"/>.</returns>
        [Description("Whether or not debug messages should be shown.")]
        public bool Debug { get; set; } = false;

        /// <summary>
        /// Gets the hint that is shown when someone gets a <see cref="CustomRole"/>.
        /// </summary>
        [Description("The hint that is shown when someone gets a custom role.")]
        public Hint GotRoleHint { get; private set; } = new("You have spawned as a {0}\n{1}", 6);

        /// <summary>
        /// Gets the hint that is shown when someone used a <see cref="CustomAbility"/>.
        /// </summary>
        [Description("The hint that is shown when someone used a custom ability.")]
        public Hint UsedAbilityHint { get; private set; } = new("Ability {0} has been activated.\n{1}", 5);

        /// <summary>
        /// Gets the hint that is shown when someone used a <see cref="CustomAbility"/>.
        /// </summary>
        [Description("The hint that is shown when someone's custom ability is ready.")]
        public Hint AbilityReadyHint { get; private set; } = new("Ability {0} is ready.\n{1}", 5);

        /// <summary>
        /// Gets or sets customroles nickname display to spectators.
        /// </summary>
        [Description("Задержка до синхронизации имён кастомных ролей и наблюдателей.")]
        public float CustomRolesSpectatorDisplayDelay { get; set; } = 2;
    }
}