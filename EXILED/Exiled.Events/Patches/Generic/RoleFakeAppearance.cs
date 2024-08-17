// -----------------------------------------------------------------------
// <copyright file="RoleFakeAppearance.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Generic
{
#pragma warning disable SA1402
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using API.Features;

    using Exiled.API.Extensions;
    using Exiled.API.Features.Pools;
    using Exiled.API.Features.Roles;

    using HarmonyLib;

    using PlayerRoles;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches <see cref="PlayerRoleManager.Update"/> to implement <see cref="IAppearancedRole.GlobalAppearance"/>.
    /// </summary>
    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.Update))]
    internal class RoleFakeAppearance
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(codeInstructions);

            LocalBuilder player = generator.DeclareLocal(typeof(Player));
            LocalBuilder role = generator.DeclareLocal(typeof(IAppearancedRole));

            Label skip = generator.DefineLabel();

            int offset = -3;
            int index = newInstructions.FindIndex(i => i.Calls(PropertyGetter(typeof(PlayerRoleManager), nameof(PlayerRoleManager.Hub)))) + offset;

            newInstructions.InsertRange(
                index,
                new[]
                {
                    // Player player = Player.Get(this.Hub);
                    // if (player == null)
                    //     skip;
                    new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(PlayerRoleManager), nameof(PlayerRoleManager.Hub))),
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),
                    new(OpCodes.Dup),
                    new(OpCodes.Stloc_S, player.LocalIndex),
                    new(OpCodes.Brfalse_S, skip),

                    // if (player.ReferenceHub == targetHub)
                    //     skip;
                    new(OpCodes.Ldloc_1),
                    new(OpCodes.Ldloc_S, player.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(Player), nameof(Player.ReferenceHub))),
                    new(OpCodes.Callvirt, Method(typeof(ReferenceHub), "op_Equality")),
                    new(OpCodes.Brtrue_S, skip),

                    // if (player.Role is not IAppearancedRole appearancedRole)
                    //     skip;
                    new(OpCodes.Ldloc_S, player.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(Player), nameof(Player.Role))),
                    new(OpCodes.Isinst, role.LocalType),
                    new(OpCodes.Dup),
                    new(OpCodes.Stloc_S, role.LocalIndex),
                    new(OpCodes.Brfalse_S, skip),

                    // roleType = appearancedRole.GetAppearanceForPlayer(Player.Get(targetHub));
                    new(OpCodes.Ldloc_S, role.LocalIndex),
                    new(OpCodes.Ldloc_1),
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),
                    new(OpCodes.Call, Method(typeof(RoleExtensions), nameof(RoleExtensions.GetAppearanceForPlayer))),
                    new(OpCodes.Stloc_2),

                    new CodeInstruction(OpCodes.Nop).WithLabels(skip),
                });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }

    /// <summary>
    /// Patches <see cref="RoleSyncInfoPack.WritePlayers"/> to implement <see cref="IAppearancedRole.GlobalAppearance"/>.
    /// </summary>
    [HarmonyPatch(typeof(RoleSyncInfoPack), nameof(RoleSyncInfoPack.WritePlayers))]
    internal class RoleFakeAppearancePack
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(codeInstructions);

            LocalBuilder player = generator.DeclareLocal(typeof(Player));
            LocalBuilder role = generator.DeclareLocal(typeof(IAppearancedRole));

            Label skip = generator.DefineLabel();

            int offset = 1;
            int index = newInstructions.FindIndex(i => i.opcode == OpCodes.Stloc_2) + offset;

            newInstructions.InsertRange(
                index,
                new[]
                {
                    // Player player = Player.Get(refHub);
                    // if (player == null)
                    //     skip;
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),
                    new(OpCodes.Dup),
                    new(OpCodes.Stloc_S, player.LocalIndex),
                    new(OpCodes.Brfalse_S, skip),

                    // if (player.ReferenceHub == this._receiverHub)
                    //     skip;
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, Field(typeof(RoleSyncInfoPack), nameof(RoleSyncInfoPack._receiverHub))),
                    new(OpCodes.Ldloc_S, player.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(Player), nameof(Player.ReferenceHub))),
                    new(OpCodes.Callvirt, Method(typeof(ReferenceHub), "op_Equality")),
                    new(OpCodes.Brtrue_S, skip),

                    // if (player.Role is not IAppearancedRole appearancedRole)
                    //     skip;
                    new(OpCodes.Ldloc_S, player.LocalIndex),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(Player), nameof(Player.Role))),
                    new(OpCodes.Isinst, role.LocalType),
                    new(OpCodes.Dup),
                    new(OpCodes.Stloc_S, role.LocalIndex),
                    new(OpCodes.Brfalse_S, skip),

                    // roleType = appearancedRole.GetAppearanceForPlayer(Player.Get(this._receiverHub));
                    new(OpCodes.Ldloc_S, role.LocalIndex),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, Field(typeof(RoleSyncInfoPack), nameof(RoleSyncInfoPack._receiverHub))),
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),
                    new(OpCodes.Call, Method(typeof(RoleExtensions), nameof(RoleExtensions.GetAppearanceForPlayer))),
                    new(OpCodes.Stloc_2),

                    new CodeInstruction(OpCodes.Nop).WithLabels(skip),
                });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}