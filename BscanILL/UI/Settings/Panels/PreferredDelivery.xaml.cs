using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using BscanILL.Export.ILL;

namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for PreferredDelivery.xaml
	/// </summary>
	public partial class PreferredDelivery : PanelBase
	{
		
		#region PreferredDelivery()
		public PreferredDelivery()
		{
			InitializeComponent();

			List<ComboBox> list = new List<ComboBox>();
			GetAllComboBoxes(this.mainGrid, ref list);

			foreach (ComboBox comboBox in list)
			{
				comboBox.Items.Add(new ComboItemExportType(BscanILL.Export.ExportType.None));
				comboBox.Items.Add(new ComboItemExportType(BscanILL.Export.ExportType.Odyssey));
				comboBox.Items.Add(new ComboItemExportType(BscanILL.Export.ExportType.Ariel));
				comboBox.Items.Add(new ComboItemExportType(BscanILL.Export.ExportType.Email));
				comboBox.Items.Add(new ComboItemExportType(BscanILL.Export.ExportType.Ftp));
				comboBox.Items.Add(new ComboItemExportType(BscanILL.Export.ExportType.SaveOnDisk));
			}

			this.DataContext = this;
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
				switch (value)
				{
					case BscanILL.Export.ExportType.None: return "Bscan ILL Selection";
					case BscanILL.Export.ExportType.Odyssey: return "Odyssey";
					case BscanILL.Export.ExportType.Ariel: return "Ariel";
					case BscanILL.Export.ExportType.ArielPatron: return "Ariel";
					case BscanILL.Export.ExportType.Email: return "Email";
					case BscanILL.Export.ExportType.Ftp: return "FTP";
					case BscanILL.Export.ExportType.FtpDir: return "FTP Directory";
					case BscanILL.Export.ExportType.ILLiad: return "ILLiad";
					case BscanILL.Export.ExportType.SaveOnDisk: return "Save on Disk";
				}

				return value.ToString();
			}
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region SelectedItemPP1
		public ComboItemExportType SelectedItemPP1
		{
			get { return GetSelectedComboItem(comboPP1, _settings.Export.ILLiad.PreferredExportPP1); }
			set { SetSettings(value, _settings.Export.ILLiad.PreferredExportPP1, MethodBase.GetCurrentMethod().Name); }
		}
		#endregion

		#region SelectedItemPP2
		public ComboItemExportType SelectedItemPP2
		{
			get { return GetSelectedComboItem(comboPP2, _settings.Export.ILLiad.PreferredExportPP2); }
			set { SetSettings(value, _settings.Export.ILLiad.PreferredExportPP2, MethodBase.GetCurrentMethod().Name); }
		}
		#endregion

		#region SelectedItemPP3
		public ComboItemExportType SelectedItemPP3
		{
			get { return GetSelectedComboItem(comboPP3, _settings.Export.ILLiad.PreferredExportPP3); }
			set { SetSettings(value, _settings.Export.ILLiad.PreferredExportPP3, MethodBase.GetCurrentMethod().Name); }
		}
		#endregion

		#region SelectedItemPN1
		public ComboItemExportType SelectedItemPN1
		{
			get { return GetSelectedComboItem(comboPN1, _settings.Export.ILLiad.PreferredExportPN1); }
			set { SetSettings(value, _settings.Export.ILLiad.PreferredExportPN1, MethodBase.GetCurrentMethod().Name); }
		}
		#endregion

		#region SelectedItemPN2
		public ComboItemExportType SelectedItemPN2
		{
			get { return GetSelectedComboItem(comboPN2, _settings.Export.ILLiad.PreferredExportPN2); }
			set { SetSettings(value, _settings.Export.ILLiad.PreferredExportPN2, MethodBase.GetCurrentMethod().Name); }
		}
		#endregion

		#region SelectedItemPN3
		public ComboItemExportType SelectedItemPN3
		{
			get { return GetSelectedComboItem(comboPN3, _settings.Export.ILLiad.PreferredExportPN3); }
			set { SetSettings(value, _settings.Export.ILLiad.PreferredExportPN3, MethodBase.GetCurrentMethod().Name); }
		}
		#endregion

		#region SelectedItemNP1
		public ComboItemExportType SelectedItemNP1
		{
			get { return GetSelectedComboItem(comboNP1, _settings.Export.ILLiad.PreferredExportNP1); }
			set { SetSettings(value, _settings.Export.ILLiad.PreferredExportNP1, MethodBase.GetCurrentMethod().Name); }
		}
		#endregion

		#region SelectedItemNP2
		public ComboItemExportType SelectedItemNP2
		{
			get { return GetSelectedComboItem(comboNP2, _settings.Export.ILLiad.PreferredExportNP2); }
			set { SetSettings(value, _settings.Export.ILLiad.PreferredExportNP2, MethodBase.GetCurrentMethod().Name); }
		}
		#endregion

		#region SelectedItemNP3
		public ComboItemExportType SelectedItemNP3
		{
			get { return GetSelectedComboItem(comboNP3, _settings.Export.ILLiad.PreferredExportNP3); }
			set { SetSettings(value, _settings.Export.ILLiad.PreferredExportNP3, MethodBase.GetCurrentMethod().Name); }
		}
		#endregion

		#region SelectedItemNN1
		public ComboItemExportType SelectedItemNN1
		{
			get { return GetSelectedComboItem(comboNN1, _settings.Export.ILLiad.PreferredExportNN1); }
			set { SetSettings(value, _settings.Export.ILLiad.PreferredExportNN1, MethodBase.GetCurrentMethod().Name); }
		}
		#endregion

		#region SelectedItemNN2
		public ComboItemExportType SelectedItemNN2
		{
			get { return GetSelectedComboItem(comboNN2, _settings.Export.ILLiad.PreferredExportNN2); }
			set { SetSettings(value, _settings.Export.ILLiad.PreferredExportNN2, MethodBase.GetCurrentMethod().Name); }
		}
		#endregion

		#region SelectedItemNN3
		public ComboItemExportType SelectedItemNN3
		{
			get { return GetSelectedComboItem(comboNN3, _settings.Export.ILLiad.PreferredExportNN3); }
			set { SetSettings(value, _settings.Export.ILLiad.PreferredExportNN3, MethodBase.GetCurrentMethod().Name); }
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods
		#endregion


		//PRIVATE METHODS
		#region private methods

		#region GetSelectedComboItem()
		private static ComboItemExportType GetSelectedComboItem(ComboBox combo, BscanILL.Export.ExportType exportTypeInSettings)
		{
			foreach (ComboItemExportType item in combo.Items)
				if (item.Value == exportTypeInSettings)
					return item;

			return null;
		}
		#endregion

		#region SetSettings()
		private void SetSettings(ComboItemExportType comboItem, BscanILL.Export.ExportType exportTypeToSet, string propertyName)
		{
			if (comboItem != null)
				exportTypeToSet = comboItem.Value;

			RaisePropertyChanged(propertyName);
		}
		#endregion

		#region GetAllComboBoxes()
		private void GetAllComboBoxes(FrameworkElement uiElement, ref List<ComboBox> list)
		{
			if (uiElement is ComboBox)
				list.Add((ComboBox)uiElement);
			else
			{
				foreach (object child in LogicalTreeHelper.GetChildren(uiElement))
					if(child is FrameworkElement)
						GetAllComboBoxes((FrameworkElement)child, ref list);
			}
		}
		#endregion

		#endregion

	}
}
