# servicefabric-loadmetricsamples
This is a sample project to show the possibilities of enhancing the Service Fabric resource balancer with extra information. This project is accompanying [this blogpost](http://dev.afas.nl/insite-afas/a-dive-into-service-fabric-resource-balancing) which gives a explanation how the resource balancer uses the information we feed it.
The code does not do anything usefull except show how to enhance the resource balancer, so the only place you can see the effect is in the Service Fabric Explorer.

To run this project you require:
* Service Fabric Version: *5.1.150*
* Visual Studio 2015

To use the custom ClusterManifest (for capacities and thresholding), you need to place the ClusterManifestTemplate.xml in the following folder: *C:\Program Files\Microsoft SDKs\Service Fabric\ClusterSetup\NonSecure*. **Note:** This manifest works only in a developer environment and is not suitable for a production cluster.