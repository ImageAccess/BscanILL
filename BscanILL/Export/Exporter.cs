using System;
using System.IO;
using System.Windows.Forms;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;
using BscanILL.Misc;


namespace BscanILL.Export
{

	/// <summary>
	/// Summary description for Exporter.
	/// </summary>
	public class Exporter
	{
		BscanILL.Export.ILLiad.IILLiad			illiad = null;
		BscanILL.Export.Ariel.IAriel			ariel = null;
		BscanILL.Export.Email.EmailExport				email = null;
		BscanILL.Export.FTP.FtpServer			ftpServer = null;
		BscanILL.Export.FTP.FtpThruFileSystem	ftpDirectory = null;
		BscanILL.Export.FileSystem.SaveOnDisk			saveOnDisk = null;
		BscanILL.Export.ILL.Odyssey				odyssey = null;
		BscanILL.Export.AE.ArticleExchange		articleExchange = null;
        BscanILL.Export.Tipasa.Tipasa           tipasa = null;
        BscanILL.Export.WorldShareILL.WorldShareILL worldShareILL = null;
		BscanILL.Export.Rapido.Rapido rapido = null;

		public event ExecutionStartedHandle					ExecutionStarted;
		public event ExecutionFinishedSuccessfullyHandle	ExecutionFinishedSuccessfully;
		public event ExecutionFinishedWithErrorHandle		ExecutionFinishedWithError;

		public event ArticleExecutionStartedHandle			ArticleExecutionStarted;
		public event ProcessSuccessfullHnd					ArticleExecutionSuccessfull;
		public event ProcessErrorHnd						ArticleExecutionError;
		public event ProgressChangedHandle					ArticleProgressChanged;
		public event ProgressCommentHandle					ArticleProgressComment;


		#region constructor
		public Exporter()
		{
		}
		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		BscanILL.SETTINGS.Settings _settings { get { return BscanILL.SETTINGS.Settings.Instance; } }

		#region ILLiad
		BscanILL.Export.ILLiad.IILLiad ILLiad
		{
			get
			{
				if (this.illiad == null)
				{
					if (_settings.Export.ILLiad.ExportToOdysseyHelper && ((int)_settings.Export.ILLiad.Version >= (int)BscanILL.Export.ILLiad.ILLiadVersion.Version7_3_0_0))
					{
						if ((int)_settings.Export.ILLiad.Version >= (int)BscanILL.Export.ILLiad.ILLiadVersion.Version8_1_0_0)
							illiad = new BscanILL.Export.ILLiad.ILLiad8_1_0_0();
						else
							illiad = new BscanILL.Export.ILLiad.ILLiad7_3_0_0();
					}
					else
						illiad = BscanILL.Export.ILLiad.ILLiadBasics.GetIlliadInstance();

					illiad.ProgressChanged += new ProgressChangedHandle(Progress_Changed);
					illiad.ProgressComment += new ProgressCommentHandle(Progress_Comment);
				}

				return this.illiad;
			}
		}
		#endregion

		#region Ariel
		BscanILL.Export.Ariel.IAriel Ariel
		{
			get
			{
				if (this.ariel == null)
				{
					if (_settings.Export.Ariel.MajorVersion == 3)
						ariel = new BscanILL.Export.Ariel.Ariel3_X();
					else
						ariel = new BscanILL.Export.Ariel.Ariel4_X();

					ariel.ProgressChanged += new ProgressChangedHandle(Progress_Changed);
					ariel.ProgressComment += new ProgressCommentHandle(Progress_Comment);
				}

				return this.ariel;
			}
		}
		#endregion

		#region Email
		BscanILL.Export.Email.EmailExport Email
		{
			get
			{
				if (this.email == null)
				{
					email = new BscanILL.Export.Email.EmailExport();

					email.ProgressChanged += new ProgressChangedHandle(Progress_Changed);
					email.ProgressComment += new ProgressCommentHandle(Progress_Comment);
				}

				return this.email;
			}
		}
		#endregion

		#region FtpServer
		BscanILL.Export.FTP.FtpServer FtpServer
		{
			get
			{
				if (this.ftpServer == null)
				{
					ftpServer = new BscanILL.Export.FTP.FtpServer();

					ftpServer.ProgressChanged += new ProgressChangedHandle(Progress_Changed);
					ftpServer.ProgressComment += new ProgressCommentHandle(Progress_Comment);
				}

				return this.ftpServer;
			}
		}
		#endregion

