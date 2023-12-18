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
using System.Windows.Media.Animation;

namespace BscanILL.UI.Frames.StartUp
{
	/// <summary>
	/// Interaction logic for ProgressUI.xaml
	/// </summary>
	public partial class StartUpUi : UserControl
	{
		bool		showProgress = true;
		
		public static DependencyProperty ProgressInternalProperty = DependencyProperty.Register("ProgressInternal", typeof(int), typeof(StartUpUi), new PropertyMetadata(0, new PropertyChangedCallback(Progress_Changed)));

		Int32Animation progressAnimation = new Int32Animation();
		Storyboard storyboard = new Storyboard();


		#region constructor
		public StartUpUi()
		{
			InitializeComponent();

			Storyboard.SetTarget(progressAnimation, this);
			Storyboard.SetTargetProperty(progressAnimation, new PropertyPath(StartUpUi.ProgressInternalProperty));

			this.storyboard.Children.Add(this.progressAnimation);
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Description
		public string Description
		{
			set
			{
				this.Dispatcher.Invoke((Action)delegate()
					{
						this.description.Text = value;
					});
			}
		}
		#endregion

		#region Progress
		public double Progress
		{
			set
			{
				this.Dispatcher.Invoke((Action)delegate()
				{
					this.ProgressInternal = Convert.ToInt32(value * 100);
				});
			}
		}
		#endregion

		#region IsProgressVisible
		public bool IsProgressVisible
		{
			get
			{
				return (bool)this.Dispatcher.Invoke((Func<bool>)delegate() { return this.showProgress; });
			}
			set
			{
				this.Dispatcher.Invoke((Action)delegate()
				{
					this.showProgress = value;
					this.progressText.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed;
				});
			}
		}
		#endregion

		#endregion


		//PRIVATE PROPERTIES
		#region private properties

		private int ProgressInternal
		{
			get { return (int)GetValue(ProgressInternalProperty); }
			set { SetValue(ProgressInternalProperty, value); }
		}

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region SetVisibility()
		public void SetVisibility(bool visible)
		{
			this.Dispatcher.Invoke((Action)delegate()
			{
				if (visible)
				{
					this.Visibility = Visibility.Visible;
					storyboard.Begin();
				}
				else
				{
					this.Visibility = Visibility.Collapsed;
					storyboard.Stop();
				}			
			});
		}
		#endregion

		#region Reset()
		public void Reset()
		{
			this.Dispatcher.Invoke((Action)delegate()
			{
				this.description.Text = "";
				this.ProgressInternal = 0;
			});
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Progress_Changed()
		static void Progress_Changed(object sender, DependencyPropertyChangedEventArgs args)
		{
			StartUpUi instance = (StartUpUi)sender;
			int progress = (int)args.NewValue;

			progress = Math.Max(0, Math.Min(100, progress));
			instance.progressText.Text = string.Format("{0}%", progress);
		}
		#endregion

		#endregion

	}
}
