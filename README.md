# PhoenixRt.Mvvm
Example scientific/medical visualization system using SoA for complex processing and pure MVVM for display.  PheonixRt.Mvvm is split in to two exe's:

* Front-end
  * UI containing the MVVM
  * Interaction with back-end services is via service helpers (using standard .NET events)
* Back-end
  * hosts the services responsible for pre-processing data
  * hosts services that visualize the data, such as calculating an MPR, a mesh intersection, or an isosurface

The important interface is between what processing should be done by services, versus what processing can be done by the View (which is WPF in this case).  The line I've drawn is that anything that has been reduced to a:

* Bitmap (including alpha values)
* 2D vector geometry, such as a line, polyline, or polygon
* 2D transformation

can be exposed as bindable properties on the ViewModel, and then any additional rendering can be done by the View (for instance to add adornments, or other rendering styles).  So the services are necessary to turn the data in to these kinds of primitives, and WPF will take it from there.

This prototype also looks at the use of the [standy pool](http://blogs.msdn.com/b/tims/archive/2010/10/29/pdc10-mysteries-of-windows-memory-management-revealed-part-two.aspx) as a means of caching large amounts of data to be ready for loading.  This is similar to what the Windows [SuperFetch](https://en.wikipedia.org/wiki/Windows_Vista_I/O_technologies#SuperFetch) feature does for DLLs, but in this case it is large volumetric data being pre-cached.
