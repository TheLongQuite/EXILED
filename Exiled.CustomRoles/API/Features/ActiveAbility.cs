// -----------------------------------------------------------------------
// <copyright file="ActiveAbility.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles.API.Features
{
    using System;
    using System.Collections.Generic;

    using Exiled.API.Features;

    using MEC;

    using YamlDotNet.Serialization;

    /// <summary>
    ///     The base class for active (on-use) abilities.
    /// </summary>
    public abstract class ActiveAbility : CustomAbility
    {
        /// <summary>
        ///     Gets or sets how long the ability lasts.
        /// </summary>
        public abstract float Duration { get; set; }

        /// <summary>
        ///     Gets or sets how long must go between ability uses.
        /// </summary>
        public abstract float Cooldown { get; set; }

        /// <summary>
        ///     Gets or sets an action to override the behavior of <see cref="CanUseAbility" />.
        /// </summary>
        [YamlIgnore]
        public virtual Func<bool>? CanUseOverride { get; set; }

        /// <summary>
        ///     Gets the last time this ability was used.
        /// </summary>
        [YamlIgnore]
        public Dictionary<Player, DateTime> LastUsed { get; } = new();

        /// <summary>
        ///     Gets all players actively using this ability.
        /// </summary>
        [YamlIgnore]
        public HashSet<Player> ActivePlayers { get; } = new();

        /// <summary>
        ///     Uses the ability.
        /// </summary>
        /// <param name="player">The <see cref="Player" /> using the ability.</param>
        public void UseAbility(Player player)
        {
            ActivePlayers.Add(player);
            LastUsed[player] = DateTime.Now;
            ShowMessage(player);
            AbilityUsed(player);
            Timing.CallDelayed(Duration, () => EndAbility(player));
            Timing.CallDelayed(Cooldown, () => RemindAbility(player));
        }

        /// <summary>
        ///     Reminds if ability is ready.
        /// </summary>
        /// <param name="player">The <see cref="Player" /> the ability is ready for.</param>
        public void RemindAbility(Player player)
        {
            if (!base.Check(player) || !player.IsConnected || Math.Abs((DateTime.Now - LastUsed[player]).TotalSeconds - Cooldown) > 1f || !CustomRoles.Instance!.Config.AbilityReadyHint.Show)
                return;

            player.ShowHint(string.Format(CustomRoles.Instance!.Config.AbilityReadyHint.Content, Name, Description), CustomRoles.Instance.Config.AbilityReadyHint.Duration);
        }

        /// <summary>
        ///     Ends the ability.
        /// </summary>
        /// <param name="player">The <see cref="Player" /> the ability is ended for.</param>
        public void EndAbility(Player player)
        {
            if (!ActivePlayers.Contains(player))
                return;

            ActivePlayers.Remove(player);
            AbilityEnded(player);
        }

        /// <summary>
        ///     Checks if the specified player is using the ability.
        /// </summary>
        /// <param name="player">The <see cref="Player" /> to check.</param>
        /// <returns>True if the player is actively using the ability.</returns>
        public override bool Check(Player player) => ActivePlayers.Contains(player);

        /// <summary>
        ///     Checks to see if the ability is usable by the player.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <param name="response">The response to send to the player.</param>
        /// <returns>True if the ability is usable.</returns>
        public virtual bool CanUseAbility(Player player, out string response)
        {
            if (CanUseOverride is not null)
            {
                response = string.Empty;
                return CanUseOverride.Invoke();
            }

            if (!LastUsed.TryGetValue(player, out DateTime lastUsed))
            {
                response = string.Empty;
                return true;
            }

            DateTime usableTime = lastUsed + TimeSpan.FromSeconds(Cooldown);
            if (DateTime.Now > usableTime)
            {
                response = string.Empty;
                return true;
            }

            Hint hint = CustomRoles.Instance!.Config.AbilityOnCooldownHint;
            response = string.Format(hint.Content, Math.Round((usableTime - DateTime.Now).TotalSeconds, 2), Name);
            if (hint.Show)
                player.ShowHint(response, hint.Duration);

            return false;
        }

        /// <summary>
        ///     Called when the ability is used.
        /// </summary>
        /// <param name="player">The <see cref="Player" /> using the ability.</param>
        protected virtual void AbilityUsed(Player player)
        {
        }

        /// <summary>
        ///     Called when the abilities duration has ended.
        /// </summary>
        /// <param name="player">The <see cref="Player" /> the ability has ended for.</param>
        protected virtual void AbilityEnded(Player player)
        {
        }

        /// <summary>
        ///     Called when the ability is successfully used.
        /// </summary>
        /// <param name="player">The <see cref="Player" /> using the ability.</param>
        protected virtual void ShowMessage(Player player) =>
            player.ShowHint(string.Format(CustomRoles.Instance!.Config.UsedAbilityHint.Content, Name, Description), CustomRoles.Instance.Config.UsedAbilityHint.Duration);
    }
}