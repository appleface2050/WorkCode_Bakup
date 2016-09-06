using System;
using System.Diagnostics;
namespace test
{
	public class test
	{
		public static void Main(string[] args)
		{
			try
			{
				EventLog ev = new EventLog("Application", System.Environment.MachineName);

				for (int i = ev.Entries.Count - 1; i >= 0; i--)
				{
					EventLogEntry CurrentEntry = ev.Entries[i];
					if (CurrentEntry.EntryType.ToString().Equals("Error"))
					{
						if (CurrentEntry.Source.Equals("MsiInstaller"))
						{
							/*
							 * Don't show very old event logs
							 */
							if (DateTime.Now.Subtract(CurrentEntry.TimeGenerated) > new TimeSpan(1, 0, 0))
								break;

							Console.WriteLine("MSI error Message :  " + CurrentEntry.Message + "\n");
							string failReason = CurrentEntry.Message.Substring(CurrentEntry.Message.IndexOf("--") + 3);
							Console.WriteLine(failReason);
							break;
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}
	}
}
