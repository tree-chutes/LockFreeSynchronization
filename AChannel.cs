using System;
using System.Collections.Generic;

namespace LockFreeSynchronization
{
	internal abstract class AChannel 
	{
		protected readonly Channel name;
		protected DateTime start;
		protected StatResult bestAverage, worstAverage, highPoint, lowPoint;
		protected long totalTicks;
		protected int highSampleSize, lowSampleSize = Int32.MaxValue;
		protected readonly List<Datum> series;
		protected readonly TimeSpan period;

		protected AChannel(Channel ch, TimeSpan priod)
		{
			period = priod;
			name = ch;
			series = new List<Datum>();
			lowPoint.Result = Double.MaxValue;
			worstAverage.Result = Double.MaxValue;
		}

		internal abstract void CalculateAverages(DateTime ts);

		public abstract void Add(Datum datum);

		virtual public void PrintReport()
		{
			if (series.Count != 0)
			{
				Console.WriteLine("High Point: Timestamp = {0}  Value = {1}", highPoint.Begin, highPoint.Result);
				Console.WriteLine("Low Point: Timestamp = {0}  Value = {1}", lowPoint.Begin, lowPoint.Result);
				Console.WriteLine("Average Span =  {0} ", period);
				Console.WriteLine("Sample Size: High = {0}  Low = {1}", highSampleSize, lowSampleSize);
				Console.WriteLine("High: Start = {0}  End = {1}  Average = {2}", bestAverage.Begin, bestAverage.End, bestAverage.Result);
				Console.WriteLine("Low: Start = {0}  End = {1}  Average = {2}", worstAverage.Begin, worstAverage.End, worstAverage.Result);
				Console.WriteLine("Performace hit = {0} (Ticks)", this.totalTicks);
			}
			else
				Console.WriteLine("NO data was captured for this channel");

		}

		public StatResult GetBestAverage()
		{
			return bestAverage;
		}

		public StatResult GetWorstAverage()
		{
			return worstAverage;
		}

		public StatResult GetHighPoint()
		{
			return highPoint;
		}

		public StatResult GetLowPoint()
		{
			return lowPoint;
		}
	}
}
