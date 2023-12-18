using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewPane.IT
{
	/*public class ItProcessing
	{
		private Skew skew;
		private Despeckle despeckle;
		private Rotation bookedgePage;
		private BlackBorderRemoval blackBorderRemoval;
		private BackgroundRemoval backgroundRemoval;
		private Invertion invertion;

		#region constructor
		internal ItProcessing()
		{
			this.skew = new Skew(false, 0);
			this.despeckle = new Despeckle(false, DespeckleSize.Size2x2, DespeckleMode.BothColors);
			this.bookedgePage = new Rotation(RotationMode.NoRotation);
			this.blackBorderRemoval = new BlackBorderRemoval(false);
			this.backgroundRemoval = new BackgroundRemoval(false);
			this.invertion = new Invertion(false);
		}

		#endregion


		#region class Skew
		public class Skew
		{
			public bool IsEnabled { get; set; }
			public double Angle { get; set; }

			#region constructor
			internal Skew(bool isEnabled, double angle)
			{
				this.IsEnabled = isEnabled;
				this.Angle = angle;
			}
			#endregion 


			//PUBLIC METHODS
			#region public methods

			#region Reset()
			public void Reset()
			{
				this.IsEnabled = false;
				this.Angle = 0;
			}
			#endregion

			#endregion 
		}
		#endregion class Skew

		#region class Despeckle
		public class Despeckle
		{
			public bool				IsEnabled { get; set; }
			public DespeckleSize	MaskSize { get; set; }
			public DespeckleMode	DespeckleMode { get; set; }


			#region constructor
			internal Despeckle(bool isEnabled, DespeckleSize size, DespeckleMode mode)
			{
				this.IsEnabled = isEnabled;
				this.MaskSize = size;
				this.DespeckleMode = mode;
			}
			#endregion


			//PUBLIC METHODS
			#region public methods
			
			#region Reset()
			public void Reset()
			{
				this.IsEnabled = false;
				this.MaskSize = DespeckleSize.Size2x2;
				this.DespeckleMode = IT.DespeckleMode.BothColors;
			}
			#endregion 

			#endregion 
		}
		#endregion

		#region class Rotation
		public class Rotation
		{
			public RotationMode Angle { get; set; }

			#region constructor
			internal Rotation(RotationMode angle)
			{
				this.Angle = angle;
			}
			#endregion 


			//PUBLIC METHODS
			#region public methods
			
			#region Reset()
			public void Reset()
			{
				this.Angle = RotationMode.NoRotation;
			}
			#endregion 

			#endregion
		}
		#endregion 

		#region class BlackBorderRemoval
		public class BlackBorderRemoval
		{
			public bool IsEnabled { get; set; }

			#region constructor
			internal BlackBorderRemoval(bool isEnabled)
			{
				this.IsEnabled = isEnabled;
			}
			#endregion 


			//PUBLIC METHODS
			#region public methods

			#region Reset()
			public void Reset()
			{
				this.IsEnabled = false;
			}
			#endregion 

			#endregion 
		}
		#endregion

		#region class BackgroundRemoval
		public class BackgroundRemoval
		{
			public bool IsEnabled { get; set; }

			#region constructor
			internal BackgroundRemoval(bool isEnabled)
			{
				this.IsEnabled = isEnabled;
			}
			#endregion 

			//PUBLIC METHODS
			#region public methods

			#region Reset()
			public void Reset()
			{
				this.IsEnabled = false;
			}
			#endregion Reset()

			#endregion 
		}
		#endregion

		#region class Invertion
		public class Invertion
		{
			public bool IsEnabled { get; set; } 

			#region constructor
			internal Invertion(bool isEnabled)
			{
				this.IsEnabled = isEnabled;
			}
			#endregion constructor


			//PUBLIC METHODS
			#region public methods

			#region Reset()
			public void Reset()
			{
				this.IsEnabled = false;
			}
			#endregion

			#endregion
		}
		#endregion


		//PUBLIC PROPERTIES

		#region public properties

		public Skew ItSkew { get { return this.skew; } }
		public Despeckle ItDespeckle { get { return this.despeckle; } }
		public Rotation ItRotation { get { return this.bookedgePage; } }
		public BlackBorderRemoval ItBlackBorderRemoval { get { return this.blackBorderRemoval; } }
		public BackgroundRemoval ItBackgroundRemoval { get { return this.backgroundRemoval; } }
		public Invertion ItInvertion { get { return this.invertion; } }

		#endregion public properties


		//PUBLIC METHODS
		#region public methods

		#region Reset()
		public void Reset()
		{
			this.skew.Reset();
			this.despeckle.Reset();
			this.bookedgePage.Reset();
			this.blackBorderRemoval.Reset();
			this.backgroundRemoval.Reset();
			this.invertion.Reset();
		}
		#endregion

		#endregion
	}*/
}
