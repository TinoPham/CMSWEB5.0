﻿using System;
using CMSWebApi.DataModels;

namespace CMSWebApi.APIFilters
{
	public class ServerErrorException: Exception
	{
		private readonly TransactionalInformation _errorInfo;

		public TransactionalInformation ErrorInfo{
			get { return this._errorInfo; }
		}

		public ServerErrorException() : base(){}

		public ServerErrorException(string format, params object[] args): base(string.Format(format, args)) { }

		public ServerErrorException(TransactionalInformation errorInfo):base()
		{
			this._errorInfo = errorInfo;
		}
		public ServerErrorException(bool returnStatus, CMSWebApi.Utils.CMSWebError errorCode, Exception innerException): this(returnStatus, errorCode.ToString(), innerException)
		{

		}
		public ServerErrorException(bool returnStatus, string errorCode, Exception innerException): base(errorCode, innerException)
		{
			if(_errorInfo == null)
				_errorInfo = new TransactionalInformation();

			_errorInfo.ReturnStatus = returnStatus;
			_errorInfo.ReturnMessage.Add(errorCode);
		}
	}
}
