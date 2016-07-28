using System;
using System.Linq;
using System.Linq.Expressions;
using PACDMModel.Model;

namespace CMSWebApi.ServiceInterfaces
{
	public interface INoteService
	{
		IQueryable<T> GetNote<T>(int userId, Expression<Func<tCMSWeb_NoteItem, T>> selector);
		tCMSWeb_NoteItem GetNoteById(int userId, int noteId);
		void InsertNote(tCMSWeb_NoteItem note);
		void EditNote(tCMSWeb_NoteItem note);
		void DeleteNote(int userId, int noteId);
		void Save();
	}
}
