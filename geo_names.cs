//*********************************************************************
//
// geo_names.cs
//
// This encapsulates all of the value-naming mechanism of  libgeotiff.
//
//	Written By: Niles Ritter
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
	public static partial class libgeotiff
	{
		static KeyInfo[] _formatInfo=new KeyInfo[]
		{
			new KeyInfo((int)tagtype_t.TYPE_BYTE, "Byte"),
			new KeyInfo((int)tagtype_t.TYPE_SHORT, "Short"),
			new KeyInfo((int)tagtype_t.TYPE_LONG, "Long"),
			new KeyInfo((int)tagtype_t.TYPE_RATIONAL, "Rational"),
			new KeyInfo((int)tagtype_t.TYPE_ASCII, "Ascii"),
			new KeyInfo((int)tagtype_t.TYPE_FLOAT, "Float"),
			new KeyInfo((int)tagtype_t.TYPE_DOUBLE, "Double"),
			new KeyInfo((int)tagtype_t.TYPE_SBYTE, "SignedByte"),
			new KeyInfo((int)tagtype_t.TYPE_SSHORT, "SignedShort"),
			new KeyInfo((int)tagtype_t.TYPE_SLONG, "SignedLong"),
			new KeyInfo((int)tagtype_t.TYPE_UNKNOWN, "Unknown"),
			END_LIST
		};

		static KeyInfo[] _tagInfo=new KeyInfo[]
		{
			new KeyInfo((int)GTIFF_PIXELSCALE, "ModelPixelScaleTag"),
			new KeyInfo((int)GTIFF_TRANSMATRIX, "ModelTransformationTag"),
			new KeyInfo((int)GTIFF_TIEPOINTS, "ModelTiepointTag"),
			// This alias maps the Intergraph symbol to the current tag
			new KeyInfo((int)GTIFF_TRANSMATRIX, "IntergraphMatrixTag"),
			END_LIST
		};

		static string FindName(KeyInfo[] info, int key)
		{
			foreach(KeyInfo i in info) if(i.ki_key==key) return i.ki_name;
			return string.Format("Unknown-{0}", key);
		}

		public static string GTIFKeyName(geokey_t key)
		{
			return FindName(_keyInfo, (int)key);
		}

		public static string GTIFTypeName(tagtype_t type)
		{
			return FindName(_formatInfo, (int)type);
		}

		public static string GTIFTagName(int tag)
		{
			return FindName(_tagInfo, tag);
		}

		public static string GTIFValueName(geokey_t key, int value)
		{
			KeyInfo[] info;

			switch(key)
			{
				// All codes using linear/angular/whatever units
				case geokey_t.GeogLinearUnitsGeoKey:
				case geokey_t.ProjLinearUnitsGeoKey:
				case geokey_t.GeogAngularUnitsGeoKey:
				case geokey_t.GeogAzimuthUnitsGeoKey:
				case geokey_t.VerticalUnitsGeoKey: info=_geounitsValue; break;

				// put other key-dependent lists here
				case geokey_t.GTModelTypeGeoKey: info=_modeltypeValue; break;
				case geokey_t.GTRasterTypeGeoKey: info=_rastertypeValue; break;
				case geokey_t.GeographicTypeGeoKey: info=_geographicValue; break;
				case geokey_t.GeogGeodeticDatumGeoKey: info=_geodeticdatumValue; break;
				case geokey_t.GeogEllipsoidGeoKey: info=_ellipsoidValue; break;
				case geokey_t.GeogPrimeMeridianGeoKey: info=_primemeridianValue; break;
				case geokey_t.ProjectedCSTypeGeoKey: info=_pcstypeValue; break;
				case geokey_t.ProjectionGeoKey: info=_projectionValue; break;
				case geokey_t.ProjCoordTransGeoKey: info=_coordtransValue; break;
				case geokey_t.VerticalCSTypeGeoKey: info=_vertcstypeValue; break;
				case geokey_t.VerticalDatumGeoKey: info=_vdatumValue; break;

				// And if all else fails...
				default: info=_csdefaultValue; break;
			}

			return FindName(info, value);
		}

		// Inverse Utilities (name=>code)
		static int FindCode(KeyInfo[] info, string key)
		{
			foreach(KeyInfo i in info) if(i.ki_name==key) return i.ki_key;

			// not a registered key; might be generic code
			if(!key.StartsWith("Unknown-")) return -1;

			try
			{
				key=key.Substring(8);
				return int.Parse(key);
			}
			catch
			{
				return -1;
			}
		}

		public static int GTIFKeyCode(string key)
		{
			return FindCode(_keyInfo, key);
		}

		public static int GTIFTypeCode(string type)
		{
			return FindCode(_formatInfo, type);
		}

		public static int GTIFTagCode(string tag)
		{
			return FindCode(_tagInfo, tag);
		}

		// The key must be determined with GTIFKeyCode() before
		// the name can be encoded.
		public static int GTIFValueCode(geokey_t key, string name)
		{
			KeyInfo[] info;

			switch(key)
			{
				// All codes using linear/angular/whatever units
				case geokey_t.GeogLinearUnitsGeoKey:
				case geokey_t.ProjLinearUnitsGeoKey:
				case geokey_t.GeogAngularUnitsGeoKey:
				case geokey_t.GeogAzimuthUnitsGeoKey: info=_geounitsValue; break;

				// put other key-dependent lists here
				case geokey_t.GTModelTypeGeoKey: info=_modeltypeValue; break;
				case geokey_t.GTRasterTypeGeoKey: info=_rastertypeValue; break;
				case geokey_t.GeographicTypeGeoKey: info=_geographicValue; break;
				case geokey_t.GeogGeodeticDatumGeoKey: info=_geodeticdatumValue; break;
				case geokey_t.GeogEllipsoidGeoKey: info=_ellipsoidValue; break;
				case geokey_t.GeogPrimeMeridianGeoKey: info=_primemeridianValue; break;
				case geokey_t.ProjectedCSTypeGeoKey: info=_pcstypeValue; break;
				case geokey_t.ProjectionGeoKey: info=_projectionValue; break;
				case geokey_t.ProjCoordTransGeoKey: info=_coordtransValue; break;
				case geokey_t.VerticalCSTypeGeoKey: info=_vertcstypeValue; break;
				case geokey_t.VerticalDatumGeoKey: info=_vdatumValue; break;

				// And if all else fails...
				default: info=_csdefaultValue; break;
			}

			return FindCode(info, name);
		}
	}
}
