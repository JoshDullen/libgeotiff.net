//********************************************************************
//
// geokeys.cs - Public registry for valid GEOTIFF GeoKeys.
//
//	Written By: Niles D. Ritter
//				The Authors
//
// Copyright (c) 1995, Niles D. Ritter
// Copyright (c) 2008 by the Authors
//
// Permission granted to use this software, so long as this copyright
// notice accompanies any products derived therefrom.
//
//********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Free.Ports.LibGeoTiff
{
	public static partial class libgeotiff
	{
		// The GvCurrentRevision number should be incremented whenever a
		// new set of Keys are defined or modified in "geokeys.inc", and comments
		// added to the "Revision History" section above. If only code
		// _values_ are augmented, the "GvCurrentMinorRev" number should
		// be incremented instead (see "geovalues.cs"). Whenever the
		// GvCurrentRevision is incremented, the GvCurrentMinorRev should
		// be reset to zero.
		//
		// The Section Numbers below refer to the GeoTIFF Spec sections
		// in which these values are documented.
		public const int GvCurrentRevision=1; // Final 1.0 Release
	}

	public enum geokey_t
	{
		BaseGeoKey=1024,	// First valid code

		// GeoTIFF GeoKey Database

		// Note: Any changes/additions to this database require
		// a change in the revision value in geokeys.cs

		// Revised 28 Sep 1995 NDR -- Added Rev. 1.0 aliases.

		// 6.2.1 GeoTIFF Configuration Keys
		GTModelTypeGeoKey=1024,		// Section 6.3.1.1 Codes
		GTRasterTypeGeoKey=1025,	// Section 6.3.1.2 Codes
		GTCitationGeoKey=1026,		// documentation

		// 6.2.2 Geographic CS Parameter Keys
		GeographicTypeGeoKey=2048,		// Section 6.3.2.1 Codes
		GeogCitationGeoKey=2049,		// documentation
		GeogGeodeticDatumGeoKey=2050,	// Section 6.3.2.2 Codes
		GeogPrimeMeridianGeoKey=2051,	// Section 6.3.2.4 codes
		GeogLinearUnitsGeoKey=2052,		// Section 6.3.1.3 Codes
		GeogLinearUnitSizeGeoKey=2053,	// meters
		GeogAngularUnitsGeoKey=2054,	// Section 6.3.1.4 Codes
		GeogAngularUnitSizeGeoKey=2055,	// radians
		GeogEllipsoidGeoKey=2056,		// Section 6.3.2.3 Codes
		GeogSemiMajorAxisGeoKey=2057,	// GeogLinearUnits
		GeogSemiMinorAxisGeoKey=2058,	// GeogLinearUnits
		GeogInvFlatteningGeoKey=2059,	// ratio
		GeogAzimuthUnitsGeoKey=2060,	// Section 6.3.1.4 Codes
		GeogPrimeMeridianLongGeoKey=2061, // GeoAngularUnit
		GeogTOWGS84GeoKey=2062,			// 2011 - proposed addition

		// 6.2.3 Projected CS Parameter Keys
		//	Several keys have been renamed,
		//	and the deprecated names aliased for backward compatibility
		ProjectedCSTypeGeoKey=3072,			// Section 6.3.3.1 codes
		PCSCitationGeoKey=3073,				// documentation
		ProjectionGeoKey=3074,				// Section 6.3.3.2 codes
		ProjCoordTransGeoKey=3075,			// Section 6.3.3.3 codes
		ProjLinearUnitsGeoKey=3076,			// Section 6.3.1.3 codes
		ProjLinearUnitSizeGeoKey=3077,		// meters
		ProjStdParallel1GeoKey=3078,		// GeogAngularUnit
		ProjStdParallelGeoKey=ProjStdParallel1GeoKey,	// ** alias **
		ProjStdParallel2GeoKey=3079,		// GeogAngularUnit
		ProjNatOriginLongGeoKey=3080,		// GeogAngularUnit
		ProjOriginLongGeoKey=ProjNatOriginLongGeoKey,	// ** alias **
		ProjNatOriginLatGeoKey=3081,		// GeogAngularUnit
		ProjOriginLatGeoKey=ProjNatOriginLatGeoKey,		// ** alias **
		ProjFalseEastingGeoKey=3082,		// ProjLinearUnits
		ProjFalseNorthingGeoKey=3083,		// ProjLinearUnits
		ProjFalseOriginLongGeoKey=3084,		// GeogAngularUnit
		ProjFalseOriginLatGeoKey=3085,		// GeogAngularUnit
		ProjFalseOriginEastingGeoKey=3086,	// ProjLinearUnits
		ProjFalseOriginNorthingGeoKey=3087,	// ProjLinearUnits
		ProjCenterLongGeoKey=3088,			// GeogAngularUnit
		ProjCenterLatGeoKey=3089,			// GeogAngularUnit
		ProjCenterEastingGeoKey=3090,		// ProjLinearUnits
		ProjCenterNorthingGeoKey=3091,		// ProjLinearUnits
		ProjScaleAtNatOriginGeoKey=3092,	// ratio
		ProjScaleAtOriginGeoKey=ProjScaleAtNatOriginGeoKey,		// ** alias **
		ProjScaleAtCenterGeoKey=3093,		// ratio
		ProjAzimuthAngleGeoKey=3094,		// GeogAzimuthUnit
		ProjStraightVertPoleLongGeoKey=3095,// GeogAngularUnit
		ProjRectifiedGridAngleGeoKey=3096,	// GeogAngularUnit

		// 6.2.4 Vertical CS Keys
		VerticalCSTypeGeoKey=4096,		// Section 6.3.4.1 codes
		VerticalCitationGeoKey=4097,	// documentation
		VerticalDatumGeoKey=4098,		// Section 6.3.4.2 codes
		VerticalUnitsGeoKey=4099,		// Section 6.3.1 (.x) codes
		// End of Data base

		ReservedEndGeoKey=32767,

		// Key space available for Private or internal use
		PrivateBaseGeoKey=32768,	// Consistent with TIFF Private tags
		PrivateEndGeoKey=65535,

		EndGeoKey=65535				// Largest Possible GeoKey ID
	}
}
