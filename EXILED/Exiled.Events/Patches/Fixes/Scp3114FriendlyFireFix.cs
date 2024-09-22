// -----------------------------------------------------------------------
// <copyright file="Scp3114FriendlyFireFix.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Fixes
{
#pragma warning disable SA1402 // File may only contain a single type
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using API.Features.Pools;

    using Exiled.API.Features;

    using Footprinting;
    using HarmonyLib;
    using InventorySystem.Items.Pickups;
    using InventorySystem.Items.ThrowableProjectiles;
    using PlayerRoles;
    using PlayerStatsSystem;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches the <see cref="Scp2176Projectile.ServerShatter()"/> delegate.
    /// Fix Throwing a ghostlight with Scp in the room stun 079.
    /// Bug reported to NW (https://git.scpslgame.com/northwood-qa/scpsl-bug-reporting/-/issues/55).
    /// </summary>
    [HarmonyPatch(typeof(Scp2176Projectile), nameof(Scp2176Projectile.ServerShatter))]
    internal class Scp3114FriendlyFireFix
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            Label cnt = generator.DefineLabel();

            int offset = 0;
            int index = newInstructions.FindIndex(x => x.LoadsField(Field(typeof(RoomLightController), nameof(RoomLightController.Instances)))) + offset;

            Label skip = newInstructions[index].labels[0];

            offset = -3;
            index += offset;

            newInstructions.InsertRange(index, new[]
            {
                // if (this.PreviousOwner.Role.GetTeam() is Team.SCPs)
                new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
                new(OpCodes.Ldfld, Field(typeof(Scp2176Projectile), nameof(Scp2176Projectile.PreviousOwner))),
                new(OpCodes.Ldfld, Field(typeof(Footprint), nameof(Footprint.Role))),
                new(OpCodes.Call, Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), new[] { typeof(RoleTypeId) })),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ceq),

                new(OpCodes.Brfalse_S, cnt),

                new(OpCodes.Pop),
                new(OpCodes.Br_S, skip),

                new CodeInstruction(OpCodes.Nop).WithLabels(cnt),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}
