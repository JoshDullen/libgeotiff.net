//*****************************************************************************
//
// Project:	libgeotiff
// Purpose:	Code to abstract translation between pixel/line and PCS
//			coordinates.
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
using System.Text;

using Free.Ports.LibTiff;

namespace Free.Ports.LibGeoTiff
{
	public static partial class libgeotiff
	{
		//***********************************************************************/
		//*							inv_geotransform()							*/
		//*																		*/
		//*				Invert a 6 term geotransform style matrix.				*/
		//***********************************************************************/
		static bool inv_geotransform(double[] gt_in, double[] gt_out)
		{
			// we assume a 3rd row that is [0 0 1]

			// Compute determinate
			double det=gt_in[0]*gt_in[4]-gt_in[1]*gt_in[3];
			if(Math.Abs(det)<0.000000000000001) return false;

			double inv_det=1.0/det;

			// compute adjoint, and devide by determinate
			gt_out[0]=gt_in[4]*inv_det;
			gt_out[3]=-gt_in[3]*inv_det;

			gt_out[1]=-gt_in[1]*inv_det;
			gt_out[4]=gt_in[0]*inv_det;

			gt_out[2]=(gt_in[1]*gt_in[5]-gt_in[2]*gt_in[4])*inv_det;
			gt_out[5]=(-gt_in[0]*gt_in[5]+gt_in[2]*gt_in[3])*inv_det;

			return true;
		}

		//***********************************************************************/
		//*							GTIFTiepointTranslate()						*/
		//***********************************************************************/
		static bool GTIFTiepointTranslate(int gcp_count, double[] gcps_in, int in_offset, double[] gcps_out, int out_offset, ref double x, ref double y)
		{
			// I would appreciate a _brief_ block of code for doing second order polynomial regression here!
			return false;
		}

		//***********************************************************************/
		//*								GTIFImageToPCS()						*/
		//***********************************************************************/

		// Translate a pixel/line coordinate to projection coordinates.
		//
		// At this time this function does not support image to PCS translations for
		// tiepoints-only definitions, only pixelscale and transformation matrix
		// formulations.
		//
		// @param gtif The handle from GTIFNew() indicating the target file.
		// @param x A reference to the double containing the pixel offset on input,
		// and into which the easting/longitude will be put on completion.
		// @param y A reference to the double containing the line offset on input,
		// and into which the northing/latitude will be put on completion.
		//
		// @return true if the transformation succeeds, or false if it fails. It may
		// fail if the file doesn't have properly setup transformation information,
		// or it is in a form unsupported by this function.
		public static bool GTIFImageToPCS(GTIF gtif, ref double x, ref double y)
		{
			bool res=false;
			int tiepoint_count, count, transform_count;
			TIFF tif=gtif.gt_tif;
			double[] tiepoints=null;
			double[] pixel_scale=null;
			double[] transform=null;

			// --------------------------------------------------------------------
			//		Fetch tiepoints and pixel scale.
			// --------------------------------------------------------------------
			object ap;
			if(!gtif.gt_methods.get(tif, (ushort)GTIFF_TIEPOINTS, out tiepoint_count, out ap)) tiepoint_count=0;
			else if(ap is double[]) tiepoints=(double[])ap;

			if(!gtif.gt_methods.get(tif, (ushort)GTIFF_PIXELSCALE, out count, out ap)) count=0;
			else if(ap is double[]) pixel_scale=(double[])ap;

			if(!gtif.gt_methods.get(tif, (ushort)GTIFF_TRANSMATRIX, out transform_count, out ap)) transform_count=0;
			else if(ap is double[]) transform=(double[])ap;

			// --------------------------------------------------------------------
			//		If we have a transformation matrix, use it.
			// --------------------------------------------------------------------
			if(transform_count==16)
			{
				double x_in=x, y_in=y;

				x=x_in*transform[0]+y_in*transform[1]+transform[3];
				y=x_in*transform[4]+y_in*transform[5]+transform[7];

				res=true;
			}
			// --------------------------------------------------------------------
			//		If the pixelscale count is zero, but we have tiepoints use
			//		the tiepoint based approach.
			// --------------------------------------------------------------------
			else if(tiepoint_count>6&&count==0)
			{
				res=GTIFTiepointTranslate(tiepoint_count/6, tiepoints, 0, tiepoints, 3, ref x, ref y);
			}
			// --------------------------------------------------------------------
			//		For now we require one tie point, and a valid pixel scale.
			// --------------------------------------------------------------------
			else if(count>=3&&tiepoint_count>=6)
			{
				x=(x-tiepoints[0])*pixel_scale[0]+tiepoints[3];
				y=(y-tiepoints[1])*(-1*pixel_scale[1])+tiepoints[4];

				res=true;
			}

			return res;
		}

