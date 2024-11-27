// -----------------------------------------------------------------------
// <copyright file="KickingEventArgs.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Player
{
    using System.Reflection;

    using API.Features;

    using CommandSystem;

    using Interfaces;

    /// <summary>
    ///     Contains all information before kicking a player from the server.
    /// </summary>
    public class KickingEventArgs : IPlayerEvent, IDeniableEvent
    {
        private bool isAllowed;
        private Player target;
        private ICommandSender sender;

        /// <summary>
        ///     Initializes a new instance of the <see cref="KickingEventArgs" /> class.
        /// </summary>
        /// <param name="target">
        ///     <inheritdoc cref="Target" />
        /// </param>
        /// <param name="issuer">
        ///     <inheritdoc cref="Player" />
        /// </param>
        /// <param name="reason">
        ///     <inheritdoc cref="Reason" />
        /// </param>
        /// <param name="fullMessage">
        ///     <inheritdoc cref="FullMessage" />
        /// </param>
        /// <param name="isAllowed">
        ///     <inheritdoc cref="IsAllowed" />
        /// </param>
        public KickingEventArgs(Player target, ICommandSender issuer, string reason, string fullMessage, bool isAllowed = true)
        {
            Target = target;
            Sender = issuer;

            Player = Player.Get(issuer) ?? Server.Host;
            Reason = reason;
            FullMessage = fullMessage;
            IsAllowed = isAllowed;
        }

        /// <summary>
        ///     Gets or sets the ban target.
        /// </summary>
        public Player Target
        {
            get => target;
            set
            {
                if (value is null || target == value)
                    return;

                if (Events.Instance.Config.ShouldLogBans && target is not null)
                    LogBanChange(Assembly.GetCallingAssembly().GetName().Name, $" changed the banned player from user {target.Nickname} ({target.UserId}) to {value.Nickname} ({value.UserId})");

                target = value;
            }
        }

        /// <summary>
        ///     Gets or sets the kick reason.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        ///     Gets or sets the full kick message.
        /// </summary>
        public string FullMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether action is taken against the target.
        /// </summary>
        public bool IsAllowed
        {
            get => isAllowed;
            set
            {
                if (isAllowed == value)
                    return;

                if (Events.Instance.Config.ShouldLogBans)
                    LogBanChange(Assembly.GetCallingAssembly().GetName().Name, $" {(value ? "allowed" : "denied")} banning user with ID: {Target.UserId}");

                isAllowed = value;
            }
        }

        /// <summary>
        ///     Gets the ban issuer.
        /// </summary>
        public Player Player { get; }

        /// <summary>
        ///     Gets or sets the ban issuer.
        /// </summary>
        public ICommandSender Sender
        {
            get => sender;
            set
            {
                if (value is null || sender == value)
                    return;

                if (Events.Instance.Config.ShouldLogBans && sender is not null)
                    LogBanChange(Assembly.GetCallingAssembly().GetName().Name, $" changed the ban sender from user {sender.LogName} to {value.LogName}");

                sender = value;
            }
        }

        /// <summary>
        /// Logs the kick, anti-backdoor protection from malicious plugins.
        /// </summary>
        /// <param name="assemblyName">The name of the calling assembly.</param>
        /// <param name="message">The message to be logged.</param>
        protected void LogBanChange(string assemblyName, string message)
        {
            if (assemblyName != "Exiled.Events")
            {
                lock (ServerLogs.LockObject)
                {
                    Log.Warn($"[ANTI-BACKDOOR]: {assemblyName} {message} - {TimeBehaviour.FormatTime("yyyy-MM-dd HH:mm:ss.fff zzz")}");
                }
            }

            ServerLogs._state = ServerLogs.LoggingState.Write;
        }
    }
}
