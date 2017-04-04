#if USE_LINQ
using System;
using System.Linq;
namespace LockFreeSynchronization
{
	sealed class ChannelLinq : AChannel
	{
		internal ChannelLinq(Channel ch, TimeSpan priod) : base(ch, priod)
		{
		}

		/// <summary>
		/// Add the specified datum to the series. The oldest will be dropped from the
		/// series until the time span from tholdest to the last timestamp equals to the
		/// target period
		/// </summary>
		/// <returns>The add.</returns>
		/// <param name="datum">Datum.</param>
		public override void Add(Datum datum)
		{
			long before;
			DateTime elapsed;

			if (series.Count == 0)
				start = datum.timeStamp;
			elapsed = start.Add(period);
			if (datum.timeStamp.CompareTo(elapsed) == 0 || datum.timeStamp.CompareTo(elapsed) > 0)
			{
				if (datum.timeStamp.CompareTo(elapsed) > 0)
				{
					do
					{
//						Console.WriteLine("{0} GAP in data removing {1} {2}", name, series[0].timeStamp, series[0].value);
						series.RemoveAt(0);
						if (series.Count != 0)
						{
							start = series[0].timeStamp;
							elapsed = start.Add(period);
						}
						else
							break;
					}
					while (datum.timeStamp.CompareTo(elapsed) != 0);
				}
				if (datum.timeStamp.CompareTo(elapsed) == 0)
				{
					if (series.Count > highSampleSize)
						highSampleSize = series.Count;
					else if (series.Count < lowSampleSize)
						lowSampleSize = series.Count;
					before = System.Environment.TickCount;
					CalculateAverages(datum.timeStamp);
					totalTicks += System.Environment.TickCount - before;
				}
			}
			series.Add(datum);
			if (datum.value > highPoint.Result)
			{
				highPoint.Result = datum.value;
				highPoint.Begin = datum.timeStamp;
			}
			else if (datum.value < lowPoint.Result)
			{
				lowPoint.Result = datum.value;
				lowPoint.Begin = datum.timeStamp;
			}
		}

		/// <summary>
		/// Calcualte the average by using Linq
		/// It is o(n) time.
		/// </summary>
		/// <param name="ts">Ts.</param>
		internal override void CalculateAverages(DateTime ts)
		{
			double tmpAverage = series.Average(d => Convert.ToDouble(d.value));
			if (tmpAverage > bestAverage.Result)
			{
				bestAverage.Result = tmpAverage;
				bestAverage.Begin = series[0].timeStamp;
				bestAverage.End = ts;
			}
			else if (tmpAverage < worstAverage.Result)
			{
				worstAverage.Result = tmpAverage;
				worstAverage.Begin = series[0].timeStamp;
				worstAverage.End = ts;
			}
			series.RemoveAt(0);
			start = series[0].timeStamp;
		}
	}
}
#endif