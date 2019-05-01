using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BukvarixBot.Core.Abstractions
{
    interface ICSVWordParser 
        : IWordParser<IEnumerable<string>>, IUidWordProvider<string, int>
    {
    }
}
