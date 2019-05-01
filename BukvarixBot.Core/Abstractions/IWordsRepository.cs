using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BukvarixBot.Core.Abstractions
{
    public interface IWordsRepository<TWord, TUid>
    {
        IEnumerable<TWord> GetWords();

        TUid GetUidWord(TWord word);
        TWord GetWord(TUid unique);
        void CreateWord(TWord unique);
    }
}
