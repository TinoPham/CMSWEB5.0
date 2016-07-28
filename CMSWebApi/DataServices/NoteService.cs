using System.Linq.Expressions;
using CMSWebApi.ServiceInterfaces;
using PACDMModel.Model;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CMSWebApi.DataServices
{
	public partial class NoteService : ServiceBase, INoteService
	{
		public NoteService(IResposity model)
			: base(model)
		{
		}

		public NoteService(ServiceBase ServiceBase) : base(ServiceBase.DBModel) { }

		public IQueryable<T> GetNote<T>(int userId, Expression<Func<tCMSWeb_NoteItem,T>> selector)
		{
			return Query<tCMSWeb_NoteItem,T>(t=>t.CreatedBy == userId, selector);
		}

		public tCMSWeb_NoteItem GetNoteById(int userId, int noteId)
		{
			return DBModel.FirstOrDefault<tCMSWeb_NoteItem>(t => t.NoteItemID == noteId && t.CreatedBy == userId);
		}

		public void InsertNote(tCMSWeb_NoteItem note)
		{
			DBModel.Insert<tCMSWeb_NoteItem>(note);
		}

		public void EditNote(tCMSWeb_NoteItem note)
		{
			DBModel.Update<tCMSWeb_NoteItem>(note);
		}

		public void DeleteNote(int userId, int noteId)
		{
			DBModel.DeleteWhere<tCMSWeb_NoteItem>(t => t.NoteItemID == noteId && t.CreatedBy == userId);
		}

		public void Save()
		{
			DBModel.Save();
		}
	}
}
