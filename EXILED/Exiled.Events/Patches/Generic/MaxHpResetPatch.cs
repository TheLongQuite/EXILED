// -----------------------------------------------------------------------
// <copyright file="MaxHpResetPatch.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Generic
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using API.Features.Pools;

    using Exiled.API.Extensions;
    using Exiled.API.Features;

    using HarmonyLib;

    using PlayerStatsSystem;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches <see cref="MaxHealthStat.ClassChanged"/>.
    /// </summary>
    [HarmonyPatch(typeof(MaxHealthStat), nameof(MaxHealthStat.ClassChanged))]
    internal class MaxHpResetPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(codeInstructions);

            int offset = -2;
            int index = newInstructions.FindLastIndex(i => i.Calls(PropertyGetter(typeof(StatBase), nameof(StatBase.MinValue)))) + offset;

            // dont reset MaxHealthStat value, we do that in ChangingRole patch, for posibility change maxhp in Spawning
            newInstructions.InsertRange(
                index,
                new CodeInstruction[]
                {
                     // ret;
                    new CodeInstruction(OpCodes.Ret).MoveLabelsFrom(newInstructions[index]),
                });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}