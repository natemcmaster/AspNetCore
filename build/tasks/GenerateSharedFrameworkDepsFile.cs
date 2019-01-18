// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.DotNet.Build.Tasks;
using Microsoft.Extensions.DependencyModel;
using RepoTasks.Utilities;

namespace RepoTasks
{
    public class GenerateSharedFrameworkDepsFile : Task
    {
        [Required]
        public string DepsFilePath { get; set; }

        [Required]
        public string TargetFramework { get; set; }

        [Required]
        public string FrameworkName { get; set; }

        [Required]
        public string FrameworkVersion { get; set; }


        [Required]
        public ITaskItem[] ReferencePaths { get; set; }

        [Required]
        public string RuntimeIdentifier { get; set; }

        public override bool Execute()
        {
            ExecuteCore();

            return !Log.HasLoggedErrors;
        }

        private void ExecuteCore()
        {
            var target = new TargetInfo(TargetFramework, RuntimeIdentifier, string.Empty, isPortable: false);
            var runtimeFiles = new List<RuntimeFile>();
            var nativeFiles = new List<RuntimeFile>();
            var resourceAssemblies = new List<ResourceAssembly>();

            foreach (var reference in ReferencePaths)
            {
                var packageId = reference.GetMetadata("NuGetPackageId");
                if (string.Equals("Microsoft.NETCore.App", packageId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var filePath = reference.ItemSpec;
                var fileName = Path.GetFileName(filePath);
                var fileVersion = FileUtilities.GetFileVersion(filePath);
                var assemblyVersion = FileUtilities.TryGetAssemblyVersion(filePath);
                var runtimeFile = new RuntimeFile(fileName,
                     fileVersion: fileVersion?.ToString() ?? string.Empty,
                     assemblyVersion: assemblyVersion?.ToString() ?? string.Empty);
                runtimeFiles.Add(runtimeFile);
            }

            var runtimePackageName = $"runtime.{RuntimeIdentifier}.{FrameworkName}";

            var runtimeLibrary = new RuntimeLibrary("package",
               runtimePackageName,
               FrameworkVersion,
               string.Empty,
               new[] { new RuntimeAssetGroup(string.Empty, runtimeFiles) },
               new[] { new RuntimeAssetGroup(string.Empty, nativeFiles) },
               Enumerable.Empty<ResourceAssembly>(),
               Array.Empty<Dependency>(),
               hashPath: null,
               path: $"{runtimePackageName.ToLowerInvariant()}/{FrameworkVersion}",
               serviceable: true);

            var context = new DependencyContext(target,
                CompilationOptions.Default,
                Enumerable.Empty<CompilationLibrary>(),
                new[] { runtimeLibrary },
                Enumerable.Empty<RuntimeFallbacks>());

            Directory.CreateDirectory(Path.GetDirectoryName(DepsFilePath));

            using (var depsStream = File.Create(DepsFilePath))
            {
                new DependencyContextWriter().Write(context, depsStream);
            }
        }
    }
}
