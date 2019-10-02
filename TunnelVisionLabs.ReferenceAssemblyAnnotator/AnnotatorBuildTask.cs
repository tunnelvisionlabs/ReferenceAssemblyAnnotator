// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    public sealed class AnnotatorBuildTask : Task
    {
        public AnnotatorBuildTask()
        {
            // These required properties will all be assigned by MSBuild. Suppress warnings about leaving them with
            // their default values.
            UnannotatedReferenceAssembly = null!;
            TargetFrameworkDirectories = null!;
            AnnotatedReferenceAssemblyDirectory = null!;
            OutputPath = null!;
            NoWarn = null!;
        }

        [Required]
        public ITaskItem UnannotatedReferenceAssembly
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] TargetFrameworkDirectories
        {
            get;
            set;
        }

        [Required]
        public string AnnotatedReferenceAssemblyDirectory
        {
            get;
            set;
        }

        [Required]
        public string OutputPath
        {
            get;
            set;
        }

        [Required]
        public string NoWarn
        {
            get;
            set;
        }

        [Output]
        public ITaskItem[]? GeneratedAssemblies
        {
            get;
            set;
        }

        public override bool Execute()
        {
            var log = new SuppressibleLoggingHelper(Log, requiredPrefix: "RA", NoWarn);

            string unannotatedReferenceAssembly = TargetFrameworkDirectories.Select(path => Path.Combine(path.ItemSpec, UnannotatedReferenceAssembly + ".dll")).FirstOrDefault(File.Exists);
            string annotatedReferenceAssembly = Path.Combine(AnnotatedReferenceAssemblyDirectory, UnannotatedReferenceAssembly + ".dll");
            bool foundAnnotatedAssembly = File.Exists(annotatedReferenceAssembly);

            Log.LogMessage($"Generating reference assembly for {UnannotatedReferenceAssembly}");
            Log.LogMessage($"  Unannotated: {unannotatedReferenceAssembly ?? "Unknown"})");
            Log.LogMessage($"  Annotated:   {annotatedReferenceAssembly}{(foundAnnotatedAssembly ? string.Empty : " (Not found)")})");
            Log.LogMessage($"  Output:      {Path.GetFullPath(OutputPath)}");

            if (unannotatedReferenceAssembly is null)
            {
                Log.LogError($"Could not find input reference assembly '{UnannotatedReferenceAssembly}'");
                return false;
            }

            if (!foundAnnotatedAssembly)
            {
                Log.LogError($"Could not find input annotated reference assembly for '{UnannotatedReferenceAssembly}'");
                return false;
            }

            Directory.CreateDirectory(OutputPath);
            string outputAssembly = Path.Combine(OutputPath, Path.GetFileName(unannotatedReferenceAssembly));
            Program.Main(log, unannotatedReferenceAssembly, annotatedReferenceAssembly, outputAssembly);
            GeneratedAssemblies = new[] { new TaskItem(outputAssembly) };

            return true;
        }
    }
}
