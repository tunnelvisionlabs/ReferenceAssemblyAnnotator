// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.Build.Utilities;

    internal readonly struct SuppressibleLoggingHelper
    {
        private readonly ImmutableHashSet<string> _noWarn;
        private readonly string _requiredPrefix;

        public SuppressibleLoggingHelper(TaskLoggingHelper helper, string requiredPrefix, string? noWarn)
        {
            if (string.IsNullOrWhiteSpace(requiredPrefix))
                throw new ArgumentException("A required warning prefix must be supplied.", nameof(requiredPrefix));

            _requiredPrefix = requiredPrefix;

            Helper = helper;

            _noWarn = noWarn
                ?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => item.StartsWith(requiredPrefix, StringComparison.OrdinalIgnoreCase))
                .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase) ?? ImmutableHashSet<string>.Empty;
        }

        public TaskLoggingHelper Helper { get; }

        public void LogWarning(string warningCode, string message, params object?[] messageArgs)
        {
            if (!warningCode.StartsWith(_requiredPrefix, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Warning code '{warningCode}' does not begin with the required prefix '{_requiredPrefix}'.", nameof(warningCode));

            if (_noWarn.Contains(warningCode))
                return;

            Helper.LogWarning(subcategory: null, warningCode, helpKeyword: null, file: null, lineNumber: 0, columnNumber: 0, endLineNumber: 0, endColumnNumber: 0, message, messageArgs);
        }
    }
}
