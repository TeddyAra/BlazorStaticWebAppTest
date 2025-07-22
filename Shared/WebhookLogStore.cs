using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace BlazorApp.Shared
{
    public static class WebhookLogStore
    {
        private static readonly ConcurrentQueue<string> _logs = new ConcurrentQueue<string>();

        public static void Add(string log)
        {
            _logs.Enqueue(log);
            while (_logs.Count > 100) _logs.TryDequeue(out _);
        }

        public static IEnumerable<string> GetAll() => _logs.ToArray();
    }
}
