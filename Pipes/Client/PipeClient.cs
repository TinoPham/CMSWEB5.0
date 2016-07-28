using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;
using Pipes.Utilities;
using System.Threading.Tasks;
using Pipes.Interfaces;
using System.IO;

namespace Pipes.Client
{
	public class PipeClient : ICommunicationClient
	{
		public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnectedEvent;
		public event EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;

		#region private fields
		private volatile bool _isStopping;
		private readonly object _lockingObject = new object();
		private const int BufferSize = 2048;
		private class Info
		{
			public readonly byte[] Buffer;
			public readonly StringBuilder StringBuilder;

			public Info()
			{
				Buffer = new byte[BufferSize];
				StringBuilder = new StringBuilder();
			}
		}
		private readonly NamedPipeClientStream _pipeClient;

		#endregion

		#region c'tor

		public PipeClient(string serverId)
		{
			_pipeClient = new NamedPipeClientStream(".", serverId, PipeDirection.InOut, PipeOptions.Asynchronous);
		}

		#endregion

		#region ICommunicationClient implementation

		/// <summary>
		/// Starts the client. Connects to the server.
		/// </summary>
		public bool Start()
		{
			//const int tryConnectTimeout = 5*60*1000; // 5 minutes
			const int tryConnectTimeout = 1000; // 5 minutes
			try
			{
				_pipeClient.Connect(tryConnectTimeout);
				return true;
			}
			catch(Exception){ return false;}
		}

		/// <summary>
		/// Stops the client. Waits for pipe drain, closes and disposes it.
		/// </summary>
		public void Stop()
		{
			try
			{
				if (_pipeClient.IsConnected)
					_pipeClient.WaitForPipeDrain();
			}
			catch( ObjectDisposedException){}
			catch (NotSupportedException) { }
			catch (IOException) { }
			finally
			{
				_pipeClient.Close();
				_pipeClient.Dispose();
			}
		}

		public Task<TaskResult> SendMessage(string message)
		{
			var taskCompletionSource = new TaskCompletionSource<TaskResult>();

			if (_pipeClient.IsConnected)
			{
				var buffer = Encoding.UTF8.GetBytes(message);
				_pipeClient.BeginWrite(buffer, 0, buffer.Length, asyncResult =>
				{
					try
					{
						taskCompletionSource.SetResult(EndWriteCallBack(asyncResult));
					}
					catch (Exception ex)
					{
						taskCompletionSource.SetException(ex);
					}

				}, null);
			}
			else
			{
				//Logger.Error("Cannot send message, pipe is not connected");
				//throw new IOException("pipe is not connected");
				taskCompletionSource.SetResult( new TaskResult{ IsSuccess = false, ErrorMessage = "Cannot send message, pipe is not connected" });
			}

			return taskCompletionSource.Task;
		}

		#endregion


		#region private methods

		/// <summary>
		/// This callback is called when the BeginWrite operation is completed.
		/// It can be called whether the connection is valid or not.
		/// </summary>
		/// <param name="asyncResult"></param>
		private TaskResult EndWriteCallBack(IAsyncResult asyncResult)
		{
			_pipeClient.EndWrite(asyncResult);
			_pipeClient.Flush();
			BeginRead(new Info());
			return new TaskResult { IsSuccess = true };
		}

		#endregion

		/// <summary>
		/// This method begins an asynchronous read operation.
		/// </summary>
		private void BeginRead(Info info)
		{
			try
			{
				if( _pipeClient != null && _pipeClient.IsConnected)
					_pipeClient.BeginRead(info.Buffer, 0, BufferSize, EndReadCallBack, info);
			}
			catch (Exception)
			{
				//throw;
			}
		}

		/// <summary>
		/// This callback is called when the BeginRead operation is completed.
		/// We can arrive here whether the connection is valid or not
		/// </summary>
		private void EndReadCallBack(IAsyncResult result)
		{
			var readBytes = _pipeClient.EndRead(result);
			if (readBytes > 0)
			{
				var info = (Info)result.AsyncState;

				// Get the read bytes and append them
				info.StringBuilder.Append(Encoding.UTF8.GetString(info.Buffer, 0, readBytes));

				if (_pipeClient.ReadMode == PipeTransmissionMode.Message) // Message is not complete, continue reading
				{
					BeginRead(info);
				}
				else // Message is completed
				{
					// Finalize the received string and fire MessageReceivedEvent
					var message = info.StringBuilder.ToString().TrimEnd('\0');

					OnMessageReceived(message);

					// Begin a new reading operation
					BeginRead(new Info());
				}
			}
			else // When no bytes were read, it can mean that the client have been disconnected
			{
				if (!_isStopping)
				{
					lock (_lockingObject)
					{
						if (!_isStopping)
						{
							OnDisconnected();
							Stop();
						}
					}
				}
			}
		}

		private void OnDisconnected()
		{
			if (ClientDisconnectedEvent != null)
			{
				ClientDisconnectedEvent(this, new ClientDisconnectedEventArgs { ClientId = null });
			}
		}

		private void OnMessageReceived(string message)
		{
			if (MessageReceivedEvent != null)
			{
				MessageReceivedEvent(this,
					new MessageReceivedEventArgs
					{
						Message = message
					});
			}
		}

	}
}
