using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace LoadMetricStateful
{
  /// <summary>
  /// An instance of this class is created for each service replica by the Service Fabric runtime.
  /// </summary>
  internal sealed class LoadMetricStatefulService : StatefulService
  {

    private int _requestCount;
    public LoadMetricStatefulService(StatefulServiceContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
    /// </summary>
    /// <remarks>
    /// For more information on service communication, see http://aka.ms/servicefabricservicecommunication
    /// </remarks>
    /// <returns>A collection of listeners.</returns>
    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
    {
      return new ServiceReplicaListener[0];
    }

    /// <summary>
    /// This is the main entry point for your service replica.
    /// This method executes when this replica of your service becomes primary and has write status.
    /// </summary>
    /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
      //Starts a seperate task, dedicated to reporting the gathered metrics on an interval.
      ReportLoad(cancellationToken);


      // TODO: Replace the following sample code with your own logic 
     
      var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

      while(true)
      {
        cancellationToken.ThrowIfCancellationRequested();

        using(var tx = this.StateManager.CreateTransaction())
        {
          // Assuming each loop here handles some request, we update the metric to reflect the work being done.
          // We use the interlocked class to make sure that we do not lose metrics because of multi threading
          Interlocked.Increment(ref _requestCount);
          var result = await myDictionary.TryGetValueAsync(tx, "Counter");

          ServiceEventSource.Current.ServiceMessage(this, "Current Counter Value: {0}",
              result.HasValue ? result.Value.ToString() : "Value does not exist.");

          await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

          // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
          // discarded, and nothing is saved to the secondary replicas.
          await tx.CommitAsync();
        }

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
        var loadmetrics = new List<LoadMetric> { new LoadMetric("RequestsPerMinute", Interlocked.Exchange(ref _requestCount ,0) / reportfrequency) };
        // Next step can fail (because of moving), implement retry logic inline with the rest of your application or just error handling 
        Partition.ReportLoad(loadmetrics);
      }
    }
  }
}
