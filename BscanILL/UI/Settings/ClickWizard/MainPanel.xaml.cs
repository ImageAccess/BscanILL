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
using System.Drawing;

namespace BscanILL.UI.Settings.ClickWizard
{
	/// <summary>
	/// Interaction logic for MainPanel.xaml
	/// </summary>
	public partial class MainPanel : UserControl
	{
		BscanILL.Settings.Settings settings = BscanILL.Settings.Settings.Instance;
		

		public event LoadScannerHnd		LoadScannerClick;
#pragma warning disable 0067
		public event ClickHnd			TurnVideoOnClick;
		public event ClickHnd			TurnVideoOffClick;
#pragma warning restore 0067
		public event ScanHnd			TestScanClick;
		public event ClickHnd			FindFocusClick;
		public event ClickHnd			SeekRegPointsClick;
		public event ClickHnd			FindLaserClick;
		public event WhiteBalanceHnd	WhiteBalanceClick;
		public event ClickHnd			LightsDistributionClick;
		public event ClickHnd			ResetLightsDistributionClick;
		public event ClickHnd			LightsOnClick;
		public event ClickHnd			LightsOffClick;
		public event ClickHnd			LaserOnClick;
		public event ClickHnd			LaserOffClick;
		public event ClickHnd			SwapCamerasClick;
		public event ClickHnd			CamerasSettingsClick;
		public event ClickHnd			CamerasDefaultSettingsClick;
		public event ClickHnd			SaveAndCloseClick;


		#region constructor
		public MainPanel()
		{
			InitializeComponent();

			this.panel01Init.LoadScannerClick += delegate(string comPort) { LoadScannerClick(comPort); };

			this.panel02InitResult.LightsOnClick += delegate() { LightsOnClick(); };
			this.panel02InitResult.LightsOffClick += delegate() { LightsOffClick(); };
			this.panel02InitResult.LaserOnClick += delegate() { LaserOnClick(); };
			this.panel02InitResult.LaserOffClick += delegate() { LaserOffClick(); };
			this.panel02InitResult.SwapCamerasClick += delegate() { SwapCamerasClick(); };

			this.panel02InitResult.CamerasSettingsClick += delegate() { CamerasSettingsClick(); };
			this.panel02InitResult.CamerasDefaultSettingsClick += delegate() { CamerasDefaultSettingsClick(); };
			this.panel02InitResult.ContinueClick += delegate() { GoTo(CalibrationState.WhiteBalance); };
			this.panel02InitResult.TestScanClick += delegate(bool distortion, bool whiteBalance, bool crop, bool bookfold, string saveTo) { TestScanClick(distortion, whiteBalance, crop, bookfold, saveTo); };

			this.panel09WhiteBalance.BackClick += delegate() { GoTo(CalibrationState.InitResult); };
			this.panel09WhiteBalance.FindClick += delegate(CanonCamera.CameraProperties.Av av) { WhiteBalanceClick(av); };
			this.panel09WhiteBalance.SkipClick += delegate() { GoTo(CalibrationState.Focus); };

			this.panel11WhiteBalanceResult.BackClick += delegate() { GoTo(CalibrationState.WhiteBalance); };
			this.panel11WhiteBalanceResult.ContinueClick += delegate() { GoTo(CalibrationState.Focus); };

			this.panel03Focus.BackClick += delegate() { GoTo(CalibrationState.WhiteBalance); };
			this.panel03Focus.FindClick += delegate() { FindFocusClick(); };
			this.panel03Focus.SkipClick += delegate() { GoTo(CalibrationState.RegPoints); };

			this.panel04FocusResult.BackClick += delegate() { GoTo(CalibrationState.Focus); };
			this.panel04FocusResult.ContinueClick += delegate() { GoTo(CalibrationState.RegPoints); };

			this.panel05RegPoints.BackClick += delegate() { GoTo(CalibrationState.Focus); };
			this.panel05RegPoints.FindClick += delegate() { SeekRegPointsClick(); };
			this.panel05RegPoints.SkipClick += delegate() { GoTo(CalibrationState.Laser); };

			this.panel06RegPointsResult.BackClick += delegate() { GoTo(CalibrationState.RegPoints); };
			this.panel06RegPointsResult.ContinueClick += delegate() { GoTo(CalibrationState.Laser); };

			this.panel07Laser.BackClick += delegate() { GoTo(CalibrationState.RegPoints); };
			this.panel07Laser.FindClick += delegate() { FindLaserClick(); };
			this.panel07Laser.SkipClick += delegate() { GoTo(CalibrationState.Light); };

			this.panel08LaserResult.BackClick += delegate() { GoTo(CalibrationState.Laser); };
			this.panel08LaserResult.ContinueClick += delegate() { GoTo(CalibrationState.Light); };

			this.panel12Light.BackClick += delegate() { GoTo(CalibrationState.Laser); };
			this.panel12Light.FindClick += delegate() { LightsDistributionClick(); };
			this.panel12Light.SkipClick += delegate() { ResetLightsDistributionClick(); };

			this.panel13LightResult.BackClick += delegate() { GoTo(CalibrationState.Light); };
			this.panel13LightResult.ContinueClick += delegate() { GoTo(CalibrationState.Test); };

			this.panel14Test.BackClick += delegate() { GoTo(CalibrationState.Light); };
			this.panel14Test.TestScanClick += delegate(bool distortion, bool whiteBalance, bool crop, bool bookfold, string saveTo) { TestScanClick(distortion, whiteBalance, crop, bookfold, saveTo); };
			this.panel14Test.CloseClick += delegate() { SaveAndCloseClick(); };
		}
		#endregion


