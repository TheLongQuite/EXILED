// -----------------------------------------------------------------------
// <copyright file="CandyHpFix.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Fixes
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using CustomPlayerEffects;

    using Exiled.API.Features;
    using Exiled.API.Features.Pools;

    using HarmonyLib;

    using PlayerStatsSystem;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Fixes new hp set in <see cref="CustomPlayerEffects.SugarHigh.Enabled()"/>.
    /// </summary>
    [HarmonyPatch(typeof(SugarHigh), nameof(SugarHigh.Enabled))]
    internal class CandyHpFix
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            int offset = 0;
            int index = newInstructions.FindIndex(i => i.Calls(PropertySetter(typeof(StatBase), nameof(StatBase.CurValue)))) + offset;

            newInstructions[index] = new CodeInstruction(OpCodes.Call, Method(typeof(CandyHpFix), nameof(CandyHpFix.Helper)));

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }

        private static void Helper(MaxHealthStat stat, float value)
        {
            if (Player.TryGet(stat.Hub, out Player player))
            {
                player.MaxHealth += value;
                return;
            }

            stat.CurValue += value;
        }
    }
}