using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BukvarixBot.Core.Abstractions
{
    public interface IGoogleDriveRepository<TFile>
    {
        void CreateCSVFile(TFile unique, string name);
    }
}
