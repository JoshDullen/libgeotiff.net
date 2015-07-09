//*********************************************************************
//
// geo_set.cs -- Public routines for GEOTIFF GeoKey access.
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

using Free.Ports.LibTiff;

namespace Free.Ports.LibGeoTiff
{
	public static partial class libgeotiff
	{
		// This function writes a geokey_t value to a GeoTIFF file.
		//
		// @param gtif The geotiff information handle from GTIFNew().
		//
		// @param keyID The geokey_t name (such as ProjectedCSTypeGeoKey).
		// This must come from the list of legal geokey_t values
		// (an enumeration) listed below.
		//
		// Note that key values aren't actually flushed to the file until
		// GTIFWriteKeys() is called. Till then
		// the new values are just kept with the GTIF structure.
		//
		// Example:
		//
		//	GTIFKeySet(gtif, GTRasterTypeGeoKey, RasterPixelIsArea);
		//	GTIFKeySet(gtif, GTCitationGeoKey, "UTM 11 North / NAD27");
		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, geodeticdatum_t val)
		{
			ushort[] val1=new ushort[] { (ushort)val };
			return GTIFKeySet(gtif, keyID, val1);
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, geographic_t val)
		{
			ushort[] val1=new ushort[] { (ushort)val };
			return GTIFKeySet(gtif, keyID, val1);
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, geounits_t val)
		{
			ushort[] val1=new ushort[] { (ushort)val };
			return GTIFKeySet(gtif, keyID, val1);
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, ellipsoid_t val)
		{
			ushort[] val1=new ushort[] { (ushort)val };
			return GTIFKeySet(gtif, keyID, val1);
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, pcstype_t val)
		{
			ushort[] val1=new ushort[] { (ushort)val };
			return GTIFKeySet(gtif, keyID, val1);
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, primemeridian_t val)
		{
			ushort[] val1=new ushort[] { (ushort)val };
			return GTIFKeySet(gtif, keyID, val1);
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, modeltype_t val)
		{
			ushort[] val1=new ushort[] { (ushort)val };
			return GTIFKeySet(gtif, keyID, val1);
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, projection_t val)
		{
			ushort[] val1=new ushort[] { (ushort)val };
			return GTIFKeySet(gtif, keyID, val1);
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, coordtrans_t val)
		{
			ushort[] val1=new ushort[] { (ushort)val };
			return GTIFKeySet(gtif, keyID, val1);
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, rastertype_t val)
		{
			ushort[] val1=new ushort[] { (ushort)val };
			return GTIFKeySet(gtif, keyID, val1);
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, vdatum_t val)
		{
			ushort[] val1=new ushort[] { (ushort)val };
			return GTIFKeySet(gtif, keyID, val1);
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, vertcstype_t val)
		{
			ushort[] val1=new ushort[] { (ushort)val };
			return GTIFKeySet(gtif, keyID, val1);
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, ushort val)
		{
			ushort[] val1=new ushort[] { val };
			return GTIFKeySet(gtif, keyID, val1);
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, ushort[] val)
		{
			if(val==null) // delete the indicated tag
			{
				if(!gtif.gt_keys.ContainsKey(keyID)) return false;
				gtif.gt_keys.Remove(keyID);
				gtif.gt_flags|=gtiff_flags.FLAG_FILE_MODIFIED;
				return true;
			}

			if(gtif.gt_keys.ContainsKey(keyID))
			{
				gtif.gt_keys.Remove(keyID);
				gtif.gt_flags|=gtiff_flags.FLAG_FILE_MODIFIED;
			}

			// We need to create the key
			try
			{
				GeoKey key=new GeoKey();
				key.gk_key=keyID;
				key.gk_type=tagtype_t.TYPE_SHORT;
				key.gk_count=val.Length;

				ushort[] tmp=new ushort[val.Length];
				val.CopyTo(tmp, 0);
				key.gk_data=tmp;

				gtif.gt_keys.Add(keyID, key);
				gtif.gt_flags|=gtiff_flags.FLAG_FILE_MODIFIED;
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, double val)
		{
			double[] val1=new double[] { val };
			return GTIFKeySet(gtif, keyID, val1);
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, double[] val)
		{
			if(val==null) // delete the indicated tag
			{
				if(!gtif.gt_keys.ContainsKey(keyID)) return false;
				gtif.gt_keys.Remove(keyID);
				gtif.gt_flags|=gtiff_flags.FLAG_FILE_MODIFIED;
				return true;
			}

			if(gtif.gt_keys.ContainsKey(keyID))
			{
				gtif.gt_keys.Remove(keyID);
				gtif.gt_flags|=gtiff_flags.FLAG_FILE_MODIFIED;
			}
			
			// We need to create the key
			try
			{
				GeoKey key=new GeoKey();
				key.gk_key=keyID;
				key.gk_type=tagtype_t.TYPE_DOUBLE;
				key.gk_count=val.Length;

				double[] tmp=new double[val.Length];
				val.CopyTo(tmp, 0);
				key.gk_data=tmp;

				gtif.gt_keys.Add(keyID, key);
				gtif.gt_flags|=gtiff_flags.FLAG_FILE_MODIFIED;
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool GTIFKeySet(GTIF gtif, geokey_t keyID, string val)
		{
			if(val==null) // delete the indicated tag
			{
				if(!gtif.gt_keys.ContainsKey(keyID)) return false;
				gtif.gt_keys.Remove(keyID);
				gtif.gt_flags|=gtiff_flags.FLAG_FILE_MODIFIED;
				return true;
			}

			if(gtif.gt_keys.ContainsKey(keyID))
			{
				gtif.gt_keys.Remove(keyID);
				gtif.gt_flags|=gtiff_flags.FLAG_FILE_MODIFIED;
			}

			val=val.Trim('\0');

			// We need to create the key
			try
			{
				GeoKey key=new GeoKey();
				key.gk_key=keyID;
				key.gk_type=tagtype_t.TYPE_ASCII;
				key.gk_count=val.Length;
				key.gk_data=val.Substring(0, val.Length);
				gtif.gt_keys.Add(keyID, key);
				gtif.gt_flags|=gtiff_flags.FLAG_FILE_MODIFIED;
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}