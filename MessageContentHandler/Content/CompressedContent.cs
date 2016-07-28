using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Commons;
using Commons.Resources;

namespace MessageContentHandler.Content
{
	internal class CompressedContent : HttpContent
	{
		private HttpContent originalContent;
		private string encodingType;

		public CompressedContent(HttpContent content, string encodingType)
		{
			originalContent = content;
			this.encodingType = encodingType;

			if (string.Compare(encodingType, HttpConstant.STR_gzip, true) != 0 && string.Compare(encodingType, HttpConstant.STR_deflate, true) != 0)
			{
				string.Format(ResourceManagers.Instance.GetResourceString(Commons.ERROR_CODE.INVALID_COMPRESSION_REQUEST), this.encodingType);
				throw new InvalidOperationException();
			}

			foreach (KeyValuePair<string, IEnumerable<string>> header in originalContent.Headers)
			{
				this.Headers.Add(header.Key, header.Value);
			}

			this.Headers.ContentEncoding.Add(encodingType);
		}

		protected override bool TryComputeLength(out long length)
		{
			length = -1;

			return false;
		}

		protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
		{
			Stream compressedStream = null;

			if (string.Compare(encodingType, HttpConstant.STR_gzip, true) == 0)
			{
				compressedStream = GZipStream(stream);
			}
			else if (string.Compare(encodingType, HttpConstant.STR_deflate, true) == 0)
			{
				compressedStream = DeflateStream(stream);
			}

			return originalContent.CopyToAsync(compressedStream).ContinueWith(tsk =>
			{
				if (compressedStream != null)
				{
					compressedStream.Dispose();
					compressedStream = null;
				}
			});
		}

		private Stream DeflateStream(Stream input)
		{
			return new DeflateStream(input, CompressionMode.Compress, leaveOpen: true);
		}
		private Stream GZipStream(Stream input)
		{
			return new GZipStream(input, CompressionMode.Compress, leaveOpen: true);
		}

	}
}
