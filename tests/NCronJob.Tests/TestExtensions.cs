using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NCronJob.Tests;

static class TestExtensions
{
    /// <summary>
    /// Extensions for testing that enables starting background services to pretend that we're running in a proper host environment. Remember to pass a
    /// <paramref name="shuttingDownCancellationToken"/> to be able to properly shut down again after executing the test.
    /// </summary>
    public static void RunBackgroundServices(this ServiceProvider serviceProvider,
        CancellationToken shuttingDownCancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var instances = new Stack<IHostedService>();

            shuttingDownCancellationToken.Register(() =>
            {
                Task.Run(async () =>
                {
                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                    try
                    {
                        while (instances.TryPop(out var instanceToStop))
                        {
                            try
                            {
                                await instanceToStop.StopAsync(timeout.Token);
                            }
                            catch (Exception exception)
                            {
                                throw new ApplicationException($"Failed to stop background service {instanceToStop}",
                                    exception);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        throw new ApplicationException("Failed to stop background services", exception);
                    }
                }, CancellationToken.None).GetAwaiter().GetResult();
            });

            foreach (var instanceToStart in serviceProvider.GetServices<IHostedService>())
            {
                try
                {
                    await instanceToStart.StartAsync(shuttingDownCancellationToken);

                    instances.Push(instanceToStart);
                }
                catch (Exception exception)
                {
                    throw new ApplicationException($"Failed to start background service {instanceToStart}", exception);
                }
            }

        }, shuttingDownCancellationToken);
    }
}
