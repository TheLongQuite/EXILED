// -----------------------------------------------------------------------
// <copyright file="Spawning.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Player
{
    using System.Reflection;

    using API.Features;
    using Exiled.Events.EventArgs.Player;

    using HarmonyLib;

    using PlayerRoles;
    using PlayerRoles.FirstPersonControl;
    using PlayerRoles.FirstPersonControl.Spawnpoints;

    using UnityEngine;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches <see cref="RoleSpawnpointManager"/> delegate.
    /// Adds the <see cref="Handlers.Player.Spawning"/> event.
    /// Fix for spawning in void.
    /// </summary>
    [HarmonyPatch(typeof(RoleSpawnpointManager), nameof(RoleSpawnpointManager.SetPosition))]
    internal static class Spawning
    {
        private static bool Prefix(ReferenceHub hub, PlayerRoleBase newRole)
        {
            if (newRole.ServerSpawnReason == RoleChangeReason.Destroyed || !Player.TryGet(hub, out Player player))
                return true;

            Vector3 oldPosition = hub.transform.position;
            float oldRotation = 0;

            if (newRole is IFpcRole fpcRole)
            {
                if (newRole.ServerSpawnFlags.HasFlag(RoleSpawnFlags.UseSpawnpoint) && fpcRole.SpawnpointHandler != null && fpcRole.SpawnpointHandler.TryGetSpawnpoint(out Vector3 position, out float horizontalRot))
                {
                    oldPosition = position;
                    oldRotation = horizontalRot;
                }

                SpawningEventArgs ev = new(player, oldPosition, oldRotation);

                Handlers.Player.OnSpawning(ev);

                player.Position = ev.Position;
                fpcRole.FpcModule.MouseLook.CurrentHorizontal = ev.HorizontalRotation;
            }
            else
            {
                Handlers.Player.OnSpawning(new(player, oldPosition, oldRotation));
            }

            return false;
        }
    }
}