//*****************************************************************************
//
// Project:	libgeotiff
// Purpose:	Code to convert a normalized GeoTIFF definition into a PROJ.4
//			(OGDI) compatible projection string.
// Author:	Frank Warmerdam, warmerda@home.com
//
//*****************************************************************************
// Copyright (c) 1999, Frank Warmerdam
// Copyright (c) 2008 by the Authors
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
using System.Globalization;
using System.Text;

using Free.Ports.Proj4;

namespace Free.Ports.LibGeoTiff
{
	public static partial class libgeotiff
	{
		static CultureInfo nc=new CultureInfo("");

		//***********************************************************************
		//							OSRProj4Tokenize()
		//
		//		Custom tokenizing function for PROJ.4 strings. The main
		//		reason we can't just use CSLTokenizeString is to handle
		//		strings with a + sign in the exponents of parameter values.
		//***********************************************************************
		static Dictionary<string, string> OSRProj4Tokenize(string pszFull)
		{
			if(pszFull==null) return null;

			char[] splitter=new char[] { ' ', '\t', '\n' };

			string[] parts=pszFull.Split(splitter, StringSplitOptions.RemoveEmptyEntries);

			Dictionary<string, string> papszTokens=new Dictionary<string, string>();

			foreach(string part in parts)
			{
				if(part[0]=='+')
				{
					string tmp=part.Substring(1);
					if(tmp.Length<=0) continue;

					string[] t=tmp.Split('=');
					if(t.Length==1) papszTokens.Add(t[0].ToLower(), "yes");
					if(t.Length>=2) papszTokens.Add(t[0].ToLower(), t[1]);
				}
			}

			return papszTokens;
		}

		//***********************************************************************
		//								OSR_GSV()
		//***********************************************************************
		static string OSR_GSV(Dictionary<string, string> papszNV, string pszField)
		{
			if(papszNV==null) return null;
			pszField=pszField.ToLower();
			if(papszNV.ContainsKey(pszField)) return papszNV[pszField];
			return null;
		}

		//***********************************************************************
		//								OSR_GDV()
		//
		//		Fetch a particular parameter out of the parameter list, or
		//		the indicated default if it isn't available. This is a
		//		helper function for importFromProj4().
		//***********************************************************************
		static double OSR_GDV(Dictionary<string, string> papszNV, string pszField, double dfDefaultValue)
		{
			string pszValue=OSR_GSV(papszNV, pszField);

			// special hack to use k_0 if available.
			if(pszValue==null&&pszField=="k") return OSR_GDV(papszNV, "k_0", dfDefaultValue);

			if(pszValue==null) return dfDefaultValue;

			try
			{
				return double.Parse(pszValue, nc);
			}
			catch
			{
			}
			return dfDefaultValue;
		}

		//***********************************************************************
		//							OSRFreeStringList()
		//***********************************************************************
		static void OSRFreeStringList(Dictionary<string, string> list)
		{
			list.Clear();
		}

