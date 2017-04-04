using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Dynastream.Fit;

namespace LockFreeSynchronization
{

	internal class LockFreeSynchronizationWorkerController
	{
		Work work;
		internal readonly Thread queueThread;
		ConcurrentQueue<WorkDatum[]> workQueue;
		readonly List<Worker> workers;
		readonly Channel[] channels;

		/// <summary>
		/// This controller will launch one thread per measuring span passed at the command line.
		/// Worker threads will extract 3 channels by default from each record read from FIT file.
		/// </summary>
		/// <param name="spans">Spans.</param>
		/// <param name="chnls">Chnls.</param>
		internal LockFreeSynchronizationWorkerController(TimeSpan[] spans, params Channel[] chnls)
		{
			work = new Work();
			work.gate = 0;
			workers = new List<Worker>(spans.Length);
			foreach (TimeSpan span in spans)
				workers.Add(LockFreeSynchronizationFactory.GetWorker(new LockFreeSynchronizationWorker(span, ref work, chnls), span));
			channels = chnls;

			workQueue = new ConcurrentQueue<WorkDatum[]>();
			queueThread = new Thread(new ThreadStart(ProcessQueue));
			queueThread.Start();
		}

		/// <summary>
		/// We will extract the available channel data (timestamp and value) in each reacord. These will be stored in list and
		/// then pushed to a queue. The idea is disengage storage device as soon as possible
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		internal void OnRecordMesg(object sender, MesgEventArgs e)
		{
			Datum datum;
			Object value;
			double measure;
			List<WorkDatum> list;
			RecordMesg record = (RecordMesg)e.mesg;

			if (record != null)
			{
				list = new List<WorkDatum>();
				foreach (Channel ch in channels)
				{
					value = record.GetFieldValue(ch.ToString());
					if (value != null)
					{
						try
						{
							measure = Convert.ToDouble(value);
							datum = new Datum(record.GetTimestamp().GetDateTime(), measure);
							list.Add(LockFreeSynchronizationFactory.GetWorkDatum(ch, datum));
						}
						catch (Exception x)
						{
							Console.WriteLine("Exception: {0}", x);
						}
					}
				}
				if (list.Count != 0)
					workQueue.Enqueue(list.ToArray());
			}
		}

		/// <summary>
		/// Queue processing thred uses a Work object shared among this thread and worker threads.
		/// I am currently using Mono on OS X. No volsile class. I used Inteclocked to implement 
		/// a simple gate: one long works as flag the other as counter. Workdata is updated from
		/// queue and gate is open.
		/// 
		/// </summary>
		internal void ProcessQueue()
		{
			int sleepMS = 2 * workers.Count;// sleep 2 milliseconds per thread
			WorkDatum[] workDta;
			try
			{
				while (Thread.CurrentThread.ThreadState != ThreadState.Aborted)
				{
					if (workQueue.TryDequeue(out workDta))
					{
						work.workData = workDta;
						Interlocked.Exchange(ref work.gate, workers.Count);
						Thread.Sleep(5);
						while (Interlocked.Read(ref work.gate) > 0)
							Thread.Sleep(sleepMS);
					}
					Thread.Sleep(sleepMS);
				}
			}
			catch (Exception x)
			{
				if (!(x is ThreadInterruptedException))
					Console.WriteLine(x);
			}
		}

		/// <summary>
		/// Now the decoder is done. We have to wait for the queuue to be empty
		/// the shut the threads and print results sequentially
		/// </summary>
		internal void PrintReport()
		{
			while (workQueue.Count != 0)
//			{
//				Console.WriteLine(workQueue.Count);
				Thread.Sleep(50);
//			}

			while (queueThread.ThreadState != ThreadState.Stopped)
			{
				queueThread.Interrupt();
				Thread.Sleep(1);
			}
			foreach (Worker w in workers)
			{
				while (w.thread.ThreadState != ThreadState.Stopped)
				{
					w.thread.Interrupt();
					Thread.Sleep(1);
				}
				w.instance.PrintReport();
			}
		}
	}
}
