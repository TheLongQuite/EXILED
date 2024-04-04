// -----------------------------------------------------------------------
// <copyright file="Npc.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features
{
#nullable enable
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CentralAuth;
    using CommandSystem;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features.Components;
    using Exiled.API.Features.Items;
    using Footprinting;
    using InventorySystem.Items.Firearms.BasicMessages;
    using InventorySystem.Items.Firearms.Modules;
    using MEC;
    using Mirror;
    using PlayerRoles;
    using PlayerRoles.FirstPersonControl;
    using RelativePositioning;
    using UnityEngine;

    using Object = UnityEngine.Object;

    /// <summary>
    /// Wrapper class for handling NPC players.
    /// </summary>
    public class Npc : Player
    {
        /// <inheritdoc cref="Player" />
        public Npc(ReferenceHub referenceHub)
            : base(referenceHub)
        {
        }

        /// <inheritdoc cref="Player" />
        public Npc(GameObject gameObject)
            : base(gameObject)
        {
        }

        /// <summary>
        /// Gets a list of Npcs.
        /// </summary>
        public static new List<Npc> List => Player.List.OfType<Npc>().ToList();

        internal static readonly List<Npc> ToDestroyOnDeath = new List<Npc>();

        /// <summary>
        /// Retrieves the NPC associated with the specified ReferenceHub.
        /// </summary>
        /// <param name="rHub">The ReferenceHub to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the ReferenceHub, or <c>null</c> if not found.</returns>
        public static new Npc? Get(ReferenceHub rHub) => Player.Get(rHub) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the GameObject, or <c>null</c> if not found.</returns>
        public static new Npc? Get(GameObject gameObject) => Player.Get(gameObject) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified user ID.
        /// </summary>
        /// <param name="userId">The user ID to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the user ID, or <c>null</c> if not found.</returns>
        public static new Npc? Get(string userId) => Player.Get(userId) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified ID.
        /// </summary>
        /// <param name="id">The ID to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the ID, or <c>null</c> if not found.</returns>
        public static new Npc? Get(int id) => Player.Get(id) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified ICommandSender.
        /// </summary>
        /// <param name="sender">The ICommandSender to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the ICommandSender, or <c>null</c> if not found.</returns>
        public static new Npc? Get(ICommandSender sender) => Player.Get(sender) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified Footprint.
        /// </summary>
        /// <param name="footprint">The Footprint to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the Footprint, or <c>null</c> if not found.</returns>
        public static new Npc? Get(Footprint footprint) => Player.Get(footprint) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified CommandSender.
        /// </summary>
        /// <param name="sender">The CommandSender to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the CommandSender, or <c>null</c> if not found.</returns>
        public static new Npc? Get(CommandSender sender) => Player.Get(sender) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified Collider.
        /// </summary>
        /// <param name="collider">The Collider to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the Collider, or <c>null</c> if not found.</returns>
        public static new Npc? Get(Collider collider) => Player.Get(collider) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified net ID.
        /// </summary>
        /// <param name="netId">The net ID to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the net ID, or <c>null</c> if not found.</returns>
        public static new Npc? Get(uint netId) => Player.Get(netId) as Npc;

        /// <summary>
        /// Retrieves the NPC associated with the specified NetworkConnection.
        /// </summary>
        /// <param name="conn">The NetworkConnection to retrieve the NPC for.</param>
        /// <returns>The NPC associated with the NetworkConnection, or <c>null</c> if not found.</returns>
        public static new Npc? Get(NetworkConnection conn) => Player.Get(conn) as Npc;

        [Obsolete("Use other overload", true)]
        public static Npc Spawn(string name, RoleTypeId role, int id, string userId, Vector3? position = null) => Spawn(name, role, position);

        /// <summary>
        /// Spawns an NPC based on the given parameters.
        /// </summary>
        /// <param name="name">The name of the NPC.</param>
        /// <param name="role">The RoleTypeId of the NPC.</param>
        /// <param name="position">The position to spawn the NPC.</param>
        /// <param name="destroyOnDeath">Will this NPC be deleted on death.</param>
        /// <returns>The <see cref="Npc"/> spawned.</returns>
        public static Npc Spawn(string name, RoleTypeId role, Vector3? position = null, bool destroyOnDeath = true)
        {
            GameObject newObject = Object.Instantiate(NetworkManager.singleton.playerPrefab);
            Npc npc = new(newObject)
            {
                IsVerified = false,
                IsNPC = true,
            };
            ReferenceHub referenceHub = npc.ReferenceHub;
            RecyclablePlayerId recyclablePlayerId = new RecyclablePlayerId(false);
            if (Player.List.Any(x => x.Id == recyclablePlayerId.Value))
                Log.Error($"Spawned NPC {name} with id unavailabale: {recyclablePlayerId.Value}");
            referenceHub._playerId = recyclablePlayerId;
            NetworkServer.AddPlayerForConnection(new FakeConnection(recyclablePlayerId.Value + 300), newObject);
            try
            {
                referenceHub.authManager.InstanceMode = ClientInstanceMode.DedicatedServer;
                referenceHub.roleManager.InitializeNewRole(RoleTypeId.None, RoleChangeReason.None);
                referenceHub.nicknameSync.Network_myNickSync = name;
            }
            catch (Exception e)
            {
                Log.Warn($"[{nameof(Npc)}.{nameof(Spawn)}] Ignore: {e}");
            }

            Dictionary.Add(newObject, npc);
            if (destroyOnDeath)
                ToDestroyOnDeath.Add(npc);

            Timing.CallDelayed(
                0.3f,
                () =>
                {
                    npc.Role.Set(role, SpawnReason.RoundStart, position is null ? RoleSpawnFlags.All : RoleSpawnFlags.AssignInventory);
                    if (position.HasValue)
                        npc.Position = position.Value;
                });

            return npc;
        }

        /// <summary>
        /// Destroys the NPC.
        /// </summary>
        public void Destroy()
        {
            NetworkConnectionToClient conn = ReferenceHub.connectionToClient;
            ReferenceHub.OnDestroy();
            CustomNetworkManager.TypedSingleton.OnServerDisconnect(conn);
            Dictionary.Remove(GameObject);
            Object.Destroy(GameObject);
        }

        /// <summary>
        /// Makes the NPC look at the specified position.
        /// </summary>a
        /// <param name="position">The position to look at.</param>
        public void LookAt(Vector3 position)
        {
            if (RoleManager.CurrentRole is IFpcRole fpc)
                fpc.LookAtPoint(position);
        }
    }
}