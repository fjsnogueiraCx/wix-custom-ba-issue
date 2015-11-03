using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace CreateInstallers
{
    internal static class Program
    {
        private static void Main()
        {
            try
            {
                // Run this in debug otherwise the files in CreateInstallers will be locked. :)
                const string projectFileName = @"..\..\..\wix-custom-ba-issue.sln";
                var pc = new ProjectCollection();
                var globalProperty = new Dictionary<string, string> {{"Configuration", "Release"}};

                var buildRequestData = new BuildRequestData(projectFileName, globalProperty, null, new[] {"Rebuild"},
                    null);

                var buildParameters = new BuildParameters(pc)
                {
                    DetailedSummary = true,
                    Loggers = new List<ILogger> {new ConsoleLogger()}
                };

                foreach (var version in new List<string> {"0.0.6.0", "1.0.0.0"})
                    BuildExamples(version, buildParameters, buildRequestData);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed!", e);
                Console.WriteLine(e.ToString());
            }
        }

        private static void BuildExamples(string versionString, BuildParameters buildParameters,
            BuildRequestData buildRequestData)
        {
            const string versionFileName = @"..\..\..\.config\VersionInfo.txt";
            File.WriteAllText(versionFileName, versionString);

            var buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequestData);

            if (buildResult.OverallResult == BuildResultCode.Success)
            {
                var output =
                    buildResult.ResultsByTarget["Rebuild"].Items.First(x => x.ItemSpec.Contains("Bootstrapper"))
                        .ItemSpec;

                var temp = Path.GetTempPath();
                var productName = Path.GetFileNameWithoutExtension(output);
                var fileName = Path.GetFileName(output);

                if (productName != null)
                {
                    var directory = Path.Combine(temp, productName, versionString);

                    if (Directory.Exists(directory))
                        Directory.Delete(directory, true);

                    Directory.CreateDirectory(directory);

                    if (fileName != null)
                        File.Copy(output, Path.Combine(directory, fileName));
                }
            }
        }
    }
}