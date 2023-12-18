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


namespace BscanILL.UI.Frames.Resend
{
	public class FrameResend : FrameBase
	{
		#region variables

		private BscanILL.UI.Frames.Resend.FrameResendUi	frameResendUi;

		#endregion


		#region constructor
		public FrameResend(BscanILL.MainWindow mainWindow)
			: base(mainWindow)
		{
			this.frameResendUi = this.MainWindow.FrameResendUi;
			this.frameResendUi.DeleteDbArticle += new FrameResendUi.DbArticleHnd(FrameResendUi_DeleteDbArticle);
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public System.Windows.Controls.UserControl	UserControl { get { return (System.Windows.Controls.UserControl)this.frameResendUi; } }
		public BscanILL.Hierarchy.Article			Article { get { return this.MainWindow.Article; } }
        public BscanILL.Hierarchy.SessionBatch      Batch { get { return this.MainWindow.Batch; } }

		#endregion


		//PUBLIC METHODS
		#region public methods
	
		#region Activate()
		public void Activate()
		{
			this.frameResendUi.Open(this.Batch);
			this.frameResendUi.Visibility = Visibility.Visible;
			
			this.IsActivated = true;
		}
		#endregion

		#region Deactivate()
		public void Deactivate()
		{
			Reset();
			this.frameResendUi.Visibility = Visibility.Hidden;
			
			this.IsActivated = false;
		}
		#endregion

		#region Reset()
		public void Reset()
		{
			try
			{
				this.frameResendUi.Reset();

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

		#region UserInteracted()
		void UserInteracted()
		{
		}
		#endregion

		#region FrameResendUi_DeleteDbArticle()
		void FrameResendUi_DeleteDbArticle(BscanILLData.Models.DbArticle articleDb)
		{
            /*         if (this.MainWindow.Article != null && this.MainWindow.Article.Id == articleDb.Id)
                       {
                           this.MainWindow.Article.StopAllAutomatedProcessing();
                           this.MainWindow.Article = null;
                       }
                       BscanILL.DB.Database.Instance.SetArticleStatus(articleDb, Hierarchy.ArticleStatus.Deleted);
           */

            this.MainWindow.DeleteArticle(articleDb);
			
		}
		#endregion

		#region FrameResendUi_DeleteDbArticle()
		/*void FrameResendUi_DeleteDbArticle(DB.DbArticle articleDb)
		{
			try
			{
				if (this.MainWindow.Article != null && this.MainWindow.Article.Id == articleDb.Id)
					this.MainWindow.Article = null;

				BscanILL.Hierarchy.Article article = new Hierarchy.Article(articleDb);

				article.Delete();
				BscanILL.DB.Database.Instance.DeleteArticle(articleDb);

			}
#if DEBUG
			catch (Exception ex)
			{
				try
				{
					Console.WriteLine("ERROR!!! FrameResend, FrameResendUi_DeleteDbArticle(): " + BscanILL.Misc.Misc.GetErrorMessage(ex));
					BscanILL.DB.Database.Instance.DeleteArticle(articleDb);
					//throw ex;
				}
				catch (Exception exception)
				{
					throw exception;
				}
			}
#else
			catch (Exception)
			{
				try
				{
					BscanILL.DB.Database.Instance.DeleteArticle(articleDb);
					//throw ex;
				}
				catch (Exception exception)
				{
					throw exception;
				}
			}
#endif
		}*/
		#endregion
	
		#endregion

	}
}
