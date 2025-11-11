using System.Diagnostics.Metrics;

namespace VerdaVida.Shared.OpenTelemetry;

public class BusinessMetrics : IDisposable
{
    private readonly Meter _meter;
    private readonly Counter<long> _estimatesReceivedCounter;
    private readonly Counter<double> _dollarValueBookedCounter;

    public BusinessMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(ActivitySourceProvider.DefaultSourceName);

        _estimatesReceivedCounter = _meter.CreateCounter<long>(
            TelemetryTags.Estimates.EstimatesReceived,
            unit: "{estimate}",
            description: "Total number of estimates received");

        _dollarValueBookedCounter = _meter.CreateCounter<double>(
            TelemetryTags.Estimates.DollarValueBooked,
            unit: "USD",
            description: "Total dollar value of estimates booked (accepted)");
    }

    public void RecordEstimateReceived()
    {
        if (_estimatesReceivedCounter.Enabled)
        {
            _estimatesReceivedCounter.Add(1);
        }
    }

    public void RecordDollarValueBooked(decimal amount)
    {
        if (_dollarValueBookedCounter.Enabled)
        {
            _dollarValueBookedCounter.Add((double)amount);
        }
    }

    public void Dispose() => _meter.Dispose();
}

