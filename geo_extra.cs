//*****************************************************************************
//
// Project:	libgeotiff
// Purpose:	Code to normalize a few common PCS values without use of CSV
//			files.
// Author:	Frank Warmerdam, warmerda@home.com
//
//*****************************************************************************
// Copyright (c) 1999, Frank Warmerdam
// Copyright (c) 2009 by the Authors
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
using System.Text;

namespace Free.Ports.LibGeoTiff
{
	public static partial class libgeotiff
	{
		#region static int[] StatePlaneTable=new int[] { ... }
		static int[] StatePlaneTable=new int[] {
			(int)pcstype_t.PCS_NAD83_Alabama_East, (int)projection_t.Proj_Alabama_CS83_East,
			(int)pcstype_t.PCS_NAD83_Alabama_West, (int)projection_t.Proj_Alabama_CS83_West,

			(int)pcstype_t.PCS_NAD83_Alaska_zone_1, (int)projection_t.Proj_Alaska_CS83_1,
			(int)pcstype_t.PCS_NAD83_Alaska_zone_2, (int)projection_t.Proj_Alaska_CS83_2,
			(int)pcstype_t.PCS_NAD83_Alaska_zone_3, (int)projection_t.Proj_Alaska_CS83_3,
			(int)pcstype_t.PCS_NAD83_Alaska_zone_4, (int)projection_t.Proj_Alaska_CS83_4,
			(int)pcstype_t.PCS_NAD83_Alaska_zone_5, (int)projection_t.Proj_Alaska_CS83_5,
			(int)pcstype_t.PCS_NAD83_Alaska_zone_6, (int)projection_t.Proj_Alaska_CS83_6,
			(int)pcstype_t.PCS_NAD83_Alaska_zone_7, (int)projection_t.Proj_Alaska_CS83_7,
			(int)pcstype_t.PCS_NAD83_Alaska_zone_8, (int)projection_t.Proj_Alaska_CS83_8,
			(int)pcstype_t.PCS_NAD83_Alaska_zone_9, (int)projection_t.Proj_Alaska_CS83_9,
			(int)pcstype_t.PCS_NAD83_Alaska_zone_10, (int)projection_t.Proj_Alaska_CS83_10,

			(int)pcstype_t.PCS_NAD83_California_1, (int)projection_t.Proj_California_CS83_1,
			(int)pcstype_t.PCS_NAD83_California_2, (int)projection_t.Proj_California_CS83_2,
			(int)pcstype_t.PCS_NAD83_California_3, (int)projection_t.Proj_California_CS83_3,
			(int)pcstype_t.PCS_NAD83_California_4, (int)projection_t.Proj_California_CS83_4,
			(int)pcstype_t.PCS_NAD83_California_5, (int)projection_t.Proj_California_CS83_5,
			(int)pcstype_t.PCS_NAD83_California_6, (int)projection_t.Proj_California_CS83_6,

			(int)pcstype_t.PCS_NAD83_Arizona_East, (int)projection_t.Proj_Arizona_CS83_east,
			(int)pcstype_t.PCS_NAD83_Arizona_Central, (int)projection_t.Proj_Arizona_CS83_Central,
			(int)pcstype_t.PCS_NAD83_Arizona_West, (int)projection_t.Proj_Arizona_CS83_west,

			(int)pcstype_t.PCS_NAD83_Arkansas_North, (int)projection_t.Proj_Arkansas_CS83_North,
			(int)pcstype_t.PCS_NAD83_Arkansas_South, (int)projection_t.Proj_Arkansas_CS83_South,

			(int)pcstype_t.PCS_NAD83_Colorado_North, (int)projection_t.Proj_Colorado_CS83_North,
			(int)pcstype_t.PCS_NAD83_Colorado_Central, (int)projection_t.Proj_Colorado_CS83_Central,
			(int)pcstype_t.PCS_NAD83_Colorado_South, (int)projection_t.Proj_Colorado_CS83_South,

			(int)pcstype_t.PCS_NAD83_Connecticut, (int)projection_t.Proj_Connecticut_CS83,

			(int)pcstype_t.PCS_NAD83_Delaware, (int)projection_t.Proj_Delaware_CS83,

			(int)pcstype_t.PCS_NAD83_Florida_East, (int)projection_t.Proj_Florida_CS83_East,
			(int)pcstype_t.PCS_NAD83_Florida_North, (int)projection_t.Proj_Florida_CS83_North,
			(int)pcstype_t.PCS_NAD83_Florida_West, (int)projection_t.Proj_Florida_CS83_West,

			(int)pcstype_t.PCS_NAD83_Hawaii_zone_1, (int)projection_t.Proj_Hawaii_CS83_1,
			(int)pcstype_t.PCS_NAD83_Hawaii_zone_2, (int)projection_t.Proj_Hawaii_CS83_2,
			(int)pcstype_t.PCS_NAD83_Hawaii_zone_3, (int)projection_t.Proj_Hawaii_CS83_3,
			(int)pcstype_t.PCS_NAD83_Hawaii_zone_4, (int)projection_t.Proj_Hawaii_CS83_4,
			(int)pcstype_t.PCS_NAD83_Hawaii_zone_5, (int)projection_t.Proj_Hawaii_CS83_5,

			(int)pcstype_t.PCS_NAD83_Georgia_East, (int)projection_t.Proj_Georgia_CS83_East,
			(int)pcstype_t.PCS_NAD83_Georgia_West, (int)projection_t.Proj_Georgia_CS83_West,

			(int)pcstype_t.PCS_NAD83_Idaho_East, (int)projection_t.Proj_Idaho_CS83_East,
			(int)pcstype_t.PCS_NAD83_Idaho_Central, (int)projection_t.Proj_Idaho_CS83_Central,
			(int)pcstype_t.PCS_NAD83_Idaho_West, (int)projection_t.Proj_Idaho_CS83_West,

			(int)pcstype_t.PCS_NAD83_Illinois_East, (int)projection_t.Proj_Illinois_CS83_East,
			(int)pcstype_t.PCS_NAD83_Illinois_West, (int)projection_t.Proj_Illinois_CS83_West,

			(int)pcstype_t.PCS_NAD83_Indiana_East, (int)projection_t.Proj_Indiana_CS83_East,
			(int)pcstype_t.PCS_NAD83_Indiana_West, (int)projection_t.Proj_Indiana_CS83_West,

			(int)pcstype_t.PCS_NAD83_Iowa_North, (int)projection_t.Proj_Iowa_CS83_North,
			(int)pcstype_t.PCS_NAD83_Iowa_South, (int)projection_t.Proj_Iowa_CS83_South,

			(int)pcstype_t.PCS_NAD83_Kansas_North, (int)projection_t.Proj_Kansas_CS83_North,
			(int)pcstype_t.PCS_NAD83_Kansas_South, (int)projection_t.Proj_Kansas_CS83_South,

			(int)pcstype_t.PCS_NAD83_Kentucky_North, (int)projection_t.Proj_Kentucky_CS83_North,
			(int)pcstype_t.PCS_NAD83_Kentucky_South, (int)projection_t.Proj_Kentucky_CS83_South,

			(int)pcstype_t.PCS_NAD83_Louisiana_North, (int)projection_t.Proj_Louisiana_CS83_North,
			(int)pcstype_t.PCS_NAD83_Louisiana_South, (int)projection_t.Proj_Louisiana_CS83_South,

			(int)pcstype_t.PCS_NAD83_Maine_East, (int)projection_t.Proj_Maine_CS83_East,
			(int)pcstype_t.PCS_NAD83_Maine_West, (int)projection_t.Proj_Maine_CS83_West,

			(int)pcstype_t.PCS_NAD83_Maryland, (int)projection_t.Proj_Maryland_CS83,

			(int)pcstype_t.PCS_NAD83_Massachusetts, (int)projection_t.Proj_Massachusetts_CS83_Mainland,
			(int)pcstype_t.PCS_NAD83_Massachusetts_Is, (int)projection_t.Proj_Massachusetts_CS83_Island,

			(int)pcstype_t.PCS_NAD83_Michigan_North, (int)projection_t.Proj_Michigan_CS83_North,
			(int)pcstype_t.PCS_NAD83_Michigan_Central, (int)projection_t.Proj_Michigan_CS83_Central,
			(int)pcstype_t.PCS_NAD83_Michigan_South, (int)projection_t.Proj_Michigan_CS83_South,

			(int)pcstype_t.PCS_NAD83_Minnesota_North, (int)projection_t.Proj_Minnesota_CS83_North,
			(int)pcstype_t.PCS_NAD83_Minnesota_Cent, (int)projection_t.Proj_Minnesota_CS83_Central,
			(int)pcstype_t.PCS_NAD83_Minnesota_South, (int)projection_t.Proj_Minnesota_CS83_South,

			(int)pcstype_t.PCS_NAD83_Mississippi_East, (int)projection_t.Proj_Mississippi_CS83_East,
			(int)pcstype_t.PCS_NAD83_Mississippi_West, (int)projection_t.Proj_Mississippi_CS83_West,

			(int)pcstype_t.PCS_NAD83_Missouri_East, (int)projection_t.Proj_Missouri_CS83_East,
			(int)pcstype_t.PCS_NAD83_Missouri_Central, (int)projection_t.Proj_Missouri_CS83_Central,
			(int)pcstype_t.PCS_NAD83_Missouri_West, (int)projection_t.Proj_Missouri_CS83_West,

			(int)pcstype_t.PCS_NAD83_Montana, (int)projection_t.Proj_Montana_CS83,

			(int)pcstype_t.PCS_NAD83_Nebraska, (int)projection_t.Proj_Nebraska_CS83,

			(int)pcstype_t.PCS_NAD83_Nevada_East, (int)projection_t.Proj_Nevada_CS83_East,
			(int)pcstype_t.PCS_NAD83_Nevada_Central, (int)projection_t.Proj_Nevada_CS83_Central,
			(int)pcstype_t.PCS_NAD83_Nevada_West, (int)projection_t.Proj_Nevada_CS83_West,

			(int)pcstype_t.PCS_NAD83_New_Hampshire, (int)projection_t.Proj_New_Hampshire_CS83,

			(int)pcstype_t.PCS_NAD83_New_Jersey, (int)projection_t.Proj_New_Jersey_CS83,

			(int)pcstype_t.PCS_NAD83_New_Mexico_East, (int)projection_t.Proj_New_Mexico_CS83_East,
			(int)pcstype_t.PCS_NAD83_New_Mexico_Cent, (int)projection_t.Proj_New_Mexico_CS83_Central,
			(int)pcstype_t.PCS_NAD83_New_Mexico_West, (int)projection_t.Proj_New_Mexico_CS83_West,

			(int)pcstype_t.PCS_NAD83_New_York_East, (int)projection_t.Proj_New_York_CS83_East,
			(int)pcstype_t.PCS_NAD83_New_York_Central, (int)projection_t.Proj_New_York_CS83_Central,
			(int)pcstype_t.PCS_NAD83_New_York_West, (int)projection_t.Proj_New_York_CS83_West,
			(int)pcstype_t.PCS_NAD83_New_York_Long_Is, (int)projection_t.Proj_New_York_CS83_Long_Island,

			(int)pcstype_t.PCS_NAD83_North_Carolina, (int)projection_t.Proj_North_Carolina_CS83,

			(int)pcstype_t.PCS_NAD83_North_Dakota_N, (int)projection_t.Proj_North_Dakota_CS83_North,
			(int)pcstype_t.PCS_NAD83_North_Dakota_S, (int)projection_t.Proj_North_Dakota_CS83_South,

			(int)pcstype_t.PCS_NAD83_Ohio_North, (int)projection_t.Proj_Ohio_CS83_North,
			(int)pcstype_t.PCS_NAD83_Ohio_South, (int)projection_t.Proj_Ohio_CS83_South,

			(int)pcstype_t.PCS_NAD83_Oklahoma_North, (int)projection_t.Proj_Oklahoma_CS83_North,
			(int)pcstype_t.PCS_NAD83_Oklahoma_South, (int)projection_t.Proj_Oklahoma_CS83_South,

			(int)pcstype_t.PCS_NAD83_Oregon_North, (int)projection_t.Proj_Oregon_CS83_North,
			(int)pcstype_t.PCS_NAD83_Oregon_South, (int)projection_t.Proj_Oregon_CS83_South,

			(int)pcstype_t.PCS_NAD83_Pennsylvania_N, (int)projection_t.Proj_Pennsylvania_CS83_North,
			(int)pcstype_t.PCS_NAD83_Pennsylvania_S, (int)projection_t.Proj_Pennsylvania_CS83_South,

			(int)pcstype_t.PCS_NAD83_Rhode_Island, (int)projection_t.Proj_Rhode_Island_CS83,

			(int)pcstype_t.PCS_NAD83_South_Carolina, (int)projection_t.Proj_South_Carolina_CS83,

			(int)pcstype_t.PCS_NAD83_South_Dakota_N, (int)projection_t.Proj_South_Dakota_CS83_North,
			(int)pcstype_t.PCS_NAD83_South_Dakota_S, (int)projection_t.Proj_South_Dakota_CS83_South,

			(int)pcstype_t.PCS_NAD83_Tennessee, (int)projection_t.Proj_Tennessee_CS83,

			(int)pcstype_t.PCS_NAD83_Texas_North, (int)projection_t.Proj_Texas_CS83_North,
			(int)pcstype_t.PCS_NAD83_Texas_North_Cen, (int)projection_t.Proj_Texas_CS83_North_Central,
			(int)pcstype_t.PCS_NAD83_Texas_Central, (int)projection_t.Proj_Texas_CS83_Central,
			(int)pcstype_t.PCS_NAD83_Texas_South_Cen, (int)projection_t.Proj_Texas_CS83_South_Central,
			(int)pcstype_t.PCS_NAD83_Texas_South, (int)projection_t.Proj_Texas_CS83_South,

			(int)pcstype_t.PCS_NAD83_Utah_North, (int)projection_t.Proj_Utah_CS83_North,
			(int)pcstype_t.PCS_NAD83_Utah_Central, (int)projection_t.Proj_Utah_CS83_Central,
			(int)pcstype_t.PCS_NAD83_Utah_South, (int)projection_t.Proj_Utah_CS83_South,

			(int)pcstype_t.PCS_NAD83_Vermont, (int)projection_t.Proj_Vermont_CS83,

			(int)pcstype_t.PCS_NAD83_Virginia_North, (int)projection_t.Proj_Virginia_CS83_North,
			(int)pcstype_t.PCS_NAD83_Virginia_South, (int)projection_t.Proj_Virginia_CS83_South,

			(int)pcstype_t.PCS_NAD83_Washington_North, (int)projection_t.Proj_Washington_CS83_North,
			(int)pcstype_t.PCS_NAD83_Washington_South, (int)projection_t.Proj_Washington_CS83_South,

			(int)pcstype_t.PCS_NAD83_West_Virginia_N, (int)projection_t.Proj_West_Virginia_CS83_North,
			(int)pcstype_t.PCS_NAD83_West_Virginia_S, (int)projection_t.Proj_West_Virginia_CS83_South,

			(int)pcstype_t.PCS_NAD83_Wisconsin_North, (int)projection_t.Proj_Wisconsin_CS83_North,
			(int)pcstype_t.PCS_NAD83_Wisconsin_Cen, (int)projection_t.Proj_Wisconsin_CS83_Central,
			(int)pcstype_t.PCS_NAD83_Wisconsin_South, (int)projection_t.Proj_Wisconsin_CS83_South,

			(int)pcstype_t.PCS_NAD83_Wyoming_East, (int)projection_t.Proj_Wyoming_CS83_East,
			(int)pcstype_t.PCS_NAD83_Wyoming_E_Cen, (int)projection_t.Proj_Wyoming_CS83_East_Central,
			(int)pcstype_t.PCS_NAD83_Wyoming_W_Cen, (int)projection_t.Proj_Wyoming_CS83_West_Central,
			(int)pcstype_t.PCS_NAD83_Wyoming_West, (int)projection_t.Proj_Wyoming_CS83_West,

			(int)pcstype_t.PCS_NAD83_Puerto_Rico_Virgin_Is, (int)projection_t.Proj_Puerto_Rico_Virgin_Is,

			(int)pcstype_t.PCS_NAD27_Alabama_East, (int)projection_t.Proj_Alabama_CS27_East,
			(int)pcstype_t.PCS_NAD27_Alabama_West, (int)projection_t.Proj_Alabama_CS27_West,

			(int)pcstype_t.PCS_NAD27_Alaska_zone_1, (int)projection_t.Proj_Alaska_CS27_1,
			(int)pcstype_t.PCS_NAD27_Alaska_zone_2, (int)projection_t.Proj_Alaska_CS27_2,
			(int)pcstype_t.PCS_NAD27_Alaska_zone_3, (int)projection_t.Proj_Alaska_CS27_3,
			(int)pcstype_t.PCS_NAD27_Alaska_zone_4, (int)projection_t.Proj_Alaska_CS27_4,
			(int)pcstype_t.PCS_NAD27_Alaska_zone_5, (int)projection_t.Proj_Alaska_CS27_5,
			(int)pcstype_t.PCS_NAD27_Alaska_zone_6, (int)projection_t.Proj_Alaska_CS27_6,
			(int)pcstype_t.PCS_NAD27_Alaska_zone_7, (int)projection_t.Proj_Alaska_CS27_7,
			(int)pcstype_t.PCS_NAD27_Alaska_zone_8, (int)projection_t.Proj_Alaska_CS27_8,
			(int)pcstype_t.PCS_NAD27_Alaska_zone_9, (int)projection_t.Proj_Alaska_CS27_9,
			(int)pcstype_t.PCS_NAD27_Alaska_zone_10, (int)projection_t.Proj_Alaska_CS27_10,

			(int)pcstype_t.PCS_NAD27_California_I, (int)projection_t.Proj_California_CS27_I,
			(int)pcstype_t.PCS_NAD27_California_II, (int)projection_t.Proj_California_CS27_II,
			(int)pcstype_t.PCS_NAD27_California_III, (int)projection_t.Proj_California_CS27_III,
			(int)pcstype_t.PCS_NAD27_California_IV, (int)projection_t.Proj_California_CS27_IV,
			(int)pcstype_t.PCS_NAD27_California_V, (int)projection_t.Proj_California_CS27_V,
			(int)pcstype_t.PCS_NAD27_California_VI, (int)projection_t.Proj_California_CS27_VI,
			(int)pcstype_t.PCS_NAD27_California_VII, (int)projection_t.Proj_California_CS27_VII,

			(int)pcstype_t.PCS_NAD27_Arizona_East, (int)projection_t.Proj_Arizona_Coordinate_System_east,
			(int)pcstype_t.PCS_NAD27_Arizona_Central, (int)projection_t.Proj_Arizona_Coordinate_System_Central,
			(int)pcstype_t.PCS_NAD27_Arizona_West, (int)projection_t.Proj_Arizona_Coordinate_System_west,

			(int)pcstype_t.PCS_NAD27_Arkansas_North, (int)projection_t.Proj_Arkansas_CS27_North,
			(int)pcstype_t.PCS_NAD27_Arkansas_South, (int)projection_t.Proj_Arkansas_CS27_South,

			(int)pcstype_t.PCS_NAD27_Colorado_North, (int)projection_t.Proj_Colorado_CS27_North,
			(int)pcstype_t.PCS_NAD27_Colorado_Central, (int)projection_t.Proj_Colorado_CS27_Central,
			(int)pcstype_t.PCS_NAD27_Colorado_South, (int)projection_t.Proj_Colorado_CS27_South,

			(int)pcstype_t.PCS_NAD27_Connecticut, (int)projection_t.Proj_Connecticut_CS27,

			(int)pcstype_t.PCS_NAD27_Delaware, 	(int)projection_t.Proj_Delaware_CS27,

			(int)pcstype_t.PCS_NAD27_Florida_East, (int)projection_t.Proj_Florida_CS27_East,
			(int)pcstype_t.PCS_NAD27_Florida_North, (int)projection_t.Proj_Florida_CS27_North,
			(int)pcstype_t.PCS_NAD27_Florida_West, (int)projection_t.Proj_Florida_CS27_West,

			(int)pcstype_t.PCS_NAD27_Hawaii_zone_1, (int)projection_t.Proj_Hawaii_CS27_1,
			(int)pcstype_t.PCS_NAD27_Hawaii_zone_2, (int)projection_t.Proj_Hawaii_CS27_2,
			(int)pcstype_t.PCS_NAD27_Hawaii_zone_3, (int)projection_t.Proj_Hawaii_CS27_3,
			(int)pcstype_t.PCS_NAD27_Hawaii_zone_4, (int)projection_t.Proj_Hawaii_CS27_4,
			(int)pcstype_t.PCS_NAD27_Hawaii_zone_5, (int)projection_t.Proj_Hawaii_CS27_5,

			(int)pcstype_t.PCS_NAD27_Georgia_East, (int)projection_t.Proj_Georgia_CS27_East,
			(int)pcstype_t.PCS_NAD27_Georgia_West, (int)projection_t.Proj_Georgia_CS27_West,

			(int)pcstype_t.PCS_NAD27_Idaho_East, (int)projection_t.Proj_Idaho_CS27_East,
			(int)pcstype_t.PCS_NAD27_Idaho_Central, (int)projection_t.Proj_Idaho_CS27_Central,
			(int)pcstype_t.PCS_NAD27_Idaho_West, (int)projection_t.Proj_Idaho_CS27_West,

			(int)pcstype_t.PCS_NAD27_Illinois_East, (int)projection_t.Proj_Illinois_CS27_East,
			(int)pcstype_t.PCS_NAD27_Illinois_West, (int)projection_t.Proj_Illinois_CS27_West,

			(int)pcstype_t.PCS_NAD27_Indiana_East, (int)projection_t.Proj_Indiana_CS27_East,
			(int)pcstype_t.PCS_NAD27_Indiana_West, (int)projection_t.Proj_Indiana_CS27_West,

			(int)pcstype_t.PCS_NAD27_Iowa_North, (int)projection_t.Proj_Iowa_CS27_North,
			(int)pcstype_t.PCS_NAD27_Iowa_South, (int)projection_t.Proj_Iowa_CS27_South,

			(int)pcstype_t.PCS_NAD27_Kansas_North, (int)projection_t.Proj_Kansas_CS27_North,
			(int)pcstype_t.PCS_NAD27_Kansas_South, (int)projection_t.Proj_Kansas_CS27_South,

			(int)pcstype_t.PCS_NAD27_Kentucky_North, (int)projection_t.Proj_Kentucky_CS27_North,
			(int)pcstype_t.PCS_NAD27_Kentucky_South, (int)projection_t.Proj_Kentucky_CS27_South,

			(int)pcstype_t.PCS_NAD27_Louisiana_North, (int)projection_t.Proj_Louisiana_CS27_North,
			(int)pcstype_t.PCS_NAD27_Louisiana_South, (int)projection_t.Proj_Louisiana_CS27_South,

			(int)pcstype_t.PCS_NAD27_Maine_East, (int)projection_t.Proj_Maine_CS27_East,
			(int)pcstype_t.PCS_NAD27_Maine_West, (int)projection_t.Proj_Maine_CS27_West,

			(int)pcstype_t.PCS_NAD27_Maryland, (int)projection_t.Proj_Maryland_CS27,

			(int)pcstype_t.PCS_NAD27_Massachusetts, (int)projection_t.Proj_Massachusetts_CS27_Mainland,
			(int)pcstype_t.PCS_NAD27_Massachusetts_Is, (int)projection_t.Proj_Massachusetts_CS27_Island,

			(int)pcstype_t.PCS_NAD27_Michigan_North, (int)projection_t.Proj_Michigan_CS27_North,
			(int)pcstype_t.PCS_NAD27_Michigan_Central, (int)projection_t.Proj_Michigan_CS27_Central,
			(int)pcstype_t.PCS_NAD27_Michigan_South, (int)projection_t.Proj_Michigan_CS27_South,

			(int)pcstype_t.PCS_NAD27_Minnesota_North, (int)projection_t.Proj_Minnesota_CS27_North,
			(int)pcstype_t.PCS_NAD27_Minnesota_Cent, (int)projection_t.Proj_Minnesota_CS27_Central,
			(int)pcstype_t.PCS_NAD27_Minnesota_South, (int)projection_t.Proj_Minnesota_CS27_South,

			(int)pcstype_t.PCS_NAD27_Mississippi_East, (int)projection_t.Proj_Mississippi_CS27_East,
			(int)pcstype_t.PCS_NAD27_Mississippi_West, (int)projection_t.Proj_Mississippi_CS27_West,

			(int)pcstype_t.PCS_NAD27_Missouri_East, (int)projection_t.Proj_Missouri_CS27_East,
			(int)pcstype_t.PCS_NAD27_Missouri_Central, (int)projection_t.Proj_Missouri_CS27_Central,
			(int)pcstype_t.PCS_NAD27_Missouri_West, (int)projection_t.Proj_Missouri_CS27_West,

			(int)pcstype_t.PCS_NAD27_Montana_North, (int)projection_t.Proj_Montana_CS27_North,
			(int)pcstype_t.PCS_NAD27_Montana_Central, (int)projection_t.Proj_Montana_CS27_Central,
			(int)pcstype_t.PCS_NAD27_Montana_South, (int)projection_t.Proj_Montana_CS27_South,

			(int)pcstype_t.PCS_NAD27_Nebraska_North, (int)projection_t.Proj_Nebraska_CS27_North,
			(int)pcstype_t.PCS_NAD27_Nebraska_South, (int)projection_t.Proj_Nebraska_CS27_South,

			(int)pcstype_t.PCS_NAD27_Nevada_East, (int)projection_t.Proj_Nevada_CS27_East,
			(int)pcstype_t.PCS_NAD27_Nevada_Central, (int)projection_t.Proj_Nevada_CS27_Central,
			(int)pcstype_t.PCS_NAD27_Nevada_West, (int)projection_t.Proj_Nevada_CS27_West,

			(int)pcstype_t.PCS_NAD27_New_Hampshire, (int)projection_t.Proj_New_Hampshire_CS27,

			(int)pcstype_t.PCS_NAD27_New_Jersey, (int)projection_t.Proj_New_Jersey_CS27,

			(int)pcstype_t.PCS_NAD27_New_Mexico_East, (int)projection_t.Proj_New_Mexico_CS27_East,
			(int)pcstype_t.PCS_NAD27_New_Mexico_Cent, (int)projection_t.Proj_New_Mexico_CS27_Central,
			(int)pcstype_t.PCS_NAD27_New_Mexico_West, (int)projection_t.Proj_New_Mexico_CS27_West,

			(int)pcstype_t.PCS_NAD27_New_York_East, (int)projection_t.Proj_New_York_CS27_East,
			(int)pcstype_t.PCS_NAD27_New_York_Central, (int)projection_t.Proj_New_York_CS27_Central,
			(int)pcstype_t.PCS_NAD27_New_York_West, (int)projection_t.Proj_New_York_CS27_West,
			(int)pcstype_t.PCS_NAD27_New_York_Long_Is, (int)projection_t.Proj_New_York_CS27_Long_Island,

			(int)pcstype_t.PCS_NAD27_North_Carolina, (int)projection_t.Proj_North_Carolina_CS27,

			(int)pcstype_t.PCS_NAD27_North_Dakota_N, (int)projection_t.Proj_North_Dakota_CS27_North,
			(int)pcstype_t.PCS_NAD27_North_Dakota_S, (int)projection_t.Proj_North_Dakota_CS27_South,

			(int)pcstype_t.PCS_NAD27_Ohio_North, (int)projection_t.Proj_Ohio_CS27_North,
			(int)pcstype_t.PCS_NAD27_Ohio_South, (int)projection_t.Proj_Ohio_CS27_South,

			(int)pcstype_t.PCS_NAD27_Oklahoma_North, (int)projection_t.Proj_Oklahoma_CS27_North,
			(int)pcstype_t.PCS_NAD27_Oklahoma_South, (int)projection_t.Proj_Oklahoma_CS27_South,

			(int)pcstype_t.PCS_NAD27_Oregon_North, (int)projection_t.Proj_Oregon_CS27_North,
			(int)pcstype_t.PCS_NAD27_Oregon_South, (int)projection_t.Proj_Oregon_CS27_South,

			(int)pcstype_t.PCS_NAD27_Pennsylvania_N, (int)projection_t.Proj_Pennsylvania_CS27_North,
			(int)pcstype_t.PCS_NAD27_Pennsylvania_S, (int)projection_t.Proj_Pennsylvania_CS27_South,

			(int)pcstype_t.PCS_NAD27_Rhode_Island, (int)projection_t.Proj_Rhode_Island_CS27,

			(int)pcstype_t.PCS_NAD27_South_Carolina_N, (int)projection_t.Proj_South_Carolina_CS27_North,
			(int)pcstype_t.PCS_NAD27_South_Carolina_S, (int)projection_t.Proj_South_Carolina_CS27_South,

			(int)pcstype_t.PCS_NAD27_South_Dakota_N, (int)projection_t.Proj_South_Dakota_CS27_North,
			(int)pcstype_t.PCS_NAD27_South_Dakota_S, (int)projection_t.Proj_South_Dakota_CS27_South,

			(int)pcstype_t.PCS_NAD27_Tennessee, (int)projection_t.Proj_Tennessee_CS27,

			(int)pcstype_t.PCS_NAD27_Texas_North, (int)projection_t.Proj_Texas_CS27_North,
			(int)pcstype_t.PCS_NAD27_Texas_North_Cen, (int)projection_t.Proj_Texas_CS27_North_Central,
			(int)pcstype_t.PCS_NAD27_Texas_Central, (int)projection_t.Proj_Texas_CS27_Central,
			(int)pcstype_t.PCS_NAD27_Texas_South_Cen, (int)projection_t.Proj_Texas_CS27_South_Central,
			(int)pcstype_t.PCS_NAD27_Texas_South, (int)projection_t.Proj_Texas_CS27_South,

			(int)pcstype_t.PCS_NAD27_Utah_North, (int)projection_t.Proj_Utah_CS27_North,
			(int)pcstype_t.PCS_NAD27_Utah_Central, (int)projection_t.Proj_Utah_CS27_Central,
			(int)pcstype_t.PCS_NAD27_Utah_South, (int)projection_t.Proj_Utah_CS27_South,

			(int)pcstype_t.PCS_NAD27_Vermont, (int)projection_t.Proj_Vermont_CS27,

			(int)pcstype_t.PCS_NAD27_Virginia_North, (int)projection_t.Proj_Virginia_CS27_North,
			(int)pcstype_t.PCS_NAD27_Virginia_South, (int)projection_t.Proj_Virginia_CS27_South,

			(int)pcstype_t.PCS_NAD27_Washington_North, (int)projection_t.Proj_Washington_CS27_North,
			(int)pcstype_t.PCS_NAD27_Washington_South, (int)projection_t.Proj_Washington_CS27_South,

			(int)pcstype_t.PCS_NAD27_West_Virginia_N, (int)projection_t.Proj_West_Virginia_CS27_North,
			(int)pcstype_t.PCS_NAD27_West_Virginia_S, (int)projection_t.Proj_West_Virginia_CS27_South,

			(int)pcstype_t.PCS_NAD27_Wisconsin_North, (int)projection_t.Proj_Wisconsin_CS27_North,
			(int)pcstype_t.PCS_NAD27_Wisconsin_Cen, (int)projection_t.Proj_Wisconsin_CS27_Central,
			(int)pcstype_t.PCS_NAD27_Wisconsin_South, (int)projection_t.Proj_Wisconsin_CS27_South,

			(int)pcstype_t.PCS_NAD27_Wyoming_East, (int)projection_t.Proj_Wyoming_CS27_East,
			(int)pcstype_t.PCS_NAD27_Wyoming_E_Cen, (int)projection_t.Proj_Wyoming_CS27_East_Central,
			(int)pcstype_t.PCS_NAD27_Wyoming_W_Cen, (int)projection_t.Proj_Wyoming_CS27_West_Central,
			(int)pcstype_t.PCS_NAD27_Wyoming_West, (int)projection_t.Proj_Wyoming_CS27_West,

			(int)pcstype_t.PCS_NAD27_Puerto_Rico, (int)projection_t.Proj_Puerto_Rico_CS27,

			KvUserDefined
		};
		#endregion

