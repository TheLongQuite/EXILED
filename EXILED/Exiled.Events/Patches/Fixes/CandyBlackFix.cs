// -----------------------------------------------------------------------
// <copyright file="CandyBlackFix.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Fixes
{
#pragma warning disable SA1402
#pragma warning disable SA1313
    using System;

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

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Fix for chamber lists weren't cleared.
    /// </summary>
    [HarmonyPatch(typeof(CandyBlack), nameof(CandyBlack.GetRandomDoor))]
    internal class CandyBlackFix
    {
        private static readonly DoorVariant[] NewCache = new DoorVariant[200];

        private static bool Prefix(ref DoorVariant __result)
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
            {
                __result = CandyBlack.GetWhitelistedDoors().RandomItem<DoorVariant>();
                return false;
            }

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
                        NewCache[maxExclusive++] = whitelistedDoor;
                }
                catch (Exception ex)
                {
                    Log.Error($"[SIGMA2]\n{ex}\nIndex: {maxExclusive - 1}\nDoorsNonAlloc: {NewCache.Length}");
                }

                if (num2 < num1)
                {
                    doorVariant = whitelistedDoor;
                    num1 = num2;
                }
            }

            __result = maxExclusive != 0 ? NewCache[UnityEngine.Random.Range(0, maxExclusive)] : doorVariant;
            return false;
        }
    }
}