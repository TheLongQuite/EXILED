// -----------------------------------------------------------------------
// <copyright file="AggregateExpectationTypeResolver.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Loader.Features.AbstractClassResolver.Parsers
{
#nullable enable

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Exiled.API.Features;
    using Exiled.API.Interfaces;
    using Extensions;
    using Interfaces;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <inheritdoc />
    public class AggregateExpectationTypeResolver : ITypeDiscriminator
    {
        private const string NamingKey = nameof(IAbstractResolvable.AbilityType);

        private readonly string targetKey;
        private readonly Dictionary<string, Type?> typeLookup;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AggregateExpectationTypeResolver"/> class.
        /// </summary>
        /// <param name="namingConvention">The <see cref="INamingConvention" /> instance.</param>
        /// <param name="type">Abstract type which resolver will resolve.</param>
        public AggregateExpectationTypeResolver(INamingConvention namingConvention, Type type)
        {
            targetKey = namingConvention.Apply(NamingKey);
            typeLookup = new Dictionary<string, Type?>();
            BaseType = type;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type? t in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(type)))
                        typeLookup.Add(t.Name, t);
                }
                catch (Exception e)
                {
                    Log.Error($"Error loading types for {assembly.FullName}.");
                    Log.Debug(e);
                }
            }
        }

        /// <inheritdoc />
        public Type BaseType { get; }

        /// <inheritdoc />
        public bool TryResolve(ParsingEventBuffer buffer, out Type? suggestedType)
        {
            if (buffer.TryFindMappingEntry(
                scalar => targetKey == scalar.Value,
                out Scalar? _,
                out ParsingEvent? value))
            {
                if (value is Scalar valueScalar)
                {
                    suggestedType = CheckName(valueScalar.Value);

                    return true;
                }

                FailEmpty();
            }

            suggestedType = null;
            return false;
        }

        private void FailEmpty()
        {
            throw new Exception($"Could not determine expectation type, {targetKey} has an empty value");
        }

        private Type? CheckName(string value)
        {
            if (typeLookup.TryGetValue(value, out Type? childType))
                return childType;

            string known = string.Join(",", typeLookup.Keys);
            throw new Exception(
                $"Could not match `{targetKey}: {value}` to a known expectation. Expecting one of: {known}");
        }
    }
}