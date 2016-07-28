using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipes.Interfaces
{
	public interface ICommunication
	{
		/// <summary>
		/// Starts the communication channel
		/// </summary>
		bool Start();

		/// <summary>
		/// Stops the communication channel
		/// </summary>
		void Stop();
	}
}
