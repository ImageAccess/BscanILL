using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Net;
using System.Threading;
using System.IO;
using System.Drawing.Printing;
using System.Windows.Input;
using System.Windows;
using System.Runtime.InteropServices;
using BscanILL.Misc;


namespace BscanILL.UI.Frames.Export
{
	public class FrameExport : FrameBase
	{
		#region variables

		private BscanILL.UI.Frames.Export.FrameExportUi	frameExportUi;        

		public event KeyEventHandler			KeyDown;
        public delegate bool BoolHnd();
        public event BoolHnd SendAll_ExportClick;
        public event BoolHnd Previous_ExportClick;
        
        public delegate void ProcessErrorHnd(Exception ex);
               
		#endregion


		#region constructor
		public FrameExport(BscanILL.MainWindow mainWindow)
			: base(mainWindow)
		{
			this.frameExportUi = this.MainWindow.FrameExportUi;            

			this.frameExportUi.ExportAllClick += new VoidHnd(ExportAll_Click);
            this.frameExportUi.ExportCurrentClick += new VoidHnd(ExportCurrent_Click);

			frameExportUi.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(Preview_KeyDown);

			this.frameExportUi.ProgressPanel.OkClick += delegate()
			{
				this.frameExportUi.ProgressPanel.Visibility = Visibility.Hidden;
				this.UnLock();
			};

            this.frameExportUi.PrintClick += new BatchPageHnd(Print_Click);

			this.frameExportUi.ProgressPanel.CloseArticleClick += delegate()
			{
                this.MainWindow.Article = null;
                this.MainWindow.Batch.Reset();
                this.MainWindow.FrameScan.Reset();
				this.MainWindow.ActivateStage(BscanILL.MainWindow.Stage.Start);				
				this.frameExportUi.ProgressPanel.Visibility = Visibility.Hidden;
				UnLock();
			};

			this.frameExportUi.ProgressPanel.KeepArticleOpenClick += delegate()
			{
				this.frameExportUi.ProgressPanel.Visibility = Visibility.Hidden;
				UnLock();
			};
			
            this.MainWindow.ArticleChanged += delegate()
            {
                if (this.IsActivated)
                    this.frameExportUi.ArticleChanged(this.Article);
            };
            
            frameExportUi.ArticleModifiedExpStageNotification += new BscanILL.Misc.ArticleHnd(ArticleChanged);			
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public BscanILL.Hierarchy.IllPage			SelectedImage { get { return this.frameExportUi.SelectedImage; } }
		public System.Windows.Controls.UserControl	UserControl { get { return (System.Windows.Controls.UserControl)this.frameExportUi; } }
		  public BscanILL.Hierarchy.Article Article
        {
            get { return this.MainWindow.Article; }
            set
            {
                if (this.Article != value)
                {
                    this.MainWindow.Article = value;
                }
            }
        }
        public BscanILL.Hierarchy.SessionBatch      Batch { get { return this.MainWindow.Batch; } }

		#endregion

        //PRIVATE PROPERTIES
        #region private properties
        BscanILL.SETTINGS.Settings settings { get { return BscanILL.SETTINGS.Settings.Instance; } }
        #endregion

		//PUBLIC METHODS
		#region public methods

        #region PreprocessExportImageDerivatives()
        public void PreprocessExportImageDerivatives()
        {

        }
        #endregion

        #region Activate()
        
		public void Activate()
		{
            this.frameExportUi.Open(this.Batch);
			    if( this.Batch.Articles.Count > 0)   // Open() method initiates stage with first article in batch - notify this in main window to set Batch.CurrentArticle with correct value
            {
                ArticleChanged(this.Batch.Articles[0]);
            }             
			
			this.IsActivated = true;            
		}
		#endregion

		#region Deactivate()
		public void Deactivate()
		{
			Reset();
			this.frameExportUi.Visibility = Visibility.Hidden;
			
			this.IsActivated = false;
		}
		#endregion

        #region ArticleChanged()
        public void ArticleChanged(BscanILL.Hierarchy.Article article)
        {
            this.Article = article;
            if (this.Article != null)
            {
                //this.Article.IsLoadedInScan = true;
            }
        }
        #endregion
        
        #region InitiatePrint()
        public void InitiatePrint()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                if ((Batch != null || Article != null) && SelectedImage != null)
                {
                    Print_Click(Batch, Article, SelectedImage);
                }
            }
        }
        #endregion

