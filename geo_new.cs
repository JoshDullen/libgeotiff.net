//*********************************************************************
//
// geo_new.cs -- Public routines for GEOTIFF GeoKey access.
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
//	20 June, 1995	Niles D. Ritter			New
//	07 July, 1995	Greg Martin				Fix index
//
// Revision 1.14' 2008/09/30 theauthors
// Port to C#
//
// Revision 1.14 2008/05/09 18:37:46 fwarmerdam
// add support for simple tags api
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
		//*********************************************************************
		//
		//							Public Routines
		//
		//*********************************************************************

		// Given an open TIFF file, look for GTIF keys and values and return GTIF structure.
		//
		// This function creates a GeoTIFF information interpretation handle
		// (GTIF) based on a passed in TIFF handle originally from
		// XTIFFOpen().
		//
		// The returned GTIF handle can be used to read or write GeoTIFF tags
		// using the various GTIF functions. The handle should be destroyed using
		// GTIFFree() before the file is closed with TIFFClose().
		//
		// If the file accessed has no GeoTIFF keys, an valid (but empty) GTIF is
		// still returned. GTIFNew() is used both for existing files being read, and
		// for new TIFF files that will have GeoTIFF tags written to them.

		public static GTIF GTIFNew(TIFF tif)
		{
			TIFFMethod default_methods=new TIFFMethod();
			_GTIFSetDefaultTIFF(default_methods);

			return GTIFNewWithMethods(tif, default_methods);
		}

		// **************************************************************************
		// *						GTIFNewWithMethods()							*
		// *																		*
		// *	Create a new geotiff, passing in the methods structure to			*
		// *	support not libtiff implementations without replacing the			*
		// *	default methods.													*
		// **************************************************************************
		public static GTIF GTIFNewWithMethods(TIFF tif, TIFFMethod methods)
		{
			GTIF gt=null;

			try
			{
				gt=new GTIF();
				gt.gt_methods=new TIFFMethod();
				gt.gt_keys=new Dictionary<geokey_t, GeoKey>();
			}
			catch
			{
				return null;
			}

			// install TIFF file and I/O methods
			gt.gt_tif=tif;
			gt.gt_methods.get=methods.get;
			gt.gt_methods.set=methods.set;
			gt.gt_methods.type=methods.type;

			gt.gt_rev_major=GvCurrentRevision;
			gt.gt_rev_minor=GvCurrentMinorRev;
			gt.gt_version=GvCurrentVersion;

			if(tif==null) return gt;

			object data;
			int nshorts;

			// since this is an array, GTIF will allocate the memory
			if(!gt.gt_methods.get(tif, (ushort)GTIFF_GEOKEYDIRECTORY, out nshorts, out data)) return gt;

			if(nshorts<4) return null;
			ushort[] shorts=data as ushort[];
			if(shorts==null||shorts.Length<4) return null;
			
			if(shorts[0]>GvCurrentVersion) return null;
			gt.gt_version=shorts[0];

			if(shorts[1]>GvCurrentRevision)
			{
				// issue warning
			}
			gt.gt_rev_major=shorts[1];
			gt.gt_rev_minor=shorts[2];

			int count=shorts[3];
			if(count==0) return gt;
			if(shorts.Length<(count*4+4)) return null;

			// If we got here, then the geokey can be parsed

			// Get the PARAMS Tags, if any
			int ndoubles;
			double[] doubles;
			if(!gt.gt_methods.get(tif, (ushort)GTIFF_DOUBLEPARAMS, out ndoubles, out data))
			{
				try
				{
					doubles=new double[MAX_VALUES];
				}
				catch
				{
					return null;
				}
			}
			else
			{
				doubles=data as double[];
				if(doubles==null) return null;
			}

			int stringsLength=0;
			string strings=null;
			if(gt.gt_methods.get(tif, (ushort)GTIFF_ASCIIPARAMS, out stringsLength, out data))
			{
				strings=data as string;
				if(strings==null) return null;
				strings=strings.TrimEnd('\0'); // last NULL doesn't count; "|" used for delimiter
				stringsLength=strings.Length;
			}

			for(int i=0; i<count; i++)
			{
				ushort KeyID=shorts[i*4+4];
				ushort TIFFTagLocation=shorts[i*4+5];
				ushort Count=shorts[i*4+6];
				ushort Value_Offset=shorts[i*4+7];

				GeoKey keyptr;

				try
				{
					keyptr=new GeoKey();

					keyptr.gk_key=(geokey_t)KeyID;

					if(TIFFTagLocation!=0) keyptr.gk_type=gt.gt_methods.type(gt.gt_tif, TIFFTagLocation);
					else keyptr.gk_type=tagtype_t.TYPE_SHORT; //gt.gt_methods.type(gt.gt_tif, (ushort)GTIFF_GEOKEYDIRECTORY);

					switch(TIFFTagLocation)
					{
						case (ushort)GTIFF_LOCAL:
							// store value into data value
							keyptr.gk_data=new ushort[] { Value_Offset };
							keyptr.gk_count=1;
							break;
						case (ushort)GTIFF_GEOKEYDIRECTORY:
							ushort[] s=new ushort[Count];
							keyptr.gk_data=s;
							Array.Copy(shorts, Value_Offset, s, 0, Count);
							keyptr.gk_count=Count;
							break;
						case (ushort)GTIFF_DOUBLEPARAMS:
							double[] d=new double[Count];
							keyptr.gk_data=d;
							Array.Copy(doubles, Value_Offset, d, 0, Count);
							keyptr.gk_count=Count;
							break;
						case (ushort)GTIFF_ASCIIPARAMS:
							string str=strings.Substring(Value_Offset, Count);
							str=str.Replace('|', '\0');
							str=str.Trim('\0');
							keyptr.gk_count=str.Length;
							keyptr.gk_data=str;
							break;
						default:
							return null; // failure
					}
				}
				catch
				{
					return null;
				}

				gt.gt_keys.Add(keyptr.gk_key, keyptr);
			}

			return gt;
		}
	}
}
