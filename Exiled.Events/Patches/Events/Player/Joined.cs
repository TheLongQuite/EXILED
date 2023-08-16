// -----------------------------------------------------------------------
// <copyright file="Joined.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Player
{
#pragma warning disable SA1600

    using System;

    using API.Features;
    using Exiled.Events.EventArgs.Player;
    using Exiled.Loader.Features;

    using HarmonyLib;

    /// <summary>
    ///     Patches <see cref="ReferenceHub.Start" />.
    ///     Adds the <see cref="Handlers.Player.Joined" /> event.
    /// </summary>
    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.Start))]
    internal static class Joined
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(ReferenceHub __instance)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            try
            {
                if (ReferenceHub.HostHub == null)
                {
                    Server.Host = new Player(__instance);
                }
            }
            catch (Exception exception)
            {
                Log.Error($"{nameof(Joined)}: {exception}\n{exception.StackTrace}");
            }
        }
    }
}
