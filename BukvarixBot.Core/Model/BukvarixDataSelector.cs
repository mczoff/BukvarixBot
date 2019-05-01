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

namespace BukvarixBot.Core.Model
{
    public class BukvarixDataSelector
    {
        readonly string _wordsFilePath = "words.txt";
        readonly IWebDriver _webDriver;
        readonly IJavaScriptExecutor _javaScriptExecutor;
        readonly string _bukvarixUrl = "https://www.bukvarix.com/keywords/?q=";
        readonly BukvarixBotContextConfig _bukvarixBotContextConfig;
        readonly string _downloadButtonClass = "report-download-button";

        readonly FileWordsRepository _fileWordsRepository;
        readonly GoogleDriveRepository _googleDriveRepository;

        readonly object _lockSingleObject = new object();
        readonly string _cacheDirectory;
        private object _lockReadWordsObject = new object();

        public Guid Id { get; set; }

        public event EventHandler OnSessionDone;

        public BukvarixDataSelector(BukvarixBotContextConfig bukvarixBotContextConfig, GoogleDriveRepository googleDriveRepository)
        {
            Id = Guid.NewGuid();

            _bukvarixBotContextConfig = bukvarixBotContextConfig;

            _cacheDirectory = _bukvarixBotContextConfig.CachePath;
            _googleDriveRepository = googleDriveRepository;
            _webDriver = new ChromeDriver(_bukvarixBotContextConfig.DriverPath, this.GenerateChromeSetting(_bukvarixBotContextConfig.CachePath));
            _javaScriptExecutor = _webDriver as IJavaScriptExecutor;
            _fileWordsRepository = new FileWordsRepository(_wordsFilePath);
        }

        private ChromeOptions GenerateChromeSetting(string cachePath)
        {
            ChromeOptions chromeOptions = new ChromeOptions();

            chromeOptions.AddUserProfilePreference("download.default_directory", Path.Combine(cachePath));
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");

            return chromeOptions;
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
            while (!cancellationToken.IsCancellationRequested)
            {
                Process currentProcess = Process.GetCurrentProcess();

                await Task.Run(async () =>
                {
                    string word = this.NextWord();

                    if (string.IsNullOrWhiteSpace(word))
                        return;

                    Guid guidSession = Guid.Empty;

                    lock (_lockSingleObject)
                        while (guidSession == Guid.Empty)
                            guidSession = this.GetIdSession(word);
                                          
                    CSVWordParser csvWordParser = new CSVWordParser(Path.Combine(_cacheDirectory, $"{guidSession}.csv"));

                    lock (_lockSingleObject)
                        foreach (var item in csvWordParser.Parse())
                            if (_fileWordsRepository.GetUidWord(item) == -1)
                                _fileWordsRepository.CreateWord(item);

                    lock (_lockSingleObject)
                        _googleDriveRepository.CreateCSVFile(Path.Combine(_cacheDirectory, $"{guidSession}.csv"), csvWordParser.GetWord(0));

                    File.Delete(Path.Combine(_cacheDirectory, $"{guidSession}.csv"));

                    OnSessionDone?.Invoke(this, new EventArgs());
                });
            }

            _webDriver?.Close();
        }

        private string NextWord()
        {
            lock (_lockReadWordsObject)
            {
                string word;

                try
                {
                    word = _fileWordsRepository.GetWord(_bukvarixBotContextConfig.IndexLine++);
                    return word;
                }
                catch
                {
                    _bukvarixBotContextConfig.IndexLine--;
                    return null;
                }
            }
        }

        private Guid GetIdSession(string word)
        {
            _webDriver.Url = $"{_bukvarixUrl}{word}";

            Guid guidSession = Guid.NewGuid();

            try
            {
                _javaScriptExecutor.ExecuteScript($@"var list, index; list = document.getElementsByClassName('{_downloadButtonClass}'); for (index = 0; index < list.length; ++index) {{ list[index].setAttribute('download', '{guidSession}.csv'); }}");
                _webDriver.FindElement(By.ClassName(_downloadButtonClass)).Click();

                this.WaitDownloadSync(guidSession);

                return guidSession;
            }

            catch (NoSuchElementException)
            {
                return Guid.Empty;
            }
        }

        private void WaitDownloadSync(Guid guidSession)
        {
            var downloadsPath = Path.Combine(_cacheDirectory, $"{guidSession}.csv");

            for (var i = 0; i < 30; i++)
            {
                if (File.Exists(downloadsPath))
                    break;

                Thread.Sleep(1000);
            }

            var length = new FileInfo(downloadsPath).Length;
            for (var i = 0; i < 30; i++)
            {
                Thread.Sleep(1000);

                var newLength = new FileInfo(downloadsPath).Length;

                if (newLength == length && length != 0)
                    break;

                length = newLength;
            }
        }
    }
}
