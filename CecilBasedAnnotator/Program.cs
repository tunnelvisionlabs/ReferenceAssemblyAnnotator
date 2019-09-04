// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CecilBasedAnnotator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TunnelVisionLabs.ReferenceAssemblyAnnotator.Program.Main(log: null, args[0], args[1], args[2]);
        }
    }
}
