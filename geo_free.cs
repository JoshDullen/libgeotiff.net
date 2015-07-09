//*********************************************************************
//
// geo_free.cs -- Public routines for GEOTIFF GeoKey access.
//
//	Written By: Niles D. Ritter
//				The Authors
//
// Copyright (c) 1995 Niles D. Ritter
// Copyright (c) 2008 by the Authors
//
// Permission granted to use this software, so long as this copyright
// notice accompanies any products derived therefrom.
//
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Free.Ports.LibGeoTiff
{
	public static partial class libgeotiff
	{
		//*********************************************************************
		//
		//							Public Routines
		//
		//*********************************************************************

		// This function deallocates an existing GeoTIFF access handle previously
		// created with GTIFNew(). If the handle was
		// used to write GeoTIFF keys to the TIFF file, the
		// GTIFWriteKeys() function should be used
		// to flush results to the file before calling GTIFFree(). GTIFFree()
		// should be called before XTIFFClose() is
		// called on the corresponding TIFF file handle.
		public static void GTIFFree(GTIF gtif)
		{
			if(gtif==null) return;
			gtif.gt_keys.Clear();
		}
	}
}