		//**********************************************************************
		//							GTIFMapSysToPCS()
		//
		//		Given a Datum, MapSys and zone value generate the best PCS
		//		code possible.
		//**********************************************************************
		public static int GTIFMapSysToPCS(int MapSys, geographic_t Datum, int nZone)
		{
			int PCSCode=KvUserDefined;

			if(MapSys==MapSys_UTM_North)
			{
				if(Datum==geographic_t.GCS_NAD27) PCSCode=(int)pcstype_t.PCS_NAD27_UTM_zone_3N+nZone-3;
				else if(Datum==geographic_t.GCS_NAD83) PCSCode=(int)pcstype_t.PCS_NAD83_UTM_zone_3N+nZone-3;
				else if(Datum==geographic_t.GCS_WGS_72) PCSCode=(int)pcstype_t.PCS_WGS72_UTM_zone_1N+nZone-1;
				else if(Datum==geographic_t.GCS_WGS_72BE) PCSCode=(int)pcstype_t.PCS_WGS72BE_UTM_zone_1N+nZone-1;
				else if(Datum==geographic_t.GCS_WGS_84) PCSCode=(int)pcstype_t.PCS_WGS84_UTM_zone_1N+nZone-1;
			}
			else if(MapSys==MapSys_UTM_South)
			{
				if(Datum==geographic_t.GCS_WGS_72) PCSCode=(int)pcstype_t.PCS_WGS72_UTM_zone_1S+nZone-1;
				else if(Datum==geographic_t.GCS_WGS_72BE) PCSCode=(int)pcstype_t.PCS_WGS72BE_UTM_zone_1S+nZone-1;
				else if(Datum==geographic_t.GCS_WGS_84) PCSCode=(int)pcstype_t.PCS_WGS84_UTM_zone_1S+nZone-1;
			}
			else if(MapSys==MapSys_State_Plane_27)
			{
				PCSCode=10000+nZone;
				for(int i=0; StatePlaneTable[i]!=KvUserDefined; i+=2)
				{
					if(StatePlaneTable[i+1]==PCSCode) PCSCode=StatePlaneTable[i];
				}

				// Old EPSG code was in error for Tennesse CS27, override
				if(nZone==4100) PCSCode=2204;
			}
			else if(MapSys==MapSys_State_Plane_83)
			{
				PCSCode=10000+nZone+30;

				for(int i=0; StatePlaneTable[i]!=KvUserDefined; i+=2)
				{
					if(StatePlaneTable[i+1]==PCSCode) PCSCode=StatePlaneTable[i];
				}

				// Old EPSG code was in error for Kentucky North CS83, override
				if(nZone==1601) PCSCode=2205;
			}

			return PCSCode;
		}

