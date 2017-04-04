using System;
using System.Collections.Generic;
using Dynastream.Fit;
using System.IO;

namespace LockFreeSynchronization
{
	/// <summary>
	/// The original version of this class included an event handler that used the creation date of the file
	/// as the initial timestamp. But the actual data has the initial timestamp of the time series earlier.
	/// Probably because the file is created lazily after the first datum is captured.
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			int minutes;
			List<TimeSpan> spans;
			LockFreeSynchronizationWorkerController workerController;

			if (args.Length < 2)
			{
				Console.WriteLine("Usage: LockFreeSynchronization.exe <filename> minutes1 minutes2 minutes3...)");
				return;
			}

			// Attempt to open .FIT file
			using (var fitSource = new FileStream(args[0], FileMode.Open))
			{
				Console.WriteLine("Opening {0}", args[0]);
				Decode decodeDemo = new Decode();
				MesgBroadcaster mesgBroadcaster = new MesgBroadcaster();

				bool status = decodeDemo.IsFIT(fitSource);
				status = decodeDemo.CheckIntegrity(fitSource);
				// Process the file
				if (status == true)
				{
					spans = new List<TimeSpan>(args.Length - 1);
					for (int i = 1; i < args.Length; i++)
					{
						if (Int32.TryParse(args[i], out minutes))
							spans.Add(new TimeSpan(0, minutes, 0));
					}
					decodeDemo.MesgEvent += mesgBroadcaster.OnMesg;
					Console.WriteLine("Decoding...");
					workerController = new LockFreeSynchronizationWorkerController(spans.ToArray(), Channel.Power , Channel.Speed , Channel.HeartRate);
					mesgBroadcaster.RecordMesgEvent += new MesgEventHandler(workerController.OnRecordMesg);
					decodeDemo.Read(fitSource);
					Console.WriteLine("Decoded FIT file {0}, Printing reports...", args[0]);
					workerController.PrintReport();
					Console.WriteLine("{0}Done.", Environment.NewLine);
                }
                else
                    Console.WriteLine("Integrity Check Failed {0}", args[0]);
            }
        }
    }
}
