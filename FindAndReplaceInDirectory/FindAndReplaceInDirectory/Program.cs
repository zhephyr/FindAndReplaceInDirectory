using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FindAndReplaceInDirectory
{
    struct SkuDatum
    {
        public string sku;
        public string oldRetail;
        public string newRetail;

        public SkuDatum(string skuStr, string oldR, string newR)
        {
            sku = skuStr;
            oldRetail = oldR;
            newRetail = newR;
        }
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
                    if (text.Contains(skuDatum.sku) && text.Contains("26.00"))
                    {
						if (lastChanged == 0.0 || float.Parse(skuDatum.oldRetail) != lastChanged)
						{
							lastChanged = float.Parse(skuDatum.oldRetail);
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
            var idx = 0;
            var listZeros = new List<int>();

			Console.Write(tabs);
			Console.ReadLine();

			for (var i = 0; i < lines.Length; i++)
			{
				if (lines[i].Contains("$26.00"))
				{
					listZeros.Add(i);
				}
			}

			if (listZeros.Count < 1)
			{
				return fileText;
			}

			//for (var i = 0; i < lines.Length; i++)
			//{
			//    if (lines[i].Contains(sku.sku))
			//    {
			//        var nearIdx = 0;
			//        foreach (var zeroIdx in listZeros)
			//        {
			//            var a = Math.Abs(nearIdx - i);
			//            var b = Math.Abs(zeroIdx - i);
			//            if (nearIdx == 0 || (b < a))
			//            {
			//                nearIdx = zeroIdx;
			//            }
			//        }

			//        listZeros.Remove(nearIdx);
			//        lines[nearIdx] = lines[nearIdx].Replace("$26.00", "$" + sku.newRetail);
			//    }
			//}

			return String.Join("\n", lines);
        }

		private static Tuple<uint[], string[], string[]> SplitFile(MarkupType fileType, string fileText)
		{
			var lines = fileText.Split('\n');
			var tabCnt = new uint[lines.Length];
			var prefixes = new string[lines.Length];

			for (int i = 0; i < lines.Length; i++)
			{
				tabCnt[i] = (uint)lines[i].Count(ch => ch == '\t');

				var idx = lines[i].IndexOfAny(fileType.elemDelim);
				prefixes[i] = lines[i].Trim().Substring(0, idx);
			}

			return new Tuple<uint[], string[], string[]>(tabCnt, prefixes, lines);
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
