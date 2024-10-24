// -----------------------------------------------------------------------
// <copyright file="HumanMaxHealth.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Fixes
{
#pragma warning disable SA1313
    using Exiled.API.Features;
    using HarmonyLib;
    using Mirror;
    using PlayerStatsSystem;
    using UnityEngine;

    /// <summary>
    /// Fix health bar for custom max hp.
    /// </summary>
    [HarmonyPatch(typeof(MaxHealthStat), nameof(MaxHealthStat.WriteValue))]
    public class HumanMaxHealth
    {
#pragma warning disable SA1313
        private static bool Prefix(MaxHealthStat __instance, NetworkWriter writer)
        {
            float value = __instance.CurValue;

            if (Player.Get(__instance.Hub).MaxHealth != default)
                value -= 100;

            int num = Mathf.Clamp(Mathf.CeilToInt(value), 0, ushort.MaxValue);
            writer.WriteUShort((ushort)num);
            return false;
        }
    }
}