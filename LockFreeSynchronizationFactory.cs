using System;
using System.Threading;
using System.Collections.Generic;

namespace LockFreeSynchronization
{
	enum Channel
	{
		Power,
		Speed,
		HeartRate
	}

	internal class Work
	{
		internal long gate;
		internal WorkDatum[] workData;
	}

	internal class WorkDatum
	{
		readonly internal Channel channel;
		readonly internal Datum datum;

		internal WorkDatum(Channel ch, Datum dtum)
		{
			channel = ch;
			datum = dtum;
		}
	}

	internal struct Datum
	{
		readonly internal System.DateTime timeStamp;
		readonly internal double value;

		internal Datum(System.DateTime ts, double v)
		{
			timeStamp = ts;
			value = v;
		}
	}

	struct Worker
	{
		internal readonly LockFreeSynchronizationWorker instance;
		internal readonly Thread thread;

		internal Worker(LockFreeSynchronizationWorker i, TimeSpan s)
		{
			instance = i;
			thread = new Thread(new ThreadStart(i.DoWork));
			thread.Name = String.Format("[Worker ({0})]", s);
			thread.Start();
		}
	}


	internal static class LockFreeSynchronizationFactory
	{
		internal static Worker GetWorker(LockFreeSynchronizationWorker worker, TimeSpan period)
		{
			return new Worker(worker, period);
		}

		internal static WorkDatum GetWorkDatum(Channel ch, Datum dtum)
		{
			return new WorkDatum(ch, dtum);
		}

		/// <summary>
		/// Use NO_LINQ to compile/use channel that proceses data with no Linq.
		/// Use USE_LINQ to compile/use channel that proceses data with Linq.
		/// </summary>
		/// <returns>The channel.</returns>
		/// <param name="ch">Ch.</param>
		/// <param name="period">Period.</param>
		internal static AChannel GetChannel(Channel ch, TimeSpan period)
		{
			AChannel ret;
			switch (ch)
			{
				default:
#if NO_LINQ
					ret = new ChannelNoLinq(ch, period);
#elif USE_LINQ
					ret = new ChannelLinq(ch, period);
#endif
					break;
			}
			return ret;
		}
	}
}
