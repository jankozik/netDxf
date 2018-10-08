# netDxf
netDxf 2.3.0 Copyright(C) 2009-2018 Daniel Carvajal, Licensed under LGPL
## Description
netDxf is a .net library programmed in C# to read and write AutoCAD dxf files. It supports AutoCad2000, AutoCad2004, AutoCad2007, AutoCad2010,  AutoCad2013, and AutoCad2018 dxf database versions, in both text and binary format.

The library is easy to use and I tried to keep the procedures as straightforward as possible, for example you will not need to fill up the table section with layers, styles or line type definitions. The DxfDocument will take care of that every time a new item is added.

If you need more information, you can find the official dxf documentation [here](https://help.autodesk.com/view/OARX/2019/ENU/?guid=GUID-235B22E0-A567-4CF6-92D3-38A2306D73F3).

Code example:

```c#
public static void Main()
{
	// your dxf file name
	string file = "sample.dxf";

	// by default it will create an AutoCad2000 DXF version
	DxfDocument dxf = new DxfDocument();
	// an entity
	Line entity = new Line(new Vector2(5, 5), new Vector2(10, 5));
	// add your entities here
	dxf.AddEntity(entity);
	// save to file
	dxf.Save(file);

	bool isBinary;
	// this check is optional but recommended before loading a DXF file
	DxfVersion dxfVersion = DxfDocument.CheckDxfFileVersion(file, out isBinary);
	// netDxf is only compatible with AutoCad2000 and higher DXF version
	if (dxfVersion < DxfVersion.AutoCad2000) return;
	// load file
	DxfDocument loaded = DxfDocument.Load(file);
}
```

## Samples and Demos 
Are contained in the source code.
Well, at the moment they are just tests for the work in progress.
## Dependencies and distribution 
* .NET Framework 4.5. netDxf only references the NET libraries System.dll and System.Drawing.dll
## Compiling
To compile the source code you will need Visual Studio 2015.
## Development Status 
Stable. See [changelog.txt](https://github.com/haplokuon/netDxf/blob/master/doc/Changelog.txt) or the [wiki page](https://github.com/haplokuon/netDxf/wiki) for information on the latest changes.
## Supported entities
* 3dFace
* Arc
* Circle
* Dimensions (aligned, linear, radial, diametric, 3 point angular, 2 line angular, and ordinate)
* Ellipse
* Hatch (including Gradient patterns)
* Image
* Insert (block references and attributes)
* Leader
* Line
* LwPolyline (light weight polyline)
* Mesh
* MLine
* MText
* Point
* PolyfaceMesh
* Polyline
* Ray
* Shape
* Solid
* Spline
* Text
* Tolerance
* Trace
* Underlay (DGN, DWF, and PDF underlays)
* Wipeout
* XLine (aka construction line)

All entities can be grouped.
All entities and table objects may contain extended data information.
AutoCad Table entities will be imported as Inserts (block references).
Both simple and complex line types are supported.
The library will never be able to read some entities like Regions and 3dSolids, since they depend on undocumented proprietary data.
