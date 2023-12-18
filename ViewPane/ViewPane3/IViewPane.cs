using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewPane.ImagePanel;
using ViewPane.Thumbnails;
using ViewPane.Hierarchy;

namespace ViewPane
{
	public delegate void ImageSelectedHnd (VpImage vpImage);
	public delegate void ThumbnailHnd(Thumbnail thumbnail);
	public delegate void ResultsThumbnailHnd(ViewPane.ItResults.Thumbnail thumbnail);
	public delegate void ZoneSelectedHnd(BIP.Geometry.RatioRect zone); 
	
	/*public interface IViewPane
	{
		// properties	
		int?									SelectedScanId { get; }
		ViewPanel.Status						PaneStatus { get; set; }

		//methods
		void ShowImage(System.Drawing.Bitmap bitmap);
		void ShowImage(string filePath);
		void ShowDefaultImage();

		void SelectImage(VpImage vpImage);
		void SelectFirstImage();
		void SelectLastImage();

		//events
		event ImageSelectedHnd ImageSelected;
		event ZoneSelectedHnd ZoneSelected;
	}*/

	#region class Licensing
	public class Licensing
	{
		public ImageProcessingMode Ip = ImageProcessingMode.Disabled;
		public PostProcessingMode PostProcessing = PostProcessingMode.Disabled;

		public Licensing()
		{
		}
		
		/*public Licensing(ImageProcessingMode ip, PostProcessingMode postProcessing)
		{
			this.Ip = ip;
			this.PostProcessing = postProcessing;
		}*/
	}
	#endregion

	#region enum ImageProcessingMode
	[Flags]
	public enum ImageProcessingMode
	{
		Disabled = 0x00,
		Basic = 0x01,
		Advanced = 0x02
	}
	#endregion

	#region enum PostProcessingMode
	[Flags]
	public enum PostProcessingMode
	{
		Disabled = 0x00,
		Basic = 0x01,
		Advanced = 0x02
	}
	#endregion


}
