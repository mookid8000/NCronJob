using System.Collections.Concurrent;
using LinkDotNet.NCronJob;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
// ReSharper disable ClassNeverInstantiated.Local
#pragma warning disable CA1861

#pragma warning disable S1144

namespace NCronJob.Tests;

public sealed class JobLifestyleTests : IDisposable
{
    private readonly CancellationTokenSource shuttingDownTokenSource = new();
    private readonly ConcurrentQueue<string> events = new();
    private readonly IInstantJobRegistry instantJobRegistry;
    private readonly ServiceProvider provider;

    public JobLifestyleTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton(events);
        services.AddCronJob<MyLifestyleCheckerJob>();

        provider = services.BuildServiceProvider();
        provider.RunBackgroundServices(shuttingDownTokenSource.Token);
        instantJobRegistry = provider.GetRequiredService<IInstantJobRegistry>();
    }

    [Fact]
    public async Task DefaultsToRegisteringJobAsScoped()
    {
        // run twice
        instantJobRegistry.AddInstantJob<MyLifestyleCheckerJob>();
        instantJobRegistry.AddInstantJob<MyLifestyleCheckerJob>();

        await Task.Delay(TimeSpan.FromSeconds(2));

        events.ShouldBe(new[] { "MyLifestyleCheckerJob created", "MyLifestyleCheckerJob created" });
    }

    [Fact]
    public async Task CanRegisterAsSingletonToo()
    {
        // run twice
        instantJobRegistry.AddInstantJob<MyLifestyleCheckerJob>();
        instantJobRegistry.AddInstantJob<MyLifestyleCheckerJob>();

        await Task.Delay(TimeSpan.FromSeconds(2));

        events.ShouldBe(new[] { "MyLifestyleCheckerJob created", "MyLifestyleCheckerJob created" });
    }

    private sealed class MyLifestyleCheckerJob : IJob
    {
        public MyLifestyleCheckerJob(ConcurrentQueue<string> events)
        {
            events.Enqueue("MyLifestyleCheckerJob created");
        }

        public Task Run(JobExecutionContext context, CancellationToken token = default) => Task.CompletedTask;
    }

    public void Dispose()
    {
        shuttingDownTokenSource.Cancel();
        shuttingDownTokenSource.Dispose();
        provider.Dispose();
    }
}
