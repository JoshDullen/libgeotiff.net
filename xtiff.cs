//*********************************************************************
//
// xtiff.cs
//
// Extended TIFF Directory GEO Tag Support.
//
// You may use this file as a template to add your own
// extended tags to the library. Only the parts of the code
// marked with "XXX" require modification.
//
// Authors:	Niles D. Ritter
//			The Authors
//
// Revisions:
//	30 Sep 2008		--	Port to C#. --The Authors.
//	18 Sep 1995		--	Deprecated Integraph Matrix tag with new one.
//						Backward compatible support provided. --NDR.
//
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;

using Free.Ports.LibTiff;

namespace Free.Ports.LibGeoTiff
{
	public static partial class libgeotiff
	{
		// Define public Tag names and values here
		// tags 33550 is a private tag registered to SoftDesk, Inc
		public const TIFFTAG TIFFTAG_GEOPIXELSCALE=(TIFFTAG)33550;
		// tags 33920-33921 are private tags registered to Intergraph, Inc
		public const TIFFTAG TIFFTAG_INTERGRAPH_MATRIX=(TIFFTAG)33920;	// $use TIFFTAG_GEOTRANSMATRIX !
		public const TIFFTAG TIFFTAG_GEOTIEPOINTS=(TIFFTAG)33922;
		// tags 34263-34264 are private tags registered to NASA-JPL Carto Group
#if JPL_TAG_SUPPORT
		public const TIFFTAG TIFFTAG_JPL_CARTO_IFD=(TIFFTAG)34263;		// $use GeoProjectionInfo !
#endif
		public const TIFFTAG TIFFTAG_GEOTRANSMATRIX=(TIFFTAG)34264;		// New Matrix Tag replaces 33920
		// tags 34735-3438 are private tags registered to SPOT Image, Inc
		public const TIFFTAG TIFFTAG_GEOKEYDIRECTORY=(TIFFTAG)34735;
		public const TIFFTAG TIFFTAG_GEODOUBLEPARAMS=(TIFFTAG)34736;
		public const TIFFTAG TIFFTAG_GEOASCIIPARAMS=(TIFFTAG)34737;

		// Define Printing method flags. These
		// flags may be passed in to TIFFPrintDirectory() to
		// indicate that those particular field values should
		// be printed out in full, rather than just an indicator
		// of whether they are present or not.
		public const TIFFPRINT TIFFPRINT_GEOKEYDIRECTORY=(TIFFPRINT)0x80000000;
		public const TIFFPRINT TIFFPRINT_GEOKEYPARAMS=(TIFFPRINT)0x40000000;

		// Tiff info structure.
		//
		//	Entry format: { TAGNUMBER, ReadCount, WriteCount, DataType, FIELDNUM, OkToChange, PassDirCountOnSet, AsciiName }
		//
		//	For ReadCount, WriteCount, -1 = unknown.
		static List<TIFFFieldInfo> xtiffFieldInfo=MakeXTiffFieldInfo();

		static List<TIFFFieldInfo> MakeXTiffFieldInfo()
		{
			List<TIFFFieldInfo> ret=new List<TIFFFieldInfo>();

			// XXX Insert Your tags here
			ret.Add(new TIFFFieldInfo(TIFFTAG_GEOPIXELSCALE, -1, -1, TIFFDataType.TIFF_DOUBLE, FIELD.CUSTOM, true, true, "GeoPixelScale"));
			ret.Add(new TIFFFieldInfo(TIFFTAG_INTERGRAPH_MATRIX, -1, -1, TIFFDataType.TIFF_DOUBLE, FIELD.CUSTOM, true, true, "Intergraph TransformationMatrix"));
			ret.Add(new TIFFFieldInfo(TIFFTAG_GEOTRANSMATRIX, -1, -1, TIFFDataType.TIFF_DOUBLE, FIELD.CUSTOM, true, true, "GeoTransformationMatrix"));
			ret.Add(new TIFFFieldInfo(TIFFTAG_GEOTIEPOINTS, -1, -1, TIFFDataType.TIFF_DOUBLE, FIELD.CUSTOM, true, true, "GeoTiePoints"));
			ret.Add(new TIFFFieldInfo(TIFFTAG_GEOKEYDIRECTORY, -1, -1, TIFFDataType.TIFF_SHORT, FIELD.CUSTOM, true, true, "GeoKeyDirectory"));
			ret.Add(new TIFFFieldInfo(TIFFTAG_GEODOUBLEPARAMS, -1, -1, TIFFDataType.TIFF_DOUBLE, FIELD.CUSTOM, true, true, "GeoDoubleParams"));
			ret.Add(new TIFFFieldInfo(TIFFTAG_GEOASCIIPARAMS, -1, -1, TIFFDataType.TIFF_ASCII, FIELD.CUSTOM, true, true, "GeoASCIIParams"));
#if JPL_TAG_SUPPORT
			ret.Add(new TIFFFieldInfo(TIFFTAG_JPL_CARTO_IFD, 1, 1, TIFFDataType.TIFF_LONG, FIELD.CUSTOM, true, true, "JPL Carto IFD offset")); // Don't use this!
#endif
			return ret;
		}

