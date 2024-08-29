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

            if (!ev.Player.TryGetCustomRole(out CustomRole role))
            {
                ev.Target.SetDispayNicknameForTargetOnly(ev.Player, role.GetSpectatorText(ev.Player));
                Log.Debug($"[Name sync] Sent name of {ev.Player.Nickname} to {ev.Target.Nickname}");
                return;
            }

            Log.Debug($"[Name sync] Name reset for {ev.Player.Nickname} of {ev.Target.Nickname}.");
            ev.Target.SetDispayNicknameForTargetOnly(ev.Player, ev.Player.CustomName);
        }
    }
}
