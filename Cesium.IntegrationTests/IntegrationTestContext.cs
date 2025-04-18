// SPDX-FileCopyrightText: 2025 Cesium contributors <https://github.com/ForNeVeR/Cesium>
//
// SPDX-License-Identifier: MIT

using AsyncKeyedLock;
using Cesium.Solution.Metadata;
using Cesium.TestFramework;
using JetBrains.Annotations;
using TruePath;
using Xunit.Abstractions;

namespace Cesium.IntegrationTests;

[UsedImplicitly]
public class IntegrationTestContext : IAsyncDisposable
{
    public const string BuildConfiguration = "Release";

    /// <summary>Semaphore that controls the amount of simultaneously running tests.</summary>
    private readonly AsyncNonKeyedLocker _testSemaphore = new(Environment.ProcessorCount);

    private readonly AsyncNonKeyedLocker _lock = new();
    private bool _initialized;
    private Exception? _initializationException;

    public AbsolutePath? VisualStudioPath { get; private set; }

    public async Task WrapTestBody(Func<Task> testBody)
    {
        using (await _testSemaphore.LockAsync())
        {
            await testBody();
        }
    }

    public async Task EnsureInitialized(ITestOutputHelper output)
    {
        using (await _lock.LockAsync())
        {
            if (_initialized)
            {
                if (_initializationException != null) throw _initializationException;
                return;
            }

            try
            {
                await InitializeOnce(output);
            }
            catch (Exception ex)
            {
                _initializationException = ex;
                throw;
            }
            finally
            {
                _initialized = true;
            }
        }
    }

    private async Task InitializeOnce(ITestOutputHelper output)
    {
        if (OperatingSystem.IsWindows())
        {
            VisualStudioPath = await WindowsEnvUtil.FindVcCompilerInstallationFolder(output);
        }

        await BuildRuntime(output);
        await BuildCompiler(output);
    }

    public async ValueTask DisposeAsync()
    {
        await DotNetCliHelper.ShutdownBuildServer();
    }

    private static async Task BuildRuntime(ITestOutputHelper output)
    {
        var runtimeProjectFile = SolutionMetadata.SourceRoot / "Cesium.Runtime/Cesium.Runtime.csproj";
        await DotNetCliHelper.BuildDotNetProject(output, BuildConfiguration, runtimeProjectFile);
    }

    private static async Task BuildCompiler(ITestOutputHelper output)
    {
        var compilerProjectFile = SolutionMetadata.SourceRoot / "Cesium.Compiler/Cesium.Compiler.csproj";
        await DotNetCliHelper.BuildDotNetProject(output, BuildConfiguration, compilerProjectFile);
    }
}
