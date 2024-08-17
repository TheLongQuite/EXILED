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
        /// Try-set a new global appearance for current <see cref="IAppearancedRole"/>.
        /// </summary>
        /// <param name="player">Target <see cref="Player"/>.</param>
        /// <param name="fakeAppearance">New <see cref="RoleTypeId"/>.</param>
        /// <param name="update">Whether or not the change-role requect should sent imidiately.</param>
        /// <returns>A boolean indicating whether or not a target <see cref="RoleTypeId"/> can be used as new appearance.</returns>
        public bool TrySetIndividualAppearance(Player player, RoleTypeId fakeAppearance, bool update = true);

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
        /// Resets current appearance to a real player <see cref="RoleTypeId"/>.
        /// </summary>
        /// <param name="update">Whether or not the change-role requect should sent imidiately.</param>\
        /// <remarks>Also clears <see cref="IndividualAppearances"/>.</remarks>
        public void ResetAppearance(bool update = true);

        /// <summary>
        /// Updates current player visibility.
        /// </summary>
        public void UpdateAppearance();
    }
}
