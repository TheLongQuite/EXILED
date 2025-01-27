// -----------------------------------------------------------------------
// <copyright file="SettingBase.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Core.UserSettings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Exiled.API.Interfaces;
    using global::UserSettings.ServerSpecific;

    /// <summary>
    /// A base class for all Server Specific Settings.
    /// </summary>
    public class SettingBase : TypeCastObject<SettingBase>, IWrapper<ServerSpecificSettingBase>
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> that contains <see cref="SettingBase"/> that are currently with players.
        /// </summary>
        public static readonly Dictionary<Player, HashSet<SettingBase>> PlayerSettings = new();

        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> that contains <see cref="int"/> Id and <see cref="SettingBase"/> with this Id.
        /// </summary>
        public static readonly Dictionary<int, SettingBase> Settings = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingBase"/> class.
        /// </summary>
        /// <param name="settingBase">A <see cref="ServerSpecificSettingBase"/> instance.</param>
        /// <param name="header"><inheritdoc cref="Header"/></param>
        internal SettingBase(ServerSpecificSettingBase settingBase, HeaderSetting header)
        {
            Base = settingBase;
            Header = header;
            Settings.Add(settingBase.SettingId, this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingBase"/> class.
        /// </summary>
        /// <param name="settingBase"><inheritdoc cref="Base"/></param>
        internal SettingBase(ServerSpecificSettingBase settingBase)
        {
            Base = settingBase;

            if (OriginalDefinition != null)
            {
                Header = OriginalDefinition.Header;
                OnTriggered = OriginalDefinition.OnTriggered;
                Label = OriginalDefinition.Label;
                HintDescription = OriginalDefinition.HintDescription;
                return;
            }

            Settings.Add(settingBase.SettingId, this);
        }

        /// <summary>
        /// Gets or sets the action to be executed when this setting is triggered.
        /// </summary>
        public event Action<Player, SettingBase> OnTriggered;

        /// <summary>
        /// Gets or sets next Id to give.
        /// </summary>
        public static int NextId { get; set; }

        /// <inheritdoc/>
        public ServerSpecificSettingBase Base { get; }

        /// <summary>
        /// Gets or sets the id of this setting.
        /// </summary>
        public int Id
        {
            get => Base.SettingId;
            set => Base.SetId(value, string.Empty);
        }

        /// <summary>
        /// Gets or sets the label of this setting.
        /// </summary>
        public string Label
        {
            get => Base.Label;
            set => Base.Label = value;
        }

        /// <summary>
        /// Gets or sets the description of this setting.
        /// </summary>
        public string HintDescription
        {
            get => Base.HintDescription;
            set => Base.HintDescription = value;
        }

        /// <summary>
        /// Gets the response mode of this setting.
        /// </summary>
        public ServerSpecificSettingBase.UserResponseMode ResponseMode => Base.ResponseMode;

        /// <summary>
        /// Gets the setting that was sent to players.
        /// </summary>
        /// <remarks>Can be <c>null</c> if this <see cref="SettingBase"/> is a prefab.</remarks>
        public SettingBase OriginalDefinition => Settings.TryGetValue(Id, out SettingBase setting) ? setting : null;

        /// <summary>
        /// Gets or sets the header of this setting.
        /// </summary>
        /// <remarks>Can be <c>null</c>.</remarks>
        public HeaderSetting Header { get; set; }

        /// <summary>
        /// Gets or sets the predicate for syncing this setting when a player joins.
        /// </summary>
        public Predicate<Player> SyncOnJoin { get; set; }

        /// <summary>
        /// Tries to get the setting with the specified id.
        /// </summary>
        /// <param name="player">Player who has received the setting.</param>
        /// <param name="id">Id of the setting.</param>
        /// <param name="setting">A <see cref="SettingBase"/> instance if found. Otherwise, <c>null</c>.</param>
        /// <typeparam name="T">Type of the setting.</typeparam>
        /// <returns><c>true</c> if the setting was found, <c>false</c> otherwise.</returns>
        public static bool TryGetSetting<T>(Player player, int id, out T setting)
            where T : SettingBase
        {
            setting = null;

            if (!PlayerSettings.TryGetValue(player, out HashSet<SettingBase> list))
                return false;

            setting = (T)list.FirstOrDefault(x => x.Id == id);
            return setting != null;
        }

        /// <summary>
        /// Creates a new instance of this setting.
        /// </summary>
        /// <param name="settingBase">A <see cref="ServerSpecificSettingBase"/> instance.</param>
        /// <returns>A new instance of this setting.</returns>
        /// <remarks>
        /// This method is used only to create a new instance of <see cref="SettingBase"/> from an existing <see cref="ServerSpecificSettingBase"/> instance.
        /// New setting won't be synced with players.
        /// </remarks>
        public static SettingBase Create(ServerSpecificSettingBase settingBase) => settingBase switch
        {
            SSButton button => new ButtonSetting(button),
            SSDropdownSetting dropdownSetting => new DropdownSetting(dropdownSetting),
            SSTextArea textArea => new TextInputSetting(textArea),
            SSGroupHeader header => new HeaderSetting(header),
            SSKeybindSetting keybindSetting => new KeybindSetting(keybindSetting),
            SSTwoButtonsSetting twoButtonsSetting => new TwoButtonsSetting(twoButtonsSetting),
            _ => new SettingBase(settingBase)
        };

        /// <summary>
        /// Creates a new instance of this setting.
        /// </summary>
        /// <param name="settingBase">A<see cref="ServerSpecificSettingBase"/> instance.</param>
        /// <typeparam name="T">Type of the setting.</typeparam>
        /// <returns>A new instance of this setting.</returns>
        /// <remarks>
        /// This method is used only to create a new instance of <see cref="SettingBase"/> from an existing <see cref="ServerSpecificSettingBase"/> instance.
        /// New setting won't be synced with players.
        /// </remarks>
        public static T Create<T>(ServerSpecificSettingBase settingBase)
            where T : SettingBase => (T)Create(settingBase);

        /// <summary>
        /// Syncs setting with all players according to the specified predicate.
        /// </summary>
        /// <param name="collection">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void AddAndSendToAll(List<SettingBase> collection, Func<Player, bool> predicate = null)
        {
            foreach (Player player in Player.List)
                AddAndSendToPlayer(player, collection, predicate);
        }

        /// <summary>
        /// Syncs setting with all players according to the specified predicate.
        /// </summary>
        /// <param name="setting">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void AddAndSendToAll(SettingBase setting, Func<Player, bool> predicate = null)
        {
            foreach (Player player in Player.List)
                AddAndSendToPlayer(player, setting, predicate);
        }

        /// <summary>
        /// Syncs setting with the specified target.
        /// </summary>
        /// <param name="player">Target player.</param>
        /// <param name="setting">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void AddAndSendToPlayer(Player player, SettingBase setting, Func<Player, bool> predicate = null)
        {
            if (predicate != null && !predicate(player))
                return;

            if (PlayerSettings.TryGetValue(player, out HashSet<SettingBase> list))
            {
                Log.Info(setting.ToString());
                list.Add(setting);
            }
            else
            {
                list = new HashSet<SettingBase> { setting };
                PlayerSettings.Add(player, list);
            }

            ServerSpecificSettingsSync.DefinedSettings = GroupByHeaders(list).Select(x => x.Base).ToArray();
            ServerSpecificSettingsSync.SendToPlayer(player.ReferenceHub);
        }

        /// <summary>
        /// Syncs setting with the specified target.
        /// </summary>
        /// <param name="player">Target player.</param>
        /// <param name="collection">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void AddAndSendToPlayer(Player player, List<SettingBase> collection, Func<Player, bool> predicate = null)
        {
            if (predicate != null && !predicate(player))
                return;

            if (PlayerSettings.TryGetValue(player, out HashSet<SettingBase> list))
            {
                list = list.Concat(collection).ToHashSet();
            }
            else
            {
                list = collection.ToHashSet();
                PlayerSettings.Add(player, list);
            }

            ServerSpecificSettingsSync.DefinedSettings = GroupByHeaders(list).Select(x => x.Base).ToArray();
            ServerSpecificSettingsSync.SendToPlayer(player.ReferenceHub);
        }

        /// <summary>
        /// Syncs setting with all players according to the specified predicate.
        /// </summary>
        /// <param name="collection">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void RemoveAndSendToAll(List<SettingBase> collection, Func<Player, bool> predicate = null)
        {
            foreach (Player player in Player.List)
                RemoveAndSendToPlayer(player, collection, predicate);
        }

        /// <summary>
        /// Syncs setting with all players according to the specified predicate.
        /// </summary>
        /// <param name="setting">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void RemoveAndSendToAll(SettingBase setting, Func<Player, bool> predicate = null)
        {
            foreach (Player player in Player.List)
                RemoveAndSendToPlayer(player, setting, predicate);
        }

        /// <summary>
        /// Syncs setting with the specified target.
        /// </summary>
        /// <param name="player">Target player.</param>
        /// <param name="collection">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void RemoveAndSendToPlayer(Player player, List<SettingBase> collection, Func<Player, bool> predicate = null)
        {
            if ((predicate != null && !predicate(player)) || !PlayerSettings.TryGetValue(player, out HashSet<SettingBase> list))
                return;

            list.RemoveWhere(collection.Contains);
            ServerSpecificSettingsSync.DefinedSettings = GroupByHeaders(list).Select(x => x.Base).ToArray();
            ServerSpecificSettingsSync.SendToPlayer(player.ReferenceHub);
        }

        /// <summary>
        /// Syncs setting with the specified target.
        /// </summary>
        /// <param name="player">Target player.</param>
        /// <param name="setting">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void RemoveAndSendToPlayer(Player player, SettingBase setting, Func<Player, bool> predicate = null)
        {
            if ((predicate != null && !predicate(player)) || !PlayerSettings.TryGetValue(player, out HashSet<SettingBase> list))
                return;

            list.Remove(setting);
            ServerSpecificSettingsSync.DefinedSettings = GroupByHeaders(list).Select(x => x.Base).ToArray();
            ServerSpecificSettingsSync.SendToPlayer(player.ReferenceHub);
        }

        /// <summary>
        /// Registers all settings from the specified collection.
        /// </summary>
        /// <param name="settings">A collection of settings to register.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="SettingBase"/> instances that were successfully registered.</returns>
        /// <remarks>This method is used to sync new settings with players.</remarks>
        public static IEnumerable<SettingBase> GroupByHeaders(IEnumerable<SettingBase> settings)
        {
            IEnumerable<IGrouping<HeaderSetting, SettingBase>> grouped = settings.Where(s => s != null).GroupBy(s => s.Header);

            List<SettingBase> result = new();
            foreach (IGrouping<HeaderSetting, SettingBase> grouping in grouped)
            {
                if (grouping.Key != null)
                    result.Add(grouping.Key);

                result.AddRange(grouping);
            }

            return result;
        }

        /// <summary>
        /// Penis Penis Penis.
        /// </summary>
        /// <param name="referenceHub"> f.</param>
        /// <param name="settingBase"> fuck you man.</param>
        public static void OnRandomSettingTriggered(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (!Player.TryGet(referenceHub, out Player player) || !Settings.TryGetValue(settingBase.SettingId, out SettingBase setting))
                return;

            setting.OnTriggered?.Invoke(player, setting);
        }

        /// <summary>
        /// Returns a string representation of this <see cref="SettingBase"/>.
        /// </summary>
        /// <returns>A string in human-readable format.</returns>
        public override string ToString() => $"{Id} ({Label}) [{HintDescription}] {{{ResponseMode}}} ^{Header}^";
    }
}