		#region FtpThruFileSystem
		BscanILL.Export.FTP.FtpThruFileSystem FtpThruFileSystem
		{
			get
			{
				if (this.ftpDirectory == null)
				{
					ftpDirectory = new BscanILL.Export.FTP.FtpThruFileSystem();

					ftpDirectory.ProgressChanged += new ProgressChangedHandle(Progress_Changed);
					ftpDirectory.ProgressComment += new ProgressCommentHandle(Progress_Comment);
				}

				return this.ftpDirectory;
			}
		}
		#endregion

		#region SaveOnDisk
		BscanILL.Export.FileSystem.SaveOnDisk SaveOnDisk
		{
			get
			{
				if (this.saveOnDisk == null)
				{
					saveOnDisk = new BscanILL.Export.FileSystem.SaveOnDisk();

					saveOnDisk.ProgressChanged += new ProgressChangedHandle(Progress_Changed);
					saveOnDisk.ProgressComment += new ProgressCommentHandle(Progress_Comment);
				}

				return this.saveOnDisk;
			}
		}
		#endregion

		#region Odyssey
		BscanILL.Export.ILL.Odyssey Odyssey
		{
			get
			{
				if (this.odyssey == null)
				{
					odyssey = new BscanILL.Export.ILL.Odyssey();

					odyssey.ProgressChanged += new ProgressChangedHandle(Progress_Changed);
					odyssey.ProgressComment += new ProgressCommentHandle(Progress_Comment);
				}

				return this.odyssey;
			}
		}
		#endregion

		#region ArticleExchange
		BscanILL.Export.AE.ArticleExchange ArticleExchange
		{
			get
			{
				if (this.articleExchange == null)
				{
					articleExchange = new BscanILL.Export.AE.ArticleExchange();

					articleExchange.ProgressChanged += new ProgressChangedHandle(Progress_Changed);
					articleExchange.ProgressComment += new ProgressCommentHandle(Progress_Comment);
				}

				return this.articleExchange;
			}
		}
		#endregion

        #region Tipasa
        BscanILL.Export.Tipasa.Tipasa Tipasa
        {
            get
            {
                if (this.tipasa == null)
                {
                    tipasa = new BscanILL.Export.Tipasa.Tipasa();

                    tipasa.ProgressChanged += new ProgressChangedHandle(Progress_Changed);
                    tipasa.ProgressComment += new ProgressCommentHandle(Progress_Comment);
                }

                return this.tipasa;
            }
        }
        #endregion

        #region WorldShareILL
        BscanILL.Export.WorldShareILL.WorldShareILL WorldShareILL
        {
            get
            {
                if (this.worldShareILL == null)
                {
                    worldShareILL = new BscanILL.Export.WorldShareILL.WorldShareILL();

                    worldShareILL.ProgressChanged += new ProgressChangedHandle(Progress_Changed);
                    worldShareILL.ProgressComment += new ProgressCommentHandle(Progress_Comment);
                }

                return this.worldShareILL;
            }
        }
		#endregion


