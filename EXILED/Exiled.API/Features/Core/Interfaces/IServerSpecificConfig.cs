// -----------------------------------------------------------------------
// <copyright file="IServerSpecificConfig.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Core.Interfaces
{
    using UserSettings;

    /// <summary>
    /// Represents a config of ServerSpecific keybinds.
    /// </summary>
    public interface IServerSpecificConfig
    {
        /// <summary>
        /// Creating a SettingBase Instanse.
        /// </summary>
        /// <returns>SettingBase.</returns>
        public abstract SettingBase Create();
    }
}