using BukvarixBot.Core.Abstractions;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BukvarixBot.Core.Model
{
    public class CSVWordParser
        : ICSVWordParser
    {
        readonly int wordIndex = 0;
        readonly string _path;

        public CSVWordParser(string path)
        {
            _path = path;
        }

        public IEnumerable<string> Parse()
        {
            using (var reader = new StreamReader(_path))
            using (var csv = new CsvReader(reader))
            {
                //Skip header
                csv.Read();

                while (csv.Read())
                    yield return csv.GetField(wordIndex);
            }
                
        }

        public string GetWord(int index)
        {
            using (var reader = new StreamReader(_path))
            using (var csv = new CsvReader(reader))
            {
                //Skip header
                csv.Read();

                for (int i = 0; i <= index; i++)
                {
                    csv.Read();

                    if (i == index)
                        return csv.GetField(wordIndex);
                }
                    
                return null;
            }
        }
    }
}