		#region Rapido
		BscanILL.Export.Rapido.Rapido Rapido
		{
			get
			{
				if (this.rapido == null)
				{
					rapido = new BscanILL.Export.Rapido.Rapido();

					rapido.ProgressChanged += new ProgressChangedHandle(Progress_Changed);
					rapido.ProgressComment += new ProgressCommentHandle(Progress_Comment);
				}

				return this.rapido;
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose()
		{
			if (this.illiad != null)
			{
				this.illiad.ProgressChanged -= new ProgressChangedHandle(Progress_Changed);
				this.illiad.ProgressComment -= new ProgressCommentHandle(Progress_Comment);
				this.illiad = null;
			}

			if (this.ariel != null)
			{
				ariel.ProgressChanged -= new ProgressChangedHandle(Progress_Changed);
				ariel.ProgressComment -= new ProgressCommentHandle(Progress_Comment);
				ariel = null;
			}

			if (this.email != null)
			{
				email.ProgressChanged -= new ProgressChangedHandle(Progress_Changed);
				email.ProgressComment -= new ProgressCommentHandle(Progress_Comment);
				email = null;
			}

			if (this.ftpServer != null)
			{
				ftpServer.ProgressChanged -= new ProgressChangedHandle(Progress_Changed);
				ftpServer.ProgressComment -= new ProgressCommentHandle(Progress_Comment);
				ftpServer = null;
			}

			if (this.ftpDirectory != null)
			{
				ftpDirectory.ProgressChanged -= new ProgressChangedHandle(Progress_Changed);
				ftpDirectory.ProgressComment -= new ProgressCommentHandle(Progress_Comment);
				ftpDirectory = null;
			}

			if (this.saveOnDisk != null)
			{
				saveOnDisk.ProgressChanged -= new ProgressChangedHandle(Progress_Changed);
				saveOnDisk.ProgressComment -= new ProgressCommentHandle(Progress_Comment);
				saveOnDisk = null;
			}

			if (this.odyssey != null)
			{
				odyssey.ProgressChanged -= new ProgressChangedHandle(Progress_Changed);
				odyssey.ProgressComment -= new ProgressCommentHandle(Progress_Comment);
				odyssey = null;
			}

			if (this.articleExchange != null)
			{
				articleExchange.ProgressChanged -= new ProgressChangedHandle(Progress_Changed);
				articleExchange.ProgressComment -= new ProgressCommentHandle(Progress_Comment);
				articleExchange = null;
			}

            if (this.tipasa != null)
            {
                tipasa.ProgressChanged -= new ProgressChangedHandle(Progress_Changed);
                tipasa.ProgressComment -= new ProgressCommentHandle(Progress_Comment);
                tipasa = null;
            }

            if (this.worldShareILL != null)
            {
                worldShareILL.ProgressChanged -= new ProgressChangedHandle(Progress_Changed);
                worldShareILL.ProgressComment -= new ProgressCommentHandle(Progress_Comment);
                worldShareILL = null;
            }
		}
		#endregion

		#region Export()
		public void Export(List<ExportUnit> exportUnits)
		{
			Thread t = new Thread(new ParameterizedThreadStart(ExportThread));
			t.Name = "ExportThread";
			t.SetApartmentState(ApartmentState.STA);
			t.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			t.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			t.Start(exportUnits);
		}
		#endregion

		#region GetFolder()
		public static bool GetFolder(ref string folderPath, string caption)
		{
			FolderBrowserDialog browserDlg = new FolderBrowserDialog();

			try{browserDlg.SelectedPath = (File.Exists(folderPath)) ? folderPath : @"c:\";}
			catch{browserDlg.SelectedPath = @"c:\";}
			
			browserDlg.ShowNewFolderButton = true;
			browserDlg.Description = caption;// "Please select directory, where to export Save On Disk articles";

			if (browserDlg.ShowDialog() == DialogResult.OK)
			{
				folderPath = browserDlg.SelectedPath;
				return true;
			}

			return false;
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region ExportThread()
		[STAThread]
		internal void ExportThread(object obj)
		{
			try
			{	
				List<ExportUnit> exportUnits = (List<ExportUnit>)obj;
                List<ExportUnit> exportPassed = new List<ExportUnit>();
                List<ExportUnit> exportFailed = new List<ExportUnit>();
                List<IllException> exportFailedEx = new List<IllException>();
                

				Execution_Started();

				foreach (ExportUnit exportUnit in exportUnits)
				{
					try
					{
						Article_ExecutionStarted(exportUnit);

						BscanILL.Export.ExportFiles.ExportFilesCreator exportFilesCreator = new ExportFiles.ExportFilesCreator();
						exportFilesCreator.ProgressChanged += delegate(double progress)
						{
							if(ArticleProgressChanged != null)
								ArticleProgressChanged(exportUnit, Convert.ToInt32(progress * 100), null);
						};
						exportFilesCreator.DescriptionChanged += delegate(string description)
						{
							if(ArticleProgressComment != null)
								ArticleProgressComment(exportUnit, description);
						};
						exportFilesCreator.Create(null, exportUnit);                        

						switch (exportUnit.ExportType)
						{
							case ExportType.Ariel: this.Ariel.ExportArticle(exportUnit); break;
							case ExportType.ArielPatron: this.Ariel.ExportArticleToPatron(exportUnit); break;
							case ExportType.Email: this.Email.ExportArticle(exportUnit); break;
							case ExportType.Ftp: this.FtpServer.ExportArticle(exportUnit); ; break;
							case ExportType.FtpDir: this.FtpThruFileSystem.ExportArticle(exportUnit); break;
							case ExportType.ILLiad: this.ILLiad.ExportArticle(exportUnit); break;
							case ExportType.Odyssey: this.Odyssey.ExportArticle(exportUnit); break;
							case ExportType.SaveOnDisk: this.SaveOnDisk.ExportArticle(exportUnit); break;
							case ExportType.ArticleExchange: this.ArticleExchange.ExportArticle(exportUnit); break;
                            case ExportType.Tipasa: this.Tipasa.ExportArticle(exportUnit); break;
                            case ExportType.WorldShareILL: this.WorldShareILL.ExportArticle(exportUnit); break;
							case ExportType.Rapido: this.Rapido.ExportArticle(exportUnit); break;
						}

						exportUnit.Status = Hierarchy.ExportStatus.Successfull;
                        exportPassed.Add(exportUnit);

						Article_ExecutionSuccessful(exportUnit);
					}
					catch (Exception ex)
					{
						exportUnit.Status = Hierarchy.ExportStatus.Error;
						exportUnit.Warnings.Add(BscanILL.Misc.Misc.GetErrorMessage(ex));
                        exportFailed.Add(exportUnit);

                        if (ex is IllException)
                        {
                            exportFailedEx.Add((IllException)ex);
                            Article_ExecutionError(exportUnit, (IllException)ex);
                        }
                        else
                        {
                            IllException tempILLEx = new IllException(ErrorCode.UnexpectedException, ex.Message);
                            exportFailedEx.Add(tempILLEx);
                            Article_ExecutionError(exportUnit, tempILLEx);
                        }
					}
				}

				SaveResults(exportUnits);

                if( exportFailed.Count > 0 )
                {
                    //at least one article failed to export - display just 'OK' button
                    //Article_ExecutionError(exportFailed[0], exportFailedEx[0]);
                    throw new Exception("Export Failed!");
                }
                else                    
                {
                    //all articles passed - display just buttons 'keep..' and 'start new..'
                    Execution_FinishedSuccessfully(exportPassed.Count);
                }  
				
			}
			catch (Exception ex)
			{
				Execution_FinishedWithError(ex);
			}
		}
		#endregion
	
		#region SaveResults()
		private static void SaveResults(List<ExportUnit> exportUnits)
		{
            // code below was causing exception when '\Log\' directory was not created before calling this method..
			//FileInfo	log = new FileInfo(Application.StartupPath + @"\Log\" + DateTime.Now.ToString("yyyy-MM") + @".txt");

			//log.Directory.Create();
			//log.Refresh();
		}
		#endregion		

		#region Execution_Started()
		private void Execution_Started()
		{
			if (ExecutionStarted != null)
				ExecutionStarted();
		}
		#endregion

		#region Execution_FinishedSuccessfully()
		private void Execution_FinishedSuccessfully( int count )
		{
			if (ExecutionFinishedSuccessfully != null)
				ExecutionFinishedSuccessfully( count );
		}
		#endregion

		#region Execution_FinishedWithError()
		private void Execution_FinishedWithError(Exception ex)
		{
			if (ExecutionFinishedWithError != null)
				//ExecutionFinishedWithError(ex);
                ExecutionFinishedWithError();
		}
		#endregion

		#region Article_ExecutionStarted()
		private void Article_ExecutionStarted(ExportUnit exportUnit)
		{
			if (ArticleExecutionStarted != null)
				ArticleExecutionStarted(exportUnit);
		}
		#endregion

		#region ArticleExecutionSuccessful()
		private void Article_ExecutionSuccessful(ExportUnit exportUnit)
		{
			if (ArticleExecutionSuccessfull != null)
				ArticleExecutionSuccessfull(exportUnit);
		}
		#endregion

		#region Article_ExecutionError()
		private void Article_ExecutionError(ExportUnit exportUnit, IllException ex)
		{
			if (ArticleExecutionError != null)
				ArticleExecutionError(exportUnit, ex);
		}
		#endregion	

		#region Progress_Changed()
		void Progress_Changed(ExportUnit exportUnit, int progress, string description)
		{
			if (ArticleProgressChanged != null)
				ArticleProgressChanged(exportUnit, progress, description);
		}
		#endregion	

		#region Progress_Comment()
		void Progress_Comment(ExportUnit exportUnit, string comment)
		{
			if (ArticleProgressComment != null)
				ArticleProgressComment(exportUnit, comment);
		}
		#endregion

		#endregion
	}

}
