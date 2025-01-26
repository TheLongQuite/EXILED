// -----------------------------------------------------------------------
// <copyright file="KeybindSetting.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Core.UserSettings
{
    using System;

    using Exiled.API.Interfaces;
    using global::UserSettings.ServerSpecific;
    using UnityEngine;

    /// <summary>
    /// Represents a keybind setting.
    /// </summary>
    public class KeybindSetting : SettingBase, IWrapper<SSKeybindSetting>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeybindSetting"/> class.
        /// </summary>
        /// <param name="label"><inheritdoc cref="SettingBase.Label"/></param>
        /// <param name="suggested"><inheritdoc cref="KeyCode"/></param>
        /// <param name="preventInteractionOnGUI"><inheritdoc cref="PreventInteractionOnGUI"/></param>
        /// <param name="hintDescription"><inheritdoc cref="SettingBase.HintDescription"/></param>
        /// <param name="header"><inheritdoc cref="SettingBase.Header"/></param>
        public KeybindSetting(string label, KeyCode suggested, bool preventInteractionOnGUI = false, string hintDescription = "", HeaderSetting header = null)
            : base(new SSKeybindSetting(NextId++, label, suggested, preventInteractionOnGUI, hintDescription), header)
        {
            Base = (SSKeybindSetting)base.Base;
            Log.Info($"Добавляем новую настройку {ToString()}");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeybindSetting"/> class.
        /// </summary>
        /// <param name="settingBase">A <see cref="SSKeybindSetting"/> instance.</param>
        internal KeybindSetting(SSKeybindSetting settingBase)
            : base(settingBase)
        {
            Base = settingBase;
            Log.Info($"Добавляем новую настройку {ToString()}");
        }

        /// <inheritdoc/>
        public new SSKeybindSetting Base { get; }

        /// <summary>
        /// Gets a value indicating whether the key is pressed.
        /// </summary>
        public bool IsPressed => Base.SyncIsPressed;

        /// <summary>
        /// Gets or sets a value indicating whether the interaction is prevented while player is in RA, Settings etc.
        /// </summary>
        public bool PreventInteractionOnGUI
        {
            get => Base.PreventInteractionOnGUI;
            set => Base.PreventInteractionOnGUI = value;
        }

        /// <summary>
        /// Gets or sets the assigned key.
        /// </summary>
        public KeyCode KeyCode
        {
            get => Base.SuggestedKey;
            set => Base.SuggestedKey = value;
        }

        /// <summary>
        /// Returns a representation of this <see cref="KeybindSetting"/>.
        /// </summary>
        /// <returns>A string in human-readable format.</returns>
        public override string ToString() => base.ToString() + $" /{Label}/ *{KeyCode}* +{PreventInteractionOnGUI}+";
    }
}