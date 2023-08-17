// -----------------------------------------------------------------------
// <copyright file="SpawningRagdoll.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Player
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

    using API.Features.Pools;

    using Exiled.Events.EventArgs.Player;

    using Handlers;

    using HarmonyLib;

    using PlayerRoles.Ragdolls;

    using PlayerStatsSystem;

    using UnityEngine;

    using static HarmonyLib.AccessTools;

    /// <summary>
    ///     Patches <see cref="RagdollManager.ServerSpawnRagdoll(ReferenceHub, DamageHandlerBase)" />.
    ///     Adds the <see cref="Player.SpawningRagdoll" /> event.
    /// </summary>
    [HarmonyPatch(typeof(RagdollManager), nameof(RagdollManager.ServerSpawnRagdoll))]
    internal static class SpawningRagdoll
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            Label cnt = generator.DefineLabel();

            LocalBuilder ev = generator.DeclareLocal(typeof(SpawningRagdollEventArgs));

            int offset = 0;
            int index = newInstructions.FindIndex(instruction => instruction.Calls(PropertySetter(typeof(BasicRagdoll), nameof(BasicRagdoll.NetworkInfo)))) + offset;

            newInstructions.InsertRange(
                index,
                new[]
                {
                    // RagdollInfo loads into stack before il inject

                    // handler
                    new CodeInstruction(OpCodes.Ldarg_1),

                    // true
                    new(OpCodes.Ldc_I4_1),

                    // SpawningRagdollEventArgs ev = new(RagdollInfo, DamageHandlerBase, bool)
                    new(OpCodes.Newobj, GetDeclaredConstructors(typeof(SpawningRagdollEventArgs))[0]),
                    new(OpCodes.Dup),
                    new(OpCodes.Dup),
                    new(OpCodes.Stloc_S, ev.LocalIndex),

                    // Player.OnSpawningRagdoll(ev)
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.OnSpawningRagdoll))),

                    // if (!ev.IsAllowed) {
                    //     Object.Destroy(gameObject);
                    //     return null;
                    // }
                    new(OpCodes.Callvirt, PropertyGetter(typeof(SpawningRagdollEventArgs), nameof(SpawningRagdollEventArgs.IsAllowed))),
                    new(OpCodes.Brtrue_S, cnt),

                    // gameobject loads into stack before il inject
                    new(OpCodes.Pop),
                    new(OpCodes.Call, Method(typeof(Object), nameof(Object.Destroy), new[] { typeof(Object) })),
                    new(OpCodes.Ldnull),
                    new(OpCodes.Ret),

                    // load ragdoll info into stack*/
                    new CodeInstruction(OpCodes.Ldloc_S, ev.LocalIndex).WithLabels(cnt),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(SpawningRagdollEventArgs), nameof(SpawningRagdollEventArgs.Info))),
                });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}