		#region class ComboApertureItem
		public class ComboApertureItem
		{
			string description;
			public CanonCamera.CameraProperties.Av Value;

			public ComboApertureItem(CanonCamera.CameraProperties.Av value)
			{
				this.description = GetApertureDescription(value);
				this.Value = value;
			}

			public string Description { get { return this.description; } }

			public override string ToString()
			{
				return this.Description;
			}
		}
		#endregion

		#region class ComboSpeedItem
		public class ComboSpeedItem
		{
			string description;
			public CanonCamera.CameraProperties.Tv Value;

			public ComboSpeedItem(CanonCamera.CameraProperties.Tv value)
			{
				this.description = CanonCamera.CameraProperties.TvProperty.GetName(value);
				this.Value = value;
			}

			public string Description { get { return this.description; } }

			public override string ToString()
			{
				return this.Description;
			}
		}
		#endregion

		#region enum CalibrationState
		public enum CalibrationState
		{
			Init,
			InitResult,
			Focus,
			FocusResult,
			RegPoints,
			RegPointsResult,
			Laser,
			LaserResult,
			WhiteBalance,
			WhiteBalanceProgress,
			WhiteBalanceResult,
			Light,
			LightResult,
			Test
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties
		public bool			SaveAllScansOnDisk { get { return this.panel02InitResult.checkSaveAllScans.IsChecked.Value; } }
		public string		SaveScansFolder { get { return this.panel02InitResult.textSave.Text; } }
		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Init()
		public void Init(string[] comPorts, string selectedComPort)
		{
			this.panel01Init.Init(comPorts, selectedComPort);
		}
		#endregion

		#region InitSuccessfull()
		public void InitSuccessfull()
		{
			GoTo(CalibrationState.InitResult);
		}
		#endregion

		#region FocusSuccessfull()
		public void FocusSuccessfull(Bitmap bitmapL, Bitmap bitmapR)
		{
			this.panel04FocusResult.Apply(bitmapL, bitmapR);
			GoTo(CalibrationState.FocusResult);
		}
		#endregion

