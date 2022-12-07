// -----------------------------------------------------------------------
// <copyright file="ReceivingStatusEffect.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Player
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using CustomPlayerEffects;

    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Player;

    using HarmonyLib;

    using NorthwoodLib.Pools;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches the <see cref="StatusEffectBase.Intensity"/> method.
    /// Adds the <see cref="Handlers.Player.ReceivingEffect"/> event.
    /// </summary>
    [HarmonyPatch(typeof(StatusEffectBase), nameof(StatusEffectBase.Intensity), MethodType.Setter)]
    internal static class ReceivingStatusEffect
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            LocalBuilder ev = generator.DeclareLocal(typeof(ReceivingEffectEventArgs));
            LocalBuilder player = generator.DeclareLocal(typeof(Player));

            Label returnLabel = generator.DefineLabel();
            Label isHostLabel = generator.DefineLabel();
            Label isNotHostLabel = generator.DefineLabel();

            const int offset = 1;
            int index = newInstructions.FindIndex(instruction => instruction.opcode == OpCodes.Ret) + offset;

            newInstructions.InsertRange(
                index,
                new[]
                {
                    // var player = Player.Get(this.Hub)
                    //
                    // if (player == null)
                    //    load host
                    // else
                    //    load player
                    new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
                    new(OpCodes.Call, PropertyGetter(typeof(StatusEffectBase), nameof(StatusEffectBase.Hub))),
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),
                    new(OpCodes.Dup),
                    new(OpCodes.Stloc_S, player.LocalIndex),
                    new(OpCodes.Brfalse_S, isHostLabel),

                    // player
                    new(OpCodes.Ldloc_S, player.LocalIndex),
                    new(OpCodes.Br_S, isNotHostLabel),

                    // Server.Host
                    new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(Server), nameof(Server.Host))).WithLabels(isHostLabel),

                    // this
                    new CodeInstruction(OpCodes.Ldarg_0).WithLabels(isNotHostLabel),

                    // value
                    new(OpCodes.Ldarg_1),

                    // this._intensity
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, Field(typeof(StatusEffectBase), nameof(StatusEffectBase._intensity))),

                    // var ev = new ReceivingEventArgs(player, this, value, currentState)
                    new(OpCodes.Newobj, GetDeclaredConstructors(typeof(ReceivingEffectEventArgs))[0]),
                    new(OpCodes.Dup),
                    new(OpCodes.Dup),
                    new(OpCodes.Stloc_S, ev.LocalIndex),
                    new(OpCodes.Call, Method(typeof(Handlers.Player), nameof(Handlers.Player.OnReceivingEffect))),

                    // if (!ev.IsAllowed)
                    //    return;
                    new(OpCodes.Callvirt, PropertyGetter(typeof(ReceivingEffectEventArgs), nameof(ReceivingEffectEventArgs.IsAllowed))),
                    new(OpCodes.Brfalse_S, returnLabel),

                    // value = ev.State
                    new(OpCodes.Ldloc_S, ev.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(ReceivingEffectEventArgs), nameof(ReceivingEffectEventArgs.State))),
                    new(OpCodes.Starg_S, 1),

                    // this.Duration = ev.Duration
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldloc_S, ev.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(ReceivingEffectEventArgs), nameof(ReceivingEffectEventArgs.Duration))),
                    new(OpCodes.Callvirt, PropertySetter(typeof(StatusEffectBase), nameof(StatusEffectBase.Duration))),
                });

            newInstructions[newInstructions.Count - 1].WithLabels(returnLabel);

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}