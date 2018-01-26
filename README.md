
<img src="https://github.com/DataMesh-OpenSource/MeshExpert-Live/blob/master/resources/datamesh.png" width="400">

   Visit us at [www.datamesh.com](http://www.datamesh.com "DataMesh Home")
                    
# METoolkit

METoolkit is a Unity toolkit for devoloping interactive mixed-reality apps for [HoloLens](https://www.microsoft.com/en-us/hololens "HoloLens Home"), Surface, iPhone/iPad, Android devices, etc. It enables easy-integration with [**MeshExpert Live!&reg;**](https://www.datamesh.com/solution/meshexpert-live "MeshExpert Live!") and [**MeshExpert DataMix&reg;**](https://www.datamesh.com/solution/meshexpert-datamix "MeshExpert DataMix").

## What's New

METoolkit now supports external capture card, specifically the [BlackMagic Intensity Shuttle capture card](https://www.blackmagicdesign.com/products/intensity).
> Note: since the Blackmagic Shuttle card only supports up to 30fps, the config parameter "**Frame_Rate=60**" in file "**Assets/StreamingAssets/MEConfigLive.ini**" should be changed to "**Frame_Rate=30**" if you are using Blackmagic Shuttle card.

Now Unity 2017 is supported by METoolkit from release version v2.3.0. The legacy support for Unity 5.5 is moved to another branch '**[METoolkit_2.2_ForUnity5.5.1](https://github.com/DataMesh-OpenSource/METoolkit/tree/METoolkit_2.2_ForUnity5.5.1)**'. **The master branch is only compatible with Unity 2017.** We recommend Unity 2017.2 when starting a new project.
> Note: to build with Unity 2017, please set .NET version to **.NET 4.6** in Unity building settings.

## Components

METoolkit consists of nine components for a wide range of functionalities.

<p align="center">
<img src="https://github.com/DataMesh-OpenSource/MeshExpert-Live/blob/master/resources/METoolkit-Structure.png" width="500">
</p>

## Getting Started

Please check out the examples at [Assets/DataMesh/Samples](https://github.com/DataMesh-OpenSource/METoolkit/tree/master/Assets/DataMesh/Samples). They are runnable code samples for each modules. Also read the [docs](http://docs.datamesh.com/projects/me-live/en/latest/METoolkit-overview/) for those examples. For instance, the sample code for Anchor module can be found [here](https://github.com/DataMesh-OpenSource/METoolkit/tree/master/Assets/DataMesh/Samples/SceneAnchorSample) and its docs are [here](http://docs.datamesh.com/projects/me-live/en/latest/toolkit/toolkit-man-world-anchor-module/).

## Documentation

For more detailed documentations for METoolkit, see the [**METoolkit Developer Manual**](http://docs.datamesh.com/projects/me-live/en/latest/METoolkit-overview/).

## License

METoolkit is opensource under MIT license. You are free to use it to create your own apps.
