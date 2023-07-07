// -----------------------------------------------------------------------
// <copyright file="Banning.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Player
{
#pragma warning disable SA1313
    using System;

    using API.Features;

    using CommandSystem;

    using Exiled.Events.EventArgs.Player;
    using Footprinting;

    using HarmonyLib;
    using PluginAPI.Events;

    using Log = API.Features.Log;

    /// <summary>
    ///     Patches <see cref="BanPlayer.BanUser(Footprint, ICommandSender, string, long)" />.
    ///     Adds the <see cref="Handlers.Player.Banning" /> event.
    /// </summary>
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), typeof(Footprint), typeof(ICommandSender), typeof(string), typeof(long))]
    internal static class Banning
    {
        private static bool Prefix(Footprint target, ICommandSender issuer, string reason, long duration, ref bool __result)
        {
            try
            {
                if (duration == 0L && target.Hub != null)
                {
                    __result = BanPlayer.KickUser(target.Hub, issuer, reason);
                    return false;
                }

                if (target.BypassStaff)
                {
                    __result = false;
                    return false;
                }

                long issuanceTime = TimeBehaviour.CurrentTimestamp();
                long banExpirationTime = TimeBehaviour.GetBanExpirationTime((uint)duration);
                string originalName = BanPlayer.ValidateNick(target.Nickname);
                string message = $"You have been banned. {(!string.IsNullOrEmpty(reason) ? "Reason: " + reason : string.Empty)}";

                Player issuerPlayer = Player.Get(issuer) ?? Server.Host;

                BanningEventArgs ev = new(Player.Get(target), issuerPlayer, duration, reason, message);

                Handlers.Player.OnBanning(ev);

                if (!ev.IsAllowed)
                {
                    __result = false;
                    return false;
                }

                duration = ev.Duration;
                reason = ev.Reason;
                message = ev.FullMessage;

                BanPlayer.ApplyIpBan(target, issuer, reason, duration);

                if (!string.IsNullOrEmpty(target.LogUserID))
                {
                    BanHandler.IssueBan(
                    new BanDetails
                    {
                        OriginalName = originalName,
                        Id = target.LogUserID,
                        IssuanceTime = issuanceTime,
                        Expires = banExpirationTime,
                        Reason = reason,
                        Issuer = issuer.LogName,
                    }, BanHandler.BanType.UserId);
                }

                if (!string.IsNullOrEmpty(target.LogUserID2))
                {
                    BanHandler.IssueBan(
                        new BanDetails
                        {
                            OriginalName = originalName,
                            Id = target.LogUserID2,
                            IssuanceTime = issuanceTime,
                            Expires = banExpirationTime,
                            Reason = reason,
                            Issuer = issuer.LogName,
                        }, BanHandler.BanType.UserId);
                }

                if (target.Hub != null)
                    ServerConsole.Disconnect(target.Hub.gameObject, message);

                __result = true;
                return false;
            }
            catch (Exception exception)
            {
                Log.Error($"Exiled.Events.Patches.Events.Player.Banning: {exception}\n{exception.StackTrace}");
                return true;
            }
        }
    }
}