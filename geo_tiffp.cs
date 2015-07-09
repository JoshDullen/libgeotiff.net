//*********************************************************************
//
// geo_tiffp.cs - Private interface for TIFF tag parsing.
//
//	Written by: Niles D. Ritter
//				The Authors
//
//	This interface file encapsulates the interface to external TIFF
//	file-io routines and definitions. The current configuration
//	assumes that the "libtiff" module is used, but if you have your
//	own TIFF reader, you may replace the definitions with your own
//	here, and replace the implementations in geo_tiffp.cs. No other
//	modules have any explicit dependence on external TIFF modules.
//
// Copyright (c) 1995, Niles D. Ritter
// Copyright (c) 2008 by the Authors
//
// Permission granted to use this software, so long as this copyright
// notice accompanies any products derived therefrom.
//
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

using Free.Ports.LibTiff;

namespace Free.Ports.LibGeoTiff
{
	public static partial class libgeotiff
	{
		const TIFFTAG GTIFF_GEOKEYDIRECTORY=TIFFTAG_GEOKEYDIRECTORY; // from xtiffio.cs
		const TIFFTAG GTIFF_DOUBLEPARAMS=TIFFTAG_GEODOUBLEPARAMS;
		const TIFFTAG GTIFF_ASCIIPARAMS=TIFFTAG_GEOASCIIPARAMS;
		const TIFFTAG GTIFF_PIXELSCALE=TIFFTAG_GEOPIXELSCALE;
		const TIFFTAG GTIFF_TRANSMATRIX=TIFFTAG_GEOTRANSMATRIX;
		const TIFFTAG GTIFF_INTERGRAPH_MATRIX=TIFFTAG_INTERGRAPH_MATRIX;
		const TIFFTAG GTIFF_TIEPOINTS=TIFFTAG_GEOTIEPOINTS;
		const TIFFTAG GTIFF_LOCAL=0;
	}

	// Method function pointer types
	public delegate bool GTGetFunction(TIFF tif, ushort tag, out int count, out object value);
	public delegate bool GTSetFunction(TIFF tif, ushort tag, int count, object value);
	public delegate tagtype_t GTTypeFunction(TIFF tif, ushort tag);

	public class TIFFMethod
	{
		public GTGetFunction get;
		public GTSetFunction set;
		public GTTypeFunction type;
	}

	//*********************************************************************
	//
	// geo_tiffp.cs Private TIFF interface module for GEOTIFF
	//
	//	This module implements the interface between the GEOTIFF
	//	tag parser and the TIFF i/o module. The current setup
	//	relies on the "libtiff" code, but if you use your own
	//	TIFF reader software, you may replace the module implementations
	//	here with your own calls. No "libtiff" dependencies occur
	//	anywhere else in this code.
	//
	//*********************************************************************/

	public static partial class libgeotiff
	{
		// tiff size array global
		static readonly uint[] _gtiff_size=new uint[] { 0, 1, 2, 4, 8, 1, 4, 8, 1, 2, 4, 1 };

		// Set up default TIFF handlers.
		static void _GTIFSetDefaultTIFF(TIFFMethod method)
		{
			if(method==null) return;

			method.get=_GTIFGetField;
			method.set=_GTIFSetField;
			method.type=_GTIFTagType;
		}

		// returns the value of TIFF tag <tag>, or if
		// the value is an array.
		static bool _GTIFGetField(TIFF tif, ushort tag, out int count, out object val)
		{
			object[] ap=new object[2];
			count=0;
			val=null;

			if(!libtiff.TIFFGetField(tif, (TIFFTAG)tag, ap)) return false;
			count=libtiff.__GetAsInt(ap, 0);
			val=ap[1];

			return true;
		}

		// Set a GeoTIFF TIFF field.
		static bool _GTIFSetField(TIFF tif, ushort tag, int count, object value)
		{
			return libtiff.TIFFSetField(tif, (TIFFTAG)tag, count, value);
		}

		// This routine is supposed to return the TagType of the <tag>
		// TIFF tag. Unfortunately, "libtiff" does not provide this
		// service by default, so we just have to "know" what type of tags
		// we've got, and how many. We only define the ones Geotiff
		// uses here, and others return UNKNOWN. The "tif" parameter
		// is provided for those TIFF implementations that provide
		// for tag-type queries.
		static tagtype_t _GTIFTagType(TIFF tif, ushort tag)
		{
			switch((TIFFTAG)tag)
			{
				case GTIFF_ASCIIPARAMS: return tagtype_t.TYPE_ASCII;
				case GTIFF_PIXELSCALE:
				case GTIFF_TRANSMATRIX:
				case GTIFF_TIEPOINTS:
				case GTIFF_DOUBLEPARAMS: return tagtype_t.TYPE_DOUBLE;
				case GTIFF_GEOKEYDIRECTORY: return tagtype_t.TYPE_SHORT;
				default: return tagtype_t.TYPE_UNKNOWN;
			}
		}
	}
}

