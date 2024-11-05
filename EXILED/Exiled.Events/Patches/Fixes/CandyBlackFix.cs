// -----------------------------------------------------------------------
// <copyright file="CandyBlackFix.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Fixes
{
#pragma warning disable SA1402
#pragma warning disable SA1313
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Pools;

    using HarmonyLib;
    using Interactables.Interobjects.DoorUtils;
    using InventorySystem.Items.Usables.Scp330;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Fix for chamber lists weren't cleared.
    /// </summary>
    [HarmonyPatch(typeof(CandyBlack), nameof(CandyBlack.GetRandomDoor))]
    internal class CandyBlackFix
    {
        private static readonly DoorVariant[] NewCache = new DoorVariant[200];

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            foreach (CodeInstruction instruction in newInstructions)
            {
                if (instruction.LoadsField(Field(typeof(CandyBlack), nameof(CandyBlack.DoorsNonAlloc))))
                {
                    instruction.operand = Field(typeof(CandyBlackFix), nameof(CandyBlackFix.NewCache));
                }
            }

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}