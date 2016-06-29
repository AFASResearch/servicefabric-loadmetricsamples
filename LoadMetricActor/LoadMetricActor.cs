﻿using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using LoadMetricActor.Interfaces;

namespace LoadMetricActor
{
  /// <remarks>
  /// This class represents an actor.
  /// Every ActorID maps to an instance of this class.
  /// The StatePersistence attribute determines persistence and replication of actor state:
  ///  - Persisted: State is written to disk and replicated.
  ///  - Volatile: State is kept in memory only and replicated.
  ///  - None: State is kept in memory only and not replicated.
  /// </remarks>
  [StatePersistence(StatePersistence.Persisted)]
  internal class LoadMetricActor : Actor, ILoadMetricActor
  {
    /// <summary>
    /// This method is called whenever an actor is activated.
    /// An actor is activated the first time any of its methods are invoked.
    /// </summary>
    protected override Task OnActivateAsync()
    {
      ActorEventSource.Current.ActorMessage(this, "Actor activated.");

      // The StateManager is this actor's private state store.
      // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
      // Any serializable object can be saved in the StateManager.
      // For more information, see http://aka.ms/servicefabricactorsstateserialization

      return this.StateManager.TryAddStateAsync("count", 0);
    }

    /// <summary>
    /// TODO: Replace with your own actor method.
    /// </summary>
    /// <returns></returns>
    Task<int> ILoadMetricActor.GetDataAsync()
    {
      // The actor recieves a request, so we increment our metric.
      ((LoadMetricActorService)ActorService).IncrementRequestCount();

      return this.StateManager.GetStateAsync<int>("data");
    }

    /// <summary>
    /// TODO: Replace with your own actor method.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    Task ILoadMetricActor.SetDataAsync(int data)
    {
      // The actor recieves a request, so we increment our metric.
      ((LoadMetricActorService)ActorService).IncrementRequestCount();

      // Requests are not guaranteed to be processed in order nor at most once.
      // The update function here verifies that the incoming count is greater than the current count to preserve order.
      return this.StateManager.AddOrUpdateStateAsync("data", data, (key, value) => data > value ? data : value);
    }
  }
}
