//*********************************************************************
//
// geo_get.cs -- Public routines for GEOTIFF GeoKey access.
//
//	Written By: Niles D. Ritter
//				The Authors
//
// Copyright (c) 1995 Niles D. Ritter
// Copyright (c) 2008 by the Authors
//
//	Permission granted to use this software, so long as this copyright
//	notice accompanies any products derived therefrom.
//
//	Revision History;
//
//	20 June, 1995	Niles D. Ritter			New
//	03 July, 1995	Greg Martin				Fix strings and index
//	06 July, 1995	Niles D. Ritter			Unfix indexing.
//	30 Sept, 2008	The Authors				Port to C#
//
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Free.Ports.LibGeoTiff
{
	public static partial class libgeotiff
	{
		// return the Header info of this geotiff file
		public static void GTIFDirectoryInfo(GTIF gtif, int[] version, out int keycount)
		{
			if(version!=null&&version.Length>=3)
			{
				version[0]=gtif.gt_version;
				version[1]=gtif.gt_rev_major;
				version[2]=gtif.gt_rev_minor;
			}
			keycount=gtif.gt_keys.Count;
		}

		public static int GTIFKeyInfo(GTIF gtif, geokey_t key, out tagtype_t type)
		{
			type=tagtype_t.TYPE_UNKNOWN;
			if(!gtif.gt_keys.ContainsKey(key)) return 0;
			GeoKey keyptr=gtif.gt_keys[key];
			type=keyptr.gk_type;
			return keyptr.gk_count;
		}

		// This function reads the value of a single GeoKey from a GeoTIFF file.
		//
		// @param gtif The geotiff information handle from GTIFNew().
		//
		// @param thekey The geokey_t name (such as ProjectedCSTypeGeoKey).
		// This must come from the list of legal geokey_t values
		// (an enumeration) listed below.
		//
		// @param val The <b>val</b> argument is a pointer to the
		// variable into which the value should be read. The type of the variable
		// varies depending on the geokey_t given. While there is no ready mapping
		// of geokey_t values onto types, in general code values are of type <i>short</i>,
		// citations are strings, and everything else is of type <i>double</i>. Note
		// that pointer's to <i>int</i> should never be passed to GTIFKeyGet() for
		// integer values as they will be shorts, and the int's may not be properly
		// initialized (and will be grossly wrong on MSB systems).
		//
		// @param index Indicates how far into the list of values
		// for this geokey to offset. Should normally be zero.
		//
		// @param count Indicates how many values
		// to read. At this time all keys except for strings have only one value,
		// so <b>index</b> should be zero, and <b>count</b> should be one.
		//
		// @return The GTIFKeyGet() function returns the number of values read. Normally
		// this would be one if successful or zero if the key doesn't exist for this
		// file.
		//
		// From geokeys.inc we see the following geokey_t values are possible:<p>
		//
		// -- 6.2.1 GeoTIFF Configuration Keys --
		//
		// ValuePair(GTModelTypeGeoKey, 1024)	-- Section 6.3.1.1 Codes
		// ValuePair(GTRasterTypeGeoKey, 1025)	-- Section 6.3.1.2 Codes
		// ValuePair(GTCitationGeoKey, 1026)	-- documentation
		//
		// -- 6.2.2 Geographic CS Parameter Keys --
		//
		// ValuePair(GeographicTypeGeoKey, 2048)		-- Section 6.3.2.1 Codes
		// ValuePair(GeogCitationGeoKey, 2049)			-- documentation
		// ValuePair(GeogGeodeticDatumGeoKey, 2050)		-- Section 6.3.2.2 Codes
		// ValuePair(GeogPrimeMeridianGeoKey, 2051)		-- Section 6.3.2.4 codes
		// ValuePair(GeogLinearUnitsGeoKey, 2052)		-- Section 6.3.1.3 Codes
		// ValuePair(GeogLinearUnitSizeGeoKey, 2053)	-- meters
		// ValuePair(GeogAngularUnitsGeoKey, 2054)		-- Section 6.3.1.4 Codes
		// ValuePair(GeogAngularUnitSizeGeoKey, 2055)	-- radians
		// ValuePair(GeogEllipsoidGeoKey, 2056)			-- Section 6.3.2.3 Codes
		// ValuePair(GeogSemiMajorAxisGeoKey, 2057)		-- GeogLinearUnits
		// ValuePair(GeogSemiMinorAxisGeoKey, 2058)		-- GeogLinearUnits
		// ValuePair(GeogInvFlatteningGeoKey, 2059)		-- ratio
		// ValuePair(GeogAzimuthUnitsGeoKey, 2060)		-- Section 6.3.1.4 Codes
		// ValuePair(GeogPrimeMeridianLongGeoKey, 2061)	-- GeoAngularUnit
		// ValuePair(GeogTOWGS84GeoKey, 2062)			-- 2011 - proposed addition 
		//
		// -- 6.2.3 Projected CS Parameter Keys --
		// --	Several keys have been renamed,--
		// --	and the deprecated names aliased for backward compatibility --
		//
		// ValuePair(ProjectedCSTypeGeoKey, 3072)			-- Section 6.3.3.1 codes
		// ValuePair(PCSCitationGeoKey, 3073)				-- documentation
		// ValuePair(ProjectionGeoKey, 3074)				-- Section 6.3.3.2 codes
		// ValuePair(ProjCoordTransGeoKey, 3075)			-- Section 6.3.3.3 codes
		// ValuePair(ProjLinearUnitsGeoKey, 3076)			-- Section 6.3.1.3 codes
		// ValuePair(ProjLinearUnitSizeGeoKey, 3077)		-- meters
		// ValuePair(ProjStdParallel1GeoKey, 3078)			-- GeogAngularUnit
		// ValuePair(ProjStdParallelGeoKey,ProjStdParallel1GeoKey)
		// ValuePair(ProjStdParallel2GeoKey, 3079)			-- GeogAngularUnit
		// ValuePair(ProjNatOriginLongGeoKey, 3080)			-- GeogAngularUnit
		// ValuePair(ProjOriginLongGeoKey, ProjNatOriginLongGeoKey)
		// ValuePair(ProjNatOriginLatGeoKey, 3081)			-- GeogAngularUnit
		// ValuePair(ProjOriginLatGeoKey, ProjNatOriginLatGeoKey)
		// ValuePair(ProjFalseEastingGeoKey, 3082)			-- ProjLinearUnits
		// ValuePair(ProjFalseNorthingGeoKey, 3083)			-- ProjLinearUnits
		// ValuePair(ProjFalseOriginLongGeoKey, 3084)		-- GeogAngularUnit
		// ValuePair(ProjFalseOriginLatGeoKey, 3085)		-- GeogAngularUnit
		// ValuePair(ProjFalseOriginEastingGeoKey, 3086)	-- ProjLinearUnits
		// ValuePair(ProjFalseOriginNorthingGeoKey, 3087)	-- ProjLinearUnits
		// ValuePair(ProjCenterLongGeoKey, 3088)			-- GeogAngularUnit
		// ValuePair(ProjCenterLatGeoKey, 3089)				-- GeogAngularUnit
		// ValuePair(ProjCenterEastingGeoKey, 3090)			-- ProjLinearUnits
		// ValuePair(ProjCenterNorthingGeoKey, 3091)		-- ProjLinearUnits
		// ValuePair(ProjScaleAtNatOriginGeoKey, 3092)		-- ratio
		// ValuePair(ProjScaleAtOriginGeoKey, ProjScaleAtNatOriginGeoKey)
		// ValuePair(ProjScaleAtCenterGeoKey, 3093)			-- ratio
		// ValuePair(ProjAzimuthAngleGeoKey, 3094)			-- GeogAzimuthUnit
		// ValuePair(ProjStraightVertPoleLongGeoKey, 3095)	-- GeogAngularUnit
		//
		// 6.2.4 Vertical CS Keys
		//
		// ValuePair(VerticalCSTypeGeoKey, 4096)	-- Section 6.3.4.1 codes
		// ValuePair(VerticalCitationGeoKey, 4097)	-- documentation
		// ValuePair(VerticalDatumGeoKey, 4098)		-- Section 6.3.4.2 codes
		// ValuePair(VerticalUnitsGeoKey, 4099)		-- Section 6.3.1 (.x) codes

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out ushort val, int index, int count)
		{
			// ignore count
			val=0;
			ushort[] vals;
			if(GTIFKeyGet(gtif, thekey, out vals, index, 1)==0) return 0;
			val=vals[0];
			return 1;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out ushort val, int index)
		{
			return GTIFKeyGet(gtif, thekey, out val, index, 0);
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out ellipsoid_t val, int index, int count)
		{
			// ignore count
			val=0;
			ushort[] vals;
			if(GTIFKeyGet(gtif, thekey, out vals, index, 1)==0) return 0;
			val=(ellipsoid_t)vals[0];
			return 1;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out ellipsoid_t val, int index)
		{
			return GTIFKeyGet(gtif, thekey, out val, index, 0);
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out geodeticdatum_t val, int index, int count)
		{
			// ignore count
			val=0;
			ushort[] vals;
			if(GTIFKeyGet(gtif, thekey, out vals, index, 1)==0) return 0;
			val=(geodeticdatum_t)vals[0];
			return 1;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out geodeticdatum_t val, int index)
		{
			return GTIFKeyGet(gtif, thekey, out val, index, 0);
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out geographic_t val, int index, int count)
		{
			// ignore count
			val=0;
			ushort[] vals;
			if(GTIFKeyGet(gtif, thekey, out vals, index, 1)==0) return 0;
			val=(geographic_t)vals[0];
			return 1;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out geographic_t val, int index)
		{
			return GTIFKeyGet(gtif, thekey, out val, index, 0);
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out modeltype_t val, int index, int count)
		{
			// ignore count
			val=0;
			ushort[] vals;
			if(GTIFKeyGet(gtif, thekey, out vals, index, 1)==0) return 0;
			val=(modeltype_t)vals[0];
			return 1;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out modeltype_t val, int index)
		{
			return GTIFKeyGet(gtif, thekey, out val, index, 0);
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out geounits_t val, int index, int count)
		{
			// ignore count
			val=0;
			ushort[] vals;
			if(GTIFKeyGet(gtif, thekey, out vals, index, 1)==0) return 0;
			val=(geounits_t)vals[0];
			return 1;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out geounits_t val, int index)
		{
			return GTIFKeyGet(gtif, thekey, out val, index, 0);
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out pcstype_t val, int index, int count)
		{
			// ignore count
			val=0;
			ushort[] vals;
			if(GTIFKeyGet(gtif, thekey, out vals, index, 1)==0) return 0;
			val=(pcstype_t)vals[0];
			return 1;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out pcstype_t val, int index)
		{
			return GTIFKeyGet(gtif, thekey, out val, index, 0);
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out projection_t val, int index, int count)
		{
			// ignore count
			val=0;
			ushort[] vals;
			if(GTIFKeyGet(gtif, thekey, out vals, index, 1)==0) return 0;
			val=(projection_t)vals[0];
			return 1;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out projection_t val, int index)
		{
			return GTIFKeyGet(gtif, thekey, out val, index, 0);
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out primemeridian_t val, int index, int count)
		{
			// ignore count
			val=0;
			ushort[] vals;
			if(GTIFKeyGet(gtif, thekey, out vals, index, 1)==0) return 0;
			val=(primemeridian_t)vals[0];
			return 1;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out primemeridian_t val, int index)
		{
			return GTIFKeyGet(gtif, thekey, out val, index, 0);
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out coordtrans_t val, int index, int count)
		{
			// ignore count
			val=0;
			ushort[] vals;
			if(GTIFKeyGet(gtif, thekey, out vals, index, 1)==0) return 0;
			val=(coordtrans_t)vals[0];
			return 1;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out coordtrans_t val, int index)
		{
			return GTIFKeyGet(gtif, thekey, out val, index, 0);
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out rastertype_t val, int index, int count)
		{
			// ignore count
			val=0;
			ushort[] vals;
			if(GTIFKeyGet(gtif, thekey, out vals, index, 1)==0) return 0;
			val=(rastertype_t)vals[0];
			return 1;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out rastertype_t val, int index)
		{
			return GTIFKeyGet(gtif, thekey, out val, index, 0);
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out vertcstype_t val, int index, int count)
		{
			// ignore count
			val=0;
			ushort[] vals;
			if(GTIFKeyGet(gtif, thekey, out vals, index, 1)==0) return 0;
			val=(vertcstype_t)vals[0];
			return 1;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out vertcstype_t val, int index)
		{
			return GTIFKeyGet(gtif, thekey, out val, index, 0);
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out vdatum_t val, int index, int count)
		{
			// ignore count
			val=0;
			ushort[] vals;
			if(GTIFKeyGet(gtif, thekey, out vals, index, 1)==0) return 0;
			val=(vdatum_t)vals[0];
			return 1;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out vdatum_t val, int index)
		{
			return GTIFKeyGet(gtif, thekey, out val, index, 0);
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out ushort[] val, int index, int count)
		{
			val=null;
			if(!gtif.gt_keys.ContainsKey(thekey)) return 0;
			GeoKey key=gtif.gt_keys[thekey];

			if(key.gk_type!=tagtype_t.TYPE_SHORT) return 0;

			ushort[] val_=(ushort[])key.gk_data;
			if(count==0||(count+index)>val_.Length) count=val_.Length-index;
			if(count<=0) return 0;
			val=new ushort[count];
			Array.Copy(val_, index, val, 0, count);
			return count;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out double val, int index, int count)
		{
			// ignore count
			val=0;
			double[] vals;
			if(GTIFKeyGet(gtif, thekey, out vals, index, 1)==0) return 0;
			val=vals[0];
			return 1;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out double val, int index)
		{
			return GTIFKeyGet(gtif, thekey, out val, index, 0);
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out double[] val, int index, int count)
		{
			val=null;
			if(!gtif.gt_keys.ContainsKey(thekey)) return 0;
			GeoKey key=gtif.gt_keys[thekey];

			if(key.gk_type!=tagtype_t.TYPE_DOUBLE) return 0;

			double[] val_=(double[])key.gk_data;
			if(count==0||(count+index)>val_.Length) count=val_.Length-index;
			if(count<=0) return 0;
			val=new double[count];
			Array.Copy(val_, index, val, 0, count);
			return count;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out string val)
		{
			val=null;
			if(!gtif.gt_keys.ContainsKey(thekey)) return 0;
			GeoKey key=gtif.gt_keys[thekey];

			if(key.gk_type!=tagtype_t.TYPE_ASCII) return 0;

			val=(string)key.gk_data;

			return val.Length;
		}

		public static int GTIFKeyGet(GTIF gtif, geokey_t thekey, out string val, int index, int count)
		{
			val=null;
			if(count<0) return 0;

			if(!gtif.gt_keys.ContainsKey(thekey)) return 0;
			GeoKey key=gtif.gt_keys[thekey];

			if(key.gk_type!=tagtype_t.TYPE_ASCII) return 0;

			string val_=(string)key.gk_data;
			if(count==0||(count+index)>val_.Length) val=val_.Substring(index);
			else val=val_.Substring(index, count);

			return val.Length;
		}
	}
}