		//***********************************************************************
		//								GTIFSetFromProj4()
		//***********************************************************************
		static bool GTIFSetFromProj4(GTIF gtif, string proj4)
		{
			Dictionary<string, string> papszNV=OSRProj4Tokenize(proj4);
			short nSpheroid=KvUserDefined;
			double dfSemiMajor=0.0, dfSemiMinor=0.0, dfInvFlattening=0.0;
			int nDatum=KvUserDefined;
			int nGCS=KvUserDefined;
			string value;

			// --------------------------------------------------------------------
			//		Get the ellipsoid definition.
			// --------------------------------------------------------------------
			value=OSR_GSV(papszNV, "ellps");
			if(value!=null) value=value.ToLower();

			if(value==null)
			{
			}
			else if(value=="WGS84") nSpheroid=(short)ellipsoid_t.Ellipse_WGS_84;
			else if(value=="clrk66") nSpheroid=(short)ellipsoid_t.Ellipse_Clarke_1866;
			else if(value=="clrk80") nSpheroid=(short)ellipsoid_t.Ellipse_Clarke_1880;
			else if(value=="GRS80") nSpheroid=(short)ellipsoid_t.Ellipse_GRS_1980;

			if(nSpheroid==KvUserDefined)
			{
				dfSemiMajor=OSR_GDV(papszNV, "a", 0.0);
				dfSemiMinor=OSR_GDV(papszNV, "b", 0.0);
				dfInvFlattening=OSR_GDV(papszNV, "rf", 0.0);
				if(dfSemiMinor!=0.0&&dfInvFlattening==0.0)
					dfInvFlattening=-1.0/(dfSemiMinor/dfSemiMajor-1.0);
			}

			// --------------------------------------------------------------------
			//		Get the GCS/Datum code.
			// --------------------------------------------------------------------
			value=OSR_GSV(papszNV, "datum");
			if(value!=null) value=value.ToLower();

			if(value==null)
			{
			}
			else if(value=="WGS84")
			{
				nGCS=(int)geographic_t.GCS_WGS_84;
				nDatum=(int)geodeticdatum_t.Datum_WGS84;
			}
			else if(value=="NAD83")
			{
				nGCS=(int)geographic_t.GCS_NAD83;
				nDatum=(int)geodeticdatum_t.Datum_North_American_Datum_1983;
			}
			else if(value=="NAD27")
			{
				nGCS=(int)geographic_t.GCS_NAD27;
				nDatum=(int)geodeticdatum_t.Datum_North_American_Datum_1927;
			}

			// --------------------------------------------------------------------
			//		Operate on the basis of the projection name.
			// --------------------------------------------------------------------
			value=OSR_GSV(papszNV, "proj");
			if(value!=null) value=value.ToLower();

			if(value==null)
			{
				OSRFreeStringList(papszNV);
				return false;
			}
			else if(value=="longlat"||value=="latlong")
			{
			}
			else if(value=="tmerc")
			{
				GTIFKeySet(gtif, geokey_t.GTModelTypeGeoKey, (ushort)modeltype_t.ModelTypeProjected);
				GTIFKeySet(gtif, geokey_t.ProjectedCSTypeGeoKey, (ushort)KvUserDefined);
				GTIFKeySet(gtif, geokey_t.ProjectionGeoKey, (ushort)KvUserDefined);
				GTIFKeySet(gtif, geokey_t.ProjCoordTransGeoKey, (ushort)coordtrans_t.CT_TransverseMercator);
				GTIFKeySet(gtif, geokey_t.ProjNatOriginLatGeoKey, OSR_GDV(papszNV, "lat_0", 0.0));
				GTIFKeySet(gtif, geokey_t.ProjNatOriginLongGeoKey, OSR_GDV(papszNV, "lon_0", 0.0));
				GTIFKeySet(gtif, geokey_t.ProjScaleAtNatOriginGeoKey, OSR_GDV(papszNV, "k", 1.0));
				GTIFKeySet(gtif, geokey_t.ProjFalseEastingGeoKey, OSR_GDV(papszNV, "x_0", 0.0));
				GTIFKeySet(gtif, geokey_t.ProjFalseNorthingGeoKey, OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="utm")
			{
				int nZone=(int)OSR_GDV(papszNV, "zone", 0);
				string south=OSR_GSV(papszNV, "south");
				GTIFKeySet(gtif, geokey_t.GTModelTypeGeoKey, (ushort)modeltype_t.ModelTypeProjected);
				GTIFKeySet(gtif, geokey_t.ProjectedCSTypeGeoKey, (ushort)KvUserDefined);
				GTIFKeySet(gtif, geokey_t.ProjectionGeoKey, (ushort)KvUserDefined);
				GTIFKeySet(gtif, geokey_t.ProjCoordTransGeoKey, (ushort)coordtrans_t.CT_TransverseMercator);
				GTIFKeySet(gtif, geokey_t.ProjNatOriginLatGeoKey, 0.0);
				GTIFKeySet(gtif, geokey_t.ProjNatOriginLongGeoKey, nZone*6-183.0);
				GTIFKeySet(gtif, geokey_t.ProjScaleAtNatOriginGeoKey, 0.9996);
				GTIFKeySet(gtif, geokey_t.ProjFalseEastingGeoKey, 500000.0);
				if(south!=null) GTIFKeySet(gtif, geokey_t.ProjFalseNorthingGeoKey, (south!=null)?10000000.0:0.0);
			}
			else if(value=="lcc"&&OSR_GDV(papszNV, "lat_0", 0.0)==OSR_GDV(papszNV, "lat_1", 0.0))
			{
				GTIFKeySet(gtif, geokey_t.GTModelTypeGeoKey, (ushort)modeltype_t.ModelTypeProjected);
				GTIFKeySet(gtif, geokey_t.ProjectedCSTypeGeoKey, (ushort)KvUserDefined);
				GTIFKeySet(gtif, geokey_t.ProjectionGeoKey, (ushort)KvUserDefined);
				GTIFKeySet(gtif, geokey_t.ProjCoordTransGeoKey, (ushort)coordtrans_t.CT_LambertConfConic_1SP);
				GTIFKeySet(gtif, geokey_t.ProjNatOriginLatGeoKey, OSR_GDV(papszNV, "lat_0", 0.0));
				GTIFKeySet(gtif, geokey_t.ProjNatOriginLongGeoKey, OSR_GDV(papszNV, "lon_0", 0.0));
				GTIFKeySet(gtif, geokey_t.ProjScaleAtNatOriginGeoKey, OSR_GDV(papszNV, "k", 1.0));
				GTIFKeySet(gtif, geokey_t.ProjFalseEastingGeoKey, OSR_GDV(papszNV, "x_0", 0.0));
				GTIFKeySet(gtif, geokey_t.ProjFalseNorthingGeoKey, OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="lcc")
			{
				GTIFKeySet(gtif, geokey_t.GTModelTypeGeoKey, (ushort)modeltype_t.ModelTypeProjected);
				GTIFKeySet(gtif, geokey_t.ProjectedCSTypeGeoKey, (ushort)KvUserDefined);
				GTIFKeySet(gtif, geokey_t.ProjectionGeoKey, (ushort)KvUserDefined);
				GTIFKeySet(gtif, geokey_t.ProjCoordTransGeoKey, (ushort)coordtrans_t.CT_LambertConfConic_2SP);
				GTIFKeySet(gtif, geokey_t.ProjFalseOriginLatGeoKey, OSR_GDV(papszNV, "lat_0", 0.0));
				GTIFKeySet(gtif, geokey_t.ProjFalseOriginLongGeoKey, OSR_GDV(papszNV, "lon_0", 0.0));
				GTIFKeySet(gtif, geokey_t.ProjStdParallel1GeoKey, OSR_GDV(papszNV, "lat_1", 0.0));
				GTIFKeySet(gtif, geokey_t.ProjStdParallel2GeoKey, OSR_GDV(papszNV, "lat_2", 0.0));
				GTIFKeySet(gtif, geokey_t.ProjFalseOriginEastingGeoKey, OSR_GDV(papszNV, "x_0", 0.0));
				GTIFKeySet(gtif, geokey_t.ProjFalseOriginNorthingGeoKey, OSR_GDV(papszNV, "y_0", 0.0));
			}
#if notdef
			else if(value=="bonne")
			{
				SetBonne(OSR_GDV(papszNV, "lat_1", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="cass")
			{
				SetCS(OSR_GDV(papszNV, "lat_0", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="nzmg")
			{
				SetNZMG(OSR_GDV(papszNV, "lat_0", -41.0),
					OSR_GDV(papszNV, "lon_0", 173.0),
					OSR_GDV(papszNV, "x_0", 2510000.0),
					OSR_GDV(papszNV, "y_0", 6023150.0));
			}
			else if(value=="cea")
			{
				SetCEA(OSR_GDV(papszNV, "lat_ts", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="merc"&&OSR_GDV(papszNV, "lat_ts", 1000.0)<999.0) // 2SP form
			{
				SetMercator2SP(OSR_GDV(papszNV, "lat_ts", 0.0),
					0.0,
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="merc") // 1SP form
			{
				SetMercator(0.0,
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "k", 1.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="stere"&&Math.Abs(OSR_GDV(papszNV, "lat_0", 0.0)-90)<0.001)
			{
				SetPS(OSR_GDV(papszNV, "lat_ts", 90.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "k", 1.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="stere"&&Math.Abs(OSR_GDV(papszNV, "lat_0", 0.0)+90)<0.001)
			{
				SetPS(OSR_GDV(papszNV, "lat_ts", -90.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "k", 1.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value.StartsWith("stere")&&OSR_GSV(papszNV, "k")!=null) // mostly sterea
			{
				SetOS(OSR_GDV(papszNV, "lat_0", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "k", 1.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="stere")
			{
				SetStereographic(OSR_GDV(papszNV, "lat_0", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					1.0,
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="eqc")
			{
				if(OSR_GDV(papszNV, "lat_0", 0.0)!=OSR_GDV(papszNV, "lat_ts", 0.0))
					SetEquirectangular2(OSR_GDV(papszNV, "lat_0", 0.0),
						OSR_GDV(papszNV, "lon_0", 0.0)+dfFromGreenwich,
						OSR_GDV(papszNV, "lat_ts", 0.0),
						OSR_GDV(papszNV, "x_0", 0.0),
						OSR_GDV(papszNV, "y_0", 0.0));
				else
					SetEquirectangular(OSR_GDV(papszNV, "lat_ts", 0.0),
						OSR_GDV(papszNV, "lon_0", 0.0)+dfFromGreenwich,
						OSR_GDV(papszNV, "x_0", 0.0),
						OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="glabsgm")
			{
				SetGaussLabordeReunion(OSR_GDV(papszNV, "lat_0", -21.116666667),
					OSR_GDV(papszNV, "lon_0", 55.53333333309)+dfFromGreenwich,
					OSR_GDV(papszNV, "k_0", 1.0),
					OSR_GDV(papszNV, "x_0", 160000.000),
					OSR_GDV(papszNV, "y_0", 50000.000));
			}
			else if(value=="gnom")
			{
				SetGnomonic(OSR_GDV(papszNV, "lat_0", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="ortho")
			{
				SetOrthographic(OSR_GDV(papszNV, "lat_0", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="laea")
			{
				SetLAEA(OSR_GDV(papszNV, "lat_0", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="aeqd")
			{
				SetAE(OSR_GDV(papszNV, "lat_0", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="eqdc")
			{
				SetEC(OSR_GDV(papszNV, "lat_1", 0.0),
					OSR_GDV(papszNV, "lat_2", 0.0),
					OSR_GDV(papszNV, "lat_0", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="mill")
			{
				SetMC(OSR_GDV(papszNV, "lat_0", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="moll")
			{
				SetMollweide(OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="eck4")
			{
				SetEckertIV(OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="eck6")
			{
				SetEckertVI(OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="poly")
			{
				SetPolyconic(OSR_GDV(papszNV, "lat_0", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="aea")
			{
				SetACEA(OSR_GDV(papszNV, "lat_1", 0.0),
					OSR_GDV(papszNV, "lat_2", 0.0),
					OSR_GDV(papszNV, "lat_0", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="robin")
			{
				SetRobinson(OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="vandg")
			{
				SetVDG(OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="sinu")
			{
				SetSinusoidal(OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="gall")
			{
				SetGS(OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="goode")
			{
				SetGH(OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="geos")
			{
				SetGEOS(OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "h", 35785831.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="lcc")
			{
				if(OSR_GDV(papszNV, "lat_0", 0.0)==OSR_GDV(papszNV, "lat_1", 0.0))
				{
					// 1SP form
					SetLCC1SP(OSR_GDV(papszNV, "lat_0", 0.0),
						OSR_GDV(papszNV, "lon_0", 0.0),
						OSR_GDV(papszNV, "k_0", 1.0),
						OSR_GDV(papszNV, "x_0", 0.0),
						OSR_GDV(papszNV, "y_0", 0.0));
				}
				else
				{
					// 2SP form
					SetLCC(OSR_GDV(papszNV, "lat_1", 0.0),
						OSR_GDV(papszNV, "lat_2", 0.0),
						OSR_GDV(papszNV, "lat_0", 0.0),
						OSR_GDV(papszNV, "lon_0", 0.0),
						OSR_GDV(papszNV, "x_0", 0.0),
						OSR_GDV(papszNV, "y_0", 0.0));
				}
			}
			else if(value=="omerc")
			{
				SetHOM(OSR_GDV(papszNV, "lat_0", 0.0),
					OSR_GDV(papszNV, "lonc", 0.0),
					OSR_GDV(papszNV, "alpha", 0.0),
					0.0, // ???
					OSR_GDV(papszNV, "k", 1.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="somerc")
			{
				SetHOM(OSR_GDV(papszNV, "lat_0", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					90.0, 90.0,
					OSR_GDV(papszNV, "k", 1.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="krovak")
			{
				SetKrovak(OSR_GDV(papszNV, "lat_0", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "alpha", 0.0),
					0.0, // pseudo_standard_parallel_1
					OSR_GDV(papszNV, "k", 1.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="iwm_p")
			{
				SetIWMPolyconic(OSR_GDV(papszNV, "lat_1", 0.0),
					OSR_GDV(papszNV, "lat_2", 0.0),
					OSR_GDV(papszNV, "lon_0", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="wag1")
			{
				SetWagner(1, 0.0,
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="wag2")
			{
				SetWagner(2, 0.0,
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="wag3")
			{
				SetWagner(3,
					OSR_GDV(papszNV, "lat_ts", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="wag1")
			{
				SetWagner(4, 0.0,
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="wag1")
			{
				SetWagner(5, 0.0,
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="wag1")
			{
				SetWagner(6, 0.0,
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="wag1")
			{
				SetWagner(7, 0.0,
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
			else if(value=="tpeqd")
			{
				SetTPED(OSR_GDV(papszNV, "lat_1", 0.0),
					OSR_GDV(papszNV, "lon_1", 0.0),
					OSR_GDV(papszNV, "lat_2", 0.0),
					OSR_GDV(papszNV, "lon_2", 0.0),
					OSR_GDV(papszNV, "x_0", 0.0),
					OSR_GDV(papszNV, "y_0", 0.0));
			}
#endif
			else
			{
				// unsupported coordinate system
				OSRFreeStringList(papszNV);
				return false;
			}

			// --------------------------------------------------------------------
			//		Write the GCS if we have it, otherwise write the datum.
			// --------------------------------------------------------------------
			if(nGCS!=KvUserDefined)
			{
				GTIFKeySet(gtif, geokey_t.GeographicTypeGeoKey, (ushort)nGCS);
			}
			else
			{
				GTIFKeySet(gtif, geokey_t.GeographicTypeGeoKey, (ushort)KvUserDefined);
				GTIFKeySet(gtif, geokey_t.GeogGeodeticDatumGeoKey, (ushort)nDatum);
			}

			// --------------------------------------------------------------------
			//		Write the ellipsoid if we don't know the GCS.
			// --------------------------------------------------------------------
			if(nGCS==KvUserDefined)
			{
				if(nSpheroid!=KvUserDefined) GTIFKeySet(gtif, geokey_t.GeogEllipsoidGeoKey, (ushort)nSpheroid);
				else
				{
					GTIFKeySet(gtif, geokey_t.GeogEllipsoidGeoKey, (ushort)KvUserDefined);
					GTIFKeySet(gtif, geokey_t.GeogSemiMajorAxisGeoKey, dfSemiMajor);
					if(dfInvFlattening==0.0) GTIFKeySet(gtif, geokey_t.GeogSemiMinorAxisGeoKey, dfSemiMajor);
					else GTIFKeySet(gtif, geokey_t.GeogInvFlatteningGeoKey, dfInvFlattening);
				}
			}

			// --------------------------------------------------------------------
			//		Linear units translation
			// --------------------------------------------------------------------
			value=OSR_GSV(papszNV, "units");
			if(value!=null) value=value.ToLower();

			if(value==null)
			{
				value=OSR_GSV(papszNV, "to_meter");
				if(value!=null)
				{
					GTIFKeySet(gtif, geokey_t.ProjLinearUnitsGeoKey, (ushort)KvUserDefined);
					GTIFKeySet(gtif, geokey_t.ProjLinearUnitSizeGeoKey, GTIFAtof(value));
				}
			}
			else if(value=="meter"||value=="m")
			{
				GTIFKeySet(gtif, geokey_t.ProjLinearUnitsGeoKey, (ushort)geounits_t.Linear_Meter);
			}
			else if(value=="us-ft")
			{
				GTIFKeySet(gtif, geokey_t.ProjLinearUnitsGeoKey, (ushort)geounits_t.Linear_Foot_US_Survey);
			}
			else if(value=="ft")
			{
				GTIFKeySet(gtif, geokey_t.ProjLinearUnitsGeoKey, (ushort)geounits_t.Linear_Foot);
			}

			OSRFreeStringList(papszNV);

			return true;
		}

		//************************************************************************
		//*							GTIFGetProj4Defn()
		//************************************************************************
		static string GTIFGetProj4Defn(GTIFDefn defn)
		{
			string projection="";
			string units="";
			double falseEasting, falseNorthing;

			if(defn==null||!defn.DefnSet) return "";

			// ====================================================================
			//		Translate the units of measure.
			//
			//		Note that even with a +units, or +to_meter in effect, it is
			//		still assumed that all the projection parameters are in
			//		meters.
			// ====================================================================
			if(defn.UOMLength==geounits_t.Linear_Meter) units="+units=m ";
			else if(defn.UOMLength==geounits_t.Linear_Foot) units="+units=ft ";
			else if(defn.UOMLength==geounits_t.Linear_Foot_US_Survey) units="+units=us-ft ";
			else if(defn.UOMLength==geounits_t.Linear_Foot_Indian) units="+units=ind-ft ";
			else if(defn.UOMLength==geounits_t.Linear_Link) units="+units=link ";
			else if(defn.UOMLength==geounits_t.Linear_Yard_Indian) units="+units=ind-yd ";
			else if(defn.UOMLength==geounits_t.Linear_Fathom) units="+units=fath ";
			else if(defn.UOMLength==geounits_t.Linear_Mile_International_Nautical) units="+units=kmi ";
			else units=string.Format("+to_meter={0}", defn.UOMLengthInMeters);

			// --------------------------------------------------------------------
			//		false easting and northing are in meters and that is what
			//		PROJ.4 wants regardless of the linear units.
			// --------------------------------------------------------------------
			falseEasting=defn.ProjParm[5];
			falseNorthing=defn.ProjParm[6];

			// ====================================================================
			//		Handle general projection methods.
			// ====================================================================

			// --------------------------------------------------------------------
			//		Geographic.
			// --------------------------------------------------------------------
			if(defn.Model==modeltype_t.ModelTypeGeographic)
			{
				projection+="+proj=latlong ";
			}
			// --------------------------------------------------------------------
			//		UTM - special case override on transverse mercator so things
			//		will be more meaningful to the user.
			// --------------------------------------------------------------------
			else if(defn.MapSys==MapSys_UTM_North)
			{
				projection+=string.Format("+proj=utm +zone={0} ", defn.Zone);
			}
			// --------------------------------------------------------------------
			//		Transverse Mercator
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_TransverseMercator)
			{
				projection+=string.Format(nc, "+proj=tmerc +lat_0={0} +lon_0={1} +k={2} +x_0={3:0.###} +y_0={4:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], defn.ProjParm[4], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		Mercator
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_Mercator)
			{
				projection+=string.Format(nc, "+proj=merc +lat_ts={0} +lon_0={1} +k={2} +x_0={3:0.###} +y_0={4:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], defn.ProjParm[4], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		Cassini/Soldner
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_CassiniSoldner)
			{
				projection+=string.Format(nc, "+proj=cass +lat_0={0} +lon_0={1} +x_0={2:0.###} +y_0={3:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		Oblique Stereographic - Should this really map onto
			//		Stereographic?
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_ObliqueStereographic)
			{
				projection+=string.Format(nc, "+proj=stere +lat_0={0} +lon_0={1} +k={2} +x_0={3:0.###} +y_0={4:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], defn.ProjParm[4], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		Stereographic
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_Stereographic)
			{
				projection+=string.Format(nc, "+proj=stere +lat_0={0} +lon_0={1} +x_0={2:0.###} +y_0={3:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		Polar Stereographic
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_PolarStereographic)
			{
				if(defn.ProjParm[0]>0.0)
					projection+=string.Format(nc, "+proj=stere +lat_0=90 +lat_ts={0} +lon_0={1} +k={2} +x_0={3:0.###} +y_0={4:0.###} ",
						defn.ProjParm[0], defn.ProjParm[1], defn.ProjParm[4], falseEasting, falseNorthing);
				else
					projection+=string.Format(nc, "+proj=stere +lat_0=-90 +lat_ts={0} +lon_0={1} +k={2} +x_0={3:0.###} +y_0={4:0.###} ",
						defn.ProjParm[0], defn.ProjParm[1], defn.ProjParm[4], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		Equirectangular
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_Equirectangular)
			{
				projection+=string.Format(nc, "+proj=eqc +lat_0={0} +lon_0={1} +lat_ts={2} +x_0={3:0.###} +y_0={4:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], defn.ProjParm[2], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		Gnomonic
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_Gnomonic)
			{
				projection+=string.Format(nc, "+proj=gnom +lat_0={0} +lon_0={1} +x_0={2:0.###} +y_0={3:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		Orthographic
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_Orthographic)
			{
				projection+=string.Format(nc, "+proj=ortho +lat_0={0} +lon_0={1} +x_0={2:0.###} +y_0={3:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		Lambert Azimuthal Equal Area
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_LambertAzimEqualArea)
			{
				projection+=string.Format(nc, "+proj=laea +lat_0={0} +lon_0={1} +x_0={2:0.###} +y_0={3:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		Azimuthal Equidistant
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_AzimuthalEquidistant)
			{
				projection+=string.Format(nc, "+proj=aeqd +lat_0={0} +lon_0={1} +x_0={2:0.###} +y_0={3:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		Miller Cylindrical
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_MillerCylindrical)
			{
				projection+=string.Format(nc, "+proj=mill +lat_0={0} +lon_0={1} +x_0={2:0.###} +y_0={3:0.###} +R_A ",
					defn.ProjParm[0], defn.ProjParm[1], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		Polyconic
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_Polyconic)
			{
				projection+=string.Format(nc, "+proj=poly +lat_0={0} +lon_0={1} +x_0={2:0.###} +y_0={3:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		AlbersEqualArea
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_AlbersEqualArea)
			{
				projection+=string.Format(nc, "+proj=aea +lat_1={0} +lat_2={1} +lat_0={2} +lon_0={3} +x_0={4:0.###} +y_0={5:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], defn.ProjParm[2], defn.ProjParm[3], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		EquidistantConic
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_EquidistantConic)
			{
				projection+=string.Format(nc, "+proj=eqdc +lat_1={0} +lat_2={1} +lat_0={2} +lon_0={3} +x_0={4:0.###} +y_0={5:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], defn.ProjParm[2], defn.ProjParm[3], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		Robinson
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_Robinson)
			{
				projection+=string.Format(nc, "+proj=robin +lon_0={0} +x_0={1:0.###} +y_0={2:0.###} ",
					defn.ProjParm[1], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		VanDerGrinten
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_VanDerGrinten)
			{
				projection+=string.Format(nc, "+proj=vandg +lon_0={0} +x_0={1:0.###} +y_0={2:0.###} +R_A ",
					defn.ProjParm[1], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		Sinusoidal
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_Sinusoidal)
			{
				projection+=string.Format(nc, "+proj=sinu +lon_0={0} +x_0={1:0.###} +y_0={2:0.###} ",
					defn.ProjParm[1], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		LambertConfConic_2SP
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_LambertConfConic_2SP)
			{
				projection+=string.Format(nc, "+proj=lcc +lat_0={0} +lon_0={1} +lat_1={2} +lat_2={3} +x_0={4:0.###} +y_0={5:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], defn.ProjParm[2], defn.ProjParm[3], falseEasting, falseNorthing);
			}
			// --------------------------------------------------------------------
			//		LambertConfConic_1SP
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_LambertConfConic_1SP)
			{
				projection+=string.Format(nc, "+proj=lcc +lat_0={0} +lat_1={1} +lon_0={2} +k_0={3} +x_0={4:0.###} +y_0={5:0.###} ",
					defn.ProjParm[0], defn.ProjParm[0], defn.ProjParm[1], defn.ProjParm[4], defn.ProjParm[5], defn.ProjParm[6]);
			}
			// --------------------------------------------------------------------
			//		CT_CylindricalEqualArea
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_CylindricalEqualArea)
			{
				projection+=string.Format(nc, "+proj=cea +lat_ts={0} +lon_0={1} +x_0={2:0.###} +y_0={3:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], defn.ProjParm[5], defn.ProjParm[6]);
			}
			// --------------------------------------------------------------------
			//		NewZealandMapGrid
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_NewZealandMapGrid)
			{
				projection+=string.Format(nc, "+proj=nzmg +lat_0={0} +lon_0={1} +x_0={2:0.###} +y_0={3:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], defn.ProjParm[5], defn.ProjParm[6]);
			}
			// --------------------------------------------------------------------
			//		Transverse Mercator - south oriented.
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_TransvMercator_SouthOriented)
			{
				// this appears to be an unsupported formulation with PROJ.4
			}
			// --------------------------------------------------------------------
			//		ObliqueMercator (Hotine)
			// --------------------------------------------------------------------
			else if(defn.CTProjection==coordtrans_t.CT_ObliqueMercator)
			{
				// not clear how ProjParm[3] - angle from rectified to skewed grid -
				// should be applied ... see the +not_rot flag for PROJ.4.
				// Just ignoring for now.
				projection+=string.Format(nc, "+proj=omerc +lat_0={0} +lonc={1} +alpha={2} +k={3} +x_0={4:0.###} +y_0={5:0.###} ",
					defn.ProjParm[0], defn.ProjParm[1], defn.ProjParm[2], defn.ProjParm[4], defn.ProjParm[5], defn.ProjParm[6]);
			}

			// If we don't have a projection leave here
			if(projection=="") return "";

			// ====================================================================
			//		Handle ellipsoid information.
			// ====================================================================
			if(defn.Ellipsoid==ellipsoid_t.Ellipse_WGS_84) projection+="+ellps=WGS84 ";
			else if(defn.Ellipsoid==ellipsoid_t.Ellipse_Clarke_1866) projection+="+ellps=clrk66 ";
			else if(defn.Ellipsoid==ellipsoid_t.Ellipse_Clarke_1880) projection+="+ellps=clrk80 ";
			else if(defn.Ellipsoid==ellipsoid_t.Ellipse_GRS_1980) projection+="+ellps=GRS80 ";
			else
			{
				if(defn.SemiMajor!=0.0&&defn.SemiMinor!=0.0)
					projection+=string.Format(nc, "+a={0:0.###} +b={1:0.###} ", defn.SemiMajor, defn.SemiMinor);
			}

			return projection+units;
		}

		//**********************************************************************
		//*							GTIFProj4FromLatLong()
		//*
		//*		Convert lat/long values to projected coordinate for a
		//*		particular definition.
		//**********************************************************************
		public static bool GTIFProj4FromLatLong(GTIFDefn psDefn, int nPoints, double[] padfX, double[] padfY)
		{
			// --------------------------------------------------------------------
			//		Get a projection definition.
			// --------------------------------------------------------------------
			string pszProjection=GTIFGetProj4Defn(psDefn);
			if(pszProjection==null) return false;

			// --------------------------------------------------------------------
			//		Parse into tokens for pj_init(), and initialize the projection.
			// --------------------------------------------------------------------
			PJ psPJ=Proj.pj_init_plus(pszProjection);
			if(psPJ==null) return false;

			// --------------------------------------------------------------------
			//		Process each of the points.
			// --------------------------------------------------------------------
			for(int i=0; i<nPoints; i++)
			{
				projUV sUV;
				sUV.u=padfX[i]*Proj.DEG_TO_RAD;
				sUV.v=padfY[i]*Proj.DEG_TO_RAD;

				sUV=Proj.pj_fwd(sUV, psPJ);

				padfX[i]=sUV.u;
				padfY[i]=sUV.v;
			}

			return true;
		}

		//***********************************************************************
		//*							GTIFProj4ToLatLong()
		//*
		//*		Convert projection coordinates to lat/long for a particular
		//*		definition.
		//***********************************************************************
		public static bool GTIFProj4ToLatLong(GTIFDefn psDefn, int nPoints, double[] padfX, double[] padfY)
		{
			// --------------------------------------------------------------------
			//		Get a projection definition.
			// --------------------------------------------------------------------
			string pszProjection=GTIFGetProj4Defn(psDefn);
			if(pszProjection==null) return false;

			// --------------------------------------------------------------------
			//		Parse into tokens for pj_init(), and initialize the projection.
			// --------------------------------------------------------------------
			PJ psPJ=Proj.pj_init_plus(pszProjection);
			if(psPJ==null) return false;

			// --------------------------------------------------------------------
			//		Process each of the points.
			// --------------------------------------------------------------------
			for(int i=0; i<nPoints; i++)
			{
				projUV sUV;

				sUV.u=padfX[i];
				sUV.v=padfY[i];

				sUV=Proj.pj_inv(sUV, psPJ);

				padfX[i]=sUV.u*Proj.RAD_TO_DEG;
				padfY[i]=sUV.v*Proj.RAD_TO_DEG;
			}

			return true;
		}
	}
}
