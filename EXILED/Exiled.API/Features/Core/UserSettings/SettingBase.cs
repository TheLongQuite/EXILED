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
    using Extensions;
    using global::UserSettings.ServerSpecific;
    using Pools;

    /// <summary>
    /// A base class for all Server Specific Settings.
    /// </summary>
    public class SettingBase : TypeCastObject<SettingBase>, IWrapper<ServerSpecificSettingBase>
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> that contains <see cref="SettingBase"/> that are currently with players.
        /// </summary>
        public static readonly Dictionary<Player, List<SettingBase>> PlayerSettings = new();

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
            Log.Info($"Добавляем новую настройку {this}");
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
                Log.Info($"Настройка {this} уже существовала, заменяем");
                Header = OriginalDefinition.Header;
                OnTriggered = OriginalDefinition.OnTriggered;
                Label = OriginalDefinition.Label;
                HintDescription = OriginalDefinition.HintDescription;
                return;
            }

            Log.Info($"Добавляем новую настройку {this}");
            Settings.Add(settingBase.SettingId, this);
        }

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
        /// Gets or sets the action to be executed when this setting is triggered.
        /// </summary>
        public Action<Player, SettingBase> OnTriggered { get; set; }

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

            if (!PlayerSettings.TryGetValue(player, out List<SettingBase> list))
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
            {
                AddAndSendToPlayer(player, collection, predicate);
            }
        }

        /// <summary>
        /// Syncs setting with all players according to the specified predicate.
        /// </summary>
        /// <param name="setting">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void AddAndSendToAll(SettingBase setting, Func<Player, bool> predicate = null)
        {
            foreach (Player player in Player.List)
            {
                AddAndSendToPlayer(player, setting, predicate);
            }
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
            {
                Log.Info("Не прокнул предикат, умираем");
                return;
            }

            if (PlayerSettings.TryGetValue(player, out List<SettingBase> list))
            {
                Log.Info("Получилось достать список, добавляем туда");
                Log.Info(setting.ToString());
                list.Add(setting);
            }
            else
            {
                Log.Info("Не достали список, создаём новый");
                list = new List<SettingBase> { setting };
                PlayerSettings.Add(player, list);
            }

            if (setting is not HeaderSetting)
            {
                Log.Info($"Интегрируем {setting}");
                ServerSpecificSettingsSync.ServerOnSettingValueReceived += setting.OnRandomSettingTriggered;
            }

            ServerSpecificSettingsSync.SendToPlayer(player.ReferenceHub, SortByHeaders(list).Select(x => x.Base).ToArray());
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
            {
                Log.Info("Не прокнул предикат, умираем");
                return;
            }

            if (PlayerSettings.TryGetValue(player, out List<SettingBase> list))
            {
                Log.Info("Получилось достать список, добавляем туда");
                Log.Info(collection.ToString(true));
                list.AddRange(collection);
            }
            else
            {
                Log.Info("Не достали список, создаём новый");
                list = collection;
                PlayerSettings.Add(player, list);
            }

            foreach (SettingBase setting in collection.Where(x => x is not HeaderSetting))
            {
                Log.Info($"Интегрируем {setting}");
                ServerSpecificSettingsSync.ServerOnSettingValueReceived += setting.OnRandomSettingTriggered;
            }

            ServerSpecificSettingsSync.SendToPlayer(player.ReferenceHub, SortByHeaders(list).Select(x => x.Base).ToArray());
        }

        /// <summary>
        /// Syncs setting with all players according to the specified predicate.
        /// </summary>
        /// <param name="collection">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void RemoveAndSendToAll(List<SettingBase> collection, Func<Player, bool> predicate = null)
        {
            foreach (Player player in Player.List)
            {
                RemoveAndSendToPlayer(player, collection, predicate);
            }
        }

        /// <summary>
        /// Syncs setting with all players according to the specified predicate.
        /// </summary>
        /// <param name="setting">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void RemoveAndSendToAll(SettingBase setting, Func<Player, bool> predicate = null)
        {
            foreach (Player player in Player.List)
            {
                RemoveAndSendToPlayer(player, setting, predicate);
            }
        }

        /// <summary>
        /// Syncs setting with the specified target.
        /// </summary>
        /// <param name="player">Target player.</param>
        /// <param name="collection">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void RemoveAndSendToPlayer(Player player, List<SettingBase> collection, Func<Player, bool> predicate = null)
        {
            if (predicate != null && !predicate(player))
            {
                Log.Info("Не прокнул предикат, умираем");
                return;
            }

            if (!PlayerSettings.TryGetValue(player, out List<SettingBase> list))
            {
                Log.Info("Не достали список, умираем");
                return;
            }

            Log.Info("Список до удаления");
            Log.Info(list.ToString(true));
            list.RemoveAll(collection.Contains);
            Log.Info("Список после удаления:");
            Log.Info(list.ToString(true));
            foreach (SettingBase setting in collection.Where(x => x is not HeaderSetting))
            {
                Log.Info($"Удаляем {setting}");
                ServerSpecificSettingsSync.ServerOnSettingValueReceived -= setting.OnRandomSettingTriggered;
            }

            ServerSpecificSettingsSync.SendToPlayer(player.ReferenceHub, SortByHeaders(list).Select(x => x.Base).ToArray());
        }

        /// <summary>
        /// Syncs setting with the specified target.
        /// </summary>
        /// <param name="player">Target player.</param>
        /// <param name="setting">A collection to send.</param>
        /// <param name="predicate">A requirement to meet.</param>
        public static void RemoveAndSendToPlayer(Player player, SettingBase setting, Func<Player, bool> predicate = null)
        {
            if (predicate != null && !predicate(player))
            {
                Log.Info("Не прокнул предикат, умираем");
                return;
            }

            if (!PlayerSettings.TryGetValue(player, out List<SettingBase> list))
            {
                Log.Info("Не достали список, умираем");
                return;
            }

            Log.Info("Список до удаления");
            Log.Info(list.ToString(true));
            list.Remove(setting);
            Log.Info("Список после удаления:");
            Log.Info(list.ToString(true));
            if (setting is not HeaderSetting)
            {
                Log.Info($"Удаляем {setting}");
                ServerSpecificSettingsSync.ServerOnSettingValueReceived -= setting.OnRandomSettingTriggered;
            }

            ServerSpecificSettingsSync.SendToPlayer(player.ReferenceHub, SortByHeaders(list).Select(x => x.Base).ToArray());
        }

        /// <summary>
        /// Registers all settings from the specified collection.
        /// </summary>
        /// <param name="settings">A collection of settings to register.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="SettingBase"/> instances that were successfully registered.</returns>
        /// <remarks>This method is used to sync new settings with players.</remarks>
        public static IEnumerable<SettingBase> SortByHeaders(IEnumerable<SettingBase> settings)
        {
            List<SettingBase> list = ListPool<SettingBase>.Pool.Get(settings.Where(x => x != null));
            List<SettingBase> list2 = new(list.Count);

            while (list.Exists(x => x.Header != null))
            {
                SettingBase setting = list.Find(x => x.Header != null);
                SettingBase header = list.Find(x => x == setting.Header);
                List<SettingBase> range = list.FindAll(x => x.Header?.Id == setting.Header.Id);

                list2.Add(header);
                list2.AddRange(range);

                list.Remove(header);
                list.RemoveAll(x => x.Header?.Id == setting.Header.Id);
            }

            list2.AddRange(list);
            ListPool<SettingBase>.Pool.Return(list);
            return list2;
        }

        /// <summary>
        /// Penis Penis Penis.
        /// </summary>
        /// <param name="referenceHub"> f.</param>
        /// <param name="settingBase"> fuck you man.</param>
        public void OnRandomSettingTriggered(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (!Player.TryGet(referenceHub, out Player player))
            {
                Log.Info("Игрока нет, умираем");
                return;
            }

            if (!Settings.TryGetValue(settingBase.SettingId, out SettingBase setting))
            {
                Log.Info("Обёртки нет, умираем");
                return;
            }

            if (setting != this)
            {
                Log.Info("Не та настройка, умираем");
                return;
            }

            OnTriggered(player, setting);
        }

        /// <summary>
        /// Returns a string representation of this <see cref="SettingBase"/>.
        /// </summary>
        /// <returns>A string in human-readable format.</returns>
        public override string ToString() => $"{Id} ({Label}) [{HintDescription}] {{{ResponseMode}}} ^{Header}^";
    }
}