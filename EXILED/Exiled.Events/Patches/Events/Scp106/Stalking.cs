// -----------------------------------------------------------------------
// <copyright file="Stalking.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Scp106
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

    using API.Features;
    using API.Features.Pools;
    using Exiled.Events.Attributes;
    using Exiled.Events.EventArgs.Scp106;
    using HarmonyLib;

    using PlayerRoles.PlayableScps.Scp106;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches <see cref="Scp106StalkAbility.ServerProcessCmd"/>.
    /// To add the <see cref="Handlers.Scp106.Stalking"/> event.
    /// </summary>
    [EventPatch(typeof(Handlers.Scp106), nameof(Handlers.Scp106.Stalking))]
    [HarmonyPatch(typeof(Scp106StalkAbility), nameof(Scp106StalkAbility.ServerProcessCmd))]
    public class Stalking
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            LocalBuilder ev = generator.DeclareLocal(typeof(StalkingEventArgs));

            Label returnLabel = generator.DefineLabel();
            int offset = 2;
            int index = newInstructions.FindIndex(instruction => instruction.operand == (object)PropertyGetter(typeof(Scp106StalkAbility), nameof(Scp106StalkAbility.StalkActive))) + offset;
            newInstructions.InsertRange(
                index,
                new CodeInstruction[]
                {
                    // Player.Get(this.Owner);
                    new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(Scp106HuntersAtlasAbility), nameof(Scp106HuntersAtlasAbility.Owner))),
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),

                    // true
                    new(OpCodes.Ldc_I4_1),

                    // StalkingEventArgs ev = new(Player, isAllowed)
                    new(OpCodes.Newobj, GetDeclaredConstructors(typeof(StalkingEventArgs))[0]),
                    new(OpCodes.Dup),
                    new(OpCodes.Dup),
                    new(OpCodes.Stloc_S, ev.LocalIndex),

                    // Handlers.Scp106.OnFinishingRecall(ev)
                    new(OpCodes.Call, Method(typeof(Handlers.Scp106), nameof(Handlers.Scp106.OnStalking))),

                    // if (!ev.IsAllowed)
                    //    return;
                    new(OpCodes.Callvirt, PropertyGetter(typeof(StalkingEventArgs), nameof(StalkingEventArgs.IsAllowed))),
                    new(OpCodes.Brfalse_S, returnLabel),
                });

            newInstructions.InsertRange(
                newInstructions.Count - 1,
                new CodeInstruction[]
                {
                    // Player.Get(this.Owner);
                    new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(Scp106HuntersAtlasAbility), nameof(Scp106HuntersAtlasAbility.Owner))),
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),

                    // true
                    new(OpCodes.Ldc_I4_1),

                    // StalkingEventArgs ev = new(Player, isAllowed)
                    new(OpCodes.Newobj, GetDeclaredConstructors(typeof(StalkingEventArgs))[0]),
                    new(OpCodes.Dup),
                    new(OpCodes.Dup),
                    new(OpCodes.Stloc_S, ev.LocalIndex),

                    // Handlers.Scp106.OnStalking(ev)
                    new(OpCodes.Call, Method(typeof(Handlers.Scp106), nameof(Handlers.Scp106.OnStalking))),

                    // if (!ev.IsAllowed)
                    //    return;
                    new(OpCodes.Callvirt, PropertyGetter(typeof(StalkingEventArgs), nameof(StalkingEventArgs.IsAllowed))),
                    new(OpCodes.Brfalse_S, returnLabel),
                });

            // replace "base.Vigor.VigorAmount < 0.25f" with "base.Vigor.VigorAmount < ev.MinimumVigor"
            offset = 0;
            index = newInstructions.FindIndex(instruction => instruction.opcode == OpCodes.Ldc_R4) + offset;
            newInstructions.RemoveAt(index);

            newInstructions.InsertRange(
                index,
                new CodeInstruction[]
                {
                    // ev.MinimumVigor
                    new(OpCodes.Ldloc_S, ev.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(StalkingEventArgs), nameof(StalkingEventArgs.MinimumVigor))),
                });

            newInstructions[newInstructions.Count - 1].WithLabels(returnLabel);

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}