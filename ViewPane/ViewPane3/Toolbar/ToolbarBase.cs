using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using ViewPane.ImagePanel;
using ViewPane.Hierarchy;

namespace ViewPane.Toolbar
{
	[SmartAssembly.Attributes.DoNotPrune]
	[SmartAssembly.Attributes.DoNotPruneType]
	[SmartAssembly.Attributes.DoNotObfuscate]
	[SmartAssembly.Attributes.DoNotObfuscateType]
	class ToolbarBase : UserControl
	{
		IImagePane imagePane = null;


		// PUBLIC PROPERTIES
		#region public properties

		internal IImagePane ImagePane { get { return this.imagePane; } }

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Init()
		public void Init(IImagePane imagePane, System.Windows.Visibility visibility)
		{
			this.imagePane = imagePane;
			this.Visibility = visibility;

			this.imagePane.ImageChanged += new ImageSelectedHnd(ImagePane_ImageChanged);
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region ImagePane_ImageChanged()
		protected virtual void ImagePane_ImageChanged(VpImage vpImage)
		{
		}
		#endregion

		#endregion
	}
}
