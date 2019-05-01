using BukvarixBot.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BukvarixBot.Core.Model
{
    public class FileWordsRepository
        : IFileWordsRepository
    {
        readonly string _path;

        public FileWordsRepository(string path)
        {
            _path = path;
        }

        public void CreateWord(string word)
            => File.AppendAllText(_path, $"{word}{Environment.NewLine}");

        public string GetWord(int unique)
            => File.ReadLines(_path).ElementAt(unique);

        public int GetUidWord(string word)
            => File.ReadLines(_path).Select((t,k) => new { Index = k, Word = t }).FirstOrDefault(t => t.Word == word)?.Index ?? -1;

        public IEnumerable<string> GetWords()
            => File.ReadLines(_path);
    }
}
