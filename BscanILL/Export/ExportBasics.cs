using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BscanILL.Export;
using BscanILL.Misc;

namespace BscanILL.Export
{
	public class ExportBasics
	{
		static protected BscanILL.SETTINGS.Settings	_settings = BscanILL.SETTINGS.Settings.Instance;
		protected Notifications						notifications = Notifications.Instance;

		BscanILL.Export.ILLiad.IILLiad				illiad = null;

		public event ProcessSuccessfullHnd			UploadSuccessfull;
		public event ProcessErrorHnd				UploadError;
		public event ProgressChangedHnd				UploadProgressChanged;
		public event ProgressDescriptionHnd			UploadDescription;

		public event ProgressChangedHandle ProgressChanged;
		public event ProgressCommentHandle ProgressComment;


		#region constructor
		public ExportBasics()
		{
		}
		#endregion


		// PROTECTED PROPERTIES
		#region protected properties
		protected bool CheckingArticleInDb { get { return _settings.Export.ILLiad.SqlEnabled; } }

		protected BscanILL.Export.ILLiad.IILLiad Illiad
		{
			get
			{
				if (this.illiad == null)
				{
					illiad = BscanILL.Export.ILLiad.ILLiadBasics.GetIlliadInstance();

					illiad.ProgressChanged += new ProgressChangedHandle(this.Progress_Changed);
					illiad.ProgressComment += new ProgressCommentHandle(this.Progress_Comment);
				}

				return this.illiad;
			}
		}
		#endregion


		// PROTECTED METHODS
		#region protected methods

		#region Upload_Successfull()
		protected void Upload_Successfull(BscanILL.Export.ExportUnit exportUnit)
		{
			if (UploadSuccessfull != null)
				UploadSuccessfull(exportUnit);
		}
		#endregion

		#region Upload_Error()
		protected void Upload_Error(BscanILL.Export.ExportUnit exportUnit, Exception ex)
		{
			if (UploadError != null)
				UploadError(exportUnit, ex);
		}
		#endregion

		#region Upload_ProgressAndDescription()
		protected void Upload_ProgressAndDescription(float progress, string description)
		{
			if (UploadProgressChanged != null)
				UploadProgressChanged(progress);

			if (UploadDescription != null)
				UploadDescription(description);
		}
		#endregion

		#region Upload_ProgressChanged()
		protected void Upload_ProgressChanged(float progress)
		{
			if (UploadProgressChanged != null)
				UploadProgressChanged(progress);
		}
		#endregion

		#region Upload_Description()
		protected void Upload_Description(string description)
		{
			if (UploadDescription != null)
				UploadDescription(description);
		}
		#endregion

		#region Progress_Changed()
		protected void Progress_Changed(ExportUnit exportUnit, int progress, string description)
		{
			if (ProgressChanged != null)
				ProgressChanged(exportUnit, progress, description);
		}
		#endregion

		#region Progress_Comment()
		protected void Progress_Comment(ExportUnit exportUnit, string comment)
		{
			if (ProgressComment != null)
				ProgressComment(exportUnit, comment);
		}
		#endregion

		#region Notify()
		protected void Notify(object sender, BscanILL.Misc.Notifications.Type type, string message, Exception ex)
		{
			notifications.Notify(sender, type, message, ex);
		}
		#endregion

		#endregion

	}
}
