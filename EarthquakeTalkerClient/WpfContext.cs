using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace EarthquakeTalkerClient
{
    public sealed class WpfContext : IContext
    {
        private readonly Dispatcher m_dispatcher = null;

        public bool IsSynchronized => m_dispatcher.Thread == Thread.CurrentThread;

        public WpfContext() : this(Dispatcher.CurrentDispatcher)
        {
        }

        public WpfContext(Dispatcher dispatcher)
        {
            m_dispatcher = dispatcher;
        }

        public void Invoke(Action action)
        {
            m_dispatcher.Invoke(action);
        }

        public void BeginInvoke(Action action)
        {
            m_dispatcher.BeginInvoke(action);
        }
    }
}
