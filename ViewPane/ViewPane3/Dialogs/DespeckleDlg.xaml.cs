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
using System.Windows.Shapes;
using System.ComponentModel;

namespace ViewPane.Dialogs
{
	/// <summary>
	/// Interaction logic for DespeckleDlg.xaml
	/// </summary>
	public partial class DespeckleDlg : Window, INotifyPropertyChanged
	{
		List<ComboDespeckleModeItem> despeckleModes = new List<ComboDespeckleModeItem>();
		ImageProcessing.NoiseReduction.DespeckleMode despeckleMode = ImageProcessing.NoiseReduction.DespeckleMode.WhiteSpecklesOnly;

		public event PropertyChangedEventHandler PropertyChanged;



		#region constructor
		private DespeckleDlg()
		{
			InitializeComponent();

			despeckleModes.Add(new ComboDespeckleModeItem(ViewPane.Languages.UiStrings.BothBlackAndWhiteSpeckles_STR, ImageProcessing.NoiseReduction.DespeckleMode.BothColors));
			despeckleModes.Add(new ComboDespeckleModeItem(ViewPane.Languages.UiStrings.BlackSpecklesOnly_STR, ImageProcessing.NoiseReduction.DespeckleMode.BlackSpecklesOnly));
			despeckleModes.Add(new ComboDespeckleModeItem(ViewPane.Languages.UiStrings.WhiteSpecklesOnly_STR, ImageProcessing.NoiseReduction.DespeckleMode.WhiteSpecklesOnly));

			this.DataContext = this;
		}

		public DespeckleDlg(ImageProcessing.NoiseReduction.DespeckleSize maskSize, ImageProcessing.NoiseReduction.DespeckleMode despeckleMode)
			:this()
		{
			this.MaskSize = maskSize;
			this.DespeckleMode = despeckleMode;
		}
		#endregion

		#region class ComboDespeckleModeItem
		public class ComboDespeckleModeItem
		{
			public string Caption { get; set; }
			public ImageProcessing.NoiseReduction.DespeckleMode DespeckleMode { get; set; }

			public ComboDespeckleModeItem(string description, ImageProcessing.NoiseReduction.DespeckleMode despeckleMode)
			{
				this.Caption = description;
				this.DespeckleMode = despeckleMode;
			}
		}	
		#endregion

		//PUBLIC PROPERTIES
		#region public properties

		public List<ComboDespeckleModeItem> DespeckleModes { get { return this.despeckleModes; } set { this.despeckleModes = value; } }

		#region SelectedDespeckleMode
		public ComboDespeckleModeItem SelectedDespeckleMode
		{
			get
			{
				foreach (ComboDespeckleModeItem despeckleMode in despeckleModes)
					if (despeckleMode.DespeckleMode == this.despeckleMode)
						return despeckleMode;

				return null;
			}
			set
			{
				if (value != null)
				{
					this.despeckleMode = value.DespeckleMode;
					RaisePropertyChanged("SelectedDespeckleMode");
				}
			}
		}		 
		#endregion

		#region MaskSize
		public ImageProcessing.NoiseReduction.DespeckleSize MaskSize
		{
			get { return (ImageProcessing.NoiseReduction.DespeckleSize)(this.combo.SelectedIndex + 1); }
			set { this.combo.SelectedIndex = (int)value - 1; }
		}		
		#endregion

		#region DespeckleMode
		public ImageProcessing.NoiseReduction.DespeckleMode DespeckleMode
		{
			get
			{
				return this.despeckleMode;

			}
			set
			{
				this.despeckleMode = value;
				RaisePropertyChanged("DespeckleMode");
				RaisePropertyChanged("SelectedDespeckleMode");
			}
		}
		#endregion

		#endregion

		//PUBLIC METHODS
		#region public methods
		#endregion

		//PRIVATE METHODS
		#region private methods

		#region Ok_Click
		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
		#endregion

		#region RaisePropertyChanged()
		private void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

		#endregion

	}
}
