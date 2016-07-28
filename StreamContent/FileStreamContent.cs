using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace StreamContent
{
	public class FileStreamContent
	{
		const int BUFFER_SIZE = 65536;
		private string FilePath { get; set; }
		private long startByte = 0;
		public FileStreamContent(string filepath, long partial = 0)
		{
			FilePath = filepath;
			startByte = partial;
		}

		public async Task WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
		{
			try
			{
				var buffer = new byte [BUFFER_SIZE];

				using (var reader = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					//var length = reader.Length;
					int length = Convert.ToInt32((reader.Length - 1) - startByte) + 1;
					var bytesRead = 1;
					reader.Seek(startByte, SeekOrigin.Begin);
					while (length > 0 && bytesRead > 0)
					{
						bytesRead = await reader.ReadAsync(buffer, 0, Math.Min(length, buffer.Length));
						await outputStream.WriteAsync(buffer, 0, bytesRead);
						length -= bytesRead;
					}
				}
			}
			catch (HttpException)
			{
				return;
			}
			finally
			{
				outputStream.Close();
			}
		}
	}
}
