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
//
// cpl_csv.cs: Support functions for accessing CSV files.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Free.Ports.LibGeoTiff
{
	public static partial class libgeotiff
	{
		// ====================================================================
		//		The CSVTable is a persistant set of info about an open CSV
		//		table. While it doesn't currently maintain a record index,
		//		or in-memory copy of the table, it could be changed to do so
		//		in the future.
		// ====================================================================
		class CSVTable
		{
			public StreamReader file;
			public string filename;
			public string[] fieldNames;
			public string[] recFields;

			public List<string[]> cache;
			public Dictionary<int, string[]> index;
		}

		static Dictionary<string, CSVTable> CSVTableList=new Dictionary<string, CSVTable>();

		//**********************************************************************
		//								CSVAccess()
		//
		//		This function will fetch a handle to the requested table.
		//		If not found in the "open table list" the table will be
		//		opened and added to the list. Eventually this function may
		//		become public with an abstracted return type so that
		//		applications can set options about the table. For now this
		//		isn't done.
		//**********************************************************************
		static CSVTable CSVAccess(string filename)
		{
			if(filename==null||filename=="") return null;

			string Filename=filename.ToLower();
			// --------------------------------------------------------------------
			//		Is the table already in the list.
			// --------------------------------------------------------------------
			if(CSVTableList.ContainsKey(Filename)) return CSVTableList[Filename];

			// --------------------------------------------------------------------
			//		Create an information structure about this table, and add to
			//		the front of the list.
			// --------------------------------------------------------------------
			CSVTable table=new CSVTable();
			table.filename=Filename;

			// --------------------------------------------------------------------
			//		If not, try to open it.
			// --------------------------------------------------------------------
			try
			{
				table.file=new StreamReader(Filename, Encoding.UTF8);
			}
			catch
			{
				return null;
			}

			CSVTableList.Add(Filename, table);

			// --------------------------------------------------------------------
			//		Read the table header record containing the field names.
			// --------------------------------------------------------------------
			table.fieldNames=CSVReadParseLine(table.file);

			return table;
		}

		//**********************************************************************
		//							CSVDeaccess()
		//**********************************************************************
		public static void CSVDeaccess(string filename)
		{
			// --------------------------------------------------------------------
			//		A NULL means deaccess all tables.
			// --------------------------------------------------------------------
			if(filename==null)
			{
				List<string> keys=new List<string>(CSVTableList.Keys);
				foreach(string key in keys) CSVDeaccess(key);
				return;
			}

			string Filename=filename.ToLower();
			// --------------------------------------------------------------------
			//		Find this table.
			// --------------------------------------------------------------------
			if(!CSVTableList.ContainsKey(Filename)) return;
			CSVTable table=CSVTableList[Filename];

			// --------------------------------------------------------------------
			//		Remove the link from the list.
			// --------------------------------------------------------------------
			CSVTableList.Remove(Filename);

			// --------------------------------------------------------------------
			//		Free the table.
			// --------------------------------------------------------------------
			if(table.file!=null) table.file.Close();
			if(table.cache!=null) table.cache.Clear();
		}

		//**********************************************************************
		//							CSVSplitLine()
		//
		//		Tokenize a CSV line into fields in the form of a string
		//		list. This is used instead of the CPLTokenizeString()
		//		because it provides correct CSV escaping and quoting
		//		semantics.
		//**********************************************************************
		static string[] CSVSplitLine(string line)
		{
			if(line==null||line=="") return null;

			List<string> ret=new List<string>();

			for(int i=0; i<line.Length; i++)
			{
				bool inString=false;

				string token="";

				// Try to find the next delimeter, marking end of token
				for(; i<line.Length; i++)
				{
					// End if this is a delimeter skip it and break.
					if(!inString&&line[i]==',')
					{
						i++;
						break;
					}

					if(line[i]=='"')
					{
						if(!inString||line[i+1]!='"')
						{
							inString=!inString;
							continue;
						}

						i++; // doubled quotes in string resolve to one quote
					}

					token+=line[i];
				}

				ret.Add(token);

				// If the last token is an empty token, then we have to catch
				// it now, otherwise we won't reenter the loop and it will be lost.
				if(i==line.Length&&line[i-1]==',') ret.Add("");
			}

			if(ret.Count==0) return new string[0];

			return ret.ToArray();
		}

		//**********************************************************************
		//							CSVIngest()
		//
		//		Load entire file into memory and setup index if possible.
		//**********************************************************************
		static void CSVIngest(string filename)
		{
			CSVTable table=CSVAccess(filename);

			if(table.cache!=null) return;

			// --------------------------------------------------------------------
			//		Ingest whole file.
			// --------------------------------------------------------------------
			table.cache=new List<string[]>();
			for(; ; )
			{
				string[] line=CSVReadParseLine(table.file);
				if(line==null) break;
				table.cache.Add(line);
			}

			table.index=new Dictionary<int, string[]>();
			try
			{
				foreach(string[] line in table.cache)
				{
					int i=atoi(line[0]);
					table.index.Add(i, line);
				}
			}
			catch
			{
				table.index.Clear();
				table.index=null;
			}

			// --------------------------------------------------------------------
			//		We should never need the file handle against, so close it.
			// --------------------------------------------------------------------
			table.file.Close();
			table.file=null;
		}

		//**********************************************************************
		//							CSVReadParseLine()
		//
		//		Read one line, and return split into fields. The return
		//		result is a stringlist, in the sense of the CSL functions.
		//**********************************************************************
		public static string[] CSVReadParseLine(StreamReader file)
		{
			if(file==null) return null;

			string line=file.ReadLine();
			if(line==null||line=="") return null;

			// --------------------------------------------------------------------
			//		If there are no quotes, then this is the simple case.
			//		Parse, and return tokens.
			// --------------------------------------------------------------------
			if(line.IndexOf('"')==-1) return CSVSplitLine(line);

			// --------------------------------------------------------------------
			//		We must now count the quotes in our working string, and as
			//		long as it is odd, keep adding new lines.
			// --------------------------------------------------------------------
			string workLine=line;

			for(; ; )
			{
				int count=0;
				foreach(char c in workLine) if(c=='"') count++;

				if(count%2==0) break;

				line=file.ReadLine();
				if(line==null||line=="") break;

				workLine+=line;
			}

			return CSVSplitLine(workLine);
		}

		//**********************************************************************
		//								CSVCompare()
		//
		//		Compare a field to a search value using a particular criteria.
		//**********************************************************************
		static bool CSVCompare(string fieldValue, string target, CSVCompareCriteria criteria)
		{
			if(criteria==CSVCompareCriteria.CC_ExactString) return fieldValue==target;
			else if(criteria==CSVCompareCriteria.CC_ApproxString) return fieldValue.ToLower()==target.ToLower();
			else if(criteria==CSVCompareCriteria.CC_Integer) return atoi(fieldValue)==atoi(target);
			return false;
		}

		//**********************************************************************
		//								CSVScanLines()
		//
		//		Read the file scanline for lines where the key field equals
		//		the indicated value with the suggested comparison criteria.
		//		Return the first matching line split into fields.
		//**********************************************************************
		public static string[] CSVScanLines(StreamReader file, int keyField, string value, CSVCompareCriteria criteria)
		{
			int testValue=atoi(value);
			for(;;)
			{
				string[] fields=CSVReadParseLine(file);
				if(fields==null) return null;
				if(fields.Length<keyField+1) continue; // not selected

				bool selected=false;
				if(criteria==CSVCompareCriteria.CC_Integer&&atoi(fields[keyField])==testValue)
					selected=true;
				else selected=CSVCompare(fields[keyField], value, criteria);

				if(selected) return fields;
			}
		}

		//**********************************************************************
		//						CSVScanLinesIngested()
		//
		//		Read the file scanline for lines where the key field equals
		//		the indicated value with the suggested comparison criteria.
		//		Return the first matching line split into fields.
		//**********************************************************************
		static string[] CSVScanLinesIngested(CSVTable table, int keyField, string value, CSVCompareCriteria criteria)
		{
			int nTestValue=0;
			if(criteria==CSVCompareCriteria.CC_Integer) nTestValue=atoi(value);

			// --------------------------------------------------------------------
			//		Short cut for indexed files.
			// --------------------------------------------------------------------
			if(keyField==0&&criteria==CSVCompareCriteria.CC_Integer&&table.index!=null)
			{
				if(table.index.ContainsKey(nTestValue)) return table.index[nTestValue];
				return null;
			}

			// --------------------------------------------------------------------
			//		Scan from in-core lines.
			// --------------------------------------------------------------------
			foreach(string[] line in table.cache)
			{
				if(line.Length<keyField+1) continue; // not selected

				bool selected=false;
				if(criteria==CSVCompareCriteria.CC_Integer&&atoi(line[keyField])==nTestValue)
					selected=true;
				else selected=CSVCompare(line[keyField], value, criteria);

				if(selected) return line;
			}

			return null;
		}

		//**********************************************************************
		//							CSVScanFile()
		//
		//		Scan a whole file using criteria similar to above, but also
		//		taking care of file opening and closing.
		//**********************************************************************
		public static string[] CSVScanFile(string filename, int keyField, string value, CSVCompareCriteria criteria)
		{
			// --------------------------------------------------------------------
			//		Get access to the table.
			// --------------------------------------------------------------------
			if(keyField<0) return null;

			CSVTable table=CSVAccess(filename);
			if(table==null) return null;

			CSVIngest(filename);

			// --------------------------------------------------------------------
			//		Does the current record match the criteria? If so, just
			//		return it again.
			// --------------------------------------------------------------------
			if(keyField>=0&&keyField<table.recFields.Length&&CSVCompare(value, table.recFields[keyField], criteria))
				return table.recFields;

			// --------------------------------------------------------------------
			//		Scan the file from the beginning, replacing the "current
			//		record" in our structure with the one that is found.
			// --------------------------------------------------------------------
			if(table.cache!=null) table.recFields=CSVScanLinesIngested(table, keyField, value, criteria);
			else
			{
				table.file.BaseStream.Seek(0, SeekOrigin.Begin);
				CSVReadParseLine(table.file); // throw away the header line

				table.recFields=CSVScanLines(table.file, keyField, value, criteria);
			}

			return table.recFields;
		}

		//**********************************************************************
		//							CPLGetFieldId()
		//
		//		Read the first record of a CSV file (rewinding to be sure),
		//		and find the field with the indicated name. Returns -1 if
		//		it fails to find the field name. Comparison is case
		//		insensitive, but otherwise exact. After this function has
		//		been called the file pointer will be positioned just after
		//		the first record.
		//**********************************************************************
		public static int CSVGetFieldId(StreamReader file, string fieldName)
		{
			file.BaseStream.Seek(0, SeekOrigin.Begin);

			string[] fields=CSVReadParseLine(file);
			for(int i=0; fields!=null&&fields[i]!=null; i++)
				if(fields[i].ToLower()==fieldName.ToLower()) return i;

			return -1;
		}

		//**********************************************************************
		//							CSVGetFileFieldId()
		//
		//		Same as CPLGetFieldId(), except that we get the file based
		//		on filename, rather than having an existing handle.
		//**********************************************************************
		public static int CSVGetFileFieldId(string filename, string fieldName)
		{
			// --------------------------------------------------------------------
			//		Get access to the table.
			// --------------------------------------------------------------------
			CSVTable table=CSVAccess(filename);
			if(table==null) return -1;

			// --------------------------------------------------------------------
			//		Find the requested field.
			// --------------------------------------------------------------------
			for(int i=0; table.fieldNames!=null&&table.fieldNames[i]!=null; i++)
				if(table.fieldNames[i].ToLower()==fieldName.ToLower()) return i;

			return -1;
		}

		//**********************************************************************
		//							CSVScanFileByName()
		//
		//		Same as CSVScanFile(), but using a field name instead of a
		//		field number.
		//**********************************************************************
		public static string[] CSVScanFileByName(string filename, string keyFieldName, string value, CSVCompareCriteria criteria)
		{
			int keyField=CSVGetFileFieldId(filename, keyFieldName);
			if(keyField==-1) return null;

			return CSVScanFile(filename, keyField, value, criteria);
		}

		//**********************************************************************
		//							CSVGetField()
		//
		//		The all-in-one function to fetch a particular field value
		//		from a CSV file. Note this function will return an empty
		//		string, rather than NULL if it fails to find the desired
		//		value for some reason. The caller can't establish that the
		//		fetch failed.
		//**********************************************************************
		public static string CSVGetField(string filename, string keyFieldName, string keyFieldValue,
			CSVCompareCriteria criteria, string targetField)
		{
			// --------------------------------------------------------------------
			//		Find the table.
			// --------------------------------------------------------------------
			CSVTable table=CSVAccess(filename);
			if(table==null) return "";

			// --------------------------------------------------------------------
			//		Find the correct record.
			// --------------------------------------------------------------------
			string[] record=CSVScanFileByName(filename, keyFieldName, keyFieldValue, criteria);
			if(record==null) return "";

			// --------------------------------------------------------------------
			//		Figure out which field we want out of this.
			// --------------------------------------------------------------------
			int iTargetField=CSVGetFileFieldId(filename, targetField);
			if(iTargetField<0) return "";

			if(iTargetField>=record.Length) return "";

			return record[iTargetField];
		}

		//**********************************************************************
		//							CSVFilename()
		//
		//		Return the full path to a particular CSV file. This will
		//		eventually be something the application can override.
		//**********************************************************************
		public delegate string CSVFilenameHookFunc(string filename);
		static CSVFilenameHookFunc CSVFilenameHook=null;

		public static string CSVFilename(string basename)
		{
			if(CSVFilenameHook==null) return CSVFilenameHook(basename);

			if(Environment.GetEnvironmentVariable("GEOTIFF_CSV")!=null) return Environment.GetEnvironmentVariable("GEOTIFF_CSV")+'/'+basename;
			if(File.Exists("/usr/local/share/epsg/csv/pcs.csv")) return "/usr/local/share/epsg/csv/"+basename;
			if(File.Exists("csv/pcs.csv")) return "csv/"+basename;
			if(File.Exists("share/epsg_csv/pcs.csv")) return "share/epsg_csv/"+basename;
			if(File.Exists("/usr/share/epsg_csv/pcs.csv")) return "/usr/share/epsg_csv/"+basename;
			return "/usr/local/share/epsg_csv/"+basename;
		}

		//**********************************************************************
		//							SetCSVFilenameHook()
		//
		//		Applications can use this to set a function that will
		//		massage CSV filenames.
		//**********************************************************************

		// Override CSV file search method.
		//
		// CSVFileOverride The pointer to a function which will return the
		// full path for a given filename.
		//
		// This function allows an application to override how the GTIFGetDefn() and
		// related function find the CSV (Comma Separated Value) values required.
		// The pfnHook argument should be a pointer to a function that will take in a
		// CSV filename and return a full path to the file. The returned string should
		// be to an internal static buffer so that the caller doesn't have to free the result.
		//
		// <b>Example:</b>
		//
		// The listgeo utility uses the following override function if the user
		// specified a CSV file directory with the -t commandline switch (argument
		// put into CSVDirName).
		//
		//	...
		//	SetCSVFilenameHook(CSVFileOverride);
		//	...
		//
		//	static string CSVFileOverride(string pszInput)
		//	{
		//	#ifdef WIN32
		//		return CSVDirName+'\\'+pszInput;
		//	#else
		//		return CSVDirName+'/'+pszInput;
		//	#endif
		//	}

		public static void SetCSVFilenameHook(CSVFilenameHookFunc newHook)
		{
			CSVFilenameHook=newHook;
		}
	}
}