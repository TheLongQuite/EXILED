// -----------------------------------------------------------------------
// <copyright file="WaitingForPlayers.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Server
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

    using API.Features.Pools;

    using HarmonyLib;

    using PluginAPI.Core;
    using PluginAPI.Events;

    using RoundRestarting;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches <see cref="CharacterClassManager.Init"/> coroutine.
    /// Adds the <see cref="Handlers.Server.WaitingForPlayers"/> event.
    /// </summary>
    [HarmonyPatch]
    internal static class WaitingForPlayers
    {
        private static MethodInfo TargetMethod()
        {
            return Method(typeof(CharacterClassManager).GetNestedType("<Init>d__70", all), "MoveNext");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            int offset = 1;
            int index = newInstructions.FindIndex(i => i.StoresField(Field(typeof(Statistics), nameof(Statistics.CurrentRound)))) + offset;

            newInstructions.InsertRange(
                index,
                new CodeInstruction[]
                {
                    // Handlers.Server.OnWaitingForPlayers()
                    new(OpCodes.Call, Method(typeof(Handlers.Server), nameof(Handlers.Server.OnWaitingForPlayers))),
                });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}