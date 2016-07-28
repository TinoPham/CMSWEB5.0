using System;
using System.Linq;
using CMSWebApi.APIFilters;
using CMSWebApi.DataModels;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using PACDMModel.Model;

namespace CMSWebApi.BusinessServices.Notes
{
	public class NoteBusinessService : BusinessBase<INoteService>
	{
		public IQueryable<Note> GetNote(int userId, int id = 0)
		{
			var notes = DataService.GetNote(userId, t => new Note()
			{
				Id = t.NoteItemID,
				UserId = t.CreatedBy,
				Content = t.NoteItemText,
				CreatedOn = t.CreatedDate
			});
			return id == 0 ? notes : notes.Where(t => t.Id == id);
		}

		public Note InsertNote(int userId, Note note)
		{
			note.UserId = userId;
			note.CreatedOn = DateTime.Now;

			var newNote = new tCMSWeb_NoteItem()
			{
				CreatedBy = note.UserId,
				CreatedDate = note.CreatedOn,
				NoteItemText = note.Content
			};

			DataService.InsertNote(newNote);
			DataService.Save();
			note.Id = newNote.NoteItemID;
			return note;
		}

		public void EditNote(int userId, Note note)
		{
			var updateNotes = DataService.GetNoteById(userId, note.Id);

			if (updateNotes == null)
			{
				throw new CmsErrorException(CMSWebError.DATA_NOT_FOUND.ToString());
			}

			updateNotes.NoteItemText = note.Content;
			DataService.EditNote(updateNotes);
			DataService.Save();
		}

		public void DeleteNote(int userId, int noteId)
		{
			DataService.DeleteNote(userId, noteId);
			DataService.Save();
		}

	}
}
