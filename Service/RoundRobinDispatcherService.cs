using SGCP.IService;

namespace SGCP.Service
{
    public class RoundRobinDispatcherService : IRoundRobinDispatcherService
    {
        private readonly List<string> _servers;
        private int _currentIndex = -1;
        private readonly object _lock = new object();

        public RoundRobinDispatcherService(List<string> servers)
        {
            _servers = servers;
        }

        public string GetNextServer()
        {
            lock (_lock)
            {
                _currentIndex = (_currentIndex + 1) % _servers.Count;
                return _servers[_currentIndex];
            }
        }
    }
}
