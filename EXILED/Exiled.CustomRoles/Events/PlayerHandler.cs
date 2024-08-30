// -----------------------------------------------------------------------
// <copyright file="PlayerHandler.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles.Events
{
    using System.Linq;

    using API;
    using API.Features;

    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using Exiled.Events.EventArgs.Player;

    using FLXLib.Extensions;

    using MEC;

    using PlayerRoles;

    /// <summary>
    ///     Event Handlers for the CustomRole API.
    /// </summary>
    public class PlayerHandler
    {
        /// <summary>
        ///     SessionVariable key.
        /// </summary>
        public const string LastCustomRoleKey = "LastCustomRole";

        /// <inheritdoc cref="Exiled.Events.Handlers.Server.WaitingForPlayers" />
        public void OnWaitingForPlayers()
        {
            Extensions.InternalPlayerToCustomRoles.Clear();
        }

        /// <inheritdoc cref="Exiled.Events.Handlers.Player.ChangingRole" />
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Reason is SpawnReason.Destroyed or SpawnReason.None)
                return;

            if (ev.Player.TryGetCustomRole(out CustomRole customRole))
                ev.Player.SessionVariables[LastCustomRoleKey] = customRole;
            else
                ev.Player.SessionVariables.Remove(LastCustomRoleKey);
        }

        /// <inheritdoc cref="Exiled.Events.Handlers.Player.SendingRole" />
        public void OnSendingRole(SendingRoleEventArgs ev)
        {
            if (ev.Target == null)
                return;

            if (ev.Player.IsDead && ev.Target.TryGetCustomRole(out CustomRole role))
            {
                ev.Player.SetDispayNicknameForTargetOnly(ev.Target, role.GetSpectatorText(ev.Target));
                Log.Debug($"[Name sync] Sent name of {ev.Target.Nickname} to {ev.Player.Nickname}");
            }
            else if (ev.Target.IsDead && ev.Player.TryGetCustomRole(out role))
            {
                ev.Target.SetDispayNicknameForTargetOnly(ev.Player, role.GetSpectatorText(ev.Player));
                Log.Debug($"[Name sync] Sent name of {ev.Player.Nickname} to {ev.Target.Nickname}");
            }
            else
            {
                Log.Debug($"[Name sync] Name reset for {ev.Player.Nickname} of {ev.Target.Nickname}.");
                ev.Target.SetDispayNicknameForTargetOnly(ev.Player, ev.Player.CustomName);
            }
        }

        /// <inheritdoc cref="Exiled.Events.Handlers.Player.ChangedNickname" />
        public void OnChangedNickname(ChangedNicknameEventArgs ev)
        {
            Timing.CallDelayed(0.1f, () =>
            {
                if (ev.Player is { IsConnected: true } && ev.Player.TryGetCustomRole(out CustomRole role))
                {
                    foreach (Player player in Player.List)
                    {
                        if (!player.IsDead)
                            continue;

                        player.SetDispayNicknameForTargetOnly(ev.Player, role.GetSpectatorText(ev.Player));
                        Log.Debug($"[Name sync] [{nameof(OnChangedNickname)}] Sent name of {ev.Player.Nickname} to {player.Nickname}");
                    }
                }
            });
        }
    }
}
