// -----------------------------------------------------------------------
// <copyright file="CommonExtensions.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Exiled.API.Features;
    using PlayerRoles;

    using UnityEngine;

    /// <summary>
    /// A set of extensions for common things.
    /// </summary>
    public static class CommonExtensions
    {
        /// <summary>
        /// Gets a random value from an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="enumerable"><see cref="IEnumerable{T}"/> to be used to get a random value.</param>
        /// <typeparam name="T">Type of <see cref="IEnumerable{T}"/> elements.</typeparam>
        /// <returns>Returns a random value from <see cref="IEnumerable{T}"/>.</returns>
        public static T GetRandomValue<T>(this IEnumerable<T> enumerable) => enumerable is null || enumerable.Count() == 0 ? default : enumerable.ElementAt(UnityEngine.Random.Range(0, enumerable.Count()));

        /// <summary>
        /// Gets a random value from an <see cref="IEnumerable{T}"/> that matches the provided condition.
        /// </summary>
        /// <param name="enumerable"><see cref="IEnumerable{T}"/> to be used to get a random value.</param>
        /// <typeparam name="T">Type of <see cref="IEnumerable{T}"/> elements.</typeparam>
        /// <param name="condition">The condition to require.</param>
        /// <returns>Returns a random value from <see cref="IEnumerable{T}"/>.</returns>
        public static T GetRandomValue<T>(this IEnumerable<T> enumerable, System.Func<T, bool> condition) => enumerable is null || enumerable.Count() == 0 ? default : enumerable.Where(condition).GetRandomValue();

        /// <summary>
        /// Adds an action for OnCollisionEnter event of specified gameObject.
        /// </summary>
        /// <param name="gameObject">GameObject to attach action.</param>
        /// <param name="action">Action on collision.</param>
        /// <param name="owner">GameObject that will be ignored in collision.</param>
        /// <param name="fuseDelay">Delay before collision may be proceeded.</param>
        public static void AttachActionOnCollision(this GameObject gameObject, Action action, Player owner = null, float fuseDelay = 0.15f)
        {
            gameObject.AddComponent<Exiled.API.Features.Components.CollisionHandler>().Init((owner ?? Server.Host).GameObject, action, fuseDelay);
        }

        /// <summary>
        /// Заменяет вспомогательные теги в тексте (в основном для кесси).
        /// </summary>
        /// <param name="text">Искомый текст.</param>
        /// <returns>Новый текст.</returns>
        public static string ReplaceVars(this string text) => text
            .Replace("{classd}", Player.List.Count(x => x.Role.Team == Team.ClassD).ToString())
            .Replace("{scps}", Player.List.Count(x => x.Role.Team == Team.SCPs).ToString())
            .Replace("{scpsno079}", Player.List.Count(x => x.Role.Team == Team.SCPs && x.Role.Type != RoleTypeId.Scp079).ToString())
            .Replace("{scientists}", Player.List.Count(x => x.Role.Team == Team.Scientists).ToString())
            .Replace("{mtf}", Player.List.Count(x => x.Role.Team == Team.FoundationForces).ToString())
            .Replace("{ci}", Player.List.Count(x => x.Role.Team == Team.ChaosInsurgency).ToString())
            .Replace("{human}", Player.List.Count(x => x.IsHuman).ToString());
    }
}