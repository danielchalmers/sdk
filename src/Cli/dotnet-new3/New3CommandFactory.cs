﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System.CommandLine;
using Microsoft.TemplateEngine.Cli;
using Microsoft.TemplateEngine.Cli.Commands;

namespace Dotnet_new3
{
    internal static class New3CommandFactory
    {
        private const string CommandName = "new3";

        private static readonly Option<bool> _debugEmitTelemetryOption = new("--debug:emit-telemetry", "Enable telemetry")
        {
            IsHidden = true
        };

        private static readonly Option<bool> _debugDisableBuiltInTemplatesOption = new("--debug:disable-sdk-templates", "Disable built-in templates")
        {
            IsHidden = true
        };

        internal static Command Create()
        {
            Command newCommand = NewCommandFactory.Create(
                CommandName,
                (ParseResult parseResult) =>
                {
                    FileInfo? outputPath = parseResult.GetValue(SharedOptions.OutputOption);
                    return HostFactory.CreateHost(parseResult.GetValue(_debugDisableBuiltInTemplatesOption), outputPath?.FullName);
                });

            newCommand.AddGlobalOption(_debugEmitTelemetryOption);
            newCommand.AddGlobalOption(_debugDisableBuiltInTemplatesOption);
            newCommand.AddCommand(new CompleteCommand());
            return newCommand;
        }
    }
}
