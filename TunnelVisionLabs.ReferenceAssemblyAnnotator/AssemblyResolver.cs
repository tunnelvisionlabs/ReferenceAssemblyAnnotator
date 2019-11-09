// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System.Collections.Generic;
    using System.IO;
    using Mono.Cecil;

    internal sealed class AssemblyResolver : IAssemblyResolver
    {
        private static readonly ReaderParameters DefaultReaderParameters = new ReaderParameters(ReadingMode.Deferred);

        private readonly string _searchDirectory;
        private readonly Dictionary<string, AssemblyDefinition?> _assembliesByFileName = new Dictionary<string, AssemblyDefinition?>();

        public AssemblyResolver(string searchDirectory)
        {
            _searchDirectory = searchDirectory;
        }

        public AssemblyDefinition? Resolve(AssemblyNameReference name)
        {
            return Resolve(name, DefaultReaderParameters);
        }

        public AssemblyDefinition? Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (!_assembliesByFileName.TryGetValue(name.Name, out var assembly))
            {
                var stream = OpenReadIfExists(Path.Combine(_searchDirectory, name.Name + ".dll"));

                assembly = stream is null ? null : AssemblyDefinition.ReadAssembly(stream, parameters);

                _assembliesByFileName.Add(name.Name, assembly);
            }

            return assembly is object && Matches(assembly.Name, name) ? assembly : null;
        }

        private static FileStream? OpenReadIfExists(string path)
        {
            try
            {
                return File.OpenRead(path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// The point of this method is to be a high-performance sanity check, not to reproduce .NET assembly loading
        /// behavior.
        /// </summary>
        private static bool Matches(AssemblyNameDefinition definition, AssemblyNameReference reference)
        {
            return definition.Name == reference.Name
                && definition.Version == reference.Version;
        }

        public void Dispose()
        {
            foreach (var loadedAssembly in _assembliesByFileName.Values)
            {
                loadedAssembly?.Dispose();
            }

            _assembliesByFileName.Clear();
        }
    }
}