		//**********************************************************************
		//							GTIFMapSysToProj()
		//
		//		Given a MapSys and zone value generate the best projection_t
		//		code possible.
		//**********************************************************************
		public static projection_t GTIFMapSysToProj(int MapSys, int nZone)
		{
			projection_t ProjCode=(projection_t)KvUserDefined;

			if(MapSys==MapSys_UTM_North) ProjCode=projection_t.Proj_UTM_zone_1N+nZone-1;
			else if(MapSys==MapSys_UTM_South) ProjCode=projection_t.Proj_UTM_zone_1S+nZone-1;
			else if(MapSys==MapSys_State_Plane_27)
			{
				ProjCode=(projection_t)(10000+nZone);

				// Tennesse override
				if(nZone==4100) ProjCode=projection_t.Proj_Tennessee_CS27;
			}
			else if(MapSys==MapSys_State_Plane_83)
			{
				ProjCode=(projection_t)(10000+nZone+30);

				// Kentucky North override
				if(nZone==1601) ProjCode=projection_t.Proj_Kentucky_CS83_North;
			}

			return ProjCode;
		}

		//**********************************************************************
		//							GTIFPCSToMapSys()
		//**********************************************************************
		//
		// Translate a PCS_ code into a UTM or State Plane map system, a datum,
		// and a zone if possible.
		//
		// @param PCSCode The projection code (PCS_*) as would be stored in the
		// ProjectedCSTypeGeoKey of a GeoTIFF file.
		//
		// @param pDatum Pointer to an integer into which the datum code (GCS_*)
		// is put if the function succeeds.
		//
		// @param pZone Pointer to an integer into which the zone will be placed
		// if the function is successful.
		//
		// @return Returns either MapSys_UTM_North, MapSys_UTM_South,
		// MapSys_State_Plane_83, MapSys_State_Plane_27 or KvUserDefined.
		// KvUserDefined indicates that the
		// function failed to recognise the projection as UTM or State Plane.
		//
		// The zone value is only set if the return code is other than KvUserDefined.
		// For utm map system the returned zone will be between 1 and 60. For
		// State Plane, the USGS state plane zone number is returned. For instance,
		// Alabama East is zone 101.
		//
		// The datum (really this is the GCS) is set to a GCS_ value such as GCS_NAD27.
		//
		// This function is useful to recognise (most) UTM and State Plane coordinate
		// systems, even if CSV files aren't available to translate them automatically.
		// It is used as a fallback mechanism by GTIFGetDefn() for normalization when
		// CSV files aren't found.

