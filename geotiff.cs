//*********************************************************************
//
// geotiff.cs - Public interface for Geotiff tag parsing.
//
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
//*********************************************************************

// Primary libgeotiff include file.
//
// This is the defacto registry for valid GEOTIFF GeoKeys
// and their associated symbolic values. This is also the only file
// of the GeoTIFF library which needs to be included in client source
// code.

using System;
using System.Collections.Generic;
using System.Text;

namespace Free.Ports.LibGeoTiff
{
	public static partial class libgeotiff
	{
		// This Version code should only change if a drastic
		// alteration is made to the GeoTIFF key structure. Readers
		// encountering a larger value should give up gracefully.
		const int GvCurrentVersion=1;

		const int LIBGEOTIFF_VERSION=1400;

		#region ato*()
		//**********************************************************************
		//							ato*()
		//**********************************************************************
		static readonly char[] wschars=new char[] { ' ', '\t', '\b', '\n', '\r' };
		static short atoshort(string s)
		{
			if(s==null||s=="") return 0;

			s=s.TrimStart();
			int i=s.IndexOfAny(wschars);
			if(i!=-1) s=s.Substring(0, i);

			try
			{
				return short.Parse(s);
			}
			catch
			{
				return 0;
			}
		}

		static int atoi(string s)
		{
			if(s==null||s=="") return 0;

			s=s.TrimStart();
			int i=s.IndexOfAny(wschars);
			if(i!=-1) s=s.Substring(0, i);

			try
			{
				return int.Parse(s);
			}
			catch
			{
				return 0;
			}
		}

		static double GTIFAtof(string s)
		{
			if(s==null||s=="") return 0;

			s=s.TrimStart();
			int i=s.IndexOfAny(wschars);
			if(i!=-1) s=s.Substring(0, i);

			try
			{
				return double.Parse(s, nc);
			}
			catch
			{
				return 0;
			}
		}
		#endregion
	}

	//*********************************************************************
	//					Public Structures & Definitions
	//*********************************************************************
	public enum tagtype_t
	{
		TYPE_BYTE=1,
		TYPE_SHORT=2,
		TYPE_LONG=3,
		TYPE_RATIONAL=4,
		TYPE_ASCII=5,
		TYPE_FLOAT=6,
		TYPE_DOUBLE=7,
		TYPE_SBYTE=8,
		TYPE_SSHORT=9,
		TYPE_SLONG=10,
		TYPE_UNKNOWN=11
	}
	
	public delegate void GTIFPrintMethod(string message, object aux);
	public delegate string GTIFReadMethod(object aux);
}
