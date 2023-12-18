using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace BscanILL.UI.Dialogs
{

	#region class ComboItemsExportType
	public class ComboItemsExportType
	{

		public static ObservableCollection<ComboItemExportType> GetList()
		{
			ObservableCollection<ComboItemExportType> list = new ObservableCollection<ComboItemExportType>();
			
			foreach (BscanILL.Export.ExportType export in Enum.GetValues(typeof(BscanILL.Export.ExportType)))
				if (export != Export.ExportType.Print && export != Export.ExportType.ArielPatron && export != Export.ExportType.None)
					list.Add(new ComboItemExportType(export));

			return list;
		}
	}
	#endregion
	
	#region class ComboItemExportType
	public class ComboItemExportType
	{
		BscanILL.Export.ExportType value;

		public ComboItemExportType(BscanILL.Export.ExportType value)
		{
			this.value = value;
		}

		public BscanILL.Export.ExportType Value { get { return value; } }

		public override string ToString()
		{
			return BscanILL.Export.Misc.GetExportTypeCaption(value);
		}
	}
	#endregion

}
