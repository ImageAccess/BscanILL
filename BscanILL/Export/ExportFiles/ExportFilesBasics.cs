using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.Export.ExportFiles
{
	public class ExportFilesBasics
	{
		protected BscanILL.SETTINGS.Settings _settings = BscanILL.SETTINGS.Settings.Instance;
		protected BscanILL.Misc.Notifications notifications = BscanILL.Misc.Notifications.Instance;

		public event ProcessSuccessfullHnd	OperationSuccessfull;
		public event ProcessErrorHnd		OperationError;
		public event ProgressChangedHnd		ProgressChanged;
		public event ProgressDescriptionHnd DescriptionChanged;


		#region Operation_Successfull()
		protected void Operation_Successfull(BscanILL.Export.ExportUnit exportUnit)
		{
			if (OperationSuccessfull != null)
				OperationSuccessfull(exportUnit);
		}
		#endregion

		#region Operation_Error()
		protected void Operation_Error(BscanILL.Export.ExportUnit exportUnit, Exception ex)
		{
			if (OperationError != null)
				OperationError(exportUnit, ex);
		}
		#endregion

		#region Progress_Changed()
		protected void Progress_Changed(double progress)
		{
			if (ProgressChanged != null)
				ProgressChanged(progress);
		}
		#endregion

		#region Description_Changed()
		protected void Description_Changed(string description)
		{
			if (DescriptionChanged != null)
				DescriptionChanged(description);
		}
		#endregion

	}

}
