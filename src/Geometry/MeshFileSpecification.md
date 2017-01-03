For anyone who might be curious about ROBLOX's mesh format, heres some specifications me and my friend were able to come up with:

Header
---

All .mesh files start with a header indicating what version it is.
So far, we've isolated 3 versions that exist:

* version 1.00
* version 1.01
* version 2.00

General notes about the mesh format
---
* The texture coordinate uses Vector3, but the Z axis is always set to 0, since its technically a Vector2.

version 1.00
----
This format is stored in ASCII, and there are 3 lines present in this file.

* The 1st line is the version header, as mentioned earlier.
* The 2nd line is a number indicating how many polygons are in the mesh.
* The 3rd line is a series of Vector3 strings encapsulated by brackets. For every polygon, you should expect 3 sets of 3 Vector3 pairs listed in the following order to define a polygon:
 * Vertex
 * Surface Normal
 * Texture Coordinate
* Notes:
 * With version 1.00, you are expected to scale the mesh down by 0.5 after parsing.
 * The texture coordinate's Y axis is flipped, so you need to read it as 1 - Y.
 
version 1.01
---

* This version is identical to version 1.00, except you don't need to scale the mesh down by 0.5 after parsing.

version 2.00
---

The primary difference between this version and the other versions, is that this format uses binary instead of ASCII. This significantly compresses the file size. In addition, it offers support to recycle vertices being used by multiple polygons.

The format uses two number types primarily:

* UInt32 
* float

Both number types are stored as 4-bytes using Little Endian.

* The file starts with a 17 byte header.
 * The first 12 bytes being the "version 2.00" text, and the last 5 always being some stub data (0A 0C 00 24 0C) that isn't necessary to read the mesh. 
* The next 4 bytes represent a UInt32, being the number of vertices to read
* The next 4 bytes represent a UInt32, being the number of polygons to build.
* From there, there are _numVertices * 36_ bytes to read.
 * Each set of 36 bytes gets split into 3 sets of 12 bytes. 
 * The sets of 12 bytes represent XYZ coordinates, so you read them 4 bytes at a time and convert them to floats.
 * The 3 sets define's a vertice's location, surface normal, and texture coordinate.
 * You should then store this data in an array somewhere for later.
* Finally, there will be _numPolygons * 3_ bytes to read, as UInt32 values.
 * For every 3 int values, use those values as 0-based indexes to fetch a vertice data set, and then use those 3 data sets to form a polygon.
