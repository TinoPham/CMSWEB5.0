using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CMSWebApi.DataModels;

namespace CMSWebApi.BusinessServices.FilesManager
{
	public interface IFilesManager
	{
		void SetWorkingFolder(string path);
		Task<IEnumerable<FileModel>> Get();
		Task<FileModel> GetByName(string fileName);
		bool DeleteFile(string fileName);
		bool DeleteFolder(string folder);
		Task<bool> TryMoveFolder(string folderFrom, string folderTo);
		Task<bool> TryDeleteFolder(string folderName);
		Task<bool> TryDelete(string fileName);
		void Delete(string fileName);
		Task Add(string fileName, byte[] dataBytes);
	}
}