		public static int GTIFPCSToMapSys(pcstype_t PCSCode, out geographic_t pDatum, out int pZone)
		{
			geographic_t Datum=(geographic_t)KvUserDefined;
			int Proj=KvUserDefined, nZone=KvUserDefined;

			// --------------------------------------------------------------------
			//		UTM with various datums. Note there are lots of PCS UTM
			//		codes not done yet which use strange datums.
			// --------------------------------------------------------------------
			if(PCSCode>=pcstype_t.PCS_NAD27_UTM_zone_3N&&PCSCode<=pcstype_t.PCS_NAD27_UTM_zone_22N)
			{
				Datum=geographic_t.GCS_NAD27;
				Proj=MapSys_UTM_North;
				nZone=PCSCode-pcstype_t.PCS_NAD27_UTM_zone_3N+3;
			}
			else if(PCSCode>=pcstype_t.PCS_NAD83_UTM_zone_3N&&PCSCode<=pcstype_t.PCS_NAD83_UTM_zone_23N)
			{
				Datum=geographic_t.GCS_NAD83;
				Proj=MapSys_UTM_North;
				nZone=PCSCode-pcstype_t.PCS_NAD83_UTM_zone_3N+3;
			}

			else if(PCSCode>=pcstype_t.PCS_WGS72_UTM_zone_1N&&PCSCode<=pcstype_t.PCS_WGS72_UTM_zone_60N)
			{
				Datum=geographic_t.GCS_WGS_72;
				Proj=MapSys_UTM_North;
				nZone=PCSCode-pcstype_t.PCS_WGS72_UTM_zone_1N+1;
			}
			else if(PCSCode>=pcstype_t.PCS_WGS72_UTM_zone_1S&&PCSCode<=pcstype_t.PCS_WGS72_UTM_zone_60S)
			{
				Datum=geographic_t.GCS_WGS_72;
				Proj=MapSys_UTM_South;
				nZone=PCSCode-pcstype_t.PCS_WGS72_UTM_zone_1S+1;
			}

			else if(PCSCode>=pcstype_t.PCS_WGS72BE_UTM_zone_1N&&PCSCode<=pcstype_t.PCS_WGS72BE_UTM_zone_60N)
			{
				Datum=geographic_t.GCS_WGS_72BE;
				Proj=MapSys_UTM_North;
				nZone=PCSCode-pcstype_t.PCS_WGS72BE_UTM_zone_1N+1;
			}
			else if(PCSCode>=pcstype_t.PCS_WGS72BE_UTM_zone_1S&&PCSCode<=pcstype_t.PCS_WGS72BE_UTM_zone_60S)
			{
				Datum=geographic_t.GCS_WGS_72BE;
				Proj=MapSys_UTM_South;
				nZone=PCSCode-pcstype_t.PCS_WGS72BE_UTM_zone_1S+1;
			}

			else if(PCSCode>=pcstype_t.PCS_WGS84_UTM_zone_1N&&PCSCode<=pcstype_t.PCS_WGS84_UTM_zone_60N)
			{
				Datum=geographic_t.GCS_WGS_84;
				Proj=MapSys_UTM_North;
				nZone=PCSCode-pcstype_t.PCS_WGS84_UTM_zone_1N+1;
			}
			else if(PCSCode>=pcstype_t.PCS_WGS84_UTM_zone_1S&&PCSCode<=pcstype_t.PCS_WGS84_UTM_zone_60S)
			{
				Datum=geographic_t.GCS_WGS_84;
				Proj=MapSys_UTM_South;
				nZone=PCSCode-pcstype_t.PCS_WGS84_UTM_zone_1S+1;
			}
			else if(PCSCode>=pcstype_t.PCS_SAD69_UTM_zone_18N&&PCSCode<=pcstype_t.PCS_SAD69_UTM_zone_22N)
			{
				Datum=(geographic_t)KvUserDefined;
				Proj=MapSys_UTM_North;
				nZone=PCSCode-pcstype_t.PCS_SAD69_UTM_zone_18N+18;
			}
			else if(PCSCode>=pcstype_t.PCS_SAD69_UTM_zone_17S&&PCSCode<=pcstype_t.PCS_SAD69_UTM_zone_25S)
			{
				Datum=(geographic_t)KvUserDefined;
				Proj=MapSys_UTM_South;
				nZone=PCSCode-pcstype_t.PCS_SAD69_UTM_zone_17S+17;
			}

			// --------------------------------------------------------------------
			//		State Plane zones, first we translate any PCS_ codes to
			//		a Proj_ code that we can get a handle on.
			// --------------------------------------------------------------------
			for(int i=0; StatePlaneTable[i]!=KvUserDefined; i+=2)
			{
				if(StatePlaneTable[i]==(int)PCSCode) PCSCode=(pcstype_t)StatePlaneTable[i+1];
			}

			if(PCSCode<=(pcstype_t)15900&&PCSCode>=(pcstype_t)10000)
			{
				if(((int)PCSCode%100)>=30)
				{
					Proj=MapSys_State_Plane_83;
					Datum=geographic_t.GCS_NAD83;
				}
				else
				{
					Proj=MapSys_State_Plane_27;
					Datum=geographic_t.GCS_NAD27;
				}

				nZone=PCSCode-(pcstype_t)10000;
				if(Datum==geographic_t.GCS_NAD83) nZone-=30;
			}

			pDatum=Datum;
			pZone=nZone;

			return Proj;
		}

