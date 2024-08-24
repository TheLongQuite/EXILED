// -----------------------------------------------------------------------
// <copyright file="IAppearancedRole.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Roles
{
    using System;
    using System.Collections.Generic;

    using Mirror;

    using PlayerRoles;

    /// <summary>
    /// Represents a role that supports a fake appearance.
    /// </summary>
    public interface IAppearancedRole
    {
        /// <summary>
        /// Gets an overriden global <see cref="RoleTypeId"/> appearance.
        /// </summary>
        public RoleTypeId GlobalAppearance { get; }

        /// <summary>
        /// Gets an overriden <see cref="RoleTypeId"/> appearance for specific <see cref="Team"/>'s.
        /// </summary>
        public IReadOnlyDictionary<Team, RoleTypeId> TeamAppearances { get; }

        /// <summary>
        /// Gets an overriden <see cref="RoleTypeId"/> appearance for specific <see cref="Player"/>'s.
        /// </summary>
        public IReadOnlyDictionary<Player, RoleTypeId> IndividualAppearances { get; }

        /// <summary>
        /// Try-set a new global appearance for current <see cref="IAppearancedRole"/>.
        /// </summary>
        /// <param name="fakeAppearance">New <see cref="RoleTypeId"/>.</param>
        /// <param name="update">Whether or not the change-role requect should sent imidiately.</param>
        /// <returns>A boolean indicating whether or not a target <see cref="RoleTypeId"/> can be used as new appearance.</returns>
        public bool TrySetGlobalAppearance(RoleTypeId fakeAppearance, bool update = true);

        /// <summary>
        /// Try-set a new team appearance for current <see cref="IAppearancedRole"/>.
        /// </summary>
        /// <param name="team">Target <see cref="Team"/>.</param>
        /// <param name="fakeAppearance">New <see cref="RoleTypeId"/>.</param>
        /// <param name="update">Whether or not the change-role requect should sent imidiately.</param>
        /// <returns>A boolean indicating whether or not a target <see cref="RoleTypeId"/> can be used as new appearance.</returns>
        public bool TrySetTeamAppearance(Team team, RoleTypeId fakeAppearance, bool update = true);

        /// <summary>
        /// Try-set a new individual appearance for current <see cref="IAppearancedRole"/>.
        /// </summary>
        /// <param name="player">Target <see cref="Player"/>.</param>
        /// <param name="fakeAppearance">New <see cref="RoleTypeId"/>.</param>
        /// <param name="update">Whether or not the change-role requect should sent imidiately.</param>
        /// <returns>A boolean indicating whether or not a target <see cref="RoleTypeId"/> can be used as new appearance.</returns>
        public bool TrySetIndividualAppearance(Player player, RoleTypeId fakeAppearance, bool update = true);

        /// <summary>
        /// resets <see cref="GlobalAppearance"/> to current <see cref="Role.Type"/>.
        /// </summary>
        /// <param name="update">Whether or not the change-role requect should sent imidiately.</param>
        public void ClearGlobalAppearance(bool update = true);

        /// <summary>
        /// Clears all custom <see cref="TeamAppearances"/>.
        /// </summary>
        /// <param name="update">Whether or not the change-role requect should sent imidiately.</param>
        public void ClearTeamAppearances(bool update = true);

        /// <summary>
        /// Clears all custom <see cref="IndividualAppearances"/>.
        /// </summary>
        /// <param name="update">Whether or not the change-role requect should sent imidiately.</param>
        public void ClearIndividualAppearances(bool update = true);

        /// <summary>
        /// Checks compatibility for target <see cref="RoleTypeId"/> appearance using <see cref="PlayerRoleBase"/>.
        /// </summary>
        /// <param name="fakeAppearance">New <see cref="RoleTypeId"/>.</param>
        /// <param name="roleBase">Target <see cref="PlayerRoleBase"/>, main class for target <see cref="RoleTypeId"/>.</param>
        /// <returns>A boolean indicating whether or not a target <see cref="RoleTypeId"/> can be used as new appearance.</returns>
        public bool CheckAppearanceCompatibility(RoleTypeId fakeAppearance, PlayerRoleBase roleBase);

        /// <summary>
        /// Overrides change role sever message, to implement <see cref="IAppearancedRole"/>, using basic <see cref="PlayerRoleBase"/>.
        /// </summary>
        /// <param name="writer"><see cref="NetworkWriter"/> to write message.</param>
        /// <param name="basicRole">Target <see cref="PlayerRoleBase"/>.</param>
        /// <remarks>Not for public usage. Called on fake <see cref="IAppearancedRole"/> class, not on real <see cref="Role"/> class.</remarks>
        void SendAppearanceSpawnMessage(NetworkWriter writer, PlayerRoleBase basicRole);

        /// <summary>
        /// Resets current appearance to a real player <see cref="RoleTypeId"/>.
        /// </summary>
        /// <param name="update">Whether or not the change-role requect should sent imidiately.</param>\
        /// <remarks>Also clears <see cref="IndividualAppearances"/>.</remarks>
        public void ResetAppearance(bool update = true);

        /// <summary>
        /// Updates current player visibility.
        /// </summary>
        public void UpdateAppearance();

        /// <summary>
        /// Updates current player visibility, for target <see cref="Player"/>.
        /// </summary>
        /// <param name="player">Target <see cref="Player"/>.</param>
        public void UpdateAppearanceFor(Player player);
    }
}
