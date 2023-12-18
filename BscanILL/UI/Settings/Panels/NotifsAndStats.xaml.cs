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

namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for NotifsAndStats.xaml
	/// </summary>
	public partial class NotifsAndStats : PanelBase
	{

		#region constructor
		public NotifsAndStats()
		{
			InitializeComponent();

			this.DataContext = this;
		}
		#endregion

		//PUBLIC PROPERTIES
		#region public properties

		#region SaveToEventLog
		public bool SaveToEventLog
		{
			get { return _settings.StatsAndNotifs.LogToEventLog; }
			set
			{
				_settings.StatsAndNotifs.LogToEventLog = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region AdminEmails
		public string AdminEmails
		{
			get { return _settings.StatsAndNotifs.AdditionalAdminRecipients; }
			set
			{
				_settings.StatsAndNotifs.AdditionalAdminRecipients = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region StatsRecipients
		public string StatsRecipients
		{
			get { return  _settings.StatsAndNotifs.AdditionalStatsRecipients; }
			set
			{
				 _settings.StatsAndNotifs.AdditionalStatsRecipients = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region TimeToSendStr
		public string TimeToSendStr
		{
			get 
			{
				try
				{
					List<DateTime> timesToSend = _settings.StatsAndNotifs.TimesToSend;
					
					string timeToSendStr = timesToSend.Count > 0 ? timesToSend[0].ToString("HH:mm") : "23:30";

					for (int i = 1; i < timesToSend.Count; i++)
						timeToSendStr += ", " + timesToSend[i].ToString("HH:mm");

					return timeToSendStr;
				}
				catch
				{
					return "23:30";
				}
			}
			set
			{
				try
				{
					_settings.StatsAndNotifs.TimesToSend = BscanILL.SETTINGS.Settings.StatsAndNotifsClass.ParseTimes(value);
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
				catch { }
			}
		}
		#endregion

		#region SendToDlsg
		public bool SendToDlsg
		{
			get { return _settings.StatsAndNotifs.SendToDLSG; }
			set
			{
				_settings.StatsAndNotifs.SendToDLSG = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods
		#endregion


		//PRIVATE METHODS
		#region private methods
		#endregion

	}
}
