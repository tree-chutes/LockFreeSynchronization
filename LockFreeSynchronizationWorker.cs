using System;
using System.Threading;
using System.Collections.Generic;

namespace LockFreeSynchronization
{
	internal class LockFreeSynchronizationWorker
	{
		Work work;
		readonly TimeSpan period;
		readonly Dictionary<Channel, AChannel> channels;

		internal LockFreeSynchronizationWorker(TimeSpan priod, ref Work wrk, params Channel[] chnls)
		{
			period = priod;
			channels = new Dictionary<Channel, AChannel>();
			foreach (Channel ch in chnls)
				channels.Add(ch, LockFreeSynchronizationFactory.GetChannel(ch, priod));
			work = wrk;
		}

		/// <summary>
		/// Process the Work data. If gate > 0, it is time do work. Do it and make sure
		/// we don't do it more than once. loop through Workdata. And send it to the
		/// appropiate channel
		/// </summary>
		internal void DoWork()
		{
			bool workDone = false;
			bool workToBedone = false;
			try
			{
				while (Thread.CurrentThread.ThreadState != ThreadState.Aborted)
				{
					if (Interlocked.Read(ref work.gate) > 0) //lock free. No thread has to wait for another
					{
						if (workToBedone) //first time
							workDone = false;
						
						if (!workDone)
						{
							foreach (WorkDatum workDatum in work.workData)
							{
								channels[workDatum.channel].Add(workDatum.datum);
							}
							Interlocked.Decrement(ref work.gate);
							workDone = true; //If gate is > 0 when exectuion returns we are assured 
							workToBedone = false;// not to execute again
						}
					}
					else
					{
						workToBedone = true;
						workDone = true;
					}
					Thread.Sleep(1); // yield frequently
				}
			}
			catch (Exception x)
			{
				if (!(x is ThreadInterruptedException))
					Console.WriteLine(x);
			}
		}

		internal void PrintReport()
		{
			Console.WriteLine("{1}[Worker {0}]",period, Environment.NewLine);
			foreach (Channel ch in channels.Keys)
			{
				Console.WriteLine("{1}{0}{1}", ch, Environment.NewLine);
				channels[ch].PrintReport();
			}
		}
	}
}