		//**********************************************************************
		//							GTIFProjToMapSys()
		//**********************************************************************

		// Translate a Proj_ code into a UTM or State Plane map system, and a zone
		// if possible.
		//
		// @param ProjCode The projection code (Proj_*) as would be stored in the
		// ProjectionGeoKey of a GeoTIFF file.
		// @param pZone Pointer to an integer into which the zone will be placed
		// if the function is successful.
		//
		// @return Returns either MapSys_UTM_North, MapSys_UTM_South,
		// MapSys_State_Plane_27, MapSys_State_Plane_83 or KvUserDefined.
		// KvUserDefined indicates that the
		// function failed to recognise the projection as UTM or State Plane.
		//
		// The zone value is only set if the return code is other than KvUserDefined.
		// For utm map system the returned zone will be between 1 and 60. For
		// State Plane, the USGS state plane zone number is returned. For instance,
		// Alabama East is zone 101.
		//
		// This function is useful to recognise UTM and State Plane coordinate
		// systems, and to extract zone numbers so the projections can be
		// represented as UTM rather than as the underlying projection method such
		// Transverse Mercator for instance.
		public static int GTIFProjToMapSys(projection_t ProjCode, out int pZone)
		{
			int nZone=KvUserDefined;
			int MapSys=KvUserDefined;

			// --------------------------------------------------------------------
			//		Handle UTM.
			// --------------------------------------------------------------------
			if(ProjCode>=projection_t.Proj_UTM_zone_1N&&ProjCode<=projection_t.Proj_UTM_zone_60N)
			{
				MapSys=MapSys_UTM_North;
				nZone=ProjCode-projection_t.Proj_UTM_zone_1N+1;
			}
			else if(ProjCode>=projection_t.Proj_UTM_zone_1S&&ProjCode<=projection_t.Proj_UTM_zone_60S)
			{
				MapSys=MapSys_UTM_South;
				nZone=ProjCode-projection_t.Proj_UTM_zone_1S+1;
			}

			// --------------------------------------------------------------------
			//		Handle State Plane. I think there are some anomolies in
			//		here, so this is a bit risky.
			// --------------------------------------------------------------------
			else if(ProjCode>=(projection_t)10101&&ProjCode<=(projection_t)15299)
			{
				if((int)ProjCode%100>=30)
				{
					MapSys=MapSys_State_Plane_83;
					nZone=ProjCode-(projection_t)10000-30;
				}
				else
				{
					MapSys=MapSys_State_Plane_27;
					nZone=ProjCode-(projection_t)10000;
				}
			}

			pZone=nZone;

			return MapSys;
		}
	}
}
