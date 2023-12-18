using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ViewPane.Hierarchy;
using System.Threading;

namespace BscanILL.UI.Frames.Edit
{
	class ManualItExecution
	{
		internal delegate void ExecutionSuccessfullHnd(VpImage selectedImage);
		internal delegate void ExecutionErrorHnd(VpImage selectedImage, Exception ex);
		internal delegate void ProgressChangedHnd(double progress);

		internal event ExecutionSuccessfullHnd ExecutionSuccessfull;
		internal event ExecutionErrorHnd ExecutionError;
		internal event ProgressChangedHnd ProgressChanged;


		#region constructor
		public ManualItExecution()
		{
		}
		#endregion


		#region RunManualIt()
		public void RunManualIt(VpImages images, VpImage selected, FrameEditUi.ItSelection itSelection, FrameEditUi.ItApplyTo applyTo, FrameEditUi.ItApplyToPages applyToPages)
		{
			ViewPane.Hierarchy.VpImages imagesToProcess = new VpImages();

            if (applyTo == FrameEditUi.ItApplyTo.RestOfBatch && selected != null)
            {
                int index = images.IndexOf(selected);

                if (index >= 0)
                {
                    for (int i = index + 1; i < images.Count; i++)
                        imagesToProcess.Add(images[i]);
                }
                else
                {
                    for (int i = 0; i < images.Count; i++)
                        if (images[i] != selected)
                            imagesToProcess.Add(images[i]);
                }
            }
            else
			if (applyTo == FrameEditUi.ItApplyTo.RestOfArticle && selected != null)
			{				
				int index = images.IndexOf(selected);

                if (index >= 0)
                {
                    for (int i = index + 1; i < images.Count; i++)
                    {
                        if (images[i].IsPullSlip)    //stop on next pull slip
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
                    //take entire first article in batch
                    for (int i = 0; i < images.Count; i++)
                    {
                        if ((images[i].IsPullSlip) && (i != 0))   //stop on next pull slip
                        {
                            break;
                        }
                        else
                        {
                           if (images[i] != selected)
                           {
                             imagesToProcess.Add(images[i]);
                           }
                        }
                    }
                }
			}
			else
            if (applyTo == FrameEditUi.ItApplyTo.EntireArticle && selected != null)   //entire current article
            {
                int index = images.IndexOf(selected);

                if (index >= 0)
                {
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
                            if (images[i] != selected)
                            {
                                imagesToProcess.Add(images[i]);
                            }
                        }
                    }
                }
                else
                {
                    //take entire first article in batch
                    for (int i = 0; i < images.Count; i++)
                    {
                        if ((images[i].IsPullSlip) && (i != 0))   //stop on next pull slip
                        {
                            break;
                        }
                        else
                        {
                            if (images[i] != selected)
                            {
                                imagesToProcess.Add(images[i]);
                            }
                        }
                    }
                }
            }
            else
            if (applyTo == FrameEditUi.ItApplyTo.EntireBatch)
			{
                for (int i = 0; i < images.Count; i++)
                {
                    if (images[i] != selected)
                        imagesToProcess.Add(images[i]);
                }
			}


			if (imagesToProcess.Count > 0)
			{
				Thread thread = new Thread(new ParameterizedThreadStart(DoManualIt_Thread));
				thread.SetApartmentState(ApartmentState.STA);
				thread.Name = "RunManualIt()";
				thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
				thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				thread.Start(new object[] { images, imagesToProcess, selected, itSelection, applyToPages });
			}
			else
			{
				if (ExecutionError != null)
					ExecutionError(selected, new Exception("No images selected to proceed!"));
			}
		}
		#endregion


