using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FindAndReplaceInDirectory
{
	struct SkuDatum
	{
		private string sku;
		private string oldRetail;
		private string newRetail;

		public SkuDatum(string skuStr, string oldR, string newR) : this()
		{
			Sku = skuStr;
			OldRetail = oldR;
			NewRetail = newR;
		}

		public string Sku { get => sku; set => sku = value; }
		public string OldRetail { get => oldRetail; set => oldRetail = value; }
		public string NewRetail { get => newRetail; set => newRetail = value; }
	}

	class Program
	{
		static void Main(string[] args)
		{
			var skuList = GetSkuData(@"C:\Users\michaelw\DB FILES\master-prices(old-new)(2).csv");
			var fileList = GetFileList(@"C:\Users\Michael.wang\Downloads\FTP Files\Views\all views\product_partials\test");
			FindAndReplace(fileList, skuList);
		}

		// Takes a list of files and skus and does a ReplaceNearest() when sku is found.
		static void FindAndReplace(List<string> fileList, List<SkuDatum> skuData)
		{
			foreach (var file in fileList)
			{
				string text = File.ReadAllText(file);
				var lastChanged = 0.0;
				foreach (var skuDatum in skuData)
				{
					if (text.Contains(skuDatum.Sku) && text.Contains("26.00"))
					{
						if (lastChanged == 0.0 || float.Parse(skuDatum.OldRetail) != lastChanged)
						{
							lastChanged = float.Parse(skuDatum.OldRetail);
							text = ReplaceNearest(skuDatum, text);
						}
					}

					File.WriteAllText(file, text);
				}
			}
		}

		// Attempted functionality: Intelligently find and replace words in files by creating a branch stucture based on the HTML
		// (or in this case, Jade/Pug) markup.
		static string ReplaceNearest(SkuDatum sku, String fileText)
		{
			var fileType = new MarkupType("Jade");
			var splitFile = SplitFile(fileType, fileText);
			var tabs = splitFile.Item1;
			var prefixes = splitFile.Item2;
			var lines = splitFile.Item3;
			var listZeros = GetReplaceIndexs(lines, "$26.00");



			return String.Join("\n", lines);
		}

		private static List<int> GetReplaceIndexs(string[] lines, string lookup)
		{
			var idxFound = new List<int>();

			for (var i = 0; i < lines.Length; i++)
			{
				if (lines[i].Contains(lookup))
				{
					idxFound.Add(i);
				}
			}
			return idxFound;
		}

		// Breaks out the file text into the information we need: number of tabs before the text, the element names, and the text itsself.
		private static Tuple<uint[], string[], string[]> SplitFile(MarkupType fileType, string fileText)
		{
			var lines = fileText.Split('\n');
			var tabCnt = new uint[lines.Length];
			var elemName = new string[lines.Length];

			for (int i = 0; i < lines.Length; i++)
			{
				tabCnt[i] = (uint)lines[i].Count(ch => ch == '\t');

				var idx = lines[i].IndexOfAny(fileType.elemDelim);
				elemName[i] = lines[i].Trim().Substring(0, idx);
			}

			return new Tuple<uint[], string[], string[]>(tabCnt, elemName, lines);
		}

		private static List<SkuDatum> GetSkuData(string filePath)
		{
			var skuList = new List<SkuDatum>();

			using (var reader = new StreamReader(filePath))
			{
				while (!reader.EndOfStream)
				{
					var dataArr = reader.ReadLine().Split('|');
					var skuDatum = new SkuDatum(dataArr[0], dataArr[4], dataArr[5]);
					skuList.Add(skuDatum);
				}
			}

			return skuList;
		}

		private static List<string> GetFileList(string root)
		{
			var fileList = new List<string>();

			try
			{
				fileList.AddRange(Directory.GetFiles(root).ToList<string>());
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			if (fileList.Any())
			{
				var subDirs = Directory.GetDirectories(root);
				foreach (var dir in subDirs)
				{
					fileList.AddRange(GetFileList(dir));
				}
			}

			return fileList;
		}
	}
}
