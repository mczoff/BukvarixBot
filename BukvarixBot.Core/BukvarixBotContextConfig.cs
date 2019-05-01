using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BukvarixBot.Core
{
    public class BukvarixBotContextConfig
    {
        public string CachePath { get; set; }
        public string DriverPath { get; set; }
        public string GoogleDriveCredentialsPath { get; set; }

        public int MaxWorkingSetProgram { get; set; }

        public int IndexLine { get; set; }
    }
}
