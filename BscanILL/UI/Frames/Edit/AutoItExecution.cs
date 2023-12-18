using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ViewPane.Hierarchy;
using BscanILL.Hierarchy;

namespace BscanILL.UI.Frames.Edit
{
	class AutoItExecution
	{		
		internal delegate void ExecutionSuccessfullHnd(VpImage selectedImage);
		internal delegate void ExecutionErrorHnd(VpImage selectedImage, Exception ex);
		internal delegate void ProgressChangedHnd(double progress);

		internal event ExecutionSuccessfullHnd	ExecutionSuccessfull;
		internal event ExecutionErrorHnd		ExecutionError;
		internal event ProgressChangedHnd		ProgressChanged;


		#region constructor
		public AutoItExecution()
		{
		}
		#endregion


		BscanILL.SETTINGS.Settings	_settings { get { return BscanILL.SETTINGS.Settings.Instance; } }


		#region RunAutoIt()
		public void RunAutoIt(VpImages images, VpImage selected, FrameEditUi.ItSelection itSelection, FrameEditUi.ItApplyTo applyTo)
		{
			ViewPane.Hierarchy.VpImages imagesToProcess = new VpImages();

			if (applyTo == FrameEditUi.ItApplyTo.Current && selected != null)
			{
				imagesToProcess.Add(selected);
			}
            else if (applyTo == FrameEditUi.ItApplyTo.RestOfBatch && selected != null)
            {
                int index = images.IndexOf(selected);

                if (index >= 0)
                {
                    for (int i = index; i < images.Count; i++)
                        imagesToProcess.Add(images[i]);
                }
                else
                    imagesToProcess = images;
            }
			else if (applyTo == FrameEditUi.ItApplyTo.RestOfArticle && selected != null)
			{
				int index = images.IndexOf(selected);

                if (index >= 0)
                {
                    for (int i = index; i < images.Count; i++)
                    {
                        if ((images[i].IsPullSlip) && (i != index))   //stop on next pull slip
                        {
                            break;
                        }
                        else
                        {
                            imagesToProcess.Add(images[i]);
                        }
                    }
                }
                else
                {
                   // imagesToProcess = images;
                    //take entire first article in batch
                    for (int i = 0; i < images.Count; i++)
                    {
                        if ((images[i].IsPullSlip) && (i != 0))   //stop on next pull slip
                        {
                            break;
                        }
                        else
                        {
                            imagesToProcess.Add(images[i]);
                        }
                    }
                }
			}
            else if (applyTo == FrameEditUi.ItApplyTo.EntireArticle && selected != null)   //entire current article
            {
                int index = images.IndexOf(selected);

                if (index < 0)
                {
                    index = 0;
                }
                int indexPullslip = 0;
                for (int i = index; i >= 0; i--)
                {
                     //search for pull slip to find beginning of article
                    if ((images[i].IsPullSlip) || (i == 0))   //stop on pull slip
                    {
                        indexPullslip = i;
                        break;
                    }
                }
                for (int i = indexPullslip; i < images.Count; i++)
                {
                    if ((images[i].IsPullSlip) && (i != indexPullslip))   //stop on next pull slip
                    {
                        break;
                    }
                    else
                    {
                        imagesToProcess.Add(images[i]);
                    }
                }

            }
            else
                imagesToProcess = images;   //EntireBatch


			if (imagesToProcess.Count > 0)
			{
				Thread thread = new Thread(new ParameterizedThreadStart(DoAutoIp_Thread));
				thread.SetApartmentState(ApartmentState.STA);
				thread.Name = "RunAutoIt()";
				thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
				thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				thread.Start(new object[] { images, imagesToProcess, selected, itSelection });
			}
			else
			{
				if (ExecutionError != null)
					ExecutionError(selected, new Exception("No images selected to proceed!"));
			}
		}
		#endregion


