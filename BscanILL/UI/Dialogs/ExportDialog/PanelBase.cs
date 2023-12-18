using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Reflection;

namespace BscanILL.UI.Dialogs.ExportDialog
{
	public class PanelBase : UserControl, INotifyPropertyChanged
	{
		string fileName = "ILL File";

		protected BscanILL.SETTINGS.Settings	_settings { get { return BscanILL.SETTINGS.Settings.Instance; } }
		protected BscanILL.Hierarchy.Article	article = null;
	
		public event PropertyChangedEventHandler PropertyChanged;


        #region class ComboItemFileColor
        public class ComboItemFileColor
        {
            BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth value;

            public ComboItemFileColor(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth value)
            {
                this.value = value;
            }

            public BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth Value { get { return value; } }

            public override string ToString()
            {
                switch (value)
                {
                    case BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal: return "Bitonal";
                    case BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Grayscale: return "Grayscale";
                    case BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Color: return "Color";
                    case BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto: return "Auto";                    
                }

                return value.ToString();
            }
        }
        #endregion

		#region class ComboItemFileFormat
		public class ComboItemFileFormat
		{
			BscanILL.Scan.FileFormat value;

			public ComboItemFileFormat(BscanILL.Scan.FileFormat value)
			{
				this.value = value;
			}

			public BscanILL.Scan.FileFormat Value { get { return value; } }

			public override string ToString()
			{
				switch (value)
				{
					case Scan.FileFormat.Tiff: return "TIFF";
					case Scan.FileFormat.Pdf: return "PDF";
					case Scan.FileFormat.SPdf: return "Searchable PDF";
					case Scan.FileFormat.Jpeg: return "JPEG";
					case Scan.FileFormat.Png: return "PNG";
					case Scan.FileFormat.Text: return "Rich Text";
					case Scan.FileFormat.Audio: return "Audio";
				}

				return value.ToString();
			}
		}
		#endregion

		#region class ComboItemScannersFileFormat
		public class ComboItemScannersFileFormat
		{
			Scanners.FileFormat value;

			public ComboItemScannersFileFormat(Scanners.FileFormat value)
			{
				this.value = value;
			}

			public Scanners.FileFormat Value { get { return value; } }

			public override string ToString()
			{
				switch (value)
				{
					case Scanners.FileFormat.Tiff: return "TIFF";
					case Scanners.FileFormat.Jpeg: return "JPEG";
					case Scanners.FileFormat.Png: return "PNG";
				}

				return value.ToString();
			}
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties
		
		#region FileNamePrefix
		public string FileNamePrefix
		{
			get { return this.fileName; }
			set
			{
				this.fileName = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region LoadArticle()
		public virtual void LoadArticle(BscanILL.Hierarchy.Article article)
		{
			this.article = article;
			this.FileNamePrefix = GetExportFileNamePrefixPreferringTransactionNumber(article);
		}
		#endregion

        // PRIVATE METHODS

        #region private methods
        private string GetExportFileNameBasedOnSettings(BscanILL.Hierarchy.Article article)
        {
            String returnVal = "";

            if (article.ExportType == Export.ExportType.SaveOnDisk)
            {
                if (_settings.Export.SaveOnDisk.ExportNameBase == SETTINGS.Settings.ExportClass.SaveOnDiskClass.ExportNameBasedOn.TransactionName)
                {
                    if (article.TransactionId.HasValue)
                    {
                        returnVal = article.TransactionId.Value.ToString();
                    }
                }
                else
                    if (_settings.Export.SaveOnDisk.ExportNameBase == SETTINGS.Settings.ExportClass.SaveOnDiskClass.ExportNameBasedOn.IllName)
                    {
                        if (article.IllNumber != null && article.IllNumber.Trim().Length > 0 && article.IllNumber.ToLower() != "n/a")
                        {
                            returnVal = article.IllNumber;
                        }
                    }
            }
            else
                if (article.ExportType == Export.ExportType.FtpDir)
                {
                    if (_settings.Export.FtpDirectory.ExportNameBase == SETTINGS.Settings.ExportClass.FtpDirClass.ExportNameBasedOn.TransactionName)
                    {
                        if (article.TransactionId.HasValue)
                        {
                            returnVal = article.TransactionId.Value.ToString();
                        }
                    }
                    else
                        if (_settings.Export.FtpDirectory.ExportNameBase == SETTINGS.Settings.ExportClass.FtpDirClass.ExportNameBasedOn.IllName)
                        {
                            if (article.IllNumber != null && article.IllNumber.Trim().Length > 0 && article.IllNumber.ToLower() != "n/a")
                            {
                                returnVal = article.IllNumber;
                            }
                        }
                }
                else
                    if (article.ExportType == Export.ExportType.Ftp)
                    {
                        if (_settings.Export.FtpServer.ExportNameBase == SETTINGS.Settings.ExportClass.FtpClass.ExportNameBasedOn.TransactionName)
                        {
                            if (article.TransactionId.HasValue)
                            {
                                returnVal = article.TransactionId.Value.ToString();
                            }
                        }
                        else
                            if (_settings.Export.FtpServer.ExportNameBase == SETTINGS.Settings.ExportClass.FtpClass.ExportNameBasedOn.IllName)
                            {
                                if (article.IllNumber != null && article.IllNumber.Trim().Length > 0 && article.IllNumber.ToLower() != "n/a")
                                {
                                    returnVal = article.IllNumber;
                                }
                            }
                    }
                    else
                        if (article.ExportType == Export.ExportType.Email)
                        {
                            if (_settings.Export.Email.ExportNameBase == SETTINGS.Settings.ExportClass.EmailClass.ExportNameBasedOn.TransactionName)
                            {
                                if (article.TransactionId.HasValue)
                                {
                                    returnVal = article.TransactionId.Value.ToString();
                                }
                            }
                            else
                                if (_settings.Export.Email.ExportNameBase == SETTINGS.Settings.ExportClass.EmailClass.ExportNameBasedOn.IllName)
                                {
                                    if (article.IllNumber != null && article.IllNumber.Trim().Length > 0 && article.IllNumber.ToLower() != "n/a")
                                    {
                                        returnVal = article.IllNumber;
                                    }
                                }
                        }
                        else
                            if (article.ExportType == Export.ExportType.Tipasa)
                            {
                                if (_settings.Export.Tipasa.ExportNameBase == SETTINGS.Settings.ExportClass.TipasaClass.ExportNameBasedOn.TransactionName)
                                {
                                    if (article.TransactionId.HasValue)
                                    {
                                        returnVal = article.TransactionId.Value.ToString();
                                    }
                                }
                                else
                                    if (_settings.Export.Tipasa.ExportNameBase == SETTINGS.Settings.ExportClass.TipasaClass.ExportNameBasedOn.IllName)
                                    {
                                        if (article.IllNumber != null && article.IllNumber.Trim().Length > 0 && article.IllNumber.ToLower() != "n/a")
                                        {
                                            returnVal = article.IllNumber;
                                        }
                                    }
                            }
                            else
                                if (article.ExportType == Export.ExportType.WorldShareILL)
                                {
                                    if (_settings.Export.WorldShareILL.ExportNameBase == SETTINGS.Settings.ExportClass.WorldShareILLClass.ExportNameBasedOn.TransactionName)
                                    {
                                        if (article.TransactionId.HasValue)
                                        {
                                            returnVal = article.TransactionId.Value.ToString();
                                        }
                                    }
                                    else
                                        if (_settings.Export.WorldShareILL.ExportNameBase == SETTINGS.Settings.ExportClass.WorldShareILLClass.ExportNameBasedOn.IllName)
                                        {
                                            if (article.IllNumber != null && article.IllNumber.Trim().Length > 0 && article.IllNumber.ToLower() != "n/a")
                                            {
                                                returnVal = article.IllNumber;
                                            }
                                        }
                                }
                                else
                            if (article.ExportType == Export.ExportType.ArticleExchange)
                            {
                                if (_settings.Export.ArticleExchange.ExportNameBase == SETTINGS.Settings.ExportClass.ArticleExchangeClass.ExportNameBasedOn.TransactionName)
                                {
                                    if (article.TransactionId.HasValue)
                                    {
                                        returnVal = article.TransactionId.Value.ToString();
                                    }
                                }
                                else
                                    if (_settings.Export.ArticleExchange.ExportNameBase == SETTINGS.Settings.ExportClass.ArticleExchangeClass.ExportNameBasedOn.IllName)
                                    {
                                        if (article.IllNumber != null && article.IllNumber.Trim().Length > 0 && article.IllNumber.ToLower() != "n/a")
                                        {
                                            returnVal = article.IllNumber;
                                        }
                                    }
                            }
                            else
                                if (article.ExportType == Export.ExportType.Ariel)
                                {
                                    if (_settings.Export.Ariel.ExportNameBase == SETTINGS.Settings.ExportClass.ArielClass.ExportNameBasedOn.TransactionName)
                                    {
                                        if (article.TransactionId.HasValue)
                                        {
                                            returnVal = article.TransactionId.Value.ToString();
                                        }
                                    }
                                    else
                                        if (_settings.Export.Ariel.ExportNameBase == SETTINGS.Settings.ExportClass.ArielClass.ExportNameBasedOn.IllName)
                                        {
                                            if (article.IllNumber != null && article.IllNumber.Trim().Length > 0 && article.IllNumber.ToLower() != "n/a")
                                            {
                                                returnVal = article.IllNumber;
                                            }
                                        }
                                }
                                else
                                    if (article.ExportType == Export.ExportType.ILLiad)
                                    {
                                        if (_settings.Export.ILLiad.ExportNameBase == SETTINGS.Settings.ExportClass.ILLiadClass.ExportNameBasedOn.TransactionName)
                                        {
                                            if (article.TransactionId.HasValue)
                                            {
                                                returnVal = article.TransactionId.Value.ToString();
                                            }
                                        }
                                        else
                                            if (_settings.Export.ILLiad.ExportNameBase == SETTINGS.Settings.ExportClass.ILLiadClass.ExportNameBasedOn.IllName)
                                            {
                                                if (article.IllNumber != null && article.IllNumber.Trim().Length > 0 && article.IllNumber.ToLower() != "n/a")
                                                {
                                                    returnVal = article.IllNumber;
                                                }
                                            }
                                    }
                                    else
                                        if (article.ExportType == Export.ExportType.Odyssey)
                                        {
                                            if (_settings.Export.Odyssey.ExportNameBase == SETTINGS.Settings.ExportClass.OdysseyClass.ExportNameBasedOn.TransactionName)
                                            {
                                                if (article.TransactionId.HasValue)
                                                {
                                                    returnVal = article.TransactionId.Value.ToString();
                                                }
                                            }
                                            else
                                                if (_settings.Export.Odyssey.ExportNameBase == SETTINGS.Settings.ExportClass.OdysseyClass.ExportNameBasedOn.IllName)
                                                {
                                                    if (article.IllNumber != null && article.IllNumber.Trim().Length > 0 && article.IllNumber.ToLower() != "n/a")
                                                    {
                                                        returnVal = article.IllNumber;
                                                    }
                                                }
                                        }

            return returnVal;
        }
        #endregion

        #region RaisePropertyChanged
        /// <summary>
		/// with get_
		/// </summary>
		/// <param name="propertyName"></param>
		protected void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName.Substring(4)));
		}
		#endregion

