using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace BscanILL.Export.ExportFiles
{
	class ExportFiles : ObservableCollection<ExportFile>
	{

		public ExportFiles()
		{
		}

		#region Dispose()
		public void Dispose()
		{
			//foreach (ExportFile exportFile in this)
				//exportFile.Delete();
		}
		#endregion
	
	}
}
