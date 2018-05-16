// List of markup types and the prefix and delimiter strings that determine the seperate element names

using System;

namespace FindAndReplaceInDirectory
{
	class MarkupType
	{
		public string prefix;
		public char[] elemDelim;
		public MarkupType(string fileType)
		{
			switch(fileType)
			{
				case "Jade":
					prefix = "";
					elemDelim = new char[] { '.', '#', '(', ' ' };
					break;
				case "HTML":
					prefix = "<";
					elemDelim = new char[] { ' ' };
					break;
				default:
					throw new NotImplementedException();
			}
		}
	}
}
