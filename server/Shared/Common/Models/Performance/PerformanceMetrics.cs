using System;
using System.Diagnostics;

namespace Shared.Common.Models.Performance
{
    public class PerformanceMetrics
    {
        public TimeSpan ExecutionTime { get; set; }
        public long MemoryUsedBytes { get; set; }
        public double CpuUsagePercentage { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        private readonly Stopwatch _stopwatch;

        public PerformanceMetrics()
        {
            _stopwatch = new Stopwatch();
            StartTime = DateTime.UtcNow;
            _stopwatch.Start();
        }

        public void StopTracking()
        {
            _stopwatch.Stop();
            EndTime = DateTime.UtcNow;
            ExecutionTime = _stopwatch.Elapsed;
            MemoryUsedBytes = Process.GetCurrentProcess().WorkingSet64;

            // Note: This is a simple CPU usage calculation
            // For more accurate CPU measurements, you might want to use proper performance counters
            CpuUsagePercentage = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds /
                                Environment.ProcessorCount /
                                ExecutionTime.TotalMilliseconds * 100;
        }
    }
}