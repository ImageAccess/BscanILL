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
using System.Speech.Synthesis;
using System.Collections.ObjectModel;
using System.Reflection;

namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for Export.xaml
	/// </summary>
	public partial class Export : PanelBase
	{		

		#region constructor
		public Export()
		{
			InitializeComponent();

			//voices
			SpeechSynthesizer speechEngine = new SpeechSynthesizer();

			foreach (InstalledVoice voice in speechEngine.GetInstalledVoices())
				if (voice.Enabled)
				{
					this.comboMp3.Items.Add(voice);

					if (voice.VoiceInfo.Name == _settings.Export.Audio.DefaultVoice)
						this.comboMp3.SelectedItem = voice;
				}

			this.DataContext = _settings.Export;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#endregion


		//PUBLIC METHODS
		#region public methods
	
		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Voice_SelectionChanged()
		private void Voice_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (comboMp3.SelectedItem != null && comboMp3.SelectedItem is InstalledVoice)
				_settings.Export.Audio.DefaultVoice = ((InstalledVoice)comboMp3.SelectedItem).VoiceInfo.Name;
		}
		#endregion

		#endregion

	}
}