        #region InitiatePageDown()
        public void InitiatePageDown()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameExportUi.viewPanel.SelectNextImage();
            }
        }
        #endregion

        #region InitiateArticleDown()
        public void InitiateArticleDown()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameExportUi.viewPanel.SelectPreviousArticle();
            }
        }
        #endregion

        #region InitiateArticleUp()
        public void InitiateArticleUp()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameExportUi.viewPanel.SelectNextArticle();
            }
        }
        #endregion

        #region InitiatePageUp()
        public void InitiatePageUp()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameExportUi.viewPanel.SelectPreviousImage();
            }
        }
        #endregion

        #region InitiateGoToHome()
        public void InitiateGoToHome()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameExportUi.viewPanel.SelectFirstImage();
            }
        }
        #endregion

        #region InitiateGoToEnd()
        public void InitiateGoToEnd()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameExportUi.viewPanel.SelectLastImage();
            }
        }
        #endregion

		#region Reset()
		public void Reset()
		{
			try
			{
				this.frameExportUi.Reset();

				ReleaseMemory();
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
			}
			finally
			{
			}
		}
		#endregion

		#region ReleaseMemory()
		private void ReleaseMemory()
		{
			BscanILL.Misc.MemoryManagement.ReleaseUnusedMemory();
		}
		#endregion

		#region Dispose()
		public override void Dispose()
		{			
			base.Dispose();
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region ShowDefaultImage()
		void ShowDefaultImage()
		{
			if (frameExportUi != null)
				frameExportUi.ShowDefaultImage();
		}
		#endregion

		#region ShowIllImage()
		void ShowIllImage(BscanILL.Hierarchy.IllPage illPage)
		{
			this.frameExportUi.ShowImage(illPage);
		}
		#endregion

		#region FrameWpf_ImageSelected()
		void FrameWpf_ImageSelected(BscanILL.UI.Misc.ImageEventArgs args)
		{
			/*BscanILL.Hierarchy.IllImage illImage = args.Image as BscanILL.Hierarchy.IllImage;
			bool formLocked = this.locked;

			try
			{
				Lock();
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, "FrameExport, FrameWpf_ImageSelected: " + ex.Message, ex);
				ShowWarningMessage(this.MainWindow, BscanILL.Languages.BscanILLStrings.Frames_ErrorChangingImageSelection_STR, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
			}
			finally
			{
				if (formLocked == false)
					UnLock();
			}*/
		}
		#endregion

		#region ExportAll_Click()
		void ExportAll_Click()
		{
			Export( true );
		}
		#endregion

        #region ExportCurrent_Click()
        void ExportCurrent_Click()
        {
            Export( false );
        }
        #endregion
        
        #region Print_Click()
        void Print_Click(BscanILL.Hierarchy.SessionBatch currentBatch, BscanILL.Hierarchy.Article currentArticle , BscanILL.Hierarchy.IllImage illImage)
        {
            BscanILL.Misc.Printing printDocs = new BscanILL.Misc.Printing(currentBatch, currentArticle, illImage);
            printDocs.PrintPageDialog();
        }
        
        void Print_Click(BscanILL.Hierarchy.SessionBatch currentBatch, BscanILL.Hierarchy.Article currentArticle , BscanILL.Hierarchy.IllPage illPage)
        {
            BscanILL.Misc.Printing printDocs = new BscanILL.Misc.Printing(currentBatch, currentArticle, illPage);
            printDocs.PrintPageDialog();
        }
        #endregion

		#region Export()
		void Export( bool exportAll )
		{
			if (this.IsLocked == false)
			{
				try
				{
					Lock();

					//BscanILL.Export.ExportUnit export = CreateExportUnit();
                    List<BscanILL.Export.ExportUnit> exports = CreateExportUnits(exportAll);
					//if (export != null)
                    if (exports.Count > 0)
					{
						this.frameExportUi.ProgressPanel.Reset();
						this.frameExportUi.ProgressPanel.Visibility = Visibility.Visible;
						
						//this.MessageWindow.Show(this.MainWindow, "Exporting...");

						BscanILL.Export.Exporter exporter = new BscanILL.Export.Exporter();

						exporter.ArticleExecutionSuccessfull += delegate(BscanILL.Export.ExportUnit exportUnit)
						{
							this.frameExportUi.Dispatcher.Invoke((Action)delegate() { ExportArticle_Successfull(exportUnit); });
						};
						exporter.ArticleExecutionError += delegate(BscanILL.Export.ExportUnit exportUnit, Exception ex)
						{
							this.frameExportUi.Dispatcher.Invoke((Action)delegate() { Export_Error(exportUnit, ex); });
						};
						exporter.ArticleProgressChanged += delegate(BscanILL.Export.ExportUnit exportUnit, int progress, string description)
						{
							this.frameExportUi.Dispatcher.Invoke((Action)delegate() { Export_ProgressChanged(exportUnit, progress, description); });
						};
						exporter.ArticleProgressComment += delegate(BscanILL.Export.ExportUnit exportUnit, string comment)
						{
							this.frameExportUi.Dispatcher.Invoke((Action)delegate() { Export_DescriptionChanged(exportUnit, comment); });
						};


                        exporter.ExecutionFinishedSuccessfully += delegate(int count)
						{
                            this.frameExportUi.Dispatcher.Invoke((Action)delegate() { ExportBatch_Successfull(count); });
						};

                        exporter.ExecutionFinishedWithError += delegate()
						{
                            this.frameExportUi.Dispatcher.Invoke((Action)delegate() { ExportBatch_Error(); });
						};

                        //if I do not create AbbyyConnector.Instance by calling method below, then I get exception when closing BscanILL in situation when I load Bscan, Reload article into Send tab and export to searchable pdf
                        //becasue in that scenario, the Abbyy engine instance is created by temp ExportThread and it gives exception when unloading Abbyy
                        BscanILL.Export.ExportFiles.Abbyy.CheckAbbyyEngineInstance(); 

						//exporter.Export(new List<BscanILL.Export.ExportUnit>() { export });
                        exporter.Export(exports);
					}
					else
						UnLock();
				}
				catch (IllException ex)
				{
					ShowWarningMessage(ex.Message);
					UnLock();
				}
				catch (Exception ex)
				{
					ShowErrorMessage("Export process was interrupted! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
					Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
					UnLock();
				}
			}
		}
		#endregion

		#region Done_Click()
		private void Done_Click(object sender, System.EventArgs e)
		{
		}
		#endregion

		#region Export_Successfull()
		void ExportArticle_Successfull(BscanILL.Export.ExportUnit exportUnit)
		{
			//this.frameExportUi.ProgressPanel.ExportArticleFinishedSuccessfully();
			//UnLock();

			//this.MainWindow.ActivateStage(BscanILL.MainWindow.Stage.Start);
			//this.MainWindow.Article = null;
			//UnLock();
		}
		#endregion


        #region ExportBatch_Successfull()
        void ExportBatch_Successfull(int articleCount)
		{
            this.frameExportUi.ProgressPanel.ExportBatchFinishedSuccessfully(articleCount);
			UnLock();			
		}
		#endregion

		#region Export_Error()
		void Export_Error(BscanILL.Export.ExportUnit exportUnit, Exception ex)
		{
			//ShowErrorMessage(BscanILL.Misc.Misc.GetErrorMessage(ex));

			this.frameExportUi.ProgressPanel.AddComment("Error: " + BscanILL.Misc.Misc.GetErrorMessage(ex));
			//this.frameExportUi.ProgressPanel.ExportFinishedWithError();
			//UnLock();
		}
		#endregion


        #region ExportBatch_Error()
        void ExportBatch_Error()
		{						
			this.frameExportUi.ProgressPanel.ExportFinishedWithError();
			UnLock();
		}
		#endregion

		#region Export_ProgressChanged()
		void Export_ProgressChanged(BscanILL.Export.ExportUnit exportUnit, int progress, string description)
		{
			this.frameExportUi.ProgressPanel.Progress = (progress / 100.0);
			if (description != null && description.Length > 0)
				this.frameExportUi.ProgressPanel.AddComment(description);
		}
		#endregion

		#region Export_DescriptionChanged()
		void Export_DescriptionChanged(BscanILL.Export.ExportUnit exportUnit, string comment)
		{
			this.frameExportUi.ProgressPanel.CurrentAction = comment;
		}
		#endregion

		#region UserInteracted()
		void UserInteracted()
		{
		}
		#endregion

		#region Preview_KeyDown()
		void Preview_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			bool shift = ((e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Shift) > 0);

			if (KeyDown != null)
				KeyDown(this, e);
		}
		#endregion
         
        #region CreateExportUnits()
        List<BscanILL.Export.ExportUnit> CreateExportUnits(bool exportAll)
        {
            bool dialogOn = settings.General.ExportDialogEnabled;
            bool previousPressed = false;

            List<BscanILL.Export.ExportUnit> exportUnits = new List<BscanILL.Export.ExportUnit>();
            List<BscanILL.Export.ExportTempUnit> exportTempUnits = new List<BscanILL.Export.ExportTempUnit>();
            int index = -1;

/*
            if (exportAll)
            {
                //foreach (BscanILL.Hierarchy.Article article in this.Batch.Articles)
                for (index = 0; index < this.Batch.Articles.Count; index++)
                {
                    BscanILL.Export.ExportUnit newExport;
                    
                    //index++;                    
                    newExport = CreateExportUnit(this.Batch.Articles[index], index, ref dialogOn, exportAll, ref previousPressed);                    
                    if (newExport != null)                    
                    {
                        exportUnits.Add(newExport);                        
                    }
                    else
                    if (previousPressed)
                    {
                        //go back to display previous article
                        if (index > 0)
                        {
                            if( exportUnits.Count > 0)                            
                                if (this.Batch.Articles[index - 1].DbArticle.Id == exportUnits[exportUnits.Count - 1].Article.DbArticle.Id)                                
                            {
                                //if last export unit belongs to previous article - delete it when going back to previous article to be displayed
                                exportUnits.RemoveAt(exportUnits.Count - 1);                                
                            }

                            index -= 2;
                        }
                    }
                }
            }
            else
            {
                if (this.Batch.CurrentArticle != null)
                {
                    index++;
                    
                    BscanILL.Export.ExportUnit newExport = CreateExportUnit(this.Batch.CurrentArticle, index, ref dialogOn, exportAll, ref previousPressed); 
                    if (newExport != null)
                    {
                        exportUnits.Add(newExport);
                    }                    
                }
            }
*/ 

            if (exportAll)
            {
                //foreach (BscanILL.Hierarchy.Article article in this.Batch.Articles)
                for (index = 0; index < this.Batch.Articles.Count; index++)
                {                    
                    BscanILL.Export.ExportTempUnit newTempExport;
                    //index++;                    
                    newTempExport = CreateExportTempUnit(this.Batch.Articles[index], index, ref dialogOn, exportAll, ref previousPressed);
                    
                    if (newTempExport != null)
                    {                        
                        exportTempUnits.Add(newTempExport);
                    }
                    else
                        if (previousPressed)
                        {
                            //go back to display previous article
                            if (index > 0)
                            {                                
                                if (exportTempUnits.Count > 0)                                    
                                    if (this.Batch.Articles[index - 1].DbArticle.Id == exportTempUnits[exportTempUnits.Count - 1].Article.DbArticle.Id)
                                    {
                                        //if last export unit belongs to previous article - delete it when going back to previous article to be displayed                                        
                                        exportTempUnits.RemoveAt(exportTempUnits.Count - 1);
                                    }

                                index -= 2;
                            }
                        }
                }
            }
            else
            {
                if (this.Batch.CurrentArticle != null)
                {
                    index++;

                    BscanILL.Export.ExportTempUnit newTempExport = CreateExportTempUnit(this.Batch.CurrentArticle, index, ref dialogOn, exportAll, ref previousPressed);
                    if (newTempExport != null)
                    {
                        exportTempUnits.Add(newTempExport);
                    }
                }
            }

            // now when we finished with export dialog we can execute the code we removed from ExportDlg.Open() method after we allowed to go back to previous
            // some database update, etc.. this way when we press previous in the export dialog to go to previous article we did not have to remove any records 
            //from the database that were set on 'next' rticle button or finish button

            foreach (BscanILL.Export.ExportTempUnit exportTempUnit in exportTempUnits)
            {
                BscanILLData.Models.DbExport dbExort = BscanILL.DB.Database.Instance.InsertExport(exportTempUnit.NewDbExport);
                BscanILL.Export.ExportUnit exportUnit = new BscanILL.Export.ExportUnit(exportTempUnit.Article, dbExort);
                
                /////////BscanILL.Export.ExportTempUnit exportTempUnit = new BscanILL.Export.ExportTempUnit(this.article, newDbExport, iPanel.GetAdditionalInfo());

                //this.article.ExportUnits.Add(exportUnit);
                exportTempUnit.Article.ExportUnits.Add(exportUnit);

                //exportUnit.AdditionalInfo = iPanel.GetAdditionalInfo();
                exportUnit.AdditionalInfo = exportTempUnit.AdditionalInfo;

                exportUnits.Add(exportUnit);
            }

            return exportUnits;
        }
        #endregion

		#region CreateExportUnit()
        BscanILL.Export.ExportUnit CreateExportUnit(BscanILL.Hierarchy.Article article, int articleIndex, ref bool dialogOn, bool exportAll, ref bool previousButtonPressed)
		{
            previousButtonPressed = false;
			BscanILL.UI.Dialogs.ExportDialog.ExportDlg	dlg = new Dialogs.ExportDialog.ExportDlg( this );
            BscanILL.Export.ExportUnit exportUnit = null;            
            if (exportAll)
            {
                exportUnit = dlg.Open(article, (articleIndex + 1), Batch.Articles.Count, dialogOn);
            }
            else
            {
                exportUnit = dlg.Open(article, 1, 1, dialogOn);
            }


            if (Previous_ExportClick != null)
            {
                if (Previous_ExportClick())
                {
                    previousButtonPressed = true;
                }
            }

            if( SendAll_ExportClick != null )
            {
                if (SendAll_ExportClick())
                {
                  dialogOn = false;
                }
            }
            
            SendAll_ExportClick = null;
            Previous_ExportClick = null;

			return exportUnit;
		}

        BscanILL.Export.ExportTempUnit CreateExportTempUnit(BscanILL.Hierarchy.Article article, int articleIndex, ref bool dialogOn, bool exportAll, ref bool previousButtonPressed)
        {
            previousButtonPressed = false;
            BscanILL.UI.Dialogs.ExportDialog.ExportDlg dlg = new Dialogs.ExportDialog.ExportDlg(this);
            BscanILL.Export.ExportTempUnit exportTempUnit = null;
            if (exportAll)
            {
                exportTempUnit = dlg.OpenTemp(article, (articleIndex + 1), Batch.Articles.Count, dialogOn);
            }
            else
            {
                exportTempUnit = dlg.OpenTemp(article, 1, 1, dialogOn);
            }


            if (Previous_ExportClick != null)
            {
                if (Previous_ExportClick())
                {
                    previousButtonPressed = true;
                }
            }

            if (SendAll_ExportClick != null)
            {
                if (SendAll_ExportClick())
                {
                    dialogOn = false;
                }
            }

            SendAll_ExportClick = null;
            Previous_ExportClick = null;

            return exportTempUnit;
        }

		#endregion        		

		#endregion

	}
}
