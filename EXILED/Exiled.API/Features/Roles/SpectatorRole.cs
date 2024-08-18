// -----------------------------------------------------------------------
// <copyright file="SpectatorRole.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Roles
{
    using System;
    using System.Collections.Generic;

    using Exiled.API.Extensions;
    using Exiled.API.Features.Pools;
    using Mirror;

    using PlayerRoles;

    using UnityEngine;

    using SpectatorGameRole = PlayerRoles.Spectating.SpectatorRole;

    /// <summary>
    /// Defines a role that represents a spectator.
    /// </summary>
    public class SpectatorRole : Role, IAppearancedRole
    {
        private RoleTypeId fakeAppearance;
        private Dictionary<Player, RoleTypeId> individualAppearances = DictionaryPool<Player, RoleTypeId>.Pool.Get();

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectatorRole"/> class.
        /// </summary>
        /// <param name="baseRole">The encapsulated <see cref="SpectatorGameRole"/>.</param>
        internal SpectatorRole(SpectatorGameRole baseRole)
            : base(baseRole)
        {
            fakeAppearance = baseRole.RoleTypeId;
            Base = baseRole;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SpectatorRole"/> class.
        /// </summary>
        ~SpectatorRole()
        {
            DictionaryPool<Player, RoleTypeId>.Pool.Return(individualAppearances);
        }

        /// <inheritdoc/>
        public override RoleTypeId Type => RoleTypeId.Spectator;

        /// <summary>
        /// Gets the <see cref="DateTime"/> at which the player died.
        /// </summary>
        public DateTime DeathTime => Round.StartedTime + ActiveTime;

        /// <summary>
        /// Gets the total amount of time the player has been dead.
        /// </summary>
        public TimeSpan DeadTime => DateTime.UtcNow - DeathTime;

        /// <summary>
        /// Gets the <see cref="Player"/>'s death position.
        /// </summary>
        public Vector3 DeathPosition => Base.DeathPosition.Position;

        /// <summary>
        /// Gets a value indicating whether the <see cref="Player"/> is ready to respawn or not.
        /// </summary>
        public bool IsReadyToRespawn => Base.ReadyToRespawn;

        /// <summary>
        /// Gets currently spectated <see cref="Player"/> by this <see cref="Player"/>. May be <see langword="null"/>.
        /// </summary>
        public Player SpectatedPlayer
        {
            get
            {
                Player spectatedPlayer = Player.Get(Base.SyncedSpectatedNetId);

                return spectatedPlayer != Owner ? spectatedPlayer : null;
            }
        }

        /// <summary>
        /// Gets the game <see cref="SpectatorGameRole"/>.
        /// </summary>
        public new SpectatorGameRole Base { get; }

        /// <inheritdoc/>
        public RoleTypeId GlobalAppearance => fakeAppearance;

        /// <inheritdoc/>
        public IReadOnlyDictionary<Player, RoleTypeId> IndividualAppearances => individualAppearances;

        /// <inheritdoc/>
        public bool TrySetGlobalAppearance(RoleTypeId fakeRole, bool update = true)
        {
            if (!RoleExtensions.TryGetRoleBase(fakeRole, out PlayerRoleBase roleBase))
                return false;

            if (!CheckAppearanceCompatibility(fakeRole, roleBase))
            {
                Log.Error($"Prevent Seld-Desync of {Owner.Nickname} ({Type}) with {fakeRole}");
                return false;
            }

            fakeAppearance = fakeRole;

            if (update)
            {
                UpdateAppearance();
            }

            return true;
        }

        /// <inheritdoc/>
        public bool TrySetIndividualAppearance(Player player, RoleTypeId fakeRole, bool update = true)
        {
            if (!RoleExtensions.TryGetRoleBase(fakeRole, out PlayerRoleBase roleBase))
                return false;

            if (!CheckAppearanceCompatibility(fakeRole, roleBase))
            {
                Log.Error($"Prevent Seld-Desync of {Owner.Nickname} ({Type}) with {fakeRole}");
                return false;
            }

            individualAppearances[player] = fakeRole;

            if (update)
            {
                UpdateAppearance();
            }

            return true;
        }

        /// <inheritdoc/>
        public void ClearIndividualAppearances(bool update = true)
        {
            individualAppearances.Clear();

            if (update)
            {
                UpdateAppearance();
            }
        }

        /// <inheritdoc/>
        public virtual bool CheckAppearanceCompatibility(RoleTypeId fakeRole, PlayerRoleBase roleBase)
        {
            return roleBase is SpectatorGameRole;
        }

        /// <inheritdoc/>
        public virtual void SendAppearanceSpawnMessage(NetworkWriter writer, PlayerRoleBase basicRole)
        {
        }

        /// <inheritdoc/>
        public void ResetAppearance(bool update = true)
        {
            ClearIndividualAppearances(false);
            fakeAppearance = Type;
            if (update)
            {
                UpdateAppearance();
            }
        }

        /// <inheritdoc/>
        public void UpdateAppearance()
        {
            if (Owner != null)
                Owner.RoleManager._sendNextFrame = true;
        }
    }
}
