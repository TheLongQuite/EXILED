// -----------------------------------------------------------------------
// <copyright file="CandyBlackFix.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Fixes
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Pools;

    using HarmonyLib;

    using InventorySystem.Items.Usables.Scp330;

    /// <summary>
    /// Fix for chamber lists weren't cleared.
    /// </summary>
    [HarmonyPatch(typeof(CandyBlack), MethodType.StaticConstructor)]
    internal class CandyBlackFix
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            int index = newInstructions.FindIndex(i => i.LoadsConstant(64));

            newInstructions[index].operand = 200;

            Log.Error(newInstructions.ToString(true));

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}