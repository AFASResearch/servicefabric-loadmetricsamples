using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace LoadMetricActor
{
  public class LoadMetricActorService : ActorService
  {
    private int _requestCount;
    public LoadMetricActorService(StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorBase> actorFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null) : base(context, actorTypeInfo, actorFactory, stateProvider, settings)
    {
    }

    public void IncrementRequestCount()
    {
      //Multiple actors can try to update at the same time so we use interlocked to make sure each increment is preserved.
      Interlocked.Increment(ref _requestCount);
    }
    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
      //Starts a seperate task, dedicated to reporting the gathered metrics on an interval.
      ReportLoad(cancellationToken);

      base.RunAsync(cancellationToken);      
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