		#region GetExportFileNamePrefixPreferringIllNumber()
		
        protected string GetExportFileNamePrefixPreferringIllNumber(BscanILL.Hierarchy.Article article)
		{            
            //first apply prefered export file name setting
            String returnVal = GetExportFileNameBasedOnSettings(article);

            if (returnVal.Length == 0)
            {
                if (article.IllNumber != null && article.IllNumber.Trim().Length > 0 && article.IllNumber.ToLower() != "n/a")
                    return article.IllNumber;
                else if (article.TransactionId.HasValue)
                    return article.TransactionId.Value.ToString();
                else
                    return article.Id.ToString("00000000");
            }
            return returnVal;
		}
		#endregion

		#region GetExportFileNamePrefixPreferringIllNumber()		

        protected string GetExportFileNamePrefixPreferringTransactionNumber(BscanILL.Hierarchy.Article article)
		{
            //first apply prefered export file name setting
            String returnVal = GetExportFileNameBasedOnSettings(article);
            
            if (returnVal.Length == 0)
            {
			    if (article.TransactionId.HasValue)
				    return article.TransactionId.Value.ToString();
			    else if (article.IllNumber != null && article.IllNumber.Trim().Length > 0 && article.IllNumber.ToLower() != "n/a")
				    return article.IllNumber;
			    else
				    return article.Id.ToString("00000000");
            }

            return returnVal;
		}
		#endregion
	
		#endregion

	}
}
