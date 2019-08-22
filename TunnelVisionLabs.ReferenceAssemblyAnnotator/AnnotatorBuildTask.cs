// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using System.Collections.Generic;
    using System.Resources;
    using System.Text;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    public sealed class AnnotatorBuildTask : Task
    {
        [Required]
        public ITaskItem[] ReferenceAssemblies
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

        [Output]
        public ITaskItem[] GeneratedAssemblies
        {
            get;
            set;
        }

        public override bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