		#region DoAutoIp_Thread()
		private void DoAutoIp_Thread(object folderObj)
		{
			object[] objects = folderObj as object[];
			VpImages allImages = objects[0] as VpImages;
			VpImages imagesToProcess = objects[1] as VpImages;
			VpImage selectedImage = objects[2] as VpImage;
			FrameEditUi.ItSelection itAutoSelection = (FrameEditUi.ItSelection)objects[3];

			try
			{
				AutoIp_Run(allImages, imagesToProcess, itAutoSelection);

				if (ExecutionSuccessfull != null)
					ExecutionSuccessfull(selectedImage);
			}
			catch (Exception ex)
			{				
				if (ExecutionError != null)
					ExecutionError(selectedImage, ex);
			}
		}
		#endregion

		#region AutoIp_Run()
		private void AutoIp_Run(VpImages allImages, VpImages imagesToProcess, FrameEditUi.ItSelection itAutoSelection)
		{	
			bool findContent = ((itAutoSelection & BscanILL.UI.Frames.Edit.FrameEditUi.ItSelection.Content) > 0);
			bool findSkew = ((itAutoSelection & BscanILL.UI.Frames.Edit.FrameEditUi.ItSelection.Deskew) > 0);
			bool findCurve = ((itAutoSelection & BscanILL.UI.Frames.Edit.FrameEditUi.ItSelection.Bookfold) > 0);
			bool findFingers = ((itAutoSelection & BscanILL.UI.Frames.Edit.FrameEditUi.ItSelection.Fingers) > 0);

			ImageProcessing.IpSettings.ItImages itImagesAll = new ImageProcessing.IpSettings.ItImages();
			ImageProcessing.IpSettings.ItImages itImages = new ImageProcessing.IpSettings.ItImages();

			foreach (VpImage vpImage in allImages)
				if (vpImage.ItImage != null && vpImage.ItImage.IsFixed == false)
					itImagesAll.Add(vpImage.ItImage);

			foreach (VpImage vpImage in imagesToProcess)
				if (vpImage.ItImage != null && vpImage.ItImage.IsFixed == false)
					itImages.Add(vpImage.ItImage);

			float						offsetInches = (float) _settings.ImageTreatment.AutoImageTreatment.ContentLocation.ContentOffsetInches;

			for (int i = 0; i < imagesToProcess.Count; i++)
			{
				try
				{
					VpImage vpImage = imagesToProcess[i];

					if (vpImage.IsFixed == false)
					{
						bool seek2Pages = vpImage.FullImageSize.Width > vpImage.FullImageSize.Height;

						vpImage.ItImage.WhiteThresholdDelta = 0;
						vpImage.ItImage.MinDelta = 20;

						ImageProcessing.Operations.ContentLocationParams contentParams = new ImageProcessing.Operations.ContentLocationParams(true, offsetInches, offsetInches, seek2Pages);
						ImageProcessing.Operations operations = new ImageProcessing.Operations(contentParams, findSkew, findCurve, false);

						vpImage.ItImage.Find(vpImage.ReducedPath, operations);
					}

					if (ProgressChanged != null)
						ProgressChanged((i + 1.0) / imagesToProcess.Count);
				}
				catch (Exception ex)
				{
					throw new Exception("Can't process image '" + imagesToProcess[i].ReducedPath + "'!", ex);
				}
			}

			if (findContent)
			{
				BIP.Geometry.InchSize? size = itImagesAll.GetArticleMedianContentSizeInInches();

				if (size != null)
					itImages.MakeDependantClipsSameSize(0.2F, size.Value, (float)_settings.ImageTreatment.AutoImageTreatment.ImageDependency.DependencyHorizontalToleranceInches,
																			(float)_settings.ImageTreatment.AutoImageTreatment.ImageDependency.DependencyVerticalToleranceInches);
			}

			if (findFingers)
			{
				ImageProcessing.Operations operations = new ImageProcessing.Operations(false, 0, false, false, true);
				
				for (int i = 0; i < imagesToProcess.Count; i++)
				{
					try
					{
						VpImage vpImage = imagesToProcess[i];

						if (vpImage.IsFixed == false)
						{

							vpImage.ItImage.Find(vpImage.ReducedPath, operations);
							vpImage.ReleaseBitmaps();
						}

						if (ProgressChanged != null)
							ProgressChanged((i + 1.0) / imagesToProcess.Count);
					}
					catch (Exception ex)
					{
						throw new Exception("Can't process image '" + imagesToProcess[i].ReducedPath + "'!", ex);
					}
				}
			}
		}
		#endregion

	}
}
