﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using BenchmarkDotNet.Validators;

namespace MemoryLookupBenchmark
{
    internal class DefaultCoreConfig : ManualConfig
    {
        public DefaultCoreConfig()
        {
            Add(MarkdownExporter.GitHub);

            Add(MemoryDiagnoser.Default);
            Add(StatisticColumn.OperationsPerSecond);
            Add(DefaultColumnProviders.Instance);

            Add(JitOptimizationsValidator.FailOnError);

            Add(Job.Core
                .WithRemoveOutliers(false)
                .With(new GcMode { Server = true })
                .With(RunStrategy.Throughput)
                .WithLaunchCount(3)
                .WithWarmupCount(5)
                .WithTargetCount(10));
        }
    }
}
