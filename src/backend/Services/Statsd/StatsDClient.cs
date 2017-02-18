using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MeasurementApi.Services
{
    public static class StatsDClient
    {
        private static IMonitoringConfiguration _config;
        private static IPEndPoint _ipEndPoint;

        [ThreadStatic]
        private static UdpClient _client;

        public static void Init(IMonitoringConfiguration configuration)
        {
            _config = configuration;
            _client= new UdpClient();
            _ipEndPoint = new IPEndPoint(IPAddress.Parse(_config.StatsDHost), _config.StatsDPort);
        }

        public async static void Send(string name, long value, string type)
        {
            if (string.IsNullOrEmpty(_config?.GraphiteKeyPrefixForStatsD))
                return;            

            var key = StatsDWriter.SanitizeKey(_config.GraphiteKeyPrefixForStatsD + "." + name);
            var stringToSend = key + ":" + value + "|" + type;
            var bytes = Encoding.ASCII.GetBytes(stringToSend);
            await _client.SendAsync(bytes, bytes.Length, _ipEndPoint);
        }

        
    }
}