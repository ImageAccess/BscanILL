using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BscanILL.Misc;

namespace BscanILL.Export.ExportFiles
{
	public class ExportBasics
	{
		protected BscanILL.SETTINGS.Settings _settings = BscanILL.SETTINGS.Settings.Instance;
		protected Notifications notifications = Notifications.Instance;

		public event BscanILL.Export.ProcessSuccessfullHnd UploadSuccessfull;
		public event BscanILL.Export.ProcessErrorHnd UploadError;
		public event BscanILL.Export.ProgressChangedHnd UploadProgressChanged;
		public event BscanILL.Export.ProgressDescriptionHnd UploadDescription;


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

		#region Reset()
		public virtual void Reset()
		{
		}
		#endregion

	}
}
