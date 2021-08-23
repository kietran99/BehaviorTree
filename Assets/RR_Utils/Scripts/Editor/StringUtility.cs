namespace RR.Utils
{
	public static class StringUtility
	{
		public static string InsertWhiteSpaces(string str)
		{
			var strBuilder = new System.Text.StringBuilder(str);
			int nWhiteSpaces = 0;
			
            for (int i = 1; i < str.Length; i++)
            {
                if (System.Char.IsUpper(str[i]))
                {
                    strBuilder.Insert(i + nWhiteSpaces, ' ');
					nWhiteSpaces++;
                }
            }

			return strBuilder.ToString();
		}
	}
}