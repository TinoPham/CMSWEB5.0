using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pipes.Utilities;

namespace Pipes.Interfaces
{
	public interface ICommunicationClient : ICommunication
	{
		/// <summary>
		/// This method sends the given message asynchronously over the communication channel
		/// </summary>
		/// <param name="message"></param>
		/// <returns>A task of TaskResult</returns>
		Task<TaskResult> SendMessage(string message);
	}
}