		#region FindingRegPointsSuccessfull()
		public void FindingRegPointsSuccessfull(BitmapSource bitmapLTop, BitmapSource bitmapLBottom, BitmapSource bitmapRTop, BitmapSource bitmapRBottom, double skewL, double skewR)
		{
			this.panel06RegPointsResult.Apply(bitmapLTop, bitmapLBottom, bitmapRTop, bitmapRBottom, skewL, skewR);
			GoTo(CalibrationState.RegPointsResult);
		}
		#endregion

		#region FindingLaserSuccessfull()
		public void FindingLaserSuccessfull(BitmapSource bitmapLLeft, BitmapSource bitmapLMiddle, BitmapSource bitmapLRight, 
			BitmapSource bitmapRLeft, BitmapSource bitmapRMiddle, BitmapSource bitmapRRight)
		{
			this.panel08LaserResult.Apply(bitmapLLeft, bitmapLMiddle, bitmapLRight, bitmapRLeft, bitmapRMiddle, bitmapRRight);
			GoTo(CalibrationState.LaserResult);
		}
		#endregion

		#region ShowWhiteBalanceProgress()
		public void ShowWhiteBalanceProgress(BitmapSource bitmapL, BitmapSource bitmapR, CanonCamera.CameraProperties.Tv tv, double shade)
		{
			this.panel10WhiteBalanceProgress.Apply(bitmapL, bitmapR, tv, shade);
			GoTo(CalibrationState.WhiteBalanceProgress);
		}
		#endregion

		#region WhiteBalanceSuccessfull()
		public void WhiteBalanceSuccessfull(CanonCamera.CameraProperties.Tv tv, double shade)
		{
			this.panel11WhiteBalanceResult.Apply(tv);
			GoTo(CalibrationState.WhiteBalanceResult);
		}
		#endregion

		#region WhiteBalanceError()
		public void WhiteBalanceError()
		{
			GoTo(CalibrationState.WhiteBalance);
		}
		#endregion

		#region LightDistributionSuccessfull()
		public void LightDistributionSuccessfull()
		{
			GoTo(CalibrationState.LightResult);
		}
		#endregion

		#region ShowImages()
		public void ShowImages(BitmapSource bitmapL, BitmapSource bitmapR)
		{
			
		}
		#endregion