		//***********************************************************************/
		//*							GTIFPCSToImage()							*/
		//***********************************************************************/

		// Translate a projection coordinate to pixel/line coordinates.
		//
		// At this time this function does not support PCS to image translations for
		// tiepoints-only based definitions, only matrix and pixelscale/tiepoints
		// formulations are supposed.
		//
		// @param gtif The handle from GTIFNew() indicating the target file.
		// @param x A reference to the double containing the pixel offset on input,
		// and into which the easting/longitude will be put on completion.
		// @param y A reference to the double containing the line offset on input,
		// and into which the northing/latitude will be put on completion.
		//
		// @return true if the transformation succeeds, or false if it fails. It may
		// fail if the file doesn't have properly setup transformation information,
		// or it is in a form unsupported by this function.
		public static bool GTIFPCSToImage(GTIF gtif, ref double x, ref double y)
		{
			bool result=false;
			int tiepoint_count, count, transform_count;
			TIFF tif=gtif.gt_tif;
			double[] tiepoints=null;
			double[] pixel_scale=null;
			double[] transform=null;

			// --------------------------------------------------------------------
			//		Fetch tiepoints and pixel scale.
			// --------------------------------------------------------------------
			object ap;
			if(!gtif.gt_methods.get(tif, (ushort)GTIFF_TIEPOINTS, out tiepoint_count, out ap)) tiepoint_count=0;
			else if(ap is double[]) tiepoints=(double[])ap;

			if(!gtif.gt_methods.get(tif, (ushort)GTIFF_PIXELSCALE, out count, out ap)) count=0;
			else if(ap is double[]) pixel_scale=(double[])ap;

			if(!gtif.gt_methods.get(tif, (ushort)GTIFF_TRANSMATRIX, out transform_count, out ap)) transform_count=0;
			else if(ap is double[]) transform=(double[])ap;

			// --------------------------------------------------------------------
			//		Handle matrix - convert to "geotransform" format, invert and
			//		apply.
			// --------------------------------------------------------------------
			if(transform_count==16)
			{
				double x_in=x, y_in=y;
				double[] gt_in=new double[6];
				double[] gt_out=new double[6];

				gt_in[0]=transform[0];
				gt_in[1]=transform[1];
				gt_in[2]=transform[3];
				gt_in[3]=transform[4];
				gt_in[4]=transform[5];
				gt_in[5]=transform[7];

				if(!inv_geotransform(gt_in, gt_out)) result=false;
				else
				{
					x=x_in*gt_out[0]+y_in*gt_out[1]+gt_out[2];
					y=x_in*gt_out[3]+y_in*gt_out[4]+gt_out[5];

					result=true;
				}
			}
			// --------------------------------------------------------------------
			//		If the pixelscale count is zero, but we have tiepoints use
			//		the tiepoint based approach.
			// --------------------------------------------------------------------
			else if(tiepoint_count>6&&count==0)
			{
				result=GTIFTiepointTranslate(tiepoint_count/6, tiepoints, 3, tiepoints, 0, ref x, ref y);
			}
			// --------------------------------------------------------------------
			//		For now we require one tie point, and a valid pixel scale.
			// --------------------------------------------------------------------
			else if(count>=3&&tiepoint_count>=6)
			{
				x=(x-tiepoints[3])/pixel_scale[0]+tiepoints[0];
				y=(y-tiepoints[4])/(-1*pixel_scale[1])+tiepoints[1];

				result=true;
			}

			return result;
		}
	}
}