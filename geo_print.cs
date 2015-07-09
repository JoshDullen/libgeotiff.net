//*********************************************************************
//
// geo_print.cs -- Key-dumping routines for GEOTIFF files.
//
//	Written By: Niles D. Ritter
//				The Authors
//
// Copyright (c) 1995 Niles D. Ritter
// Copyright (c) 2008-2009 by the Authors
//
// Permission granted to use this software, so long as this copyright
// notice accompanies any products derived therefrom.
//
//	Revision History;
//
//	20 June, 1995		Niles D. Ritter		New
//	07 July, 1995		NDR					Fix indexing
//	27 July, 1995		NDR					Added Import utils
//	28 July, 1995		NDR					Made parser more strict.
//	29 Sept, 1995		NDR					Fixed matrix printing.
//	30 Sept, 2008		The Authors			Port to C#
//	11 Dec, 2009		The Authors			Update to current SVN version
//
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Free.Ports.LibTiff;

namespace Free.Ports.LibGeoTiff
{
	public static partial class libgeotiff
	{
		const string FMT_GEOTIFF="Geotiff_Information:";
		const string FMT_VERSION="Version: {0}";
		const string FMT_REV="Key_Revision: {0}.{1}";
		const string FMT_TAGS="Tagged_Information:";
		const string FMT_TAGEND="End_Of_Tags.";
		const string FMT_KEYS="Keyed_Information:";
		const string FMT_KEYEND="End_Of_Keys.";
		const string FMT_GEOEND="End_Of_Geotiff.";
		const string FMT_DOUBLE="{0,-17:g15}";
		const string FMT_SHORT="{0,-11}";

		// Print off the directory info, using whatever method is specified
		// (defaults to fprintf if null). The "aux" parameter is provided for user
		// defined method for passing parameters or whatever.
		//
		// The output format is a "GeoTIFF meta-data" file, which may be
		// used to import information with the GTIFFImport() routine.
		public static void GTIFPrint(GTIF gtif)
		{
			GTIFPrint(gtif, null, null);
		}

		public static void GTIFPrint(GTIF gtif, GTIFPrintMethod print, object aux)
		{
			if(print==null) print=DefaultPrint;
			if(aux==null) aux=Console.Out;

			string message=FMT_GEOTIFF+"\n"; print(message, aux);
			message=string.Format("\t"+FMT_VERSION+"\n", gtif.gt_version); print(message, aux);
			message=string.Format("\t"+FMT_REV+"\n", gtif.gt_rev_major, gtif.gt_rev_minor); print(message, aux);
			message=string.Format("\t{0}\n", FMT_TAGS); print(message, aux);

			PrintGeoTags(gtif, print, aux);

			message=string.Format("\t\t{0}\n", FMT_TAGEND); print(message, aux);
			message=string.Format("\t{0}\n", FMT_KEYS); print(message, aux);

			foreach(GeoKey key in gtif.gt_keys.Values) PrintKey(key, print, aux);

			message=string.Format("\t\t{0}\n", FMT_KEYEND); print(message, aux);
			message=string.Format("\t{0}\n", FMT_GEOEND); print(message, aux);
		}

		static void PrintGeoTags(GTIF gt, GTIFPrintMethod print, object aux)
		{
			TIFF tif=gt.gt_tif;
			if(tif==null) return;

			object data;
			int count;

			if(gt.gt_methods.get(tif, (ushort)GTIFF_TIEPOINTS, out count, out data))
				PrintTag((int)GTIFF_TIEPOINTS, count/3, (double[])data, 3, print, aux);

			if(gt.gt_methods.get(tif, (ushort)GTIFF_PIXELSCALE, out count, out data))
				PrintTag((int)GTIFF_PIXELSCALE, count/3, (double[])data, 3, print, aux);

			if(gt.gt_methods.get(tif, (ushort)GTIFF_TRANSMATRIX, out count, out data))
				PrintTag((int)GTIFF_TRANSMATRIX, count/4, (double[])data, 4, print, aux);
		}

		static void PrintTag(int tag, int nrows, double[] data, int ncols, GTIFPrintMethod print, object aux)
		{
			print("\t\t", aux);
			print(GTIFTagName(tag), aux);
			string message=string.Format(" ({0},{1}):\n", nrows, ncols);
			print(message, aux);

			int ind=0;
			for(int i=0; i<nrows; i++)
			{
				print("\t\t\t", aux);
				for(int j=0; j<ncols; j++)
				{
					message=string.Format(FMT_DOUBLE, data[ind++]);
					print(message, aux);

					if(j<ncols-1) print(" ", aux);
				}
				print("\n", aux);
			}
		}

