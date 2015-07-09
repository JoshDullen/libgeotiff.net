//*********************************************************************
//
// geo_keyp.cs - private interface for GeoTIFF geokey tag parsing
//
//	Written by: Niles D. Ritter
//				The Authors
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
	// This structure contains the internal program
	// representation of the key entry.
	class GeoKey
	{
		public geokey_t gk_key;		// GeoKey ID
		public tagtype_t gk_type;	// TIFF data type
		public int gk_count;		// number of values
		public object gk_data;		// pointer to data, or value
	}

	public class GTIF
	{
		internal TIFF gt_tif;			// TIFF file descriptor
		internal TIFFMethod gt_methods;	// TIFF i/o methods
		internal gtiff_flags gt_flags;	// file flags

		internal ushort gt_version;		// GeoTIFF Version
		internal ushort gt_rev_major;	// GeoKey Key Revision
		internal ushort gt_rev_minor;	// GeoKey Code Revision

		internal Dictionary<geokey_t, GeoKey> gt_keys;	// array of keys
	}

	enum gtiff_flags
	{
		FLAG_FILE_OPEN=1,
		FLAG_FILE_MODIFIED=2
	}

	static partial class libgeotiff
	{
		const int MAX_KEYINDEX=65535;	// largest possible key
		const int MAX_KEYS=100;			// maximum keys in a file
		const int MAX_VALUES=1000;		// maximum values in a tag
	}
}
