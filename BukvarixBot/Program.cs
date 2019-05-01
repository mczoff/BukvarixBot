using BukvarixBot.Core;
using BukvarixBot.Core.Model;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BukvarixBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            BukvarixBotContext bukvarixCore = new BukvarixBotContext("startupContextConfig.json");
            await bukvarixCore.StartAsync();
        }
    }
}
