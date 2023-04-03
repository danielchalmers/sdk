// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine;
using Microsoft.DotNet.Tools;
using Microsoft.DotNet.Tools.Build;
using LocalizableStrings = Microsoft.DotNet.Tools.Build.LocalizableStrings;

namespace Microsoft.DotNet.Cli
{
    internal static class BuildCommandParser
    {
        public static readonly string DocsLink = "https://aka.ms/dotnet-build";

        public static readonly CliArgument<IEnumerable<string>> SlnOrProjectArgument = new CliArgument<IEnumerable<string>>(CommonLocalizableStrings.SolutionOrProjectArgumentName)
        {
            Description = CommonLocalizableStrings.SolutionOrProjectArgumentDescription,
            Arity = ArgumentArity.ZeroOrMore
        };

        public static readonly CliOption<string> OutputOption = new ForwardedOption<string>("--output", "-o")
        {
            Description = LocalizableStrings.OutputOptionDescription,
            HelpName = LocalizableStrings.OutputOptionName
        }.ForwardAsOutputPath("OutputPath");

        public static readonly CliOption<bool> NoIncrementalOption = new("--no-incremental") { Description = LocalizableStrings.NoIncrementalOptionDescription };

        public static readonly CliOption<bool> NoDependenciesOption = new ForwardedOption<bool>("--no-dependencies")
        {
            Description = LocalizableStrings.NoDependenciesOptionDescription
        }.ForwardAs("-property:BuildProjectReferences=false");

        public static readonly CliOption<bool> NoLogoOption = new ForwardedOption<bool>("--nologo")
        {
            Description = LocalizableStrings.CmdNoLogo
        }.ForwardAs("-nologo");

        public static readonly CliOption<bool> NoRestoreOption = CommonOptions.NoRestoreOption;

        public static readonly CliOption<bool> SelfContainedOption = CommonOptions.SelfContainedOption;

        public static readonly CliOption<bool> NoSelfContainedOption = CommonOptions.NoSelfContainedOption;

        public static readonly CliOption<string> RuntimeOption = CommonOptions.RuntimeOption;

        public static readonly CliOption<string> FrameworkOption = CommonOptions.FrameworkOption(LocalizableStrings.FrameworkOptionDescription);

        public static readonly CliOption<string> ConfigurationOption = CommonOptions.ConfigurationOption(LocalizableStrings.ConfigurationOptionDescription);

        private static readonly CliCommand Command = ConstructCommand();

        public static CliCommand GetCommand()
        {
            return Command;
        }

        private static CliCommand ConstructCommand()
        {
            DocumentedCommand command = new("build", DocsLink, LocalizableStrings.AppFullName);

            command.Arguments.Add(SlnOrProjectArgument);
            RestoreCommandParser.AddImplicitRestoreOptions(command, includeRuntimeOption: false, includeNoDependenciesOption: false);
            command.Options.Add(FrameworkOption);
            command.Options.Add(ConfigurationOption);
            command.Options.Add(RuntimeOption.WithHelpDescription(command, LocalizableStrings.RuntimeOptionDescription));
            command.Options.Add(CommonOptions.VersionSuffixOption);
            command.Options.Add(NoRestoreOption);
            command.Options.Add(CommonOptions.InteractiveMsBuildForwardOption);
            command.Options.Add(CommonOptions.VerbosityOption);
            command.Options.Add(CommonOptions.DebugOption);
            command.Options.Add(OutputOption);
            command.Options.Add(NoIncrementalOption);
            command.Options.Add(NoDependenciesOption);
            command.Options.Add(NoLogoOption);
            command.Options.Add(SelfContainedOption);
            command.Options.Add(NoSelfContainedOption);
            command.Options.Add(CommonOptions.ArchitectureOption);
            command.Options.Add(CommonOptions.OperatingSystemOption);
            command.Options.Add(CommonOptions.DisableBuildServersOption);

            command.SetAction(BuildCommand.Run);

            return command;
        }
    }
}
