namespace RR.Utils
{
	public static class StringUtility
	{
		public static string InsertWhiteSpaces(string str)
		{
			var strBuilder = new System.Text.StringBuilder(str);
			int nWhiteSpaces = 0;
			string str2D = "2D", str3D = "3D";


            for (int i = 1; i < str.Length; i++)
            {
				if (i < str.Length - 1 && (str.Substring(i, 2).Equals(str2D) || str.Substring(i, 2).Equals(str3D)))
				{
					strBuilder.Insert(i + nWhiteSpaces, ' ');
					nWhiteSpaces++;
					i++;
					continue;
				}

                if (System.Char.IsUpper(str[i]) && 
					((i == 0 || System.Char.IsLower(str[i - 1])) 
					|| (i == str.Length - 1 || System.Char.IsLower(str[i + 1]))))
                {
                    strBuilder.Insert(i + nWhiteSpaces, ' ');
					nWhiteSpaces++;
                }
            }

			return strBuilder.ToString();
		}
	}
}