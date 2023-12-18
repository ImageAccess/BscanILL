#define Dynamic_Rapido
//in case of switching to staticly attached library, add RapidoWrapper.dll reference into the BscanILL project and comment out line above

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArticleExchangeWrapper;
using BscanILL.Hierarchy;
using System.IO;
using BscanILL.Misc;
using BscanILL.Export.AdditionalInfo;


namespace BscanILL.Export.Rapido
{
    class Rapido : ExportBasics
    {
        //in debugging mode need to set static otherwise it shows null in rapidoWrapper_ProgressChanged() when debugging
        //when using released exe, the progress bar does not update, so I guess I need this variable as 'static' for relase too
        private static ExportUnit currentExportUnit = null;
        //private ExportUnit currentExportUnit = null;

#if !Dynamic_Rapido
		//bob-no-dll-reference
		RapidoExLibrisWrapper rapidoWrapper ;
#endif

        BscanILL.SETTINGS.Settings.ExportClass.RapidoClass rapidoSettings = BscanILL.SETTINGS.Settings.Instance.Export.Rapido;

		#region constructor
		public Rapido()
		{
            if (BscanILL.SETTINGS.Settings.Instance.Licensing.RapidoEnabled)
            {
                if ((rapidoSettings.ApiKey == null) || (rapidoSettings.ApiKey.Length == 0))
                {
                    throw new Exception("API Key not set!");
                }
                else
                {
                    try
                    {
#if !Dynamic_Rapido
                        rapidoWrapper = new RapidoExLibrisWrapper();
#else
                        BscanILL.Misc.DllsLoader.LoadLibraries(this, rapidoSettings.ApiKey, "rapidoWrapper_ProgressChanged");
#endif
                    }
                    catch (Exception ex)
                    {
                        throw new Exception( ex.Message );
                    }
                }
            }
            else
            {
                throw new Exception("Rapido Export Not Supported in License File! Please Contact DLSG's Tech Support!");
            }

            //if (_settings.Export.ArticleExchange.UpdateILLiad)
            //illiad = ILLiadBasics.GetIlliadInstance();
        }
#endregion

#region destructor
		public void Dispose()
		{
		}
#endregion


        //PUBLIC METHODS
#region public methods

#region ExportArticle()
        public void ExportArticle(ExportUnit exportUnit)
        {
            try
            {
#if !Dynamic_Rapido
                if( rapidoWrapper == null)
#else
                if ( ! BscanILL.Misc.DllsLoader.IsRapidoLoaded)
#endif
                {
                   throw new Exception("Cannot Load Rapido Dlls! Please Contact DLSG's Tech Support!");
                }

#if !Dynamic_Rapido
				rapidoWrapper.Clear();
#else
                BscanILL.Misc.DllsLoader.Clear.Invoke(BscanILL.Misc.DllsLoader.RapidoObject, null);
#endif

#if !Dynamic_Rapido
				rapidoWrapper.ApiKey = rapidoSettings.ApiKey;
#else
                //  no need to set here becasue Api key is passed to dll when calling LoadLibraries( ) to load library dynamically in code above
                //RapidoDLLWrapper.DllsLoader.ApiKey.SetValue(RapidoDLLWrapper.DllsLoader.RapidoObject, rapidoSettings.ApiKey);   
#endif

                Article article = exportUnit.Article;
                FileInfo source = exportUnit.Files[0];

                currentExportUnit = exportUnit;

                Progress_Comment(exportUnit, "---Exporting Article (ILL#:" + exportUnit.Article.IllNumber + ", TN:" + exportUnit.Article.TransactionId + ") via Rapido---");

                Progress_Changed(exportUnit, 5, "Checking data...");                

                //check if Doc ID for Rapido delivery are filled in..
                if ((article.IllNumber == null) || (article.IllNumber.Length == 0))
                {
                    if (article.TransactionId == null) 
                    {
                        throw new Exception("Document ID not set!");
                    }
                }

                //int iNum = 0;
                if (article.IllNumber != null)
                {
                    //iNum = int.Parse(article.IllNumber);  //no need to convert to digits - allow to use string docID - maybe needed with Rapido
#if !Dynamic_Rapido
				    rapidoWrapper.DocID = article.IllNumber;
#else
                    BscanILL.Misc.DllsLoader.DocID.SetValue(BscanILL.Misc.DllsLoader.RapidoObject, article.IllNumber);
#endif
                }
                else
                {
                    //use TN
#if !Dynamic_Rapido
				    rapidoWrapper.DocID = article.TransactionId.ToString();
#else
                    BscanILL.Misc.DllsLoader.DocID.SetValue(BscanILL.Misc.DllsLoader.RapidoObject, article.TransactionId.ToString());
#endif
                }

                SetFile(exportUnit.Files[0]);

                Progress_Changed(exportUnit, 10, "Data checked. Uploading File " + source.Name + "...");

#if !Dynamic_Rapido
                rapidoWrapper.ProgressChanged += new RapidoExLibrisWrapper.ProgressChangedHnd(rapidoWrapper_ProgressChanged);
#else
                if (BscanILL.Misc.DllsLoader.DelegadeForRapido != null)
                {                  
                    BscanILL.Misc.DllsLoader.ProgressChanged.AddEventHandler(BscanILL.Misc.DllsLoader.RapidoObject, BscanILL.Misc.DllsLoader.DelegadeForRapido);
                }
#endif
                try
                {
#if !Dynamic_Rapido				
				    int iExecutionResult = rapidoWrapper.PostArticle();
#else                    
                    int iExecutionResult = (int)BscanILL.Misc.DllsLoader.PostArticle.Invoke(BscanILL.Misc.DllsLoader.RapidoObject, null);
#endif
                    if (iExecutionResult == 0)
                    {
                        Progress_Changed(exportUnit, 100, "Image '" + source.Name + "' uploaded successfully.");
                    }
                    else
                    {
                        Progress_Changed(exportUnit, 100 , "");
#if !Dynamic_Rapido
					    throw new Exception(rapidoWrapper.LastError);
#else
                        string lastErr = (string)BscanILL.Misc.DllsLoader.LastError.GetValue(BscanILL.Misc.DllsLoader.RapidoObject);
                        throw new Exception(lastErr);
#endif
                    }

                    //**** no sending of confirmation email unlike with plain Article Exchange delivery method****
                    //Progress_Comment(exportUnit, "Sending confirmation email...");
                    //SendConfirmationEmail(exportUnit, aeWrapper.RetrievalUrl, aeWrapper.RetrievalPasscode);
                    //Progress_Changed(exportUnit, 50, "Confirmation email sent successfully.");
                    
                }
                catch (Exception ex)
                {
                    Notify(this, Notifications.Type.Error, ex.Message, ex);
                    throw new IllException("Rapido post has failed! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
                }
            }
            catch (IllException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Notify(this, Notifications.Type.Error, ex.Message, ex);
                throw new IllException("Rapido export has failed! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
            }

            finally
            {
#if !Dynamic_Rapido
				rapidoWrapper.ProgressChanged -= new RapidoExLibrisWrapper.ProgressChangedHnd(rapidoWrapper_ProgressChanged);
#else
                if ((BscanILL.Misc.DllsLoader.IsRapidoLoaded) && (BscanILL.Misc.DllsLoader.DelegadeForRapido != null))
                    BscanILL.Misc.DllsLoader.ProgressChanged.RemoveEventHandler(BscanILL.Misc.DllsLoader.RapidoObject, BscanILL.Misc.DllsLoader.DelegadeForRapido);
#endif
                currentExportUnit = null;
            }
        }
#endregion

#endregion

        // PRIVATE METHODS
#region private methods

#region SetFile()
        private void SetFile(FileInfo file)
        {
            switch (file.Extension.ToLower())
            {                
                case ".jpg":
                case ".jpeg":
#if !Dynamic_Rapido
					//rapidoWrapper.FileType = BscanILL.Misc.RapidoFileTypes.Jpeg;
					rapidoWrapper.FileType = RapidoWrapper.RapidoFileTypes.Jpeg;
#else
                    BscanILL.Misc.DllsLoader.FileType.SetValue(BscanILL.Misc.DllsLoader.RapidoObject, BscanILL.Misc.RapidoFileTypes.Jpeg);
#endif
                    break;

                case ".tif":
                case ".tiff":
#if !Dynamic_Rapido
					//rapidoWrapper.FileType = BscanILL.Misc.RapidoFileTypes.Tiff;
					rapidoWrapper.FileType = RapidoWrapper.RapidoFileTypes.Tiff;
#else
                    BscanILL.Misc.DllsLoader.FileType.SetValue(BscanILL.Misc.DllsLoader.RapidoObject, BscanILL.Misc.RapidoFileTypes.Tiff);
#endif
                    break;

                case ".bmp":
#if !Dynamic_Rapido
							//rapidoWrapper.FileType = BscanILL.Misc.RapidoFileTypes.Bmp;
							rapidoWrapper.FileType = RapidoWrapper.RapidoFileTypes.Bmp;
#else
                    BscanILL.Misc.DllsLoader.FileType.SetValue(BscanILL.Misc.DllsLoader.RapidoObject, BscanILL.Misc.RapidoFileTypes.Bmp);
#endif
                    break;

                case ".png":
#if !Dynamic_Rapido
							//rapidoWrapper.FileType = BscanILL.Misc.RapidoFileTypes.Png;
							rapidoWrapper.FileType = RapidoWrapper.RapidoFileTypes.Png;
#else
                    BscanILL.Misc.DllsLoader.FileType.SetValue(BscanILL.Misc.DllsLoader.RapidoObject, BscanILL.Misc.RapidoFileTypes.Png);
#endif
                    break;

                case ".zip":
#if !Dynamic_Rapido
							//rapidoWrapper.FileType = BscanILL.Misc.RapidoFileTypes.Zip;
							rapidoWrapper.FileType = RapidoWrapper.RapidoFileTypes.Zip;
#else
                    BscanILL.Misc.DllsLoader.FileType.SetValue(BscanILL.Misc.DllsLoader.RapidoObject, BscanILL.Misc.RapidoFileTypes.Zip);
#endif
                    break;

                case ".pdf":
#if !Dynamic_Rapido
							//rapidoWrapper.FileType = BscanILL.Misc.RapidoFileTypes.Pdf;
							rapidoWrapper.FileType = RapidoWrapper.RapidoFileTypes.Pdf;
#else
                    BscanILL.Misc.DllsLoader.FileType.SetValue(BscanILL.Misc.DllsLoader.RapidoObject, BscanILL.Misc.RapidoFileTypes.Pdf);
#endif
                    break;

                default:
#if !Dynamic_Rapido
							//rapidoWrapper.FileType = BscanILL.Misc.RapidoFileTypes.None;
							rapidoWrapper.FileType = RapidoWrapper.RapidoFileTypes.None;
#else
                    BscanILL.Misc.DllsLoader.FileType.SetValue(BscanILL.Misc.DllsLoader.RapidoObject, BscanILL.Misc.RapidoFileTypes.None);
#endif
                    break;
            }

#if !Dynamic_Rapido
				rapidoWrapper.FilePath = fi.FullName;
#else     
            BscanILL.Misc.DllsLoader.FileInfo.SetValue(BscanILL.Misc.DllsLoader.RapidoObject, file.FullName);
#endif
        }
#endregion

        private void rapidoWrapper_ProgressChanged(double progress)
        {
            if (currentExportUnit != null)
            { 
              Progress_Changed(currentExportUnit, 10 + Convert.ToInt32(progress * 80.0), "");            //updating progress in range <10,90>            
            }
        }

#endregion
    }
}
