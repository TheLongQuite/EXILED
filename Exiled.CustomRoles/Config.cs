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
        /// Gets the hint that is shown when someone used a cooldowned <see cref="CustomAbility"/>.
        /// </summary>
        [Description("The hint that is shown when someone tries to use cooldowned ability. Also used in console respond. {0} - remaining cooldown, {1} - ability name")]
        public Hint AbilityOnCooldownHint { get; private set; } = new Hint("Способность на перезарядке!\nПодождите ещё {0} секунд перед использованием.", 5);

        /// <summary>
        /// Gets the hint that is shown when someone tries to use <see cref="CustomAbility"/> without required energy.
        /// </summary>
        [Description("The hint that is shown when someone tries to use ability without required energy. Also used in console respond. {0} - current energy, {1} - required energy")]
        public Hint InsufficientEnergyHint { get; private set; } = new Hint("Недостаточно энергии!\nУ вас {0}/{1}", 5);

        /// <summary>
        /// Gets the hint that is shown when someone tries to use <see cref="CustomAbility"/> without required level.
        /// </summary>
        [Description("The hint that is shown when someone tries to use ability without required level. Also used in console respond. {0} - current level, {1} - required level")]
        public Hint InsufficientLevelHint { get; private set; } = new Hint("Недостаточный уровень!\nУ вас {0}/{1}", 5);

        /// <summary>
        /// Gets or sets customroles nickname display to spectators.
        /// </summary>
        [Description("Задержка до синхронизации имён кастомных ролей и наблюдателей.")]
        public float CustomRolesSpectatorDisplayDelay { get; set; } = 2;
    }
}
