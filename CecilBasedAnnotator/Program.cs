// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CecilBasedAnnotator
{
    using System.Collections.Immutable;

    internal class Program
    {
        private static void Main(string[] args)
        {
            TunnelVisionLabs.ReferenceAssemblyAnnotator.Program.Main(
                log: null,
                referenceAssembly: args[0],
                targetFrameworkDirectories: args[1].Split(';').ToImmutableArray(),
                annotatedReferenceAssembly: args[2],
                outputAssembly: args[3]);
        }
    }
}
