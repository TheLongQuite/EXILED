// -----------------------------------------------------------------------
// <copyright file="CandyBlackFix.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Fixes
{
    using System;

#pragma warning disable SA1402
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Pools;
    using HarmonyLib;
    using Interactables.Interobjects.DoorUtils;
    using InventorySystem.Items.Usables.Scp330;
    using PlayerRoles;
    using PlayerRoles.PlayableScps;

    /// <summary>
    /// Fix for chamber lists weren't cleared.
    /// </summary>
    // [HarmonyPatch(typeof(CandyBlack), MethodType.StaticConstructor)]
    internal class CandyBlackFix
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            int index = newInstructions.FindIndex(i => i.LoadsConstant(64));

            newInstructions[index].operand = 200;

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }

    /// <summary>
    /// Fix for chamber lists weren't cleared.
    /// </summary>
    [HarmonyPatch(typeof(CandyBlack), nameof(CandyBlack.GetRandomDoor))]
    internal class CandyBlackTest
    {
        private static bool Prefix()
        {
            int index1 = 0;
            try
            {
                foreach (ReferenceHub allHub in ReferenceHub.AllHubs)
                {
                    if (allHub.roleManager.CurrentRole is FpcStandardScp currentRole)
                    {
                        CandyBlack.PositionsCache[index1] = currentRole.FpcModule.Position;
                        CandyBlack.PositionModifiers[index1] = currentRole.RoleTypeId == RoleTypeId.Scp0492;
                        if (++index1 >= 2)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[SIGMA1]\n{ex}\nIndex: {index1}\nPositionsCache: {CandyBlack.PositionsCache.Length}\nPositionModifiers: {CandyBlack.PositionModifiers.Length}");
            }

            if (index1 == 0)
                return CandyBlack.GetWhitelistedDoors().RandomItem<DoorVariant>();

            DoorVariant doorVariant = null;
            float num1 = float.MaxValue;
            int maxExclusive = 0;

            foreach (DoorVariant whitelistedDoor in CandyBlack.GetWhitelistedDoors())
            {
                float num2 = 0.0f;
                bool flag = true;
                for (int index2 = 0; index2 < index1; ++index2)
                {
                    float sqrMagnitude = (whitelistedDoor.transform.position - CandyBlack.PositionsCache[index2]).sqrMagnitude;
                    if (CandyBlack.PositionModifiers[index2])
                        sqrMagnitude *= 0.3f;
                    if (sqrMagnitude < 750.0)
                        flag = false;
                    num2 += sqrMagnitude;
                }

                try
                {
                    if (flag)
                        CandyBlack.DoorsNonAlloc[maxExclusive++] = whitelistedDoor;
                }
                catch (Exception ex)
                {
                    Log.Error($"[SIGMA2]\n{ex}\nIndex: {maxExclusive - 1}\nDoorsNonAlloc: {CandyBlack.DoorsNonAlloc.Length}");
                }

                if (num2 < num1)
                {
                    doorVariant = whitelistedDoor;
                    num1 = num2;
                }
            }

            return maxExclusive != 0 ? CandyBlack.DoorsNonAlloc[UnityEngine.Random.Range(0, maxExclusive)] : doorVariant;
        }
    }
}