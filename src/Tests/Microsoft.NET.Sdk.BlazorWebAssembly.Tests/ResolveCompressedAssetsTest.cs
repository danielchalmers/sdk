﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.AspNetCore.StaticWebAssets.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.NET.TestFramework;
using Moq;
using Xunit;

namespace Microsoft.NET.Sdk.BlazorWebAssembly.Tests;

public class ResolveCompressedAssetsTest
{
    public string ItemSpec { get; }

    public string OriginalItemSpec { get; }

    public string OutputBasePath { get; }

    public ResolveCompressedAssetsTest()
    {
        OutputBasePath = Path.Combine(TestContext.Current.TestExecutionDirectory, nameof(ResolveCompressedAssetsTest));
        ItemSpec = Path.Combine(OutputBasePath, Guid.NewGuid().ToString("N") + ".tmp");
        OriginalItemSpec = Path.Combine(OutputBasePath, Guid.NewGuid().ToString("N") + ".tmp");
    }

    [Fact]
    public void ResolvesExplicitlyProvidedAssets()
    {
        // Arrange
        var errorMessages = new List<string>();
        var buildEngine = new Mock<IBuildEngine>();
        buildEngine.Setup(e => e.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
            .Callback<BuildErrorEventArgs>(args => errorMessages.Add(args.Message));

        var asset = new StaticWebAsset()
        {
            Identity = ItemSpec,
            OriginalItemSpec = OriginalItemSpec,
            RelativePath = Path.GetFileName(ItemSpec),
        }.ToTaskItem();

        var gzipExplicitAsset = new TaskItem(asset.ItemSpec, asset.CloneCustomMetadata());
        gzipExplicitAsset.SetMetadata("ConfigurationName", "BuildCompressionGzip");

        var brotliExplicitAsset = new TaskItem(asset.ItemSpec, asset.CloneCustomMetadata());
        brotliExplicitAsset.SetMetadata("ConfigurationName", "BuildCompressionBrotli");

        var task = new ResolveCompressedAssets()
        {
            OutputPath = OutputBasePath,
            BuildEngine = buildEngine.Object,
            CandidateAssets = new[] { asset },
            Formats = "gzip;brotli",
            ExplicitAssets = new[] { gzipExplicitAsset, brotliExplicitAsset },
        };

        // Act
        var result = task.Execute();

        // Assert
        result.Should().BeTrue();
        task.AssetsToCompress.Should().HaveCount(2);
        task.AssetsToCompress[0].ItemSpec.Should().EndWith(".gz");
        task.AssetsToCompress[1].ItemSpec.Should().EndWith(".br");
    }

    [Fact]
    public void ResolvesAssetsMatchingIncludePattern()
    {
        // Arrange
        var errorMessages = new List<string>();
        var buildEngine = new Mock<IBuildEngine>();
        buildEngine.Setup(e => e.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
            .Callback<BuildErrorEventArgs>(args => errorMessages.Add(args.Message));

        var asset = new StaticWebAsset()
        {
            Identity = ItemSpec,
            OriginalItemSpec = OriginalItemSpec,
            RelativePath = Path.GetFileName(ItemSpec),
        }.ToTaskItem();

        var task = new ResolveCompressedAssets()
        {
            OutputPath = OutputBasePath,
            BuildEngine = buildEngine.Object,
            CandidateAssets = new[] { asset },
            IncludePatterns = "**\\*.tmp",
            Formats = "gzip;brotli",
        };

        // Act
        var result = task.Execute();

        // Assert
        result.Should().BeTrue();
        task.AssetsToCompress.Should().HaveCount(2);
        task.AssetsToCompress[0].ItemSpec.Should().EndWith(".gz");
        task.AssetsToCompress[1].ItemSpec.Should().EndWith(".br");
    }

    [Fact]
    public void ExcludesAssetsMatchingExcludePattern()
    {
        // Arrange
        var errorMessages = new List<string>();
        var buildEngine = new Mock<IBuildEngine>();
        buildEngine.Setup(e => e.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
            .Callback<BuildErrorEventArgs>(args => errorMessages.Add(args.Message));

        var asset = new StaticWebAsset()
        {
            Identity = ItemSpec,
            OriginalItemSpec = OriginalItemSpec,
            RelativePath = Path.GetFileName(ItemSpec),
        }.ToTaskItem();

        var task = new ResolveCompressedAssets()
        {
            OutputPath = OutputBasePath,
            BuildEngine = buildEngine.Object,
            IncludePatterns = "**\\*",
            ExcludePatterns = "**\\*.tmp",
            CandidateAssets = new[] { asset },
            Formats = "gzip;brotli"
        };

        // Act
        var result = task.Execute();

        // Assert
        result.Should().BeTrue();
        task.AssetsToCompress.Should().HaveCount(0);
    }

    [Fact]
    public void DeduplicatesAssetsResolvedBothExplicitlyAndFromPattern()
    {
        // Arrange
        var errorMessages = new List<string>();
        var buildEngine = new Mock<IBuildEngine>();
        buildEngine.Setup(e => e.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
            .Callback<BuildErrorEventArgs>(args => errorMessages.Add(args.Message));

        var asset = new StaticWebAsset()
        {
            Identity = ItemSpec,
            OriginalItemSpec = OriginalItemSpec,
            RelativePath = Path.GetFileName(ItemSpec),
        }.ToTaskItem();

        var gzipExplicitAsset = new TaskItem(asset.ItemSpec, asset.CloneCustomMetadata());
        gzipExplicitAsset.SetMetadata("ConfigurationName", "BuildCompressionGzip");

        var brotliExplicitAsset = new TaskItem(asset.ItemSpec, asset.CloneCustomMetadata());
        brotliExplicitAsset.SetMetadata("ConfigurationName", "BuildCompressionBrotli");

        var buildTask = new ResolveCompressedAssets()
        {
            OutputPath = OutputBasePath,
            BuildEngine = buildEngine.Object,
            CandidateAssets = new[] { asset },
            IncludePatterns = "**\\*.tmp",
            ExplicitAssets = new[] { gzipExplicitAsset, brotliExplicitAsset },
            Formats = "gzip;brotli"
        };

        // Act
        var buildResult = buildTask.Execute();

        // Assert
        buildResult.Should().BeTrue();
        buildTask.AssetsToCompress.Should().HaveCount(2);
        buildTask.AssetsToCompress[0].ItemSpec.Should().EndWith(".gz");
        buildTask.AssetsToCompress[1].ItemSpec.Should().EndWith(".br");
    }

    [Fact]
    public void IgnoresAssetsCompressedInPreviousTaskRun()
    {
        // Arrange
        var errorMessages = new List<string>();
        var buildEngine = new Mock<IBuildEngine>();
        buildEngine.Setup(e => e.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
            .Callback<BuildErrorEventArgs>(args => errorMessages.Add(args.Message));

        var asset = new StaticWebAsset()
        {
            Identity = ItemSpec,
            OriginalItemSpec = OriginalItemSpec,
            RelativePath = Path.GetFileName(ItemSpec),
        }.ToTaskItem();

        // Act/Assert
        var task1 = new ResolveCompressedAssets()
        {
            OutputPath = OutputBasePath,
            BuildEngine = buildEngine.Object,
            CandidateAssets = new[] { asset },
            IncludePatterns = "**\\*.tmp",
            Formats = "gzip",
        };

        var result1 = task1.Execute();

        result1.Should().BeTrue();
        task1.AssetsToCompress.Should().HaveCount(1);
        task1.AssetsToCompress[0].ItemSpec.Should().EndWith(".gz");

        var brotliExplicitAsset = new TaskItem(asset.ItemSpec, asset.CloneCustomMetadata());
        brotliExplicitAsset.SetMetadata("ConfigurationName", "BuildCompressionBrotli");

        var task2 = new ResolveCompressedAssets()
        {
            OutputPath = OutputBasePath,
            BuildEngine = buildEngine.Object,
            CandidateAssets = new[] { asset, task1.AssetsToCompress[0] },
            IncludePatterns = "**\\*.tmp",
            ExplicitAssets = new[] { brotliExplicitAsset },
            Formats = "gzip;brotli"
        };

        var result2 = task2.Execute();

        result2.Should().BeTrue();
        task2.AssetsToCompress.Should().HaveCount(1);
        task2.AssetsToCompress[0].ItemSpec.Should().EndWith(".br");
    }
}
