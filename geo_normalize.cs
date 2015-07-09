//*****************************************************************************
//
// Project:	libgeotiff
// Purpose:	Include file related to geo_normalize.c containing Code to
//			normalize PCS and other composite codes in a GeoTIFF file.
// Author:	Frank Warmerdam, warmerda@home.com
//
//*****************************************************************************
// Copyright (c) 1999, Frank Warmerdam
// Copyright (c) 2008-2009 by the Authors
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//*****************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Free.Ports.LibGeoTiff
{
	public static partial class libgeotiff
	{
		// Include file for extended projection definition normalization api.
		public const int MAX_GTIF_PROJPARMS=10;

		// Holds a definition of a coordinate system in normalized form.
		public class GTIFDefn
		{
			// From GTModelTypeGeoKey tag. Can have the values ModelTypeGeographic
			// or ModelTypeProjected.
			public modeltype_t Model;

			// From ProjectedCSTypeGeoKey tag. For example PCS_NAD27_UTM_zone_3N.
			public pcstype_t PCS;

			// From GeographicTypeGeoKey tag. For example GCS_WGS_84 or
			// GCS_Voirol_1875_Paris. Includes datum and prime meridian value.
			public geographic_t GCS;

			// From ProjLinearUnitsGeoKey. For example Linear_Meter.
			public geounits_t UOMLength;

			// One UOMLength = UOMLengthInMeters meters.
			public double UOMLengthInMeters;

			// The angular units of the GCS.
			public geounits_t UOMAngle;

			// One UOMAngle = UOMLengthInDegrees degrees.
			public double UOMAngleInDegrees;

			// Datum from GeogGeodeticDatumGeoKey tag. For example Datum_WGS84
			public geodeticdatum_t Datum;

			// Prime meridian from GeogPrimeMeridianGeoKey. For example PM_Greenwich or PM_Paris.
			public primemeridian_t PM;

			// Decimal degrees of longitude between this prime meridian and
			// Greenwich. Prime meridians to the west of Greenwich are negative.
			public double PMLongToGreenwich;

			// Ellipsoid identifier from GeogELlipsoidGeoKey. For example
			// Ellipse_Clarke_1866.
			public ellipsoid_t Ellipsoid;

			// The length of the semi major ellipse axis in meters.
			public double SemiMajor;

			// The length of the semi minor ellipse axis in meters.
			public double SemiMinor;

			// TOWGS84 transformation values (0/3/7)
			public int TOWGS84Count;

			// TOWGS84 transformation values
			public double[] TOWGS84=new double[7];

			// Projection id from ProjectionGeoKey. For example Proj_UTM_11S.
			public projection_t ProjCode;

			// EPSG identifier for underlying projection method. From the EPSG
			// TRF_METHOD table.
			public short Projection;

			// GeoTIFF identifier for underlying projection method. While some of
			// these values have corresponding vlaues in EPSG (Projection field),
			// others do not. For example CT_TransverseMercator.
			public coordtrans_t CTProjection;

			// Number of projection parameters in ProjParm and ProjParmId.
			public int nParms;

			// Projection parameter value. The identify of this parameter
			// is established from the corresponding entry in ProjParmId. The
			// value will be measured in meters, or decimal degrees if it is a
			// linear or angular measure.
			public double[] ProjParm=new double[MAX_GTIF_PROJPARMS];

			// Projection parameter identifier. For example ProjFalseEastingGeoKey.
			// The value will be 0 for unused table entries.
			public geokey_t[] ProjParmId=new geokey_t[MAX_GTIF_PROJPARMS]; // geokey identifier, eg. ProjFalseEastingGeoKey

			// Special zone map system code (MapSys_UTM_South, MapSys_UTM_North,
			// MapSys_State_Plane or KvUserDefined if none apply.
			public int MapSys;

			// UTM, or State Plane Zone number, zero if not known.
			public int Zone;

			// Do we have any definition at all? 0 if no geokeys found
			public bool DefnSet;
		}

		// These are useful for recognising UTM and State Plane, with or without
		// CSV files being found.
		const int MapSys_UTM_North=-9001;
		const int MapSys_UTM_South=-9002;
		const int MapSys_State_Plane_27=-9003;
		const int MapSys_State_Plane_83=-9004;

		// EPSG Codes for projection parameters. Unfortunately, these bear no
		// relationship to the GeoTIFF codes even though the names are so similar.
		const int EPSGNatOriginLat=8801;
		const int EPSGNatOriginLong=8802;
		const int EPSGNatOriginScaleFactor=8805;
		const int EPSGFalseEasting=8806;
		const int EPSGFalseNorthing=8807;
		const int EPSGProjCenterLat=8811;
		const int EPSGProjCenterLong=8812;
		const int EPSGAzimuth=8813;
		const int EPSGAngleRectifiedToSkewedGrid=8814;
		const int EPSGInitialLineScaleFactor=8815;
		const int EPSGProjCenterEasting=8816;
		const int EPSGProjCenterNorthing=8817;
		const int EPSGPseudoStdParallelLat=8818;
		const int EPSGPseudoStdParallelScaleFactor=8819;
		const int EPSGFalseOriginLat=8821;
		const int EPSGFalseOriginLong=8822;
		const int EPSGStdParallel1Lat=8823;
		const int EPSGStdParallel2Lat=8824;
		const int EPSGFalseOriginEasting=8826;
		const int EPSGFalseOriginNorthing=8827;
		const int EPSGSphericalOriginLat=8828;
		const int EPSGSphericalOriginLong=8829;
		const int EPSGInitialLongitude=8830;
		const int EPSGZoneWidth=8831;
		const int EPSGLatOfStdParallel=8832;
		const int EPSGOriginLong=8833;
		const int EPSGTopocentricOriginLa =8834;
		const int EPSGTopocentricOriginLong=8835;
		const int EPSGTopocentricOriginHeight=8836;

		//**********************************************************************
		//							GTIFGetPCSInfo()
		//**********************************************************************
		public static bool GTIFGetPCSInfo(pcstype_t PCSCode, out string EPSGName, out projection_t projOp,
			out geounits_t UOMLengthCode, out geographic_t geogCS)
		{
			geographic_t datum;
			int zone;

			int Proj=GTIFPCSToMapSys(PCSCode, out datum, out zone);
			if((Proj==MapSys_UTM_North||Proj==MapSys_UTM_South)&&datum!=(geographic_t)KvUserDefined)
			{
				string datumName=null;
				switch(datum)
				{
					case geographic_t.GCS_NAD27: datumName="NAD27"; break;
					case geographic_t.GCS_NAD83: datumName="NAD83"; break;
					case geographic_t.GCS_WGS_72: datumName="WGS 72"; break;
					case geographic_t.GCS_WGS_72BE: datumName="WGS 72BE"; break;
					case geographic_t.GCS_WGS_84: datumName="WGS 84"; break;
					default: break;
				}

				if(datumName!=null)
				{
					EPSGName=string.Format("{0} / UTM zone {1}{2}", datumName, zone, (Proj==MapSys_UTM_North)?'N':'S');
					projOp=((Proj==MapSys_UTM_North)?projection_t.Proj_UTM_zone_1N-1:projection_t.Proj_UTM_zone_1S-1)+zone;
					UOMLengthCode=geounits_t.Linear_Meter;
					geogCS=datum;
					return true;
				}
			}

			// --------------------------------------------------------------------
			//		Search the pcs.override table for this PCS.
			// --------------------------------------------------------------------
			string filename=CSVFilename("pcs.override.csv");
			string[] record=CSVScanFileByName(filename, "COORD_REF_SYS_CODE", ((int)PCSCode).ToString(),
				CSVCompareCriteria.CC_Integer);

			EPSGName="";
			UOMLengthCode=(geounits_t)KvUserDefined;
			projOp=(projection_t)KvUserDefined;
			geogCS=(geographic_t)KvUserDefined;

			// --------------------------------------------------------------------
			//		If not found, search the EPSG PCS database.
			// --------------------------------------------------------------------
			if(record==null)
			{
				filename=CSVFilename("pcs.csv");
				record=CSVScanFileByName(filename, "COORD_REF_SYS_CODE", ((int)PCSCode).ToString(),
					CSVCompareCriteria.CC_Integer);
				if(record==null) return false;
			}

			// --------------------------------------------------------------------
			//		Get the name
			// --------------------------------------------------------------------
			EPSGName=CSLGetField(record, CSVGetFileFieldId(filename, "COORD_REF_SYS_NAME"));

			// --------------------------------------------------------------------
			//		Get the UOM Length code
			// --------------------------------------------------------------------
			string value=CSLGetField(record, CSVGetFileFieldId(filename, "UOM_CODE"));
			if(atoi(value)>0) UOMLengthCode=(geounits_t)atoi(value);

			// --------------------------------------------------------------------
			//		Get the Coord Op code
			// --------------------------------------------------------------------
			value=CSLGetField(record, CSVGetFileFieldId(filename, "COORD_OP_CODE"));
			if(atoshort(value)>0) projOp=(projection_t)atoi(value);

			// --------------------------------------------------------------------
			//		Get the GeogCS (Datum with PM) code
			// --------------------------------------------------------------------
			value=CSLGetField(record, CSVGetFileFieldId(filename, "SOURCE_GEOGCRS_CODE"));
			if(atoi(value)>0) geogCS=(geographic_t)atoi(value);

			return true;
		}

		//**********************************************************************
		//							GTIFAngleToDD()
		//
		//		Convert a numeric angle to decimal degress.
		//**********************************************************************
		public static double GTIFAngleToDD(double angle, geounits_t UOMAngle)
		{
			if(UOMAngle==geounits_t.Angular_DMS_Sexagesimal) // DDD.MMSSsss
			{
				string szAngleString=string.Format("{0,12:F7}", angle);
				angle=GTIFAngleStringToDD(szAngleString, UOMAngle);
			}
			else
			{
				double dfInDegrees=1.0;
				string dummy;
				GTIFGetUOMAngleInfo(UOMAngle, out dummy, out dfInDegrees);
				angle=angle*dfInDegrees;
			}

			return angle;
		}

		//**********************************************************************
		//						GTIFAngleStringToDD()
		//
		//		Convert an angle in the specified units to decimal degrees.
		//**********************************************************************
		public static double GTIFAngleStringToDD(string angle, geounits_t UOMAngle)
		{
			if(UOMAngle==geounits_t.Angular_DMS_Sexagesimal) // DDD.MMSSsss
			{
				double tmp=GTIFAtof(angle);
				bool sign=tmp<0;
				tmp=Math.Abs(tmp);

				double dfAngle=Math.Floor(tmp);
				tmp-=dfAngle;
				tmp*=100;

				double dfMinutes=Math.Floor(tmp);
				tmp-=dfMinutes;

				dfAngle+=dfMinutes/60;
				dfAngle+=tmp/36;

				if(sign) dfAngle*=-1;

				return dfAngle;
			}

			if(UOMAngle==geounits_t.Angular_Grad||UOMAngle==geounits_t.Angular_Gon) return 180*(GTIFAtof(angle)/200);
			if(UOMAngle==geounits_t.Angular_Radian) return 180*(GTIFAtof(angle)/Math.PI);
			if(UOMAngle==geounits_t.Angular_Arc_Minute) return GTIFAtof(angle)/60;
			if(UOMAngle==geounits_t.Angular_Arc_Second) return GTIFAtof(angle)/3600;

			// decimal degrees ... some cases missing but seeminly never used
#if DEBUG
			if(!(UOMAngle==geounits_t.Angular_Degree||UOMAngle==(geounits_t)KvUserDefined||UOMAngle==0))
				throw new ArgumentOutOfRangeException("UOMAngle");
#endif
			return GTIFAtof(angle);
		}

		//**********************************************************************
		//							GTIFGetGCSInfo()
		//
		//		Fetch the datum, and prime meridian related to a particular GCS.
		//**********************************************************************
		public static bool GTIFGetGCSInfo(geographic_t GCSCode, out string name, out geodeticdatum_t datum,
			out primemeridian_t pm, out geounits_t UOMAngle)
		{
			name=null;
			datum=(geodeticdatum_t)KvUserDefined;

			// --------------------------------------------------------------------
			//		Handle some "well known" GCS codes directly.
			// --------------------------------------------------------------------
			pm=primemeridian_t.PM_Greenwich;
			UOMAngle=geounits_t.Angular_DMS_Hemisphere;
			if(GCSCode==geographic_t.GCS_NAD27)
			{
				datum=geodeticdatum_t.Datum_North_American_Datum_1927;
				name="NAD27";
				return true;
			}
			else if(GCSCode==geographic_t.GCS_NAD83)
			{
				datum=geodeticdatum_t.Datum_North_American_Datum_1983;
				name="NAD83";
				return true;
			}
			else if(GCSCode==geographic_t.GCS_WGS_84)
			{
				datum=geodeticdatum_t.Datum_WGS84;
				name="WGS 84";
				return true;
			}
			else if(GCSCode==geographic_t.GCS_WGS_72)
			{
				datum=geodeticdatum_t.Datum_WGS72;
				name="WGS 72";
				return true;
			}
			else if(GCSCode==(geographic_t)KvUserDefined) return false;

			// --------------------------------------------------------------------
			//		Search the database for the corresponding datum code.
			// --------------------------------------------------------------------
			string filename=CSVFilename("gcs.override.csv");
			datum=(geodeticdatum_t)atoi(CSVGetField(filename, "COORD_REF_SYS_CODE", ((int)GCSCode).ToString(),
				CSVCompareCriteria.CC_Integer, "DATUM_CODE"));

			if((int)datum<1)
			{
				filename=CSVFilename("gcs.csv");
				datum=(geodeticdatum_t)atoi(CSVGetField(filename, "COORD_REF_SYS_CODE", ((int)GCSCode).ToString(),
					CSVCompareCriteria.CC_Integer, "DATUM_CODE"));
			}

			if((int)datum<1) return false;

			pm=(primemeridian_t)KvUserDefined;
			UOMAngle=(geounits_t)KvUserDefined;
			name=null;

			// --------------------------------------------------------------------
			//		Get the PM.
			// --------------------------------------------------------------------
			pm=(primemeridian_t)atoi(CSVGetField(filename, "COORD_REF_SYS_CODE", ((int)GCSCode).ToString(),
				CSVCompareCriteria.CC_Integer, "PRIME_MERIDIAN_CODE"));
			if((int)pm<1) return false;

			// --------------------------------------------------------------------
			//		Get the angular units.
			// --------------------------------------------------------------------
			UOMAngle=(geounits_t)atoi(CSVGetField(filename, "COORD_REF_SYS_CODE", ((int)GCSCode).ToString(),
				CSVCompareCriteria.CC_Integer, "UOM_CODE"));
			if((int)UOMAngle<1) return false;

			// --------------------------------------------------------------------
			//		Get the name.
			// --------------------------------------------------------------------
			name=CSVGetField(filename, "COORD_REF_SYS_CODE", ((int)GCSCode).ToString(), CSVCompareCriteria.CC_Integer,
				"COORD_REF_SYS_NAME");

			return true;
		}

		//**********************************************************************
		//						GTIFGetEllipsoidInfo()
		//
		//		Fetch info about an ellipsoid. Axes are always returned in
		//		meters. SemiMajor computed based on inverse flattening
		//		where that is provided.
		//**********************************************************************
		public static bool GTIFGetEllipsoidInfo(ellipsoid_t ellipseCode, out string name, out double semiMajor,
			out double semiMinor)
		{
			// --------------------------------------------------------------------
			//		Try some well known ellipsoids.
			// --------------------------------------------------------------------
			name=null;
			semiMajor=0.0;
			semiMinor=0.0;

			double invFlattening=0;

			if(ellipseCode==ellipsoid_t.Ellipse_Clarke_1866)
			{
				name="Clarke 1866";
				semiMajor=6378206.4;
				semiMinor=6356583.8;
				invFlattening=0.0;
			}
			else if(ellipseCode==ellipsoid_t.Ellipse_GRS_1980)
			{
				name="GRS 1980";
				semiMajor=6378137.0;
				semiMinor=0.0;
				invFlattening=298.257222101;
			}
			else if(ellipseCode==ellipsoid_t.Ellipse_WGS_84)
			{
				name="WGS 84";
				semiMajor=6378137.0;
				semiMinor=0.0;
				invFlattening=298.257223563;
			}
			else if(ellipseCode==(ellipsoid_t)7043)
			{
				name="WGS 72";
				semiMajor=6378135.0;
				semiMinor=0.0;
				invFlattening=298.26;
			}

			if(name!=null)
			{
				if(semiMinor==0.0) semiMinor=semiMajor*(1-1.0/invFlattening);
				return true;
			}

			// --------------------------------------------------------------------
			//		Get the semi major axis.
			// --------------------------------------------------------------------
			semiMajor=GTIFAtof(CSVGetField(CSVFilename("ellipsoid.csv"), "ELLIPSOID_CODE",
				((int)ellipseCode).ToString(), CSVCompareCriteria.CC_Integer, "SEMI_MAJOR_AXIS"));

			double toMeters=1.0;

			// --------------------------------------------------------------------
			//		Get the translation factor into meters.
			// --------------------------------------------------------------------
			geounits_t UOMLength=(geounits_t)atoi(CSVGetField(CSVFilename("ellipsoid.csv"), "ELLIPSOID_CODE",
				((int)ellipseCode).ToString(), CSVCompareCriteria.CC_Integer, "UOM_CODE"));

			string dummy;
			GTIFGetUOMLengthInfo(UOMLength, out dummy, out toMeters);
			semiMajor*=toMeters;

			// --------------------------------------------------------------------
			//		Get the semi-minor. If the Semi-minor axis
			//		isn't available, compute it based on the inverse flattening.
			// --------------------------------------------------------------------
			semiMinor=GTIFAtof(CSVGetField(CSVFilename("ellipsoid.csv"), "ELLIPSOID_CODE",
				((int)ellipseCode).ToString(), CSVCompareCriteria.CC_Integer, "SEMI_MINOR_AXIS"))*toMeters;

			if(semiMinor==0.0)
			{
				invFlattening=GTIFAtof(CSVGetField(CSVFilename("ellipsoid.csv"), "ELLIPSOID_CODE",
					((int)ellipseCode).ToString(), CSVCompareCriteria.CC_Integer, "INV_FLATTENING"));
				semiMinor=semiMajor*(1-1.0/invFlattening);
			}

			// --------------------------------------------------------------------
			//		Get the name.
			// --------------------------------------------------------------------
			name=CSVGetField(CSVFilename("ellipsoid.csv"), "ELLIPSOID_CODE", ((int)ellipseCode).ToString(),
				CSVCompareCriteria.CC_Integer, "ELLIPSOID_NAME");

			return true;
		}

		//**********************************************************************
		//							GTIFGetPMInfo()
		//
		//		Get the offset between a given prime meridian and Greenwich
		//		in degrees.
		//**********************************************************************
		public static bool GTIFGetPMInfo(primemeridian_t PMCode, out string name, out double offset)
		{
			// --------------------------------------------------------------------
			//		Use a special short cut for Greenwich, since it is so common.
			// --------------------------------------------------------------------
			offset=0.0;
			name="Greenwich";
			if(PMCode==primemeridian_t.PM_Greenwich) return true;

			string pszFilename=CSVFilename("prime_meridian.csv");

			// --------------------------------------------------------------------
			//		Search the database for the corresponding datum code.
			// --------------------------------------------------------------------
			int UOMAngle=atoi(CSVGetField(pszFilename, "PRIME_MERIDIAN_CODE", ((int)PMCode).ToString(),
				CSVCompareCriteria.CC_Integer, "UOM_CODE"));
			if(UOMAngle<1) return false;

			// --------------------------------------------------------------------
			//		Get the PM offset.
			// --------------------------------------------------------------------
			offset=GTIFAngleStringToDD(CSVGetField(pszFilename, "PRIME_MERIDIAN_CODE", ((int)PMCode).ToString(),
				CSVCompareCriteria.CC_Integer, "GREENWICH_LONGITUDE"), (geounits_t)UOMAngle);

			// --------------------------------------------------------------------
			//		Get the name.
			// --------------------------------------------------------------------
			name=CSVGetField(pszFilename, "PRIME_MERIDIAN_CODE", ((int)PMCode).ToString(),
				CSVCompareCriteria.CC_Integer, "PRIME_MERIDIAN_NAME");

			return true;
		}

		//**********************************************************************
		//							GTIFGetDatumInfo()
		//
		//		Fetch the ellipsoid, and name for a datum.
		//**********************************************************************
		public static bool GTIFGetDatumInfo(geodeticdatum_t datumCode, out string name, out ellipsoid_t ellipsoid)
		{
			name=null;
			ellipsoid=(ellipsoid_t)KvUserDefined;

			// --------------------------------------------------------------------
			//		Handle a few built-in datums.
			// --------------------------------------------------------------------
			if(datumCode==geodeticdatum_t.Datum_North_American_Datum_1927)
			{
				ellipsoid=ellipsoid_t.Ellipse_Clarke_1866;
				name="North American Datum 1927";
			}
			else if(datumCode==geodeticdatum_t.Datum_North_American_Datum_1983)
			{
				ellipsoid=ellipsoid_t.Ellipse_GRS_1980;
				name="North American Datum 1983";
			}
			else if(datumCode==geodeticdatum_t.Datum_WGS84)
			{
				ellipsoid=ellipsoid_t.Ellipse_WGS_84;
				name="World Geodetic System 1984";
			}
			else if(datumCode==geodeticdatum_t.Datum_WGS72)
			{
				ellipsoid=(ellipsoid_t)7043; // WGS7
				name="World Geodetic System 1972";
			}

			if(name!=null) return true;

			string pszFilename=CSVFilename("datum.csv");

			// --------------------------------------------------------------------
			//		If we can't find datum.csv then gdal_datum.csv is an
			//		acceptable fallback. Mostly this is for GDAL.
			// --------------------------------------------------------------------
			if(!File.Exists(pszFilename))
				if(File.Exists(CSVFilename("gdal_datum.csv")))
					pszFilename=CSVFilename("gdal_datum.csv");

			// --------------------------------------------------------------------
			//		Search the database for the corresponding datum code.
			// --------------------------------------------------------------------
			ellipsoid=(ellipsoid_t)atoi(CSVGetField(pszFilename, "DATUM_CODE", ((int)datumCode).ToString(),
				CSVCompareCriteria.CC_Integer, "ELLIPSOID_CODE"));

			// --------------------------------------------------------------------
			//		Get the name
			// --------------------------------------------------------------------
			name=CSVGetField(pszFilename, "DATUM_CODE", ((int)datumCode).ToString(),
				CSVCompareCriteria.CC_Integer, "DATUM_NAME");

			return true;
		}

		//**********************************************************************
		//						GTIFGetUOMLengthInfo()
		//
		//		Note: This function should eventually also know how to
		//		lookup length aliases in the UOM_LE_ALIAS table.
		//**********************************************************************
		public static bool GTIFGetUOMLengthInfo(geounits_t UOMLengthCode, out string UOMName, out double inMeters)
		{
			// --------------------------------------------------------------------
			//		We short cut meter to save work and avoid failure for missing
			//		in the most common cases.
			// --------------------------------------------------------------------
			if(UOMLengthCode==geounits_t.Linear_Meter)
			{
				UOMName="meter";
				inMeters=1.0;
				return true;
			}

			if(UOMLengthCode==geounits_t.Linear_Foot)
			{
				UOMName="foot";
				inMeters=0.3048;
				return true;
			}

			if(UOMLengthCode==geounits_t.Linear_Foot_US_Survey)
			{
				UOMName="US survey foot";
				inMeters=12.0/39.37;
				return true;
			}

			UOMName=null;
			inMeters=0;

			// --------------------------------------------------------------------
			//		Search the units database for this unit. If we don't find
			//		it return failure.
			// --------------------------------------------------------------------
			string filename=CSVFilename("unit_of_measure.csv");

			string[] record=CSVScanFileByName(filename, "UOM_CODE", ((int)UOMLengthCode).ToString(), CSVCompareCriteria.CC_Integer);
			if(record==null) return false;

			// --------------------------------------------------------------------
			//		Get the name.
			// --------------------------------------------------------------------
			int iNameField=CSVGetFileFieldId(filename, "UNIT_OF_MEAS_NAME");
			UOMName=CSLGetField(record, iNameField);

			// --------------------------------------------------------------------
			//		Get the A and B factor fields, and create the multiplicative
			//		factor.
			// --------------------------------------------------------------------
			int iBFactorField=CSVGetFileFieldId(filename, "FACTOR_B");
			int iCFactorField=CSVGetFileFieldId(filename, "FACTOR_C");

			if(GTIFAtof(CSLGetField(record, iCFactorField))>0.0)
				inMeters=GTIFAtof(CSLGetField(record, iBFactorField))/GTIFAtof(CSLGetField(record, iCFactorField));

			return true;
		}

		//**********************************************************************
		//						GTIFGetUOMAngleInfo()
		//**********************************************************************
		public static bool GTIFGetUOMAngleInfo(geounits_t UOMAngleCode, out string UOMName, out double inDegrees)
		{
			UOMName=null;
			inDegrees=0;

			switch(UOMAngleCode)
			{
				case geounits_t.Angular_Radian:
					UOMName="radian";
					inDegrees=180.0/Math.PI;
					break;
				case geounits_t.Angular_Degree:
				case geounits_t.Angular_DMS:
				case geounits_t.Angular_DMS_Hemisphere:
				case geounits_t.Angular_DMS_Sexagesimal:
				case geounits_t.Angular_Simple_Degree:
					UOMName="degree";
					inDegrees=1.0;
					break;
				case geounits_t.Angular_Arc_Minute:
					UOMName="arc-minute";
					inDegrees=1/60.0;
					break;
				case geounits_t.Angular_Arc_Second:
					UOMName="arc-second";
					inDegrees=1/3600.0;
					break;
				case geounits_t.Angular_Grad:
					UOMName="grad";
					inDegrees=180.0/200.0;
					break;
				case geounits_t.Angular_Gon:
					UOMName="gon";
					inDegrees=180.0/200.0;
					break;
				case geounits_t.Angular_Microradian:
					UOMName="microradian";
					inDegrees=180.0/(Math.PI*1000000.0);
					break;
			}

			if(UOMName!=null) return true;

			string pszFilename=CSVFilename("unit_of_measure.csv");
			string pszUOMName=CSVGetField(pszFilename, "UOM_CODE", ((int)UOMAngleCode).ToString(),
				CSVCompareCriteria.CC_Integer, "UNIT_OF_MEAS_NAME");

			inDegrees=0;
			UOMName=null;

			// --------------------------------------------------------------------
			//		If the file is found, read from there. Note that FactorC is
			//		an empty field for any of the DMS style formats, and in this
			//		case we really want to return the default InDegrees value
			//		(1.0) from above.
			// --------------------------------------------------------------------
			if(pszUOMName!="")
			{
				double inRadians;

				double factorB=GTIFAtof(CSVGetField(pszFilename, "UOM_CODE", ((int)UOMAngleCode).ToString(),
					CSVCompareCriteria.CC_Integer, "FACTOR_B"));

				double factorC=GTIFAtof(CSVGetField(pszFilename, "UOM_CODE", ((int)UOMAngleCode).ToString(),
					CSVCompareCriteria.CC_Integer, "FACTOR_C"));

				if(factorC!=0.0)
				{
					inRadians=(factorB/factorC);
					inDegrees=inRadians*180.0/Math.PI;
				}

				// We do a special override of some of the DMS formats name
				if(UOMAngleCode==geounits_t.Angular_Degree||UOMAngleCode==geounits_t.Angular_DMS||
					UOMAngleCode==geounits_t.Angular_DMS_Hemisphere||UOMAngleCode==geounits_t.Angular_DMS_Sexagesimal||
					UOMAngleCode==geounits_t.Angular_Simple_Degree)
				{
					inDegrees=1.0;
					UOMName="degree";
					return true;
				}

				UOMName=pszUOMName;
			}
			else return false;

			return true;
		}

		//**********************************************************************
		//					EPSGProjMethodToCTProjMethod()
		//
		//		Convert between the EPSG enumeration for projection methods,
		//		and the GeoTIFF CT codes.
		//**********************************************************************
		static coordtrans_t EPSGProjMethodToCTProjMethod(int EPSG)
		{
			// see trf_method.csv for list of EPSG codes
			switch(EPSG)
			{
				case 9801: return coordtrans_t.CT_LambertConfConic_1SP;
				case 9802: return coordtrans_t.CT_LambertConfConic_2SP;
				case 9803: return coordtrans_t.CT_LambertConfConic_2SP; // Belgian variant not supported
				case 9804: return coordtrans_t.CT_Mercator; // 1SP and 2SP not differentiated
				case 9805: return coordtrans_t.CT_Mercator; // 1SP and 2SP not differentiated
				case 9841: return coordtrans_t.CT_Mercator; // Mercator 1SP (Spherical) For EPSG:3785 // 1SP and 2SP not differentiated
				case 1024: return coordtrans_t.CT_Mercator; // Google Mercator For EPSG:3857 // 1SP and 2SP not differentiated
				case 9806: return coordtrans_t.CT_CassiniSoldner;
				case 9807: return coordtrans_t.CT_TransverseMercator;
				case 9808: return coordtrans_t.CT_TransvMercator_SouthOriented;
				case 9809: return coordtrans_t.CT_ObliqueStereographic;
				case 9810: return coordtrans_t.CT_PolarStereographic; // case 9829: variant B not quite the same
				case 9811: return coordtrans_t.CT_NewZealandMapGrid;
				case 9812: return coordtrans_t.CT_ObliqueMercator; // is hotine actually different?
				case 9813: return coordtrans_t.CT_ObliqueMercator_Laborde;
				case 9814: return coordtrans_t.CT_ObliqueMercator_Rosenmund; // swiss
				case 9815: return coordtrans_t.CT_ObliqueMercator;
				case 9816: return (coordtrans_t)KvUserDefined; // tunesia mining grid has no counterpart
				case 9820:
				case 1027: return coordtrans_t.CT_LambertAzimEqualArea;
				case 9822: return coordtrans_t.CT_AlbersEqualArea;
				case 9834: return coordtrans_t.CT_CylindricalEqualArea;
			}

			return (coordtrans_t)KvUserDefined;
		}

		//**********************************************************************
		//							SetGTParmIds()
		//
		//		This is hardcoded logic to set the GeoTIFF parmaeter
		//		identifiers for all the EPSG supported projections. As the
		//		trf_method.csv table grows with new projections, this code
		//		will need to be updated.
		//**********************************************************************
		static bool SetGTParmIds(coordtrans_t CTProjection, geokey_t[] projParmId, int[] EPSGCodes)
		{
			if(EPSGCodes==null) EPSGCodes=new int[7];
			if(projParmId==null) projParmId=new geokey_t[7];

			// psDefn.nParms=7;
			switch(CTProjection)
			{
				case coordtrans_t.CT_CassiniSoldner:
				case coordtrans_t.CT_NewZealandMapGrid:
					projParmId[0]=geokey_t.ProjNatOriginLatGeoKey;
					projParmId[1]=geokey_t.ProjNatOriginLongGeoKey;
					projParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					projParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					EPSGCodes[0]=EPSGNatOriginLat;
					EPSGCodes[1]=EPSGNatOriginLong;
					EPSGCodes[5]=EPSGFalseEasting;
					EPSGCodes[6]=EPSGFalseNorthing;
					return true;

				case coordtrans_t.CT_ObliqueMercator:
					projParmId[0]=geokey_t.ProjCenterLatGeoKey;
					projParmId[1]=geokey_t.ProjCenterLongGeoKey;
					projParmId[2]=geokey_t.ProjAzimuthAngleGeoKey;
					projParmId[3]=geokey_t.ProjRectifiedGridAngleGeoKey;
					projParmId[4]=geokey_t.ProjScaleAtCenterGeoKey;
					projParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					projParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					EPSGCodes[0]=EPSGProjCenterLat;
					EPSGCodes[1]=EPSGProjCenterLong;
					EPSGCodes[2]=EPSGAzimuth;
					EPSGCodes[3]=EPSGAngleRectifiedToSkewedGrid;
					EPSGCodes[4]=EPSGInitialLineScaleFactor;
					EPSGCodes[5]=EPSGProjCenterEasting; // EPSG proj method 9812 uses EPSGFalseEasting, but 9815 uses EPSGProjCenterEasting
					EPSGCodes[6]=EPSGProjCenterNorthing; // EPSG proj method 9812 uses EPSGFalseNorthing, but 9815 uses EPSGProjCenterNorthing
					return true;

				case coordtrans_t.CT_ObliqueMercator_Laborde:
					projParmId[0]=geokey_t.ProjCenterLatGeoKey;
					projParmId[1]=geokey_t.ProjCenterLongGeoKey;
					projParmId[2]=geokey_t.ProjAzimuthAngleGeoKey;
					projParmId[4]=geokey_t.ProjScaleAtCenterGeoKey;
					projParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					projParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					EPSGCodes[0]=EPSGProjCenterLat;
					EPSGCodes[1]=EPSGProjCenterLong;
					EPSGCodes[2]=EPSGAzimuth;
					EPSGCodes[4]=EPSGInitialLineScaleFactor;
					EPSGCodes[5]=EPSGProjCenterEasting;
					EPSGCodes[6]=EPSGProjCenterNorthing;
					return true;

				case coordtrans_t.CT_LambertConfConic_1SP:
				case coordtrans_t.CT_Mercator:
				case coordtrans_t.CT_ObliqueStereographic:
				case coordtrans_t.CT_PolarStereographic:
				case coordtrans_t.CT_TransverseMercator:
				case coordtrans_t.CT_TransvMercator_SouthOriented:
					projParmId[0]=geokey_t.ProjNatOriginLatGeoKey;
					projParmId[1]=geokey_t.ProjNatOriginLongGeoKey;
					projParmId[4]=geokey_t.ProjScaleAtNatOriginGeoKey;
					projParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					projParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					EPSGCodes[0]=EPSGNatOriginLat;
					EPSGCodes[1]=EPSGNatOriginLong;
					EPSGCodes[4]=EPSGNatOriginScaleFactor;
					EPSGCodes[5]=EPSGFalseEasting;
					EPSGCodes[6]=EPSGFalseNorthing;
					return true;

				case coordtrans_t.CT_LambertConfConic_2SP:
					projParmId[0]=geokey_t.ProjFalseOriginLatGeoKey;
					projParmId[1]=geokey_t.ProjFalseOriginLongGeoKey;
					projParmId[2]=geokey_t.ProjStdParallel1GeoKey;
					projParmId[3]=geokey_t.ProjStdParallel2GeoKey;
					projParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					projParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					EPSGCodes[0]=EPSGFalseOriginLat;
					EPSGCodes[1]=EPSGFalseOriginLong;
					EPSGCodes[2]=EPSGStdParallel1Lat;
					EPSGCodes[3]=EPSGStdParallel2Lat;
					EPSGCodes[5]=EPSGFalseOriginEasting;
					EPSGCodes[6]=EPSGFalseOriginNorthing;
					return true;

				case coordtrans_t.CT_AlbersEqualArea:
					projParmId[0]=geokey_t.ProjStdParallel1GeoKey;
					projParmId[1]=geokey_t.ProjStdParallel2GeoKey;
					projParmId[2]=geokey_t.ProjNatOriginLatGeoKey;
					projParmId[3]=geokey_t.ProjNatOriginLongGeoKey;
					projParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					projParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					EPSGCodes[0]=EPSGStdParallel1Lat;
					EPSGCodes[1]=EPSGStdParallel2Lat;
					EPSGCodes[2]=EPSGFalseOriginLat;
					EPSGCodes[3]=EPSGFalseOriginLong;
					EPSGCodes[5]=EPSGFalseOriginEasting;
					EPSGCodes[6]=EPSGFalseOriginNorthing;
					return true;

				case coordtrans_t.CT_SwissObliqueCylindrical:
					projParmId[0]=geokey_t.ProjCenterLatGeoKey;
					projParmId[1]=geokey_t.ProjCenterLongGeoKey;
					projParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					projParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					EPSGCodes[0]=EPSGProjCenterLat;
					EPSGCodes[1]=EPSGProjCenterLong;
					EPSGCodes[5]=EPSGFalseOriginEasting;
					EPSGCodes[6]=EPSGFalseOriginNorthing;
					return true;

				case coordtrans_t.CT_LambertAzimEqualArea:
					projParmId[0]=geokey_t.ProjCenterLatGeoKey;
					projParmId[1]=geokey_t.ProjCenterLongGeoKey;
					projParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					projParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					EPSGCodes[0]=EPSGNatOriginLat;
					EPSGCodes[1]=EPSGNatOriginLong;
					EPSGCodes[5]=EPSGFalseEasting;
					EPSGCodes[6]=EPSGFalseNorthing;
					return true;

				case coordtrans_t.CT_CylindricalEqualArea:
					projParmId[0]=geokey_t.ProjStdParallel1GeoKey;
					projParmId[1]=geokey_t.ProjNatOriginLongGeoKey;
					projParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					projParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					EPSGCodes[0]=EPSGStdParallel1Lat;
					EPSGCodes[1]=EPSGFalseOriginLong;
					EPSGCodes[5]=EPSGFalseOriginEasting;
					EPSGCodes[6]=EPSGFalseOriginNorthing;
					return true;

				default:
					return false;
			}
		}

		//**********************************************************************
		//							GTIFGetProjTRFInfo()
		//
		//		Transform a PROJECTION_TRF_CODE into a projection method,
		//		and a set of parameters. The parameters identify will
		//		depend on the returned method, but they will all have been
		//		normalized into degrees and meters.
		//**********************************************************************
		// COORD_OP_CODE from coordinate_operation.csv.
		public static bool GTIFGetProjTRFInfo(projection_t projTRFCode, out string projTRFName, out short projMethod, double[] projParms)
		{
			if(projParms==null) projParms=new double[7];

			if((projTRFCode>=projection_t.Proj_UTM_zone_1N&&projTRFCode<=projection_t.Proj_UTM_zone_60N)||
				(projTRFCode>=projection_t.Proj_UTM_zone_1S&&projTRFCode<=projection_t.Proj_UTM_zone_60S))
			{
				bool north;
				int zone;
				if(projTRFCode<=projection_t.Proj_UTM_zone_60N)
				{
					north=true;
					zone=projTRFCode-projection_t.Proj_UTM_zone_1N+1;
				}
				else
				{
					north=false;
					zone=projTRFCode-projection_t.Proj_UTM_zone_1S+1;
				}

				projTRFName=string.Format("UTM zone {0}{1}", zone, north?'N':'S');

				projMethod=9807;

				projParms[0]=0;
				projParms[1]=-183+6*zone;
				projParms[2]=0;
				projParms[3]=0;
				projParms[4]=0.9996;
				projParms[5]=500000;
				projParms[6]=north?0:10000000;

				return true;
			}

			// --------------------------------------------------------------------
			//		Get the proj method. If this fails to return a meaningful
			//		number, then the whole function fails.
			// --------------------------------------------------------------------
			projTRFName=null;
			
			string pszFilename=CSVFilename("projop_wparm.csv");

			projMethod=atoshort(CSVGetField(pszFilename, "COORD_OP_CODE", ((int)projTRFCode).ToString(),
				CSVCompareCriteria.CC_Integer, "COORD_OP_METHOD_CODE"));

			if(projMethod==0) return false;

			// --------------------------------------------------------------------
			//		Initialize a definition of what EPSG codes need to be loaded
			//		into what fields in adfProjParms.
			// --------------------------------------------------------------------
			coordtrans_t CTProjMethod=EPSGProjMethodToCTProjMethod(projMethod);
			int[] EPSGCodes=new int[7];
			SetGTParmIds(CTProjMethod, null, EPSGCodes);

			// --------------------------------------------------------------------
			//		Get the parameters for this projection. For the time being
			//		I am assuming the first four parameters are angles, the
			//		fifth is unitless (normally scale), and the remainder are
			//		linear measures. This works fine for the existing
			//		projections, but is a pretty fragile approach.
			// --------------------------------------------------------------------
			for(int i=0; i<7; i++)
			{
				int EPSGCode=EPSGCodes[i];

				// Establish default
				if(EPSGCode==EPSGAngleRectifiedToSkewedGrid) projParms[i]=90.0;
				else if(EPSGCode==EPSGNatOriginScaleFactor||EPSGCode==EPSGInitialLineScaleFactor||
					EPSGCode==EPSGPseudoStdParallelScaleFactor) projParms[i]=1.0;
				else projParms[i]=0.0;

				// If there is no parameter, skip
				if(EPSGCode==0) continue;

				// Find the matching parameter
				int iEPSG=0;
				for(; iEPSG<7; iEPSG++)
				{
					string paramCodeID=string.Format("PARAMETER_CODE_{0}", iEPSG+1);
					if(atoi(CSVGetField(pszFilename, "COORD_OP_CODE", ((int)projTRFCode).ToString(),
						CSVCompareCriteria.CC_Integer, paramCodeID))==EPSGCode) break;
				}

				// not found, accept the default
				if(iEPSG==7) continue;
				{
					// for CT_ObliqueMercator try alternate parameter codes first
					// because EPSG proj method 9812 uses EPSGFalseXXXXX, but 9815 uses EPSGProjCenterXXXXX
					if(CTProjMethod==coordtrans_t.CT_ObliqueMercator&&EPSGCode==EPSGProjCenterEasting) EPSGCode=EPSGFalseEasting;
					else if(CTProjMethod==coordtrans_t.CT_ObliqueMercator&&EPSGCode==EPSGProjCenterNorthing) EPSGCode=EPSGFalseNorthing;
					else continue;

					for(iEPSG=0; iEPSG<7; iEPSG++)
					{
						string paramCodeID=string.Format("PARAMETER_CODE_{0}", iEPSG+1);

						if(atoi(CSVGetField(pszFilename, "COORD_OP_CODE", ((int)projTRFCode).ToString(), CSVCompareCriteria.CC_Integer, paramCodeID))==EPSGCode)
							break;
					}

					if(iEPSG==7) continue;
				}

				// Get the value, and UOM
				string paramUOMID=string.Format("PARAMETER_UOM_{0}", iEPSG+1);
				string paramValueID=string.Format("PARAMETER_VALUE_{0}", iEPSG+1);

				geounits_t UOM=(geounits_t)atoi(CSVGetField(pszFilename, "COORD_OP_CODE", ((int)projTRFCode).ToString(),
					CSVCompareCriteria.CC_Integer, paramUOMID));
				string value=CSVGetField(pszFilename, "COORD_OP_CODE", ((int)projTRFCode).ToString(),
					CSVCompareCriteria.CC_Integer, paramValueID);

				// Transform according to the UOM
				if((int)UOM>=9100&&(int)UOM<9200) projParms[i]=GTIFAngleStringToDD(value, UOM);
				else if((int)UOM>9000&&(int)UOM<9100)
				{
					double inMeters;
					string dummy;

					if(!GTIFGetUOMLengthInfo(UOM, out dummy, out inMeters)) inMeters=1.0;
					projParms[i]=GTIFAtof(value)*inMeters;
				}
				else projParms[i]=GTIFAtof(value);
			}

			// --------------------------------------------------------------------
			//		Get the name.
			// --------------------------------------------------------------------
			projTRFName=CSVGetField(pszFilename, "COORD_OP_CODE", ((int)projTRFCode).ToString(),
				CSVCompareCriteria.CC_Integer, "COORD_OP_NAME");

			return true;
		}

		//**********************************************************************
		//						GTIFFetchProjParms()
		//
		//		Fetch the projection parameters for a particular projection
		//		from a GeoTIFF file, and fill the GTIFDefn structure out
		//		with them.
		//**********************************************************************
		static void GTIFFetchProjParms(GTIF gtif, GTIFDefn defn)
		{
			double NatOriginLong=0.0, NatOriginLat=0.0, RectGridAngle=0.0;
			double FalseEasting=0.0, FalseNorthing=0.0, NatOriginScale=1.0;
			double StdParallel1=0.0, StdParallel2=0.0, Azimuth=0.0;

			// --------------------------------------------------------------------
			//		Get the false easting, and northing if available.
			// --------------------------------------------------------------------
			if(GTIFKeyGet(gtif, geokey_t.ProjFalseEastingGeoKey, out FalseEasting, 0, 1)==0&&
				GTIFKeyGet(gtif, geokey_t.ProjCenterEastingGeoKey, out FalseEasting, 0, 1)==0&&
				GTIFKeyGet(gtif, geokey_t.ProjFalseOriginEastingGeoKey, out FalseEasting, 0, 1)==0) FalseEasting=0.0;

			if(GTIFKeyGet(gtif, geokey_t.ProjFalseNorthingGeoKey, out FalseNorthing, 0, 1)==0&&
				GTIFKeyGet(gtif, geokey_t.ProjCenterNorthingGeoKey, out FalseNorthing, 0, 1)==0&&
				GTIFKeyGet(gtif, geokey_t.ProjFalseOriginNorthingGeoKey, out FalseNorthing, 0, 1)==0) FalseNorthing=0.0;

			switch(defn.CTProjection)
			{
				case coordtrans_t.CT_Stereographic:
					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLongGeoKey, out NatOriginLong, 0, 1)==0) NatOriginLong=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLatGeoKey, out NatOriginLat, 0, 1)==0) NatOriginLat=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjScaleAtNatOriginGeoKey, out NatOriginScale, 0, 1)==0) NatOriginScale=1.0;

					// notdef: should transform to decimal degrees at this point
					defn.ProjParm[0]=NatOriginLat;
					defn.ProjParmId[0]=geokey_t.ProjCenterLatGeoKey;
					defn.ProjParm[1]=NatOriginLong;
					defn.ProjParmId[1]=geokey_t.ProjCenterLongGeoKey;
					defn.ProjParm[4]=NatOriginScale;
					defn.ProjParmId[4]=geokey_t.ProjScaleAtNatOriginGeoKey;
					defn.ProjParm[5]=FalseEasting;
					defn.ProjParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					defn.ProjParm[6]=FalseNorthing;
					defn.ProjParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					defn.nParms=7;
					break;

				case coordtrans_t.CT_LambertConfConic_1SP:
				case coordtrans_t.CT_Mercator:
				case coordtrans_t.CT_ObliqueStereographic:
				case coordtrans_t.CT_TransverseMercator:
				case coordtrans_t.CT_TransvMercator_SouthOriented:
					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLongGeoKey, out NatOriginLong, 0, 1)==0) NatOriginLong=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLatGeoKey, out NatOriginLat, 0, 1)==0) NatOriginLat=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjScaleAtNatOriginGeoKey, out NatOriginScale, 0, 1)==0) NatOriginScale=1.0;

					// notdef: should transform to decimal degrees at this point
					defn.ProjParm[0]=NatOriginLat;
					defn.ProjParmId[0]=geokey_t.ProjNatOriginLatGeoKey;
					defn.ProjParm[1]=NatOriginLong;
					defn.ProjParmId[1]=geokey_t.ProjNatOriginLongGeoKey;
					defn.ProjParm[4]=NatOriginScale;
					defn.ProjParmId[4]=geokey_t.ProjScaleAtNatOriginGeoKey;
					defn.ProjParm[5]=FalseEasting;
					defn.ProjParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					defn.ProjParm[6]=FalseNorthing;
					defn.ProjParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					defn.nParms=7;
					break;

				case coordtrans_t.CT_ObliqueMercator: // hotine
					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLongGeoKey, out NatOriginLong, 0, 1)==0) NatOriginLong=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLatGeoKey, out NatOriginLat, 0, 1)==0) NatOriginLat=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjAzimuthAngleGeoKey, out Azimuth, 0, 1)==0) Azimuth=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjRectifiedGridAngleGeoKey, out RectGridAngle, 0, 1)==0) RectGridAngle=90.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjScaleAtNatOriginGeoKey, out NatOriginScale, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjScaleAtCenterGeoKey, out NatOriginScale, 0, 1)==0) NatOriginScale=1.0;

					// notdef: should transform to decimal degrees at this point
					defn.ProjParm[0]=NatOriginLat;
					defn.ProjParmId[0]=geokey_t.ProjCenterLatGeoKey;
					defn.ProjParm[1]=NatOriginLong;
					defn.ProjParmId[1]=geokey_t.ProjCenterLongGeoKey;
					defn.ProjParm[2]=Azimuth;
					defn.ProjParmId[2]=geokey_t.ProjAzimuthAngleGeoKey;
					defn.ProjParm[3]=RectGridAngle;
					defn.ProjParmId[3]=geokey_t.ProjRectifiedGridAngleGeoKey;
					defn.ProjParm[4]=NatOriginScale;
					defn.ProjParmId[4]=geokey_t.ProjScaleAtCenterGeoKey;
					defn.ProjParm[5]=FalseEasting;
					defn.ProjParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					defn.ProjParm[6]=FalseNorthing;
					defn.ProjParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					defn.nParms=7;
					break;

				case coordtrans_t.CT_CassiniSoldner:
				case coordtrans_t.CT_Polyconic:
					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLongGeoKey, out NatOriginLong, 0, 1)==0) NatOriginLong=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLatGeoKey, out NatOriginLat, 0, 1)==0) NatOriginLat=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjScaleAtNatOriginGeoKey, out NatOriginScale, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjScaleAtCenterGeoKey, out NatOriginScale, 0, 1)==0) NatOriginScale=1.0;

					// notdef: should transform to decimal degrees at this point
					defn.ProjParm[0]=NatOriginLat;
					defn.ProjParmId[0]=geokey_t.ProjNatOriginLatGeoKey;
					defn.ProjParm[1]=NatOriginLong;
					defn.ProjParmId[1]=geokey_t.ProjNatOriginLongGeoKey;
					defn.ProjParm[4]=NatOriginScale;
					defn.ProjParmId[4]=geokey_t.ProjScaleAtNatOriginGeoKey;
					defn.ProjParm[5]=FalseEasting;
					defn.ProjParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					defn.ProjParm[6]=FalseNorthing;
					defn.ProjParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					defn.nParms=7;
					break;

				case coordtrans_t.CT_AzimuthalEquidistant:
				case coordtrans_t.CT_MillerCylindrical:
				case coordtrans_t.CT_Gnomonic:
				case coordtrans_t.CT_LambertAzimEqualArea:
				case coordtrans_t.CT_Orthographic:
				case coordtrans_t.CT_NewZealandMapGrid:
					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLongGeoKey, out NatOriginLong, 0, 1)==0) NatOriginLong=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLatGeoKey, out NatOriginLat, 0, 1)==0) NatOriginLat=0.0;

					// notdef: should transform to decimal degrees at this point
					defn.ProjParm[0]=NatOriginLat;
					defn.ProjParmId[0]=geokey_t.ProjCenterLatGeoKey;
					defn.ProjParm[1]=NatOriginLong;
					defn.ProjParmId[1]=geokey_t.ProjCenterLongGeoKey;
					defn.ProjParm[5]=FalseEasting;
					defn.ProjParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					defn.ProjParm[6]=FalseNorthing;
					defn.ProjParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					defn.nParms=7;
					break;

				case coordtrans_t.CT_Equirectangular:
					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLongGeoKey, out NatOriginLong, 0, 1)==0) NatOriginLong=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLatGeoKey, out NatOriginLat, 0, 1)==0) NatOriginLat=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjStdParallel1GeoKey, out StdParallel1, 0, 1)==0) StdParallel1=0.0;

					// notdef: should transform to decimal degrees at this point

					defn.ProjParm[0]=NatOriginLat;
					defn.ProjParmId[0]=geokey_t.ProjCenterLatGeoKey;
					defn.ProjParm[1]=NatOriginLong;
					defn.ProjParmId[1]=geokey_t.ProjCenterLongGeoKey;
					defn.ProjParm[2]=StdParallel1;
					defn.ProjParmId[2]=geokey_t.ProjStdParallel1GeoKey;
					defn.ProjParm[5]=FalseEasting;
					defn.ProjParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					defn.ProjParm[6]=FalseNorthing;
					defn.ProjParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					defn.nParms=7;
					break;

				case coordtrans_t.CT_Robinson:
				case coordtrans_t.CT_Sinusoidal:
				case coordtrans_t.CT_VanDerGrinten:
					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLongGeoKey, out NatOriginLong, 0, 1)==0) NatOriginLong=0.0;

					// notdef: should transform to decimal degrees at this point
					defn.ProjParm[1]=NatOriginLong;
					defn.ProjParmId[1]=geokey_t.ProjCenterLongGeoKey;
					defn.ProjParm[5]=FalseEasting;
					defn.ProjParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					defn.ProjParm[6]=FalseNorthing;
					defn.ProjParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					defn.nParms=7;
					break;

				case coordtrans_t.CT_PolarStereographic:
					if(GTIFKeyGet(gtif, geokey_t.ProjStraightVertPoleLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjNatOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLongGeoKey, out NatOriginLong, 0, 1)==0) NatOriginLong=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLatGeoKey, out NatOriginLat, 0, 1)==0) NatOriginLat=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjScaleAtNatOriginGeoKey, out NatOriginScale, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjScaleAtCenterGeoKey, out NatOriginScale, 0, 1)==0) NatOriginScale=1.0;

					// notdef: should transform to decimal degrees at this point
					defn.ProjParm[0]=NatOriginLat;
					defn.ProjParmId[0]=geokey_t.ProjNatOriginLatGeoKey; ;
					defn.ProjParm[1]=NatOriginLong;
					defn.ProjParmId[1]=geokey_t.ProjStraightVertPoleLongGeoKey;
					defn.ProjParm[4]=NatOriginScale;
					defn.ProjParmId[4]=geokey_t.ProjScaleAtNatOriginGeoKey;
					defn.ProjParm[5]=FalseEasting;
					defn.ProjParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					defn.ProjParm[6]=FalseNorthing;
					defn.ProjParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					defn.nParms=7;
					break;

				case coordtrans_t.CT_LambertConfConic_2SP:
					if(GTIFKeyGet(gtif, geokey_t.ProjStdParallel1GeoKey, out StdParallel1, 0, 1)==0) StdParallel1=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjStdParallel2GeoKey, out StdParallel2, 0, 1)==0) StdParallel1=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLongGeoKey, out NatOriginLong, 0, 1)==0) NatOriginLong=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLatGeoKey, out NatOriginLat, 0, 1)==0) NatOriginLat=0.0;

					// notdef: should transform to decimal degrees at this point
					defn.ProjParm[0]=NatOriginLat;
					defn.ProjParmId[0]=geokey_t.ProjFalseOriginLatGeoKey;
					defn.ProjParm[1]=NatOriginLong;
					defn.ProjParmId[1]=geokey_t.ProjFalseOriginLongGeoKey;
					defn.ProjParm[2]=StdParallel1;
					defn.ProjParmId[2]=geokey_t.ProjStdParallel1GeoKey;
					defn.ProjParm[3]=StdParallel2;
					defn.ProjParmId[3]=geokey_t.ProjStdParallel2GeoKey;
					defn.ProjParm[5]=FalseEasting;
					defn.ProjParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					defn.ProjParm[6]=FalseNorthing;
					defn.ProjParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					defn.nParms=7;
					break;

				case coordtrans_t.CT_AlbersEqualArea:
				case coordtrans_t.CT_EquidistantConic:
					if(GTIFKeyGet(gtif, geokey_t.ProjStdParallel1GeoKey, out StdParallel1, 0, 1)==0) StdParallel1=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjStdParallel2GeoKey, out StdParallel2, 0, 1)==0) StdParallel2=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLongGeoKey, out NatOriginLong, 0, 1)==0) NatOriginLong=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLatGeoKey, out NatOriginLat, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLatGeoKey, out NatOriginLat, 0, 1)==0) NatOriginLat=0.0;

					// notdef: should transform to decimal degrees at this point
					defn.ProjParm[0]=StdParallel1;
					defn.ProjParmId[0]=geokey_t.ProjStdParallel1GeoKey;
					defn.ProjParm[1]=StdParallel2;
					defn.ProjParmId[1]=geokey_t.ProjStdParallel2GeoKey;
					defn.ProjParm[2]=NatOriginLat;
					defn.ProjParmId[2]=geokey_t.ProjNatOriginLatGeoKey;
					defn.ProjParm[3]=NatOriginLong;
					defn.ProjParmId[3]=geokey_t.ProjNatOriginLongGeoKey;
					defn.ProjParm[5]=FalseEasting;
					defn.ProjParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					defn.ProjParm[6]=FalseNorthing;
					defn.ProjParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					defn.nParms=7;
					break;

				case coordtrans_t.CT_CylindricalEqualArea:
					if(GTIFKeyGet(gtif, geokey_t.ProjStdParallel1GeoKey, out StdParallel1, 0, 1)==0) StdParallel1=0.0;

					if(GTIFKeyGet(gtif, geokey_t.ProjNatOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjFalseOriginLongGeoKey, out NatOriginLong, 0, 1)==0&&
						GTIFKeyGet(gtif, geokey_t.ProjCenterLongGeoKey, out NatOriginLong, 0, 1)==0) NatOriginLong=0.0;

					// notdef: should transform to decimal degrees at this point
					defn.ProjParm[0]=StdParallel1;
					defn.ProjParmId[0]=geokey_t.ProjStdParallel1GeoKey;
					defn.ProjParm[1]=NatOriginLong;
					defn.ProjParmId[1]=geokey_t.ProjNatOriginLongGeoKey;
					defn.ProjParm[5]=FalseEasting;
					defn.ProjParmId[5]=geokey_t.ProjFalseEastingGeoKey;
					defn.ProjParm[6]=FalseNorthing;
					defn.ProjParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

					defn.nParms=7;
					break;
			}

			// --------------------------------------------------------------------
			//		Normalize any linear parameters into meters. In GeoTIFF
			//		the linear projection parameter tags are normally in the
			//		units of the coordinate system described.
			// --------------------------------------------------------------------
			for(int iParm=0; iParm<defn.nParms; iParm++)
			{
				switch(defn.ProjParmId[iParm])
				{
					case geokey_t.ProjFalseEastingGeoKey:
					case geokey_t.ProjFalseNorthingGeoKey:
					case geokey_t.ProjFalseOriginEastingGeoKey:
					case geokey_t.ProjFalseOriginNorthingGeoKey:
					case geokey_t.ProjCenterEastingGeoKey:
					case geokey_t.ProjCenterNorthingGeoKey:
						if(defn.UOMLengthInMeters!=0&&defn.UOMLengthInMeters!=1.0) defn.ProjParm[iParm]*=defn.UOMLengthInMeters;
						break;
				}
			}
		}

		//**********************************************************************
		//							GTIFGetDefn()
		//**********************************************************************
		//
		// @param psGTIF GeoTIFF information handle as returned by GTIFNew.
		// @param psDefn Pointer to an existing GTIFDefn structure. This structure
		// does not need to have been pre-initialized at all.
		//
		// @return true if the function has been successful, otherwise false.
		//
		// This function reads the coordinate system definition from a GeoTIFF file,
		// and normalizes it into a set of component information using
		// definitions from CSV (Comma Seperated Value ASCII) files derived from
		// EPSG tables. This function is intended to simplify correct support for
		// reading files with defined PCS (Projected Coordinate System) codes that
		// wouldn't otherwise be directly known by application software by reducing
		// it to the underlying projection method, parameters, datum, ellipsoid,
		// prime meridian and units.
		//
		// The application should pass a pointer to an existing uninitialized
		// GTIFDefn structure, and GTIFGetDefn() will fill it in. The fuction
		// currently always returns TRUE but in the future will return FALSE if
		// CSV files are not found. In any event, all geokeys actually found in the
		// file will be copied into the GTIFDefn. However, if the CSV files aren't
		// found codes implied by other codes will not be set properly.
		//
		// GTIFGetDefn() will not generally work if the EPSG derived CSV files cannot
		// be found. By default a modest attempt will be made to find them, but
		// in general it is necessary for the calling application to override the
		// logic to find them. This can be done by calling the SetCSVFilenameHook()
		// function to override the search method based on application knowledge of
		// where they are found.
		//
		// The normalization methodology operates by fetching tags from the GeoTIFF
		// file, and then setting all other tags implied by them in the structure. The
		// implied relationships are worked out by reading definitions from the 
		// various EPSG derived CSV tables.
		//
		// For instance, if a PCS (ProjectedCSTypeGeoKey) is found in the GeoTIFF file
		// this code is used to lookup a record in the horiz_cs.csv CSV
		// file. For example given the PCS 26746 we can find the name
		// (NAD27 / California zone VI), the GCS 4257 (NAD27), and the ProjectionCode
		// 10406 (California CS27 zone VI). The GCS, and ProjectionCode can in turn
		// be looked up in other tables until all the details of units, ellipsoid,
		// prime meridian, datum, projection (LambertConfConic_2SP) and projection
		// parameters are established. A full listgeo dump of a file
		// for this result might look like the following, all based on a single PCS
		// value:
		//
		//	%listgeo -norm ~/data/geotiff/pci_eg/spaf27.tif
		//	Geotiff_Information:
		//		Version: 1
		//		Key_Revision: 1.0
		//		Tagged_Information:
		//			ModelTiepointTag (2,3):
		//				0			0			0
		//				1577139.71	634349.176	0
		//			ModelPixelScaleTag (1,3):
		//				195.509321	198.32184	0
		//			End_Of_Tags.
		//		Keyed_Information:
		//			GTModelTypeGeoKey (Short,1): ModelTypeProjected
		//			GTRasterTypeGeoKey (Short,1): RasterPixelIsArea
		//			ProjectedCSTypeGeoKey (Short,1): PCS_NAD27_California_VI
		//			End_Of_Keys.
		//		End_Of_Geotiff.
		//
		//	PCS = 26746 (NAD27 / California zone VI)
		//	Projection = 10406 (California CS27 zone VI)
		//	Projection Method: CT_LambertConfConic_2SP
		//		ProjStdParallel1GeoKey: 33.883333
		//		ProjStdParallel2GeoKey: 32.766667
		//		ProjFalseOriginLatGeoKey: 32.166667
		//		ProjFalseOriginLongGeoKey: -116.233333
		//		ProjFalseEastingGeoKey: 609601.219202
		//		ProjFalseNorthingGeoKey: 0.000000
		//	GCS: 4267/NAD27
		//	Datum: 6267/North American Datum 1927
		//	Ellipsoid: 7008/Clarke 1866 (6378206.40,6356583.80)
		//	Prime Meridian: 8901/Greenwich (0.000000)
		//	Projection Linear Units: 9003/US survey foot (0.304801m)
		//
		// Note that GTIFGetDefn() does not inspect or return the tiepoints and scale.
		// This must be handled seperately as it normally would. It is intended to
		// simplify capture and normalization of the coordinate system definition.
		// Note that GTIFGetDefn() also does the following things:
		//
		//	*	Convert all angular values to decimal degrees.
		//	*	Convert all linear values to meters.
		//	*	Return the linear units and conversion to meters for the tiepoints and
		//		scale (though the tiepoints and scale remain in their native units).
		//	*	When reading projection parameters a variety of differences between
		//		different GeoTIFF generators are handled, and a normalized set of
		//		parameters for each projection are always returned.
		//
		// Code fields in the GTIFDefn are filled with KvUserDefined if there is not value to
		// assign. The parameter lists for each of the underlying projection transform methods
		// can be found at the Projections (http://www.remotesensing.org/geotiff/proj_list) page.
		// Note that nParms will be set based on the maximum parameter used. Some of the
		// parameters may not be used in which case the GTIFDefn::ProjParmId[] will be zero.
		// This is done to retain correspondence to the EPSG parameter numbering scheme.
		//
		//The geotiff_proj4.cs module distributed with libgeotiff can be used as an example of
		// code that converts a GTIFDefn into another projection system.
		//
		// @see GTIFKeySet(), SetCSVFilenameHook()
		public static bool GTIFGetDefn(GTIF gtif, GTIFDefn defn)
		{
			// --------------------------------------------------------------------
			//		Initially we default all the information we can.
			// --------------------------------------------------------------------
			defn.DefnSet=true;
			defn.Model=(modeltype_t)KvUserDefined;
			defn.PCS=(pcstype_t)KvUserDefined;
			defn.GCS=(geographic_t)KvUserDefined;
			defn.UOMLength=(geounits_t)KvUserDefined;
			defn.UOMLengthInMeters=1.0;
			defn.UOMAngle=(geounits_t)KvUserDefined;
			defn.UOMAngleInDegrees=1.0;
			defn.Datum=(geodeticdatum_t)KvUserDefined;
			defn.Ellipsoid=(ellipsoid_t)KvUserDefined;
			defn.SemiMajor=0.0;
			defn.SemiMinor=0.0;
			defn.PM=(primemeridian_t)KvUserDefined;
			defn.PMLongToGreenwich=0.0;
			defn.TOWGS84Count=0;
			for(int i=0; i<defn.TOWGS84.Length; i++) defn.TOWGS84[i]=0;

			defn.ProjCode=(projection_t)KvUserDefined;
			defn.Projection=KvUserDefined;
			defn.CTProjection=(coordtrans_t)KvUserDefined;

			defn.nParms=0;
			for(int i=0; i<MAX_GTIF_PROJPARMS; i++)
			{
				defn.ProjParm[i]=0.0;
				defn.ProjParmId[i]=0;
			}

			defn.MapSys=KvUserDefined;
			defn.Zone=0;

			// --------------------------------------------------------------------
			//		Do we have any geokeys?
			// --------------------------------------------------------------------
			int nKeyCount=0;
			int[] anVersion=new int[3];
			GTIFDirectoryInfo(gtif, anVersion, out nKeyCount);

			if(nKeyCount==0)
			{
				defn.DefnSet=false;
				return false;
			}

			// --------------------------------------------------------------------
			//		Try to get the overall model type.
			// --------------------------------------------------------------------
			GTIFKeyGet(gtif, geokey_t.GTModelTypeGeoKey, out defn.Model, 0, 1);

			// --------------------------------------------------------------------
			//		Extract the Geog units.
			// --------------------------------------------------------------------
			geounits_t nGeogUOMLinear=geounits_t.Linear_Meter;
			GTIFKeyGet(gtif, geokey_t.GeogLinearUnitsGeoKey, out nGeogUOMLinear, 0, 1);

			// --------------------------------------------------------------------
			//		Try to get a PCS.
			// --------------------------------------------------------------------
			string dummy;

			if(GTIFKeyGet(gtif, geokey_t.ProjectedCSTypeGeoKey, out defn.PCS, 0, 1)==1
				&&defn.PCS!=(pcstype_t)KvUserDefined) // Translate this into useful information.
				GTIFGetPCSInfo(defn.PCS, out dummy, out defn.ProjCode, out defn.UOMLength, out defn.GCS);

			// --------------------------------------------------------------------
			//		If we have the PCS code, but didn't find it in the CSV files
			//		(likely because we can't find them) we will try some "jiffy
			//		rules" for UTM and state plane.
			// --------------------------------------------------------------------
			if(defn.PCS!=(pcstype_t)KvUserDefined&&defn.ProjCode==(projection_t)KvUserDefined)
			{
				int mapSys, zone;
				geographic_t GCS=defn.GCS;

				mapSys=GTIFPCSToMapSys(defn.PCS, out GCS, out zone);
				if(mapSys!=KvUserDefined)
				{
					defn.ProjCode=GTIFMapSysToProj(mapSys, zone);
					defn.GCS=GCS;
				}
			}

			// --------------------------------------------------------------------
			//		If the Proj_ code is specified directly, use that.
			// --------------------------------------------------------------------
			if(defn.ProjCode==(projection_t)KvUserDefined)
				GTIFKeyGet(gtif, geokey_t.ProjectionGeoKey, out defn.ProjCode, 0, 1);

			if(defn.ProjCode!=(projection_t)KvUserDefined)
			{
				// We have an underlying projection transformation value. Look
				// this up. For a PCS of "WGS 84 / UTM 11" the transformation
				// would be Transverse Mercator, with a particular set of options.
				// The nProjTRFCode itself would correspond to the name
				// "UTM zone 11N", and doesn't include datum info.

				GTIFGetProjTRFInfo(defn.ProjCode, out dummy, out defn.Projection, defn.ProjParm);

				// Set the GeoTIFF identity of the parameters.
				defn.CTProjection=EPSGProjMethodToCTProjMethod(defn.Projection);

				SetGTParmIds(defn.CTProjection, defn.ProjParmId, null);
				defn.nParms=7;
			}

			// --------------------------------------------------------------------
			//		Try to get a GCS. If found, it will override any implied by
			//		the PCS.
			// --------------------------------------------------------------------
			GTIFKeyGet(gtif, geokey_t.GeographicTypeGeoKey, out defn.GCS, 0, 1);
			if(defn.GCS<(geographic_t)1||defn.GCS>=(geographic_t)KvUserDefined)
				defn.GCS=(geographic_t)KvUserDefined;

			// --------------------------------------------------------------------
			//		Derive the datum, and prime meridian from the GCS.
			// --------------------------------------------------------------------
			if(defn.GCS!=(geographic_t)KvUserDefined)
				GTIFGetGCSInfo(defn.GCS, out dummy, out defn.Datum, out defn.PM, out defn.UOMAngle);

			// --------------------------------------------------------------------
			//		Handle the GCS angular units. GeogAngularUnitsGeoKey
			//		overrides the GCS or PCS setting.
			// --------------------------------------------------------------------
			GTIFKeyGet(gtif, geokey_t.GeogAngularUnitsGeoKey, out defn.UOMAngle, 0, 1);
			if(defn.UOMAngle!=(geounits_t)KvUserDefined)
				GTIFGetUOMAngleInfo(defn.UOMAngle, out dummy, out defn.UOMAngleInDegrees);

			// --------------------------------------------------------------------
			//		Check for a datum setting, and then use the datum to derive
			//		an ellipsoid.
			// --------------------------------------------------------------------
			GTIFKeyGet(gtif, geokey_t.GeogGeodeticDatumGeoKey, out defn.Datum, 0, 1);
			if(defn.Datum!=(geodeticdatum_t)KvUserDefined)
				GTIFGetDatumInfo(defn.Datum, out dummy, out defn.Ellipsoid);

			// --------------------------------------------------------------------
			//		Check for an explicit ellipsoid. Use the ellipsoid to
			//		derive the ellipsoid characteristics, if possible.
			// --------------------------------------------------------------------
			GTIFKeyGet(gtif, geokey_t.GeogEllipsoidGeoKey, out defn.Ellipsoid, 0, 1);
			if(defn.Ellipsoid!=(ellipsoid_t)KvUserDefined)
				GTIFGetEllipsoidInfo(defn.Ellipsoid, out dummy, out defn.SemiMajor, out defn.SemiMinor);

			// --------------------------------------------------------------------
			//		Check for overridden ellipsoid parameters. It would be nice
			//		to warn if they conflict with provided information, but for
			//		now we just override.
			// --------------------------------------------------------------------
			GTIFKeyGet(gtif, geokey_t.GeogSemiMajorAxisGeoKey, out defn.SemiMajor, 0, 1);
			GTIFKeyGet(gtif, geokey_t.GeogSemiMinorAxisGeoKey, out defn.SemiMinor, 0, 1);

			double invFlattening;
			if(GTIFKeyGet(gtif, geokey_t.GeogInvFlatteningGeoKey, out invFlattening, 0, 1)==1)
			{
				if(invFlattening!=0.0) defn.SemiMinor=defn.SemiMajor*(1-1.0/invFlattening);
				else defn.SemiMinor=defn.SemiMajor;
			}

			// --------------------------------------------------------------------
			//		Get the prime meridian info.
			// --------------------------------------------------------------------
			GTIFKeyGet(gtif, geokey_t.GeogPrimeMeridianGeoKey, out defn.PM, 0, 1);

			if(defn.PM!=(primemeridian_t)KvUserDefined) GTIFGetPMInfo(defn.PM, out dummy, out defn.PMLongToGreenwich);
			else
			{
				GTIFKeyGet(gtif, geokey_t.GeogPrimeMeridianLongGeoKey, out defn.PMLongToGreenwich, 0, 1);
				defn.PMLongToGreenwich=GTIFAngleToDD(defn.PMLongToGreenwich, defn.UOMAngle);
			}

			// --------------------------------------------------------------------
			//		Get the TOWGS84 parameters.
			// --------------------------------------------------------------------
			defn.TOWGS84Count=GTIFKeyGet(gtif, geokey_t.GeogTOWGS84GeoKey, out defn.TOWGS84, 0, 7);

			// --------------------------------------------------------------------
			//		Have the projection units of measure been overridden? We
			//		should likely be doing something about angular units too,
			//		but these are very rarely not decimal degrees for actual
			//		file coordinates.
			// --------------------------------------------------------------------
			GTIFKeyGet(gtif, geokey_t.ProjLinearUnitsGeoKey, out defn.UOMLength, 0, 1);
			if(defn.UOMLength!=(geounits_t)KvUserDefined)
				GTIFGetUOMLengthInfo(defn.UOMLength, out dummy, out defn.UOMLengthInMeters);

			// --------------------------------------------------------------------
			//		Handle a variety of user defined transform types.
			// --------------------------------------------------------------------
			if(GTIFKeyGet(gtif, geokey_t.ProjCoordTransGeoKey, out defn.CTProjection, 0, 1)==1)
				GTIFFetchProjParms(gtif, defn);

			// --------------------------------------------------------------------
			//		Try to set the zoned map system information.
			// --------------------------------------------------------------------
			defn.MapSys=GTIFProjToMapSys(defn.ProjCode, out defn.Zone);

			// --------------------------------------------------------------------
			//		If this is UTM, and we were unable to extract the projection
			//		parameters from the CSV file, just set them directly now,
			//		since it's pretty easy, and a common case.
			// --------------------------------------------------------------------
			if((defn.MapSys==MapSys_UTM_North||defn.MapSys==MapSys_UTM_South)&&
				defn.CTProjection==(coordtrans_t)KvUserDefined)
			{
				defn.CTProjection=coordtrans_t.CT_TransverseMercator;
				defn.nParms=7;
				defn.ProjParmId[0]=geokey_t.ProjNatOriginLatGeoKey;
				defn.ProjParm[0]=0.0;

				defn.ProjParmId[1]=geokey_t.ProjNatOriginLongGeoKey;
				defn.ProjParm[1]=defn.Zone*6-183.0;

				defn.ProjParmId[4]=geokey_t.ProjScaleAtNatOriginGeoKey;
				defn.ProjParm[4]=0.9996;

				defn.ProjParmId[5]=geokey_t.ProjFalseEastingGeoKey;
				defn.ProjParm[5]=500000.0;

				defn.ProjParmId[6]=geokey_t.ProjFalseNorthingGeoKey;

				if(defn.MapSys==MapSys_UTM_North) defn.ProjParm[6]=0.0;
				else defn.ProjParm[6]=10000000.0;
			}

			return true;
		}

		//**********************************************************************
		//							GTIFDecToDMS()
		//
		//		Convenient function to translate decimal degrees to DMS
		//		format for reporting to a user.
		//**********************************************************************
		public static string GTIFDecToDMS(double angle, string axis, int precision)
		{
			double round=0.5/60;
			for(int i=0; i<precision; i++) round=round*0.1;

			int degrees=(int)Math.Abs(angle);
			int minutes=(int)((Math.Abs(angle)-degrees)*60+round);
			double seconds=Math.Abs((Math.Abs(angle)*3600-degrees*3600-minutes*60));

			string hemisphere;
			if(axis.ToLower()=="long")
			{
				if(angle<0.0) hemisphere="W";
				else hemisphere="E";
			}
			else if(angle<0.0) hemisphere="S";
			else hemisphere="N";

			return string.Format(nc, "{0}d{1}'{2:00."+"".PadRight(precision, '#')+"}\"{3}",
				degrees, minutes, seconds, hemisphere);
		}

		//**********************************************************************
		//							GTIFPrintDefn()
		//
		//		Report the contents of a GTIFDefn structure ... mostly for
		//		debugging.
		//**********************************************************************
		public static void GTIFPrintDefn(GTIFDefn defn, StreamWriter file)
		{
			projection_t dummyproj;
			geounits_t dummyunit;
			geographic_t dummygeogr;
			geodeticdatum_t dummydatum;
			primemeridian_t dummypm;
			ellipsoid_t dummyellip;
			short dummyshort;
			double dummydouble;
			string name;

			// --------------------------------------------------------------------
			//		Do we have anything to report?
			// --------------------------------------------------------------------
			if(!defn.DefnSet)
			{
				file.WriteLine("No GeoKeys found.");
				return;
			}

			// --------------------------------------------------------------------
			//		Get the PCS name if possible.
			// --------------------------------------------------------------------
			if(defn.PCS!=(pcstype_t)KvUserDefined)
			{
				GTIFGetPCSInfo(defn.PCS, out name, out dummyproj, out dummyunit, out dummygeogr);
				if(name==null) name="name unknown";

				file.WriteLine("PCS = {0} ({1})", (int)defn.PCS, name);
			}

			// --------------------------------------------------------------------
			//	Dump the projection code if possible.
			// --------------------------------------------------------------------
			if(defn.ProjCode!=(projection_t)KvUserDefined)
			{
				GTIFGetProjTRFInfo(defn.ProjCode, out name, out dummyshort, null);
				if(name==null) name="";

				file.WriteLine("Projection = {0} ({1})", (int)defn.ProjCode, name);
			}

			// --------------------------------------------------------------------
			//		Try to dump the projection method name, and parameters if possible.
			// --------------------------------------------------------------------
			if(defn.CTProjection!=(coordtrans_t)KvUserDefined)
			{
				name=GTIFValueName(geokey_t.ProjCoordTransGeoKey, (int)defn.CTProjection);
				if(name==null) name="(unknown)";

				file.WriteLine("Projection Method: "+name);

				for(int i=0; i<defn.nParms; i++)
				{
					if(defn.ProjParmId[i]==0) continue;

					name=GTIFKeyName(defn.ProjParmId[i]);
					if(name==null) name="(unknown)";

					if(i<4)
					{
						string pszAxisName;

						if(name.Contains("Long")) pszAxisName="Long";
						else if(name.Contains("Lat")) pszAxisName="Lat";
						else pszAxisName="?";

						file.WriteLine("\t{0}: {1} ({2})", name, defn.ProjParm[i], GTIFDecToDMS(defn.ProjParm[i], pszAxisName, 2));
					}
					else if(i==4) file.WriteLine("\t{0}: {1}", name, defn.ProjParm[i]);
					else file.WriteLine("\t{0}: {1} m\n", name, defn.ProjParm[i]);
				}
			}

			// --------------------------------------------------------------------
			//		Report the GCS name, and number.
			// --------------------------------------------------------------------
			if(defn.GCS!=(geographic_t)KvUserDefined)
			{
				GTIFGetGCSInfo(defn.GCS, out name, out dummydatum, out dummypm, out dummyunit);
				if(name==null) name="(unknown)";

				file.WriteLine("GCS: {0}/{1}", (int)defn.GCS, name);
			}

			// --------------------------------------------------------------------
			//		Report the datum name.
			// --------------------------------------------------------------------
			if(defn.Datum!=(geodeticdatum_t)KvUserDefined)
			{
				GTIFGetDatumInfo(defn.Datum, out name, out dummyellip);
				if(name==null) name="(unknown)";

				file.WriteLine("Datum: {0}/{1}", (int)defn.Datum, name);
			}

			// --------------------------------------------------------------------
			//		Report the ellipsoid.
			// --------------------------------------------------------------------
			if(defn.Ellipsoid!=(ellipsoid_t)KvUserDefined)
			{
				GTIFGetEllipsoidInfo(defn.Ellipsoid, out name, out dummydouble, out dummydouble);
				if(name==null) name="(unknown)";

				file.WriteLine("Ellipsoid: {0}/{1} ({2:F2},{3:F2})", (int)defn.Ellipsoid, name,
					defn.SemiMajor, defn.SemiMinor);
			}

			// --------------------------------------------------------------------
			//		Report the prime meridian.
			// --------------------------------------------------------------------
			if(defn.PM!=(primemeridian_t)KvUserDefined)
			{
				GTIFGetPMInfo(defn.PM, out name, out dummydouble);
				if(name==null) name="(unknown)";

				file.WriteLine("Prime Meridian: {0}/{1} ({2}/{3})", (int)defn.PM, name, defn.PMLongToGreenwich,
					GTIFDecToDMS(defn.PMLongToGreenwich, "Long", 2));
			}

			// --------------------------------------------------------------------
			//		Report the projection units of measure (currently just linear).
			// --------------------------------------------------------------------
			if(defn.UOMLength!=(geounits_t)KvUserDefined)
			{
				GTIFGetUOMLengthInfo(defn.UOMLength, out name, out dummydouble);
				if(name==null) name="(unknown)";

				file.WriteLine("Projection Linear Units: {0}/{1} ({2}m)", (int)defn.UOMLength, name, defn.UOMLengthInMeters);
			}

			// --------------------------------------------------------------------
			//		Report TOWGS84 parameters.
			// --------------------------------------------------------------------
			if(defn.TOWGS84Count>0)
			{
				file.Write("TOWGS84: ");

				for(int i=0; i<defn.TOWGS84Count; i++)
				{
					if(i>0) file.Write(",");
					file.Write("{0}", defn.TOWGS84[i]);
				}
				file.WriteLine();
			}

			// --------------------------------------------------------------------
			//		Report the projection units of measure (currently just
			//		linear).
			// --------------------------------------------------------------------
			if(defn.UOMLength!=(geounits_t)KvUserDefined)
			{
				string UOMname=null;
				double inMeter=0;

				if(!GTIFGetUOMLengthInfo(defn.UOMLength, out UOMname, out inMeter)) UOMname="(unknown)";
				file.WriteLine("Projection Linear Units: {0}/{1} ({2}m)", defn.UOMLength, UOMname, defn.UOMLengthInMeters);
			}
		}

		//**********************************************************************
		//							GTIFDeaccessCSV()
		//
		//		Free all cached CSV info.
		//**********************************************************************
		public static void GTIFDeaccessCSV()
		{
			CSVDeaccess(null);
		}
	}
}
