// -----------------------------------------------------------------------
// <copyright file="Shooting.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Player
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using API.Features;
    using API.Features.Pools;

    using CustomPlayerEffects;

    using Exiled.Events.EventArgs.Player;

    using HarmonyLib;

    using InventorySystem.Items.Firearms.BasicMessages;

    using static HarmonyLib.AccessTools;

    /// <summary>
    ///     Patches <see cref="FirearmBasicMessagesHandler.ServerShotReceived" />.
    ///     Adds the <see cref="Handlers.Player.Shooting" /> events.
    /// </summary>
    [HarmonyPatch(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerShotReceived))]
    internal static class Shooting
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            Label returnLabel = generator.DefineLabel();

            LocalBuilder player = generator.DeclareLocal(typeof(Player));

            int offset = -1;
            int index = newInstructions.FindIndex(instruction => instruction.Calls(Method(typeof(SpawnProtected), nameof(SpawnProtected.CheckPlayer)))) + offset;

            newInstructions.InsertRange(
                index,
                new[]
                {
                    // Player.Get(referenceHub)
                    new CodeInstruction(OpCodes.Ldloc_0).MoveLabelsFrom(newInstructions[index]),
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),
                    new(OpCodes.Dup),
                    new(OpCodes.Stloc_S, player.LocalIndex),

                    // Check to make sure the player is not null. (Yes, this happens)
                    new(OpCodes.Brfalse_S, returnLabel),
                    new(OpCodes.Ldloc_S, player.LocalIndex),

                    // msg
                    new(OpCodes.Ldarg_1),

                    // ShootingEventArgs ev = new(Player, ShotMessage)
                    new(OpCodes.Newobj, GetDeclaredConstructors(typeof(ShootingEventArgs))[0]),
                    new(OpCodes.Dup),

                    // Handlers.Player.OnShooting(ev)
                    new(OpCodes.Call, Method(typeof(Handlers.Player), nameof(Handlers.Player.OnShooting))),

                    // if (!ev.IsAllowed)
                    //     return;
                    new(OpCodes.Callvirt, PropertyGetter(typeof(ShootingEventArgs), nameof(ShootingEventArgs.IsAllowed))),
                    new(OpCodes.Brfalse_S, returnLabel),
                });

            newInstructions[newInstructions.Count - 1].WithLabels(returnLabel);

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}