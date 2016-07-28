using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;

namespace AppUpgrade.Pipes
{
	public delegate void DelegateMessage(string Reply);
	internal class PipeServer
	{
		public event DelegateMessage PipeMessage;
		string _pipeName;
		public bool Listen(string PipeName)
		{
			try
			{
				// Set to class level var so we can re-use in the async callback method
				_pipeName = PipeName;
				// Create the new async pipe 
				NamedPipeServerStream pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

				// Wait for a connection
				pipeServer.BeginWaitForConnection(new AsyncCallback(WaitForConnectionCallBack), pipeServer);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private void WaitForConnectionCallBack(IAsyncResult iar)
		{
			try
			{
				// Get the pipe
				NamedPipeServerStream pipeServer = (NamedPipeServerStream)iar.AsyncState;
				// End waiting for the connection
				pipeServer.EndWaitForConnection(iar);

				byte[] buffer = new byte[255];

				// Read the incoming message
				pipeServer.Read(buffer, 0, 255);

				// Convert byte buffer to string
				string stringData = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
				
				// Pass message back to calling form
				PipeMessage.Invoke(stringData);

				// Kill original sever and create new wait server
				pipeServer.Close();
				pipeServer = null;
				//pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

				// Recursively wait for the connection again and again....
				//pipeServer.BeginWaitForConnection(new AsyncCallback(WaitForConnectionCallBack), pipeServer);
			}
			catch
			{
				return;
			}
		}
	}
}
