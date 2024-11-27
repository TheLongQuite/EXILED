// -----------------------------------------------------------------------
// <copyright file="Shot.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Player
{
#pragma warning disable SA1402 // File may only contain a single type

    using System.Collections.Generic;
    using System.Reflection.Emit;

    using API.Features;
    using API.Features.Pools;

    using EventArgs.Player;
    using Exiled.Events.Attributes;
    using HarmonyLib;

    using InventorySystem.Items.Firearms;
    using InventorySystem.Items.Firearms.Modules;

    using UnityEngine;

    using static HarmonyLib.AccessTools;

    using Item = API.Features.Items.Item;

    /// <summary>
    /// Patches <see cref="SingleBulletHitscan.Fire()" />.
    /// Adds the <see cref="Handlers.Player.Shot" /> events.
    /// </summary>
    [EventPatch(typeof(Handlers.Player), nameof(Handlers.Player.Shot))]
    [HarmonyPatch(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.ServerProcessTargetHit))]
    internal static class Shot
    {
        /// <summary>
        /// Process shot.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="firearm">The firearm.</param>
        /// <param name="hit">The raycast hit.</param>
        /// <param name="destructible">The destructible.</param>
        /// <param name="damage">The damage.</param>
        /// <returns>If the shot is allowed.</returns>
        internal static bool ProcessShot(ReferenceHub player, Firearm firearm, RaycastHit hit, IDestructible destructible, ref float damage)
        {
            ShotEventArgs shotEvent = new(Player.Get(player), Item.Get<API.Features.Items.Firearm>(firearm), hit, destructible, damage);

            Handlers.Player.OnShot(shotEvent);

            if (shotEvent.CanHurt)
                damage = shotEvent.Damage;

            return shotEvent.CanHurt;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            Label returnLabel = generator.DefineLabel();
            Label jump = generator.DefineLabel();

            int offset = 1;
            int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Stloc_0) + offset;

            newInstructions.InsertRange(
                index,
                new CodeInstruction[]
                {
                    // this.Hub
                    new(OpCodes.Ldarg_1),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(HitboxIdentity), nameof(HitboxIdentity.TargetHub))),

                    // this.Firearm
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(SingleBulletHitscan), nameof(SingleBulletHitscan.Firearm))),

                    // hit
                    new(OpCodes.Ldarg_2),

                    // destructible
                    new(OpCodes.Ldloc_1),

                    // damage
                    new(OpCodes.Ldloca_S, 0),

                    new(OpCodes.Call, Method(typeof(Shot), nameof(ProcessShot), new[] { typeof(ReferenceHub), typeof(Firearm), typeof(RaycastHit), typeof(IDestructible), typeof(float).MakeByRefType(), })),

                    // if (!ev.CanHurt)
                    //    return;
                    new(OpCodes.Brfalse_S, returnLabel),
                });

            /*offset = -3;
            index = newInstructions.FindLastIndex(
                instruction => instruction.Calls(Method(typeof(StandardHitregBase), nameof(StandardHitregBase.PlaceBulletholeDecal)))) + offset;

            // replace the original goto label
            newInstructions.FindAll(instruction => instruction.opcode == OpCodes.Brfalse).ForEach(instruction => instruction.operand = jump);

            newInstructions.InsertRange(
                index,
                new[]
                {
                    new CodeInstruction(OpCodes.Nop).WithLabels(jump),

                    // this.Hub
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(StandardHitregBase), nameof(StandardHitregBase.Hub))),

                    // this.Firearm
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(StandardHitregBase), nameof(StandardHitregBase.Firearm))),

                    // hit
                    new(OpCodes.Ldarg_2),

                    // destructible
                    new(OpCodes.Ldnull),

                    // damage
                    new(OpCodes.Ldc_R4, 0f),
                    new(OpCodes.Stloc_S, 1),
                    new(OpCodes.Ldloca_S, 1),

                    // Shot.ProcessShot
                    new(OpCodes.Call, Method(typeof(Shot), nameof(ProcessShot), new[] { typeof(ReferenceHub), typeof(Firearm), typeof(RaycastHit), typeof(IDestructible), typeof(float).MakeByRefType(), })),
                    new(OpCodes.Pop),
                });*/

            newInstructions[newInstructions.Count - 1].WithLabels(returnLabel);

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }

    /* TODO
    /// <summary>
    /// Patches <see cref="DisruptorHitreg.ServerPerformShot" />.
    /// Adds the <see cref="Handlers.Player.Shot" /> events.
    /// </summary>
    [EventPatch(typeof(Handlers.Player), nameof(Handlers.Player.Shot))]
    [HarmonyPatch(typeof(DisruptorHitregModule), nameof(DisruptorHitregModule.ServerProcessTargetHit))]
    internal static class ShotDisruptor
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            Label returnLabel = generator.DefineLabel();

            int offset = -3;
            int index = newInstructions.FindIndex(instruction => instruction.Calls(Method(typeof(DisruptorHitreg), nameof(DisruptorHitreg.CreateExplosion)))) + offset;

            newInstructions.InsertRange(
                index,
                new CodeInstruction[]
                {
                    // this.Hub
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(DisruptorHitreg), nameof(DisruptorHitreg.Hub))),

                    // this.Firearm
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(DisruptorHitreg), nameof(DisruptorHitreg.Firearm))),

                    // hit
                    new(OpCodes.Ldloc_S, 7),

                    // destructible
                    new(OpCodes.Ldloc_S, 8),

                    // damage
                    new(OpCodes.Ldc_R4, 0f),
                    new(OpCodes.Stloc_S, 9),
                    new(OpCodes.Ldloca_S, 9),

                    new(OpCodes.Call, Method(typeof(Shot), nameof(Shot.ProcessShot), new[] { typeof(ReferenceHub), typeof(Firearm), typeof(RaycastHit), typeof(IDestructible), typeof(float).MakeByRefType(), })),

                    // if (!ev.CanHurt)
                    //    return;
                    new(OpCodes.Brfalse_S, returnLabel),
                });

            offset = -5;
            index = newInstructions.FindLastIndex(instruction => instruction.Calls(Method(typeof(IDestructible), nameof(IDestructible.Damage)))) + offset;

            newInstructions.InsertRange(
                index,
                new[]
                {
                    // this.Hub
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(DisruptorHitreg), nameof(DisruptorHitreg.Hub))),

                    // this.Firearm
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(DisruptorHitreg), nameof(DisruptorHitreg.Firearm))),

                    // hit
                    new(OpCodes.Ldloc_S, 7),

                    // destructible
                    new(OpCodes.Ldloc_S, 8),

                    // damage
                    new(OpCodes.Ldloca_S, 9),

                    new(OpCodes.Call, Method(typeof(Shot), nameof(Shot.ProcessShot), new[] { typeof(ReferenceHub), typeof(Firearm), typeof(RaycastHit), typeof(IDestructible), typeof(float).MakeByRefType(), })),

                    // if (!ev.CanHurt)
                    //    return;
                    new(OpCodes.Brfalse_S, returnLabel),
                });

            newInstructions[newInstructions.Count - 1].WithLabels(returnLabel);

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
    */
}
