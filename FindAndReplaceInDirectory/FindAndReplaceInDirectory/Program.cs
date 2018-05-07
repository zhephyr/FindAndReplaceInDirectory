using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var skuList = GetSkuData(@"C:\Users\Michael.wang\Downloads\FTP Files\DB Files\master-prices(old-new)(2).csv");
            var fileList = GetFileList(@"C:\Users\Michael.wang\Downloads\FTP Files\Views\all views\product_partials\test");
            FindAndReplace(fileList, skuList);
        }

        static void FindAndReplace(List<string> fileList, List<SkuDatum> skuData)
        {
            foreach (var file in fileList)
            {
                string text = File.ReadAllText(file);
                foreach (var skuDatum in skuData)
                {
                    if (text.Contains(skuDatum.sku) && text.Contains("26.00"))
                    {
                        text = ReplaceNearest(skuDatum, text);
                    }

                    File.WriteAllText(file, text);
                }
            }
        }

        static string ReplaceNearest(SkuDatum sku, String fileText)
        {
            var lines = fileText.Split('\n');
            var idx = 0;
            var listZeros = new List<int>();

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

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(sku.sku))
                {
                    var nearIdx = 0;
                    foreach (var zeroIdx in listZeros)
                    {
                        var a = Math.Abs(nearIdx - i);
                        var b = Math.Abs(zeroIdx - i);
                        if (nearIdx == 0 || (b < a))
                        {
                            nearIdx = zeroIdx;
                        }
                    }

                    listZeros.Remove(nearIdx);
                    lines[nearIdx] = lines[nearIdx].Replace("$26.00", "$" + sku.newRetail);
                }
            }

            return String.Join("\n", lines);
        }

        static List<SkuDatum> GetSkuData(string filePath)
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

        static List<string> GetFileList(string root)
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