		static void PrintKey(GeoKey key, GTIFPrintMethod print, object aux)
		{
			print("\t\t", aux);

			geokey_t keyid=key.gk_key;
			print(GTIFKeyName(keyid), aux);

			int count=key.gk_count;
			string message=string.Format(" ({0},{1}): ", GTIFTypeName(key.gk_type), count);
			print(message, aux);

			object data=key.gk_data;

			switch(key.gk_type)
			{
				case tagtype_t.TYPE_ASCII:
					{
						string str=data as string;
						if(str==null) throw new Exception("string expected.");
						if(str.Length<count) throw new Exception("string too short.");

						message="\"";
						
						for(int i=0; i<count; i++)
						{
							char c=str[i];

							if(c=='\n') message+="\\n";
							else if(c=='\\') message+="\\\\";
							else if(c=='\t') message+="\\t";
							else if(c=='\b') message+="\\b";
							else if(c=='\r') message+="\\r";
							else if(c=='"') message+="\\\"";
							else if(c=='\0') message+="\\0";
							else message+=c;
						}

						message+="\"\n";
						print(message, aux);
					}
					break;
				case tagtype_t.TYPE_DOUBLE:
					double[] dptr=data as double[];
					if(dptr==null) throw new Exception("double[] expected.");
					if(dptr.Length<count) throw new Exception("double[] too short.");
					for(int i=0; i<count; i+=3)
					{
						int done=Math.Min(i+3, count);
						for(int j=i; j<done; j++)
						{
							message=string.Format(FMT_DOUBLE, dptr[j]);
							print(message, aux);
						}
						print("\n", aux);
					}
					break;
				case tagtype_t.TYPE_SHORT:
					ushort[] sptr=data as ushort[];
					if(sptr==null) throw new Exception("ushort[] expected.");
					if(sptr.Length<count) throw new Exception("ushort[] too short.");
					if(count==1)
					{
						print(GTIFValueName(keyid, sptr[0]), aux);
						print("\n", aux);
					}
					else
					{
						for(int i=0; i<count; i+=3)
						{
							int done=Math.Min(i+3, count);
							for(int j=i; j<done; j++)
							{
								message=string.Format(FMT_SHORT, sptr[j]);
								print(message, aux);
							}
							print("\n", aux);
						}
					}
					break;
				default:
					message=string.Format("Unknown Type ({0})\n", key.gk_type);
					print(message, aux);
					break;
			}
		}

		static void DefaultPrint(string str, object aux)
		{
			// Pretty boring
			TextWriter writer=aux as TextWriter;
			if(writer!=null) writer.Write(str);
		}

		// Importing metadata file

		// Import the directory info, using whatever method is specified
		// (defaults to fscanf if null). The "aux" parameter is provided for user
		// defined method for passing file or whatever.
		//
		// The input format is a "GeoTIFF meta-data" file, which may be
		// generated by the GTIFFPrint() routine.
		public static bool GTIFImport(GTIF gtif)
		{
			return GTIFImport(gtif, null, null);
		}

		public static bool GTIFImport(GTIF gtif, GTIFReadMethod scan, object aux)
		{
			if(scan==null) scan=DefaultRead;
			if(aux==null) aux=Console.In;

			string message=scan(aux);
			if(message==null||message.Length<20) return false;
			if(message.ToLower().Substring(0, 20)!=FMT_GEOTIFF.ToLower().Substring(0, 20)) return false;

			message=scan(aux);
			if(message==null||message.Length<10) return false;
			if(message.ToLower().Substring(0, 8)!=FMT_VERSION.ToLower().Substring(0, 8)) return false;
			message=message.Substring(9);
			try
			{
				gtif.gt_version=ushort.Parse(message);
			}
			catch
			{
				return false;
			}

			message=scan(aux);
			if(message==null||message.Length<15) return false;
			if(message.ToLower().Substring(0, 13)!=FMT_REV.ToLower().Substring(0, 13)) return false;
			message=message.Substring(14);
			try
			{
				string[] spl=message.Split('.');
				if(spl.Length!=2) return false;
				gtif.gt_rev_major=ushort.Parse(spl[0]);
				gtif.gt_rev_minor=ushort.Parse(spl[1]);
			}
			catch
			{
				return false;
			}

			message=scan(aux);
			if(message==null||message.Length<19) return false;
			if(message.ToLower().Substring(0, 19)!=FMT_TAGS.ToLower().Substring(0, 19)) return false;

			int status;
			while((status=ReadTag(gtif, scan, aux))>0) ;
			if(status<0) return false;

			message=scan(aux);
			if(message==null||message.Length<18) return false;
			if(message.ToLower().Substring(0, 18)!=FMT_KEYS.ToLower().Substring(0, 18)) return false;

			while((status=ReadKey(gtif, scan, aux))>0) ;
			return (status==0); // success
		}

		static int StringError(string str)
		{
			Console.Error.WriteLine("Parsing Error at '{0}'", str);
			return -1;
		}