		#region GetApertureItems()
		public static List<ComboApertureItem> GetApertureItems()
		{
			List<ComboApertureItem> items = new List<ComboApertureItem>();

			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av2_8));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av3_2));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av3_5));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av4_5));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av5_6));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av6_7));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av10));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av11));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av13));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av14));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av16));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av18));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av19));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av20));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av22));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av25));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av27));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av29));
			items.Add(new ComboApertureItem(CanonCamera.CameraProperties.Av.Av32));

			return items;
		}
		#endregion

		#region GetSpeedItems()
		public static List<ComboSpeedItem> GetSpeedItems()
		{
			List<ComboSpeedItem> items = new List<ComboSpeedItem>();

			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv30));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv25));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv20));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv10));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv8));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv6));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv4));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv3));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv2));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv0_7));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv0_5));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv0_4));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv0_3));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d4));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d5));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d8));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d10));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d15));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d20));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d25));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d30));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d40));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d50));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d60));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d80));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d100));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d125));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d160));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d200));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d250));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d320));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d350));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d400));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d500));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d640));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d750));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d800));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d1000));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d1250));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d1500));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d1600));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d2000));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d2500));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d3000));
			items.Add(new ComboSpeedItem(CanonCamera.CameraProperties.Tv.Tv1d4000));
			
			return items;
		}
		#endregion

		#region GetApertureDescription()
		public static string GetApertureDescription(CanonCamera.CameraProperties.Av av)
		{
			switch (av)
			{
				case CanonCamera.CameraProperties.Av.Av2_8: return "2.8";
				case CanonCamera.CameraProperties.Av.Av3_2: return "3.2";
				case CanonCamera.CameraProperties.Av.Av3_5: return "3.5";
				case CanonCamera.CameraProperties.Av.Av4_5: return "4.5";
				case CanonCamera.CameraProperties.Av.Av5_6: return "5.6";
				case CanonCamera.CameraProperties.Av.Av6_7: return "6.7";
				case CanonCamera.CameraProperties.Av.Av10: return "10";
				case CanonCamera.CameraProperties.Av.Av11: return "11";
				case CanonCamera.CameraProperties.Av.Av13: return "13";
				case CanonCamera.CameraProperties.Av.Av14: return "14";
				case CanonCamera.CameraProperties.Av.Av16: return "16";
				case CanonCamera.CameraProperties.Av.Av18: return "18";
				case CanonCamera.CameraProperties.Av.Av19: return "19";
				case CanonCamera.CameraProperties.Av.Av20: return "20";
				case CanonCamera.CameraProperties.Av.Av22: return "22";
				case CanonCamera.CameraProperties.Av.Av25: return "25";
				case CanonCamera.CameraProperties.Av.Av27: return "27";
				case CanonCamera.CameraProperties.Av.Av29: return "29";
				case CanonCamera.CameraProperties.Av.Av32: return "32";
				default: return "Unknown";
			}
		}
		#endregion

		#region GetSpeedDescription()
		/*public static string GetSpeedDescription(CanonCamera.CameraProperties.Tv tv)
		{
			switch (tv)
			{
				case CanonCamera.CameraProperties.Tv.Tv30: return "30 seconds";
				case CanonCamera.CameraProperties.Tv.Tv25: return "25 seconds";
				case CanonCamera.CameraProperties.Tv.Tv20: return "20 seconds";
				case CanonCamera.CameraProperties.Tv.Tv10: return "10 seconds";
				case CanonCamera.CameraProperties.Tv.Tv8: return "8 seconds";
				case CanonCamera.CameraProperties.Tv.Tv6: return "6 seconds";
				case CanonCamera.CameraProperties.Tv.Tv4: return "4 seconds";
				case CanonCamera.CameraProperties.Tv.Tv3: return "3 seconds";
				case CanonCamera.CameraProperties.Tv.Tv2: return "2 seconds";
				case CanonCamera.CameraProperties.Tv.Tv1: return "1 second";
				case CanonCamera.CameraProperties.Tv.Tv0_7: return "0.7 second";
				case CanonCamera.CameraProperties.Tv.Tv0_5: return "0.5 second";
				case CanonCamera.CameraProperties.Tv.Tv0_4: return "0.4 second";
				case CanonCamera.CameraProperties.Tv.Tv0_3: return "0.3 second";
				case CanonCamera.CameraProperties.Tv.Tv1d4: return "1/4 second";
				case CanonCamera.CameraProperties.Tv.Tv1d5: return "1/5 second";
				case CanonCamera.CameraProperties.Tv.Tv1d8: return "1/8 second";
				case CanonCamera.CameraProperties.Tv.Tv1d10: return "1/10 second";
				case CanonCamera.CameraProperties.Tv.Tv1d15: return "1/15 second";
				case CanonCamera.CameraProperties.Tv.Tv1d20: return "1/20 second";
				case CanonCamera.CameraProperties.Tv.Tv1d25: return "1/25 second";
				case CanonCamera.CameraProperties.Tv.Tv1d30: return "1/30 second";
				case CanonCamera.CameraProperties.Tv.Tv1d40: return "1/40 second";
				case CanonCamera.CameraProperties.Tv.Tv1d50: return "1/50 second";
				case CanonCamera.CameraProperties.Tv.Tv1d60: return "1/60 second";
				case CanonCamera.CameraProperties.Tv.Tv1d80: return "1/80 second";
				case CanonCamera.CameraProperties.Tv.Tv1d100: return "1/100 second";
				case CanonCamera.CameraProperties.Tv.Tv1d125: return "1/125 second";
				case CanonCamera.CameraProperties.Tv.Tv1d160: return "1/160 second";
				case CanonCamera.CameraProperties.Tv.Tv1d200: return "1/200 second";
				case CanonCamera.CameraProperties.Tv.Tv1d250: return "1/250 second";
				case CanonCamera.CameraProperties.Tv.Tv1d320: return "1/320 second";
				case CanonCamera.CameraProperties.Tv.Tv1d350: return "1/350 second";
				case CanonCamera.CameraProperties.Tv.Tv1d400: return "1/400 second";
				case CanonCamera.CameraProperties.Tv.Tv1d500: return "1/500 second";
				case CanonCamera.CameraProperties.Tv.Tv1d640: return "1/640 second";
				case CanonCamera.CameraProperties.Tv.Tv1d750: return "1/750 second";
				case CanonCamera.CameraProperties.Tv.Tv1d800: return "1/800 second";
				case CanonCamera.CameraProperties.Tv.Tv1d1000: return "1/1000 second";
				case CanonCamera.CameraProperties.Tv.Tv1d1250: return "1/1250 second";
				case CanonCamera.CameraProperties.Tv.Tv1d1500: return "1/1500 second";
				case CanonCamera.CameraProperties.Tv.Tv1d1600: return "1/1600 second";
				case CanonCamera.CameraProperties.Tv.Tv1d2000: return "1/2000 second";
				case CanonCamera.CameraProperties.Tv.Tv1d2500: return "1/2500 second";
				case CanonCamera.CameraProperties.Tv.Tv1d3000: return "1/3000 second";
				case CanonCamera.CameraProperties.Tv.Tv1d4000: return "1/4000 second";
				default: return "Unknown";
			}
		}*/
		#endregion

		#region GetClip()
		internal static System.Windows.Media.Imaging.BitmapSource GetClip(System.Drawing.Bitmap bitmap, System.Drawing.Rectangle clip)
		{
			clip.Intersect(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));

			System.Drawing.Bitmap crop = ImageProcessing.ImageCopier.Copy(bitmap, clip);

			System.Windows.Media.Imaging.BitmapSource bmpSource = BscanILL.UI.Misc.GetBitmapSource(crop);
			crop.Dispose();

			return bmpSource;
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region GoTo()
		public void GoTo(CalibrationState state)
		{
			this.panel01Init.Visibility = (state == CalibrationState.Init) ? Visibility.Visible : Visibility.Hidden;
			this.panel02InitResult.Visibility = (state == CalibrationState.InitResult) ? Visibility.Visible : Visibility.Hidden;
			this.panel03Focus.Visibility = (state == CalibrationState.Focus) ? Visibility.Visible : Visibility.Hidden;
			this.panel04FocusResult.Visibility = (state == CalibrationState.FocusResult) ? Visibility.Visible : Visibility.Hidden;
			this.panel05RegPoints.Visibility = (state == CalibrationState.RegPoints) ? Visibility.Visible : Visibility.Hidden;
			this.panel06RegPointsResult.Visibility = (state == CalibrationState.RegPointsResult) ? Visibility.Visible : Visibility.Hidden;
			this.panel07Laser.Visibility = (state == CalibrationState.Laser) ? Visibility.Visible : Visibility.Hidden;
			this.panel08LaserResult.Visibility = (state == CalibrationState.LaserResult) ? Visibility.Visible : Visibility.Hidden;
			this.panel09WhiteBalance.Visibility = (state == CalibrationState.WhiteBalance) ? Visibility.Visible : Visibility.Hidden;
			this.panel10WhiteBalanceProgress.Visibility = (state == CalibrationState.WhiteBalanceProgress) ? Visibility.Visible : Visibility.Hidden;
			this.panel11WhiteBalanceResult.Visibility = (state == CalibrationState.WhiteBalanceResult) ? Visibility.Visible : Visibility.Hidden;
			this.panel12Light.Visibility = (state == CalibrationState.Light) ? Visibility.Visible : Visibility.Hidden;
			this.panel13LightResult.Visibility = (state == CalibrationState.LightResult) ? Visibility.Visible : Visibility.Hidden;
			this.panel14Test.Visibility = (state == CalibrationState.Test) ? Visibility.Visible : Visibility.Hidden;
		}
		#endregion

		#endregion



	}
}