		#region DoManualIt_Thread()
		private void DoManualIt_Thread(object folderObj)
		{
			object[]					objects = folderObj as object[];
			VpImages					allImages = objects[0] as VpImages;
			VpImages					imagesToProcess = objects[1] as VpImages;
			VpImage						selectedImage = objects[2] as VpImage;
			FrameEditUi.ItSelection		itSelection = (FrameEditUi.ItSelection)objects[3];
			FrameEditUi.ItApplyToPages	applyToPages = (FrameEditUi.ItApplyToPages)objects[4];

			try
			{
				ManualIp_Run(allImages, imagesToProcess, selectedImage, itSelection, applyToPages);

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

		#region ManualIp_Run()
		private void ManualIp_Run(VpImages allImages, VpImages imagesToProcess, VpImage selectedImage, FrameEditUi.ItSelection itSelection, FrameEditUi.ItApplyToPages applyToPages)
		{
			bool applyContent = ((itSelection & BscanILL.UI.Frames.Edit.FrameEditUi.ItSelection.Content) > 0);
			bool applySkew = ((itSelection & BscanILL.UI.Frames.Edit.FrameEditUi.ItSelection.Deskew) > 0);
			bool applyCurve = ((itSelection & BscanILL.UI.Frames.Edit.FrameEditUi.ItSelection.Bookfold) > 0);
			bool applyFingers = ((itSelection & BscanILL.UI.Frames.Edit.FrameEditUi.ItSelection.Fingers) > 0);

			ImageProcessing.IpSettings.ItImages itImagesAll = new ImageProcessing.IpSettings.ItImages();
			ImageProcessing.IpSettings.ItImages itImages = new ImageProcessing.IpSettings.ItImages();

			foreach (VpImage vpImage in allImages)
				if (vpImage.ItImage != null && vpImage.ItImage.IsFixed == false)
					itImagesAll.Add(vpImage.ItImage);

			foreach (VpImage vpImage in imagesToProcess)
				if (vpImage.ItImage != null && vpImage.ItImage.IsFixed == false)
					itImages.Add(vpImage.ItImage);

			ImageProcessing.IpSettings.ItImage	image = selectedImage.ItImage;
			ImageProcessing.IpSettings.ItPage	pageL = null, pageR = null;
			ImageProcessing.IpSettings.ItPage	pageLtemp, pageRtemp;
			
			GetLeftAndRightPage(image, out pageLtemp, out pageRtemp);

			if ((applyToPages & FrameEditUi.ItApplyToPages.Left) > 0)
				pageL = pageLtemp;
			if ((applyToPages & FrameEditUi.ItApplyToPages.Right) > 0)
				pageR = pageRtemp;

			ActivateClips(itImages, image, applyToPages);

			for (int i = 0; i < itImages.Count; i++)
			{
				try
				{
					ImageProcessing.IpSettings.ItImage itImage = itImages[i];

					if (itImage.IsFixed == false && itImage != image/* && itImage.IsIndependent == false*/)
					{
						ImageProcessing.IpSettings.ItPage itPageL, itPageR;
						GetLeftAndRightPage(itImage, out itPageL, out itPageR);

						if (applySkew)
						{
							if (pageL != null && itImage.PageL.IsActive)
								itImage.PageL.SetSkew(pageL.Skew, 1.0F);
							if (pageR != null && itImage.PageR.IsActive)
								itImage.PageR.SetSkew(pageR.Skew, 1.0F);
						}
						if (applyCurve)
						{
							if (pageL != null && itImage.PageL.IsActive)
								itImage.PageL.Bookfolding.ImportSettings(pageL.Bookfolding);
							if (pageR != null && itImage.PageR.IsActive)
								itImage.PageR.Bookfolding.ImportSettings(pageR.Bookfolding);

							itImage.OpticsCenter = image.OpticsCenter;
						}
						if (applyFingers)
						{
							itImage.ClearFingers();

							foreach (ImageProcessing.IpSettings.Finger finger in image.GetFingers())
							{
								if (pageL != null && itImage.PageL.IsActive && finger.Page == pageL)
								{
									ImageProcessing.IpSettings.Finger f = ImageProcessing.IpSettings.Finger.GetFinger(pageL, finger.RectangleNotSkewed, false);

									if (f != null)
										itImage.AddFinger(f);
								}
								else if (pageR != null && itImage.PageR.IsActive && finger.Page == pageR)
								{
									ImageProcessing.IpSettings.Finger f = ImageProcessing.IpSettings.Finger.GetFinger(pageR, finger.RectangleNotSkewed, false);

									if (f != null)
										itImage.AddFinger(f);
								}
							}
						}
					}

					if (ProgressChanged != null)
						ProgressChanged(0.5 + ((i + 1.0) / imagesToProcess.Count) / 2.0);
				}
				catch (Exception ex)
				{
					throw new Exception("Can't apply settings to image '" + imagesToProcess[i].FullPath + "'! " + ex.Message, ex);
				}
			}
		}
		#endregion

		#region ActivateClips()
		private void ActivateClips(ImageProcessing.IpSettings.ItImages images, ImageProcessing.IpSettings.ItImage selectedImage, FrameEditUi.ItApplyToPages applyToPages)
		{
			ImageProcessing.IpSettings.ItPage	pageL, pageR ;

			GetLeftAndRightPage(selectedImage, out pageL, out pageR);
			
			if (applyToPages == FrameEditUi.ItApplyToPages.All)
			{
				if (pageL != null && pageR != null)
				{
					//two pages, adjust both
					for (int i = 0; i < images.Count; i++)
					{
						ImageProcessing.IpSettings.ItImage itImage = images[i];

						itImage.PageL.Activate(pageL.ClipRect, true);
						itImage.PageR.Activate(pageR.ClipRect, true);
					}
				}
				else if (pageL != null)
				{
					//no right page, adjust left, disable right
					for (int i = 0; i < images.Count; i++)
					{
						ImageProcessing.IpSettings.ItImage itImage = images[i];

						itImage.PageL.Activate(pageL.ClipRect, true);
						itImage.PageR.Deactivate();
					}
				}
				else if (pageR != null)
				{
					//no left page, copy right to left and disable right
					for (int i = 0; i < images.Count; i++)
					{
						ImageProcessing.IpSettings.ItImage itImage = images[i];

						itImage.PageL.Activate(pageR.ClipRect, true);
						itImage.PageR.Deactivate();
					}
				}
			}
			else if (applyToPages == FrameEditUi.ItApplyToPages.Left)
			{
				if (pageL != null)
				{
					//make left pages same as pageL
					for (int i = 0; i < images.Count; i++)
					{
						ImageProcessing.IpSettings.ItImage itImage = images[i];
						ImageProcessing.IpSettings.ItPage itPageL, itPageR;
						
						GetLeftAndRightPage(itImage, out itPageL, out itPageR);

						if (itPageL != null && itPageR != null)
						{
							//two pages image, change only left page
							itImage.PageR.Activate(itPageR.ClipRect, true);
							itImage.PageL.Activate(pageL.ClipRect, true);
						}
						else if (itPageL != null)
						{
							// left page image, adjust left, disable right
							itImage.PageL.Activate(pageL.ClipRect, true);
							itImage.PageR.Deactivate();
						}
						else if (itPageR != null)
						{
							//right page image, add left page
							itImage.PageR.Activate(itPageR.ClipRect, true);
							itImage.PageL.Activate(pageL.ClipRect, true);
						}
					}
				}
				else
				{
					// delete all left pages
					for (int i = 0; i < images.Count; i++)
					{
						ImageProcessing.IpSettings.ItImage itImage = images[i];
						ImageProcessing.IpSettings.ItPage itPageL, itPageR;

						GetLeftAndRightPage(itImage, out itPageL, out itPageR);

						if (itPageL != null && itPageR != null)
						{
							//copy right page to 1 and disable 1
							itImage.PageL.Activate(itPageR.ClipRect, true);
							itImage.PageR.Deactivate();
						}
						else if (itPageL != null)
						{
							itImage.PageL.Activate(new BIP.Geometry.RatioRect(0,0,1,1), true);
							itImage.PageR.Deactivate();
						}
						else if (itPageR != null)
						{
							//no changes needed
						}
					}
				}
			}
			else if (applyToPages == FrameEditUi.ItApplyToPages.Right)
			{
				if (pageR != null)
				{
					for (int i = 0; i < images.Count; i++)
					{
						ImageProcessing.IpSettings.ItImage itImage = images[i];
						ImageProcessing.IpSettings.ItPage itPageL, itPageR;

						GetLeftAndRightPage(itImage, out itPageL, out itPageR);

						if (itPageL != null)
						{
							itImage.PageL.Activate(itPageL.ClipRect, true);
							itImage.PageR.Activate(pageR.ClipRect, true);
						}
						else
						{
							itImage.PageL.Activate(pageR.ClipRect, true);
							itImage.PageR.Deactivate();
						}
					}
				}
				else
				{
					for (int i = 0; i < images.Count; i++)
					{
						ImageProcessing.IpSettings.ItImage itImage = images[i];
						ImageProcessing.IpSettings.ItPage itPageL, itPageR;

						GetLeftAndRightPage(itImage, out itPageL, out itPageR);
						
						itImage.PageL.Activate(itPageL.ClipRect, true);
						itImage.PageR.Deactivate();
					}
				}
			}
		}
		#endregion

		#region GetLeftPage()
		private void GetLeftAndRightPage(ImageProcessing.IpSettings.ItImage image, out ImageProcessing.IpSettings.ItPage pageL,
			out ImageProcessing.IpSettings.ItPage pageR)
		{
			if (image.TwoPages)
			{
				if (image.PageL.ClipRect.X <= image.PageR.ClipRect.X)
				{
					pageL = image.PageL;
					pageR = image.PageR;
				}
				else
				{
					pageL = image.PageR;
					pageR = image.PageL;
				}
			}
			else if (image.PageL.IsActive && (IsLeftPage(image.PageL.ClipRect) == false))
			{
				pageL = null;
				pageR = image.PageL;
			}
			else if (image.PageR.IsActive && (IsLeftPage(image.PageR.ClipRect) == false))
			{
				pageL = null;
				pageR = image.PageR;
			}
			else if (image.PageR.IsActive && IsLeftPage(image.PageR.ClipRect))
			{
				pageL = image.PageR;
				pageR = null;
			}
			else
			{
				pageL = image.PageL;
				pageR = null;
			}
		}
		#endregion

		#region IsLeftPage()
		private bool IsLeftPage(BIP.Geometry.RatioRect ratioRect)
		{
			bool isOnLeft =  ((ratioRect.Left + (ratioRect.Width) / 2) < 0.6);
			bool isBigEnough = (ratioRect.Width > 0.7);

			return isOnLeft || isBigEnough;
		}
		#endregion

	}
}
