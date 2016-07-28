using System;
using System.Collections;
using System.Collections.Generic;
using CMSWebApi.DataModels;
using CMSWebApi.Utils;
using Microsoft.SqlServer.Server;

namespace CMSWebApi.APIFilters
{
	public class CmsErrorException: Exception
	{
		private readonly TransactionalInformation _errorInfo;

		private readonly IDictionary _data = new Dictionary<string, object>();
		public override IDictionary Data
		{
			get { return _data;}
		}

		private readonly object _errorObj = new object();
		public object ObjectError
		{
			get { return _errorObj; }
		}

		public override string Source { get; set; }

		public TransactionalInformation ErrorInfo{
			get { return this._errorInfo; }
		}

		public CmsErrorException() : base(){}

		public CmsErrorException(string format, params object[] args): base(string.Format(format, args)) { }

		public CmsErrorException(TransactionalInformation errorInfo):base()
		{
			this._errorInfo = errorInfo;
		}

		public CmsErrorException(CMSWebError error) : this(error.ToString())
		{

		}
		
		public CmsErrorException(string errorCode)
		{
			if (_errorInfo == null)
				_errorInfo = new TransactionalInformation();

			_errorInfo.ReturnStatus = false;
			_errorInfo.ReturnMessage.Add(errorCode);
		}

		public CmsErrorException(string errorCode, string msg): base(msg)
		{
			if (_errorInfo == null)
				_errorInfo = new TransactionalInformation();

			_errorInfo.ReturnStatus = false;
			_errorInfo.ReturnMessage.Add(errorCode);
		}

		public CmsErrorException(string errorCode, object errorObj, string msg = ""): base(msg)
		{
			if (_errorInfo == null)
				_errorInfo = new TransactionalInformation();

			_errorInfo.ReturnStatus = false;
			_errorInfo.ReturnMessage.Add(errorCode);

			_errorObj = (object)errorObj;
		}

		public void AddErrorsMessageData(string key, string msg)
		{
			_data.Add(key, msg);
		}

		public CmsErrorException(string errorCode, string fieldName, string dataError)
		{
			if (_errorInfo == null)
				_errorInfo = new TransactionalInformation();

			_errorInfo.ReturnStatus = false;
			_errorInfo.ReturnMessage.Add(errorCode);
			_errorInfo.ValidationErrors.Add(fieldName, dataError);
		}

		public CmsErrorException(bool returnStatus, string errorCode)
		{
			if(_errorInfo == null)
				_errorInfo = new TransactionalInformation();

			_errorInfo.ReturnStatus = returnStatus;
			_errorInfo.ReturnMessage.Add(errorCode);
		}
	}
}
