// -----------------------------------------------------------------------
// <copyright file="CustomAbility.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles.API.Features
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using Exiled.API.Features;
    using Exiled.API.Features.Attributes;
    using Exiled.API.Interfaces;

    using YamlDotNet.Serialization;

    /// <summary>
    /// The custom ability base class.
    /// </summary>
    public abstract class CustomAbility
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAbility"/> class.
        /// </summary>
        public CustomAbility()
        {
            AbilityType = GetType().Name;
            Init();
        }

        /// <summary>
        /// Gets a list of all registered custom abilities.
        /// </summary>
        public static HashSet<CustomAbility> Registered { get; } = new HashSet<CustomAbility>();

        /// <summary>
        /// Gets all players who have this ability.
        /// </summary>
        [YamlIgnore]
        public HashSet<Player> Players { get; } = new HashSet<Player>();

        /// <summary>
        /// Gets or sets the name of the ability.
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the ability.
        /// </summary>
        public abstract string Description { get; set; }

        /// <summary>
        /// Gets the <see cref="Type"/> for this ability.
        /// </summary>
        [Description("Changing this will likely break your config.")]
        public string AbilityType { get; }

        /// <summary>
        /// Gets a <see cref="CustomRole"/> by name.
        /// </summary>
        /// <param name="name">The name of the role to get.</param>
        /// <returns>The role, or <see langword="null"/> if it doesn't exist.</returns>
        public static CustomAbility Get(string name) => Registered?.FirstOrDefault(r => r.Name == name);

        /// <summary>
        /// Tries to get a <see cref="CustomRole"/> by name.
        /// </summary>
        /// <param name="name">The name of the role to get.</param>
        /// <param name="customAbility">The custom role.</param>
        /// <returns>True if the role exists.</returns>
        /// <exception cref="ArgumentNullException">If the name is <see langword="null"/> or an empty string.</exception>
        public static bool TryGet(string name, out CustomAbility customAbility)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            customAbility = Get(name);

            return customAbility is not null;
        }

        /// <summary>
        /// Checks to see if the specified player has this ability.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to check.</param>
        /// <returns>True if the player has this ability.</returns>
        public virtual bool Check(Player player) => Players.Contains(player);

        /// <summary>
        /// Adds this ability to the player.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to give the ability to.</param>
        public void AddAbility(Player player)
        {
            Players.Add(player);
            AbilityAdded(player);
        }

        /// <summary>
        /// Removes this ability from the player.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to remove this ability from.</param>
        public void RemoveAbility(Player player)
        {
            Players.Remove(player);
            AbilityRemoved(player);
        }

        /// <summary>
        /// Initializes this ability.
        /// </summary>
        public void Init() => SubscribeEvents();

        /// <summary>
        /// Destroys this ability.
        /// </summary>
        public void Destroy() => UnsubscribeEvents();

        /// <summary>
        /// Loads the internal event handlers for the ability.
        /// </summary>
        protected virtual void SubscribeEvents()
        {
        }

        /// <summary>
        /// Unloads the internal event handlers for the ability.
        /// </summary>
        protected virtual void UnsubscribeEvents()
        {
        }

        /// <summary>
        /// Called when the ability is first added to the player.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> using the ability.</param>
        protected virtual void AbilityAdded(Player player)
        {
        }

        /// <summary>
        /// Called when the ability is being removed.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> using the ability.</param>
        protected virtual void AbilityRemoved(Player player)
        {
        }
    }
}
