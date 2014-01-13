using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IRCBotv2.Daemons
{
	public static class QueueProcessor
	{
		public static void Start(Queue<Task> queue)
		{
			var nextActionTime = DateTime.UtcNow;

			while (true)
			{
				if (queue.Count > 0 && DateTime.Compare(DateTime.UtcNow, nextActionTime) > 0)
				{
					var task = queue.Dequeue();

					task.Start();

					nextActionTime = DateTime.UtcNow.Add(TimeSpan.FromSeconds(2));
				}

				Thread.Sleep(100);
			}
		}
	}
}
