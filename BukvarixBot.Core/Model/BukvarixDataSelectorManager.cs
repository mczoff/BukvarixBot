using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BukvarixBot.Core.Model
{
    public class BukvarixDataSelectorManager
    {
        readonly List<Tuple<BukvarixDataSelector, CancellationTokenSource>> _dataselectors;

        readonly BukvarixBotContextConfig _bukvarixBotContextConfig;
        readonly GoogleDriveRepository _googleDriveRepository;

        public BukvarixDataSelectorManager(BukvarixBotContextConfig bukvarixBotContextConfig, GoogleDriveRepository googleDriveRepository)
        {
            _bukvarixBotContextConfig = bukvarixBotContextConfig;
            _googleDriveRepository = googleDriveRepository;

            _dataselectors = new List<Tuple<BukvarixDataSelector, CancellationTokenSource>>();
        }

        public void Create(int count)
        {
            CancellationTokenSource cancellationTokenSounrce = new CancellationTokenSource();

            for (int i = 0; i < count; i++)
                AddNewSelector(cancellationTokenSounrce);
        }

        private void AddNewSelector(CancellationTokenSource cancellationTokenSounrce)
        {
            BukvarixDataSelector selector = new BukvarixDataSelector(_bukvarixBotContextConfig, _googleDriveRepository);

            selector.OnSessionDone += delegate (object sender, EventArgs args)
            {
                Process process = Process.GetCurrentProcess();

                if (process.PrivateMemorySize64 > _bukvarixBotContextConfig.MaxWorkingSetProgram)
                {
                    var selectorTuple = _dataselectors.FirstOrDefault(t => t.Item1.Id == (sender as BukvarixDataSelector).Id);
                    selectorTuple.Item2.Cancel();
                }
            };

            _dataselectors.Add(Tuple.Create(selector, cancellationTokenSounrce));
        }

        public void Delete(int count)
        {
            _dataselectors.Take(count).Select(t => t.Item2).ToList().ForEach(t => t.Cancel());
        }

        public async Task WaitAllDataSelectors()
        { 
            await Task.WhenAll(_dataselectors.Select(t => t.Item1.StartAsync(t.Item2.Token)));
        }
    }
}
