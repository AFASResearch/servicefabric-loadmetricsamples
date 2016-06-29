using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace LoadMetricActor.Interfaces
{
  /// <summary>
  /// This interface defines the methods exposed by an actor.
  /// Clients use this interface to interact with the actor that implements it.
  /// </summary>
  public interface ILoadMetricActor : IActor
  {
    /// <summary>
    /// TODO: Replace with your own actor method.
    /// </summary>
    /// <returns></returns>
    Task<int> GetDataAsync();

    /// <summary>
    /// TODO: Replace with your own actor method.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    Task SetDataAsync(int data);
  }
}
