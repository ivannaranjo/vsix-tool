using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Settings;
using Microsoft.Win32;
using System.Globalization;
using Microsoft.Extensions.CommandLineUtils;

namespace VsixTool
{
    public static class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication { Name = "vsix" };
            app.HelpOption("-?|-h|--help");

            var vsixPath = app.Option(
                "-f|--vsix",
                "The path to the vsix to install",
                CommandOptionType.SingleValue);
            var version = app.Option(
                "-v|--version",
                "The version of VS to install to.",
                CommandOptionType.SingleValue);
            var suffix = app.Option(
                "-s|--suffix",
                "The Visual Studio root suffix to use.",
                CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (!vsixPath.HasValue())
                {
                    Console.Error.WriteLine("Need to specify the --vsix parameter.");
                    return 1;
                }
                if (!File.Exists(vsixPath.Value()))
                {
                    Console.Error.Write($"Cannot find the vsix at {vsixPath.Value()}");
                    return 1;
                }

                var versionValue = version.HasValue() ? version.Value() : "14.0";
                var vsExe = GetVersionExe(versionValue);
                if (string.IsNullOrEmpty(vsExe))
                {
                    Console.Error.WriteLine("Cannot find Visual Studio " + version);
                    return 1;
                }

                var suffixValue = suffix.HasValue() ? suffix.Value() : "exp";

                var vsixPackage = ExtensionManagerService.CreateInstallableExtension(vsixPath.Value());
                Console.Error.WriteLine($"Installing {vsixPackage.Header.Name} version {vsixPackage.Header.Version}");

                try
                {
                    Install(vsExe, vsixPackage, suffixValue);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to install extension: {ex.Message}");
                    return 1;
                }

                return 0;
            });

            return app.Execute(args);
        }

        public static IEnumerable<decimal?> FindVsVersions()
        {
            using (var software = Registry.LocalMachine.OpenSubKey("SOFTWARE"))
            using (var ms = software.OpenSubKey("Microsoft"))
            using (var vs = ms.OpenSubKey("VisualStudio"))
                return vs.GetSubKeyNames()
                        .Select(s =>
                {
                    decimal v;
                    if (!decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out v))
                        return new decimal?();
                    return v;
                })
                .Where(d => d.HasValue)
                .OrderBy(d => d);
        }

        public static string GetVersionExe(string version)
        {
            return Registry.GetValue($@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\{version}\Setup\VS", "EnvironmentPath", null) as string;
        }

        public static void Install(string vsExe, IInstallableExtension vsix, string rootSuffix)
        {
            using (var esm = ExternalSettingsManager.CreateForApplication(vsExe, rootSuffix))
            {
                var ems = new ExtensionManagerService(esm);
                IInstalledExtension installedVsix = null;
                if (ems.TryGetInstalledExtension(vsix.Header.Identifier, out installedVsix))
                {
                    Console.WriteLine($"Extension {vsix.Header.Name} is installed with version {installedVsix.Header.Version}, unistalling it.");
                    ems.Uninstall(installedVsix);
                }

                ems.Install(vsix, perMachine: false);
            }
        }
    }
}
