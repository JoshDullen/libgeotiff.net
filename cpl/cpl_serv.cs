namespace Free.Ports.LibGeoTiff
{
	public enum CSVCompareCriteria
	{
		CC_ExactString,
		CC_ApproxString,
		CC_Integer
	}

	public static partial class libgeotiff
	{
		//**********************************************************************
		//								CSLGetField()
		//
		//		Fetches the indicated field, being careful not to crash if
		//		the field doesn't exist within this string list. The
		//		returned pointer should not be freed, and doesn't
		//		necessarily last long.
		//**********************************************************************
		public static string CSLGetField(string[] papszStrList, int iField)
		{
			if(papszStrList==null||iField<0) return "";
			if(iField>=papszStrList.Length) return "";
			return papszStrList[iField];
		}
	}
}