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
        public ITaskItem[] References { get; set; }

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

            foreach (var reference in References)
            {
                var filePath = reference.ItemSpec;
                var fileExt = Path.GetExtension(filePath);
                if (string.Equals(".pdb", fileExt, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var fileName = Path.GetFileName(filePath);
                var fileVersion = FileUtilities.GetFileVersion(filePath)?.ToString() ?? string.Empty;
                var assemblyVersion = FileUtilities.TryGetAssemblyVersion(filePath);
                if (assemblyVersion == null)
                {
                    var nativeFile = new RuntimeFile(fileName, null, fileVersion);
                    nativeFiles.Add(nativeFile);
                }
                else
                {
                    var runtimeFile = new RuntimeFile(fileName,
                        fileVersion: fileVersion,
                        assemblyVersion: assemblyVersion.ToString());
                    runtimeFiles.Add(runtimeFile);
                }
            }

            var runtimePackageName = $"runtime.{RuntimeIdentifier}.{FrameworkName}";

            var runtimeLibrary = new RuntimeLibrary("package",
               runtimePackageName,
               FrameworkVersion,
               hash: string.Empty,
               runtimeAssemblyGroups: new[] { new RuntimeAssetGroup(string.Empty, runtimeFiles) },
               nativeLibraryGroups: new[] { new RuntimeAssetGroup(string.Empty, nativeFiles) },
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

            try
            {
                using (var depsStream = File.Create(DepsFilePath))
                {
                    new DependencyContextWriter().Write(context, depsStream);
                }
            }
            catch (Exception ex)
            {
                // If there is a problem, ensure we don't write a partially complete version to disk.
                if (File.Exists(DepsFilePath))
                {
                    File.Delete(DepsFilePath);
                }
                Log.LogErrorFromException(ex);
            }
        }
    }
}
