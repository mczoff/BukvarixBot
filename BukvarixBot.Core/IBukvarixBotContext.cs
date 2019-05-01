using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BukvarixBot.Core
{
    public interface IBukvarixBotContext
        : IDisposable
    {
        void Start();
        void Start(CancellationToken cancellationToken);

        Task StartAsync();
        Task StartAsync(CancellationToken cancellationToken);
    }
}
