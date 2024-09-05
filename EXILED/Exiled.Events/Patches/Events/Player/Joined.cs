// -----------------------------------------------------------------------
// <copyright file="Joined.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Player
{
#pragma warning disable SA1600
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

    using API.Features;
    using HarmonyLib;

    /// <summary>
    ///     Patches <see cref="ReferenceHub.Start" />.
    ///     Adds the <see cref="Handlers.Player.Joined" /> event.
    /// </summary>
    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.Start))]
    internal static class Joined
    {
        private static void Postfix(ReferenceHub __instance)
        {
            if (ReferenceHub.HostHub == null)
            {
                Server.Host = new Player(__instance);
            }
        }
    }
}
