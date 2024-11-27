// -----------------------------------------------------------------------
// <copyright file="RestartingRound.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Server
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using API.Features.Pools;
    using Exiled.Events.Attributes;
    using HarmonyLib;

    using RoundRestarting;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches <see cref="RoundRestart.InitiateRoundRestart"/>.
    /// Adds the <see cref="Handlers.Server.RestartingRound" /> event.
    /// </summary>
    [EventPatch(typeof(Handlers.Server), nameof(Handlers.Server.RestartingRound))]
    [HarmonyPatch(typeof(RoundRestart), nameof(RoundRestart.InitiateRoundRestart))]
    internal static class RestartingRound
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            newInstructions.InsertRange(
                0,
                new CodeInstruction[]
                {
                    // Handlers.Server.OnRestartingRound()
                    new(OpCodes.Call, Method(typeof(Handlers.Server), nameof(Handlers.Server.OnRestartingRound))),

                    // API.Features.Log.Debug("Round restarting", Loader.ShouldDebugBeShown)
                    new(OpCodes.Ldstr, "Round restarting"),
                    new(OpCodes.Call, Method(typeof(API.Features.Log), nameof(API.Features.Log.Debug), new[] { typeof(string) })),
                });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}