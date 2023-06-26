// -----------------------------------------------------------------------
// <copyright file="PlayerHandler.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles.Events
{
    using System.Linq;

    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.CustomRoles.API;
    using Exiled.CustomRoles.API.Features;
    using Exiled.Events.EventArgs.Player;
    using FLXLib.Extensions;
    using MEC;

    /// <summary>
    /// Event Handlers for the CustomRole API.
    /// </summary>
    public class PlayerHandler
    {
        /// <inheritdoc cref="Exiled.Events.Handlers.Player.ChangingRole"/>
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Reason == SpawnReason.Destroyed)
                return;
            Timing.CallDelayed(CustomRoles.Instance!.Config.CustomRolesSpectatorDisplayDelay, () =>
            {
                if (ev.Player.IsDead)
                {
                    Log.Debug($"Player is now a spectrator. Sending data of CustomRoles...");
                    foreach (var player in Player.List)
                    {
                        if (player.GetCustomRoles().FirstOrDefault() is not CustomRole role)
                            continue;
                        ev.Player.SetDispayNicknameForTargetOnly(player, role.GetSpectatorText(player));
                        Log.Debug($"[Name sync] Sent name of {player.Nickname} to {ev.Player.Nickname}");
                    }
                }
                else
                {
                    Log.Debug($"Player is a regular player. Sending real data of names");
                    foreach (var player in Player.List)
                    {
                        if (player.GetCustomRoles().IsEmpty())
                            continue;
                        Log.Debug($"[Name sync] Name reset for {ev.Player.Nickname} of {player.Nickname}.");
                        ev.Player.SetDispayNicknameForTargetOnly(player, player.CustomName);
                    }
                }
            });
        }
    }
}