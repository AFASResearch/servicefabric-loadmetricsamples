using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace LoadMetricStateless
{
  /// <summary>
  /// An instance of this class is created for each service instance by the Service Fabric runtime.
  /// </summary>
  internal sealed class LoadMetricStatelessService : StatelessService
  {
    private int _requestCount;
    public LoadMetricStatelessService(StatelessServiceContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
    /// </summary>
    /// <returns>A collection of listeners.</returns>
    protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
    {
      return new ServiceInstanceListener[0];
    }

    /// <summary>
    /// This is the main entry point for your service instance.
    /// </summary>
    /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
      //Starts a seperate task, dedicated to reporting the gathered metrics on an interval.
      ReportLoad(cancellationToken);


      // TODO: Replace the following sample code with your own logic 

      long iterations = 0;

      while(true)
      {
        cancellationToken.ThrowIfCancellationRequested();
        // Assuming each loop here handles some request, we update the metric to reflect the work being done.
        // We use the interlocked class to make sure that we do not lose metrics because of multi threading
        Interlocked.Increment(ref _requestCount);
        ServiceEventSource.Current.ServiceMessage(this, "Working-{0}", ++iterations);

        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
      }
    }
    private async Task ReportLoad(CancellationToken cancellationToken)
    {
      const int reportfrequency = 4;
      while(!cancellationToken.IsCancellationRequested)
      {
        // We only report ever so often, the resource balancer will not even look at new reports every 5 minutes (by default).
        await Task.Delay(TimeSpan.FromMinutes(reportfrequency), cancellationToken);
        // We create a list of all metrics we want to report.
        var loadmetrics = new List<LoadMetric> { new LoadMetric("RequestsPerMinute", Interlocked.Exchange(ref _requestCount, 0) / reportfrequency) };
        // Next step can fail (because of moving), implement retry logic inline with the rest of your application or just error handling 
        Partition.ReportLoad(loadmetrics);
      }
    }
  }
}
