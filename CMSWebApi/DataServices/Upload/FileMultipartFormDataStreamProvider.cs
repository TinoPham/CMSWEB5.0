using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CMSWebApi.DataServices.Upload
{
	public class FileMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
	{
		public FileMultipartFormDataStreamProvider(string rootPath) : base(rootPath)
		{
		}

		public override string GetLocalFileName(System.Net.Http.Headers.HttpContentHeaders headers)
		{
			var name = !string.IsNullOrWhiteSpace(headers.ContentDisposition.FileName)
				? headers.ContentDisposition.FileName
				: "NoName";
			return name.Trim(new char[] {'"'}).Replace("&", "and");
		}
	}
}
