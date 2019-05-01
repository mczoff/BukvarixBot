using BukvarixBot.Core.Model;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BukvarixBot.Core
{
    public class BukvarixBotContext
    {
        readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(3);
        readonly string _startupConfig;
        readonly GoogleDriveRepository _googleDriveRepository;
        readonly BukvarixBotContextConfig _bukvarixBotContextConfig;

        readonly BukvarixDataSelectorManager _bukvarixDataSelectorManager;

        public BukvarixBotContext(string startupConfig)
        {
            _startupConfig = startupConfig;
            _bukvarixBotContextConfig = JsonConvert.DeserializeObject<BukvarixBotContextConfig>(File.ReadAllText(startupConfig));
            _bukvarixBotContextConfig = _bukvarixBotContextConfig ?? new BukvarixBotContextConfig();
            _googleDriveRepository = new GoogleDriveRepository(_bukvarixBotContextConfig.GoogleDriveCredentialsPath);

            _bukvarixDataSelectorManager = new BukvarixDataSelectorManager(_bukvarixBotContextConfig, _googleDriveRepository);
        }

        [Obsolete("Use StartAsync with method Wait()")]
        public void Start()
            => this.Start(CancellationToken.None);

        [Obsolete("Use StartAsync with method Wait()")]
        public void Start(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task StartAsync()
            => await StartAsync(CancellationToken.None);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _googleDriveRepository.CreateCSVFolder($"bukvarix{DateTime.Now}");

            _bukvarixDataSelectorManager.Create(2);

            try
            {
                await _bukvarixDataSelectorManager.WaitAllDataSelectors();
            }
            finally
            {
                File.WriteAllText(_startupConfig, JsonConvert.SerializeObject(_bukvarixBotContextConfig));
            }
        }
    }
}
