//*********************************************************************
//
// geo_write.cs -- Public routines for GEOTIFF GeoKey access.
//
//	Written By: Niles D. Ritter
//				The Authors
//
// Copyright (c) 1995 Niles D. Ritter
// Copyright (c) 2008 by the Authors
//
// Permission granted to use this software, so long as this copyright
// notice accompanies any source code derived therefrom.
//
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Free.Ports.LibGeoTiff
{
	public static partial class libgeotiff
	{
		// This function flushes all the GeoTIFF keys that have been set with the 
		// GTIFKeySet() function into the associated 
		// TIFF file.
		//
		// @param gt The GeoTIFF handle returned by GTIFNew.
		//
		// GTIFWriteKeys() should be called before
		// GTIFFree() is used to deallocate a GeoTIFF access handle.
		public static bool GTIFWriteKeys(GTIF gt)
		{
			if((gt.gt_flags&gtiff_flags.FLAG_FILE_MODIFIED)==0) return true;
			if(gt.gt_tif==null) return false;

			List<geokey_t> keys=new List<geokey_t>(gt.gt_keys.Keys);
			keys.Sort();

			List<ushort> shorts=new List<ushort>();
			List<ushort> shortsValues=new List<ushort>();
			List<double> doubles=new List<double>();
			string strings="";

			// Set up header of ProjectionInfo tag
			shorts.Add((ushort)GvCurrentVersion);
			shorts.Add((ushort)GvCurrentRevision);
			shorts.Add((ushort)GvCurrentMinorRev);
			shorts.Add((ushort)keys.Count);

			int shortOffset=4+keys.Count*4;

			foreach(geokey_t key in keys)
			{
				GeoKey keyptr=gt.gt_keys[key];
				if(keyptr.gk_type==tagtype_t.TYPE_ASCII)
				{
					string str=keyptr.gk_data as string;
					if(str==null) str="";

					str=str.Trim('\0');
					str+="|";
					str=str.Replace('\0', '|');

					shorts.Add((ushort)key);
					shorts.Add((ushort)GTIFF_ASCIIPARAMS);
					shorts.Add((ushort)str.Length);
					shorts.Add((ushort)strings.Length);
					strings+=str;

					continue;
				}

				if(keyptr.gk_type==tagtype_t.TYPE_DOUBLE)
				{
					double[] dbl=keyptr.gk_data as double[];
					if(dbl==null)
					{
						shorts.Add((ushort)key);
						shorts.Add((ushort)GTIFF_DOUBLEPARAMS);
						shorts.Add((ushort)0);
						shorts.Add((ushort)0);
					}
					else
					{
						shorts.Add((ushort)key);
						shorts.Add((ushort)GTIFF_DOUBLEPARAMS);
						shorts.Add((ushort)dbl.Length);
						shorts.Add((ushort)doubles.Count);
						doubles.AddRange(dbl);
					}
					continue;
				}

				ushort[] sht=keyptr.gk_data as ushort[];
				if(sht==null)
				{
					shorts.Add((ushort)key);
					shorts.Add((ushort)GTIFF_LOCAL);
					shorts.Add((ushort)0);
					shorts.Add((ushort)0);
				}
				else
				{
					if(sht.Length<2)
					{
						shorts.Add((ushort)key);
						shorts.Add((ushort)GTIFF_LOCAL);
						shorts.Add((ushort)sht.Length);
						if(sht.Length==1) shorts.Add(sht[0]);
						else shorts.Add(0);
					}
					else
					{
						shorts.Add((ushort)key);
						shorts.Add((ushort)GTIFF_GEOKEYDIRECTORY);
						shorts.Add((ushort)sht.Length);
						shorts.Add((ushort)(shortOffset+shortsValues.Count));
						shortsValues.AddRange(sht);
					}
				}
			}

			if(shorts.Count!=shortOffset) return false;
			shorts.AddRange(shortsValues);

			// Write out the Key Directory
			gt.gt_methods.set(gt.gt_tif, (ushort)GTIFF_GEOKEYDIRECTORY, shorts.Count, shorts.ToArray());

			// Write out the params directories
			if(doubles.Count>0) gt.gt_methods.set(gt.gt_tif, (ushort)GTIFF_DOUBLEPARAMS, doubles.Count, doubles.ToArray());
			if(strings.Length>0) gt.gt_methods.set(gt.gt_tif, (ushort)GTIFF_ASCIIPARAMS, strings.Length, strings);

			gt.gt_flags&=~gtiff_flags.FLAG_FILE_MODIFIED;

			return true;
		}
	}
}