		static void XTIFFLocalDefaultDirectory(TIFF tif)
		{
			// Install the extended Tag field info
			libtiff._TIFFMergeFieldInfo(tif, xtiffFieldInfo);
		}

		//*********************************************************************
		//	Nothing below this line should need to be changed.
		//*********************************************************************

		static TIFFExtendProc _ParentExtender;

		// This is the callback procedure, and is
		// called by the DefaultDirectory method
		// every time a new TIFF directory is opened.
		static void XTIFFDefaultDirectory(TIFF tif)
		{
			// set up our own defaults
			XTIFFLocalDefaultDirectory(tif);

			// Since an XTIFF client module may have overridden
			// the default directory method, we call it now to
			// allow it to set up the rest of its own methods.
			if(_ParentExtender!=null) _ParentExtender(tif);
		}

		// Registers an extension with libtiff for adding GeoTIFF tags.
		// After this one-time intialization, any TIFF open function may be called in
		// the usual manner to create a TIFF file that compatible with libgeotiff.
		// The XTIFF open functions are simply for convenience: they call this
		// and then pass their parameters on to the appropriate TIFF open function.
		//
		// <p>This function may be called any number of times safely, since it will
		// only register the extension the first time it is called.
		static bool first_time=true;
		public static void XTIFFInitialize()
		{
			if(!first_time) return; // Been there. Done that.
			first_time=false;

			// Grab the inherited method and install
			_ParentExtender=libtiff.TIFFSetTagExtender(XTIFFDefaultDirectory);
		}

		// GeoTIFF compatible TIFF file open function.
		//
		// @param name The filename of a TIFF file to open.
		// @param mode The open mode ("r", "w" or "a").
		//
		// @return a TIFF for the file, or NULL if the open failed.
		
		// This function is used to open GeoTIFF files instead of TIFFOpen() from
		// libtiff. Internally it calls TIFFOpen(), but sets up some extra hooks
		// so that GeoTIFF tags can be extracted from the file. If XTIFFOpen() isn't
		// used, GTIFNew() won't work properly. Files opened
		// with XTIFFOpen() should be closed with XTIFFClose().

		// The name of the file to be opened should be passed as <b>name</b>, and an
		// opening mode ("r", "w" or "a") acceptable to TIFFOpen() should be passed as the
		// <b>mode</b>.

		// If XTIFFOpen() fails it will return NULL. Otherwise, normal TIFFOpen()
		// error reporting steps will have already taken place.
		public static TIFF XTIFFOpen(string name, string mode)
		{
			// Set up the callback
			XTIFFInitialize();

			// Open the file; the callback will set everything up
			return libtiff.TIFFOpen(name, mode);
		}

		public static TIFF XTIFFFdOpen(Stream fd, string name, string mode)
		{
			// Set up the callback
			XTIFFInitialize();

			// Open the file; the callback will set everything up
			return libtiff.TIFFFdOpen(fd, name, mode);
		}

		public static TIFF XTIFFClientOpen(string name, string mode, Stream thehandle, TIFFReadWriteProc RWProc, TIFFReadWriteProc RWProc2, TIFFSeekProc SProc, TIFFCloseProc CProc, TIFFSizeProc SzProc)
		{
			// Set up the callback
			XTIFFInitialize();

			// Open the file; the callback will set everything up
			return libtiff.TIFFClientOpen(name, mode, thehandle, RWProc, RWProc2, SProc, CProc, SzProc);
		}

		// Close a file opened with XTIFFOpen().
		//
		// @param tif The file handle returned by XTIFFOpen().
		//
		// If a GTIF structure was created with GTIFNew()
		// for this file, it should be freed with GTIFFree()
		// <i>before</i> calling XTIFFClose().
		public static void XTIFFClose(TIFF tif)
		{
			libtiff.TIFFClose(tif);
		}
	}
}
