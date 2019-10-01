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

        public SuppressibleLoggingHelper(TaskLoggingHelper helper, string? noWarn)
        {
            Helper = helper;

            _noWarn = noWarn
                ?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => item.Length != 0)
                .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase) ?? ImmutableHashSet<string>.Empty;
        }

        public TaskLoggingHelper Helper { get; }

        public void LogWarning(string warningCode, string message, params object?[] messageArgs)
        {
            if (_noWarn.Contains(warningCode))
                return;

            Helper.LogWarning(subcategory: null, warningCode, helpKeyword: null, file: null, lineNumber: 0, columnNumber: 0, endLineNumber: 0, endColumnNumber: 0, message, messageArgs);
        }
    }
}
