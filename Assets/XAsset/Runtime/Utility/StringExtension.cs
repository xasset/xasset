namespace libx
{ 
	public static class StringExtension
	{
		public static int IntValue(this string s)
		{
			int result;
			if (int.TryParse(s, out result)) {
				return result;	
			}
			return 0;
		}
	}
}