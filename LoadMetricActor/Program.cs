using System;
using System.Threading;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace LoadMetricActor
{
  internal static class Program
  {
    /// <summary>
    /// This is the entry point of the service host process.
    /// </summary>
    private static void Main()
    {
      try
      {
        // This line registers an Actor Service to host your actor class with the Service Fabric runtime.
        // The contents of your ServiceManifest.xml and ApplicationManifest.xml files
        // are automatically populated when you build this project.
        // For more information, see http://aka.ms/servicefabricactorsplatform

        ActorRuntime.RegisterActorAsync<LoadMetricActor>(
           (context, actorType) => new LoadMetricActorService(context, actorType, () => new LoadMetricActor())).GetAwaiter().GetResult();

        Thread.Sleep(Timeout.Infinite);
      }
      catch(Exception e)
      {
        ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
        throw;
      }
    }
  }
}