		static int ReadTag(GTIF gt, GTIFReadMethod scan, object aux)
		{
			string message=scan(aux);
			if(message==null) return -1;
			if(message.Length>=12) if(message.ToLower().Substring(0, 12)==FMT_TAGEND.ToLower().Substring(0, 12)) return 0;

			try
			{
				int firstp=message.IndexOfAny(new char[] { '(', ' ', '\t' });
				if(firstp<0) return StringError(message);

				string tagname=message.Substring(0, firstp).Trim();
				message=message.Substring(firstp);

				int colon=message.IndexOf(':');
				if(colon<0) return StringError(message);

				message=message.Substring(0, colon);
				message=message.Trim(' ', '\t', '(', ')');

				string[] spl=message.Split(',');
				if(spl.Length!=2) return StringError(message);

				int nrows=int.Parse(spl[0]);
				int ncols=int.Parse(spl[1]);

				int tag=GTIFTagCode(tagname);
				if(tag<0) return StringError(tagname);

				int count=nrows*ncols;

				double[] dptr=new double[count];

				for(int i=0; i<nrows; i++)
				{
					message=scan(aux);
					if(message==null) return -1;

					spl=message.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
					if(spl.Length!=ncols) StringError(message);

					for(int j=0; j<ncols; j++)
					{
						message=spl[j];
						dptr[i*ncols+j]=GTIFAtof(message);
					}
				}
				gt.gt_methods.set(gt.gt_tif, (ushort)tag, count, dptr);
			}
			catch
			{
				return StringError(message);
			}

			return 1;
		}

		static int ReadKey(GTIF gt, GTIFReadMethod scan, object aux)
		{
			string message=scan(aux);
			if(message==null) return -1;
			if(message.Length>=12) if(message.ToLower().Substring(0, 12)==FMT_KEYEND.ToLower().Substring(0, 12)) return 0;

			try
			{
				int firstp=message.IndexOfAny(new char[] { '(', ' ', '\t' });
				if(firstp<0) return StringError(message);

				string name=message.Substring(0, firstp).Trim();
				string message1=message.Substring(firstp);

				int colon=message1.IndexOf(':');
				if(colon<0) return StringError(message1);

				string head=message1.Substring(0, colon);
				head=head.Trim(' ', '\t', '(', ')');

				string[] spl=head.Split(',');
				if(spl.Length!=2) return StringError(head);

				string type=spl[0];
				int count=int.Parse(spl[1]);

				// skip white space
				string data=message1.Substring(colon+1).Trim();
				if(data.Length==0) return StringError(message);

				if(GTIFKeyCode(name)<0) return StringError(name);
				geokey_t key=(geokey_t)GTIFKeyCode(name);

				if(GTIFTypeCode(type)<0) return StringError(type);
				tagtype_t ktype=(tagtype_t)GTIFTypeCode(type);

				switch(ktype)
				{
					case tagtype_t.TYPE_ASCII:
						{
							string cdata="";

							int firstDoubleQuote=data.IndexOf('"');
							if(firstDoubleQuote<0) return StringError(data);

							data=data.Substring(firstDoubleQuote+1);

							bool wasesc=false;
							char c='\0';
							for(int i=0; i<data.Length; i++)
							{
								c=data[i];
								if(wasesc)
								{
									if(c=='\\') cdata+='\\';
									else if(c=='"') cdata+='"';
									else if(c=='n') cdata+='\n';
									else if(c=='t') cdata+='\t';
									else if(c=='b') cdata+='\b';
									else if(c=='r') cdata+='\r';
									else if(c=='0') cdata+='\0';

									wasesc=false;
									continue;
								}

								if(c=='\\')
								{
									wasesc=true;
									continue;
								}

								if(c=='\0') break;
								if(c=='"') break;

								cdata+=c;

								if(cdata.Length==count)
								{
									c=data[i+1];
									break;
								}
							}

							if(cdata.Length<count) return StringError(message);
							if(c!='"') return StringError(message);

							GTIFKeySet(gt, key, cdata);
						}
						break;
					case tagtype_t.TYPE_DOUBLE:
						{
							double[] dptr=new double[count];
							int i=0;
							for(; count>0; count-=3)
							{
								spl=data.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
								if(spl.Length!=Math.Min(3, count)) return StringError(data);

								foreach(string part in spl)
								{
									message=part;
									dptr[i++]=GTIFAtof(part);
								}

								if(count>3) message=data=scan(aux);
							}
							GTIFKeySet(gt, key, dptr);
						}
						break;
					case tagtype_t.TYPE_SHORT:
						if(count==1)
						{
							int icode=GTIFValueCode(key, data);
							if(icode<0) return StringError(data);
							ushort code=(ushort)icode;
							GTIFKeySet(gt, key, code);
						}
						else // multi-valued short - no such thing yet
						{
							ushort[] sptr=new ushort[count];
							int i=0;
							for(; count>0; count-=3)
							{
								spl=data.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
								if(spl.Length!=Math.Min(3, count)) return StringError(data);

								foreach(string part in spl)
								{
									message=part;
									sptr[i++]=ushort.Parse(part);
								}

								if(count>3) message=data=scan(aux);
							}
							GTIFKeySet(gt, key, sptr);
						}
						break;
					default: return -1;
				}
			}
			catch
			{
				return StringError(message);
			}
			return 1;
		}

		static string DefaultRead(object aux)
		{
			// Pretty boring
			TextReader reader=aux as TextReader;
			if(reader!=null) return reader.ReadLine().Trim();
			return null;
		}
	}
}
