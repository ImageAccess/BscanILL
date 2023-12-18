using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;

namespace BscanILL.UI.Controls.ScannerControls
{
	public class GridLengthAnimation : GridLengthAnimationBase
	{
		public static readonly DependencyProperty ByProperty = DependencyProperty.Register("By", typeof(double?), typeof(GridLengthAnimation), new PropertyMetadata(null));
		public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(double?), typeof(GridLengthAnimation), new PropertyMetadata(null));
		public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(double?), typeof(GridLengthAnimation), new PropertyMetadata(null));

		public double? By
		{
			get { return (double?)this.GetValue(ByProperty); }
			set { this.SetValue(ByProperty, value); }
		}

		public double? From
		{
			get { return (double?)this.GetValue(FromProperty); }
			set { this.SetValue(FromProperty, value); }
		}

		public double? To
		{
			get { return (double?)this.GetValue(ToProperty); }
			set { this.SetValue(ToProperty, value); }
		}

		protected override Freezable CreateInstanceCore()
		{
			return new GridLengthAnimation();
		}

		public override GridLength GetCurrentValueCore(GridLength defaultOriginValue, GridLength defaultDestinationValue, AnimationClock animationClock)
		{
			if (From == null)
				throw new Exception("From must be specified in a GridLengthAnimation");

			double a_to;

			if (To != null)
				a_to = To.Value;
			else if (By != null)
				a_to = From.Value + By.Value;
			else
				throw new Exception("Either To or By must be specified in a GridLengthAnimation");

			return new GridLength(From.Value + ((a_to - From.Value) * animationClock.CurrentProgress.Value));
		}
	}

	public abstract class GridLengthAnimationBase : AnimationTimeline
	{
		protected GridLengthAnimationBase()
		{
		}

		public override sealed Type TargetPropertyType { get { return typeof(GridLength); } }
		public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
		{
			if (defaultOriginValue is GridLength == false)
				throw new ArgumentException("Parameter must be a GridLength", "defaultOriginValue");

			if (defaultDestinationValue is GridLength == false)
				throw new ArgumentException("Parameter must be a GridLength", "defaultDestinationValue");

			return GetCurrentValueCore((GridLength)defaultOriginValue, (GridLength)defaultDestinationValue, animationClock);
		}

		public abstract GridLength GetCurrentValueCore(GridLength defaultOriginValue, GridLength defaultDestinationValue, AnimationClock animationClock);
	}
}
