using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.ModelBinding;
using CMSWebApi.APIFilters;
using CMSWebApi.DataModels;
using CMSWebApi.Utils;

namespace CMSWebApi.BusinessServices.FilesManager
{
	public class FilesManager : IFilesManager
	{
		private string _workingFolder;

		public FilesManager()
		{
		}

		public FilesManager(string workingFolder)
		{
			_workingFolder = workingFolder;

			CheckTargetDirectory();
		}

		public void SetWorkingFolder(string path)
		{
			if (!Directory.Exists(path))
			{
				if (!CMSWebApi.Utils.Utilities.CreateDir(path))
				{
					throw new CmsErrorException(CMSWebError.WORKING_FOLDER_CANNOT_SET.ToString(), _workingFolder);
				}
			}
			_workingFolder = path;
		}

		public async Task<IEnumerable<FileModel>> Get()
		{
			var files = new List<FileModel>();
			var fileFolder = new DirectoryInfo(this._workingFolder);

			await Task.Factory.StartNew(() =>
			{
				var filesread = fileFolder.EnumerateFiles().ToList();

				filesread.ForEach(f =>
				{
					var data = File.ReadAllBytes(f.FullName);
					var fm = new FileModel()
					{
						Name = f.Name,
						CreatedOn = f.CreationTime,
						Modified = f.LastWriteTime,
						Size = f.Length,
						Data = data,
						ExFile = f.Extension,
						Path = ""//f.DirectoryName
					};
					files.Add(fm);
				});
			});
			return files;
		}

		public async Task<FileModel> GetByName(string fileName)
		{
			var file = new FileModel();
			var fileFolder = new DirectoryInfo(this._workingFolder);
			await Task.Factory.StartNew(() =>
			{
				file =
					fileFolder.EnumerateFiles()
						.Where(f => f.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
						.Select(f => new FileModel()
						{
							Name = f.Name,
							CreatedOn = f.CreationTime,
							Modified = f.LastWriteTime,
							Size = f.Length,
							Data = File.ReadAllBytes(f.FullName),
							ExFile = f.Extension,
							Path = f.DirectoryName
						}).FirstOrDefault();
			});
			return file;
		}

		public async Task<bool> TryMoveFolder(string folderFrom, string folderTo)
		{
			try
			{
				return await Task.Factory.StartNew(() =>
				{
					if (folderFrom == null || folderTo == null)
					{
						return false;
					}
					var pathFrom = Path.Combine(_workingFolder, folderFrom);
					var pathTo = Path.Combine(_workingFolder, folderTo);
					Directory.Move(pathFrom, pathTo);
					return true;
				});
			}
			catch (Exception)
			{
				return false;
			}
		}

		public async Task<bool> TryDeleteFolder(string folderName)
		{
			try
			{
				return await Task.Factory.StartNew(() =>
				{
					if (folderName == null)
					{
						return false;
					}
					var pathToDelete = Path.Combine(_workingFolder, folderName);
					Directory.Delete(pathToDelete, true);
					return true;
				});
			}
			catch (Exception)
			{
				return false;
			}
		}

		public async Task<bool> TryDelete(string fileName)
		{
			try
			{
				var filePath = Directory.GetFiles(this._workingFolder, fileName).FirstOrDefault();

				return await Task.Factory.StartNew(() =>
				{
					if (filePath == null)
					{
						return false;
					}

					File.Delete(filePath);
					return true;
				});
			}
			catch (Exception)
			{
				return false;
			}
		}

		public bool DeleteFile(string fileName)
		{
			var filePath = Path.Combine(_workingFolder, fileName);
			return CMSWebApi.Utils.Utilities.DeleteFile(filePath);
		}

		public bool DeleteFolder(string folder)
		{
			var filePath = Path.Combine(_workingFolder, folder);
			return CMSWebApi.Utils.Utilities.DeleteFolder(filePath);
		}

		public async void Delete(string fileName)
		{
			try
			{
				var filePath = Directory.GetFiles(this._workingFolder, fileName).FirstOrDefault();

				await Task.Factory.StartNew(() =>
				{
					if (filePath == null)
					{
						throw new CmsErrorException(CMSWebError.WORKING_FOLDER_CANNOT_FOUND.ToString(), "the destination path " + this._workingFolder + " could not be found");
					}

					File.Delete(filePath);
				});
			}
			catch (Exception)
			{
				throw new CmsErrorException(CMSWebError.WORKING_FILE_CANNOT_DELETE.ToString(), "Delete " + fileName + " failed");
			}
		}

		public async Task Add(string fileName, byte[] dataBytes)
		{
			string path = Path.Combine(this._workingFolder, fileName);

		     await WriteFile(path, dataBytes);
		}

		private async Task WriteFile(string fpath, byte[] mem)
		{
			if (mem == null || mem.Length == 0 || string.IsNullOrEmpty(fpath))
			{
				throw new CmsErrorException(CMSWebError.DATA_NOT_FOUND.ToString());
			}

			if (File.Exists(fpath))
			{
				throw new CmsErrorException(CMSWebError.WORKING_FILE_EXIST.ToString(), fpath);
			}

			using (var fs = new FileStream(fpath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
			{
				await fs.WriteAsync(mem, 0, mem.Length);
				fs.Close();
			}
		}

		private void CheckTargetDirectory()
		{
			if (!Directory.Exists(this._workingFolder))
			{
				throw new CmsErrorException(CMSWebError.DATA_NOT_FOUND.ToString(), _workingFolder);
			}
		}
	}
}
