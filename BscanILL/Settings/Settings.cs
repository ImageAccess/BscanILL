using System;
using System.Collections;
using System.Text;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Speech.Synthesis;

using BscanILL.Export;
using BscanILL.Export.Printing;
using BscanILL.Export.ILLiad;
using System.Windows;



namespace BscanILL.SETTINGS
{
	[XmlRoot("ScannerSettings", Namespace = "BscanILL.Settings.Settings")]
	[Serializable]
	public class Settings : INotifyPropertyChanged
	{
		static Settings instance = null;	
		static string	settingsFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\DLSG\BscanILL\Settings\BscanILL.settings"; 

		public GeneralClass			General				{ get; set; }
		public IllClass				ILL					{ get; set; }
		public ImageTreatmentClass	ImageTreatment		{ get; set; }
		public ExportClass			Export				{ get; set; }
		public StatsAndNotifsClass	StatsAndNotifs		{ get; set; }
		public UserInterfaceClass	UserInterface		{ get; set; }
		public LicensingClass		Licensing			{ get; set; }
		public FormsProcessingClass FormsProcessing		{ get; set; }
		public Scanners.Settings	Scanner				{ get; set; }
		
		/*public Scanners.Settings	Scanner
		{
			get { return Scanners.Settings.Instance; }
			set
			{
				Scanners.Settings.Instance = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); 
			}
		}*/

		public event PropertyChangedEventHandler	PropertyChanged;


		#region constructor
		private Settings()
		{			
			this.General = new GeneralClass();
			this.ILL = new IllClass();
			this.Scanner = new Scanners.Settings();
			this.ImageTreatment = new ImageTreatmentClass();
			this.Export = new ExportClass();
			this.StatsAndNotifs = new StatsAndNotifsClass();
			this.UserInterface = new UserInterfaceClass();
			this.Licensing = new LicensingClass();
			this.FormsProcessing = new FormsProcessingClass();

			this.Licensing.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
			{
			};

			this.Scanner.PropertyChanged += new PropertyChangedEventHandler(RaisePropertyChanged);

			//Settings.instance = this;
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		//public Scanners.Settings	Scanner { get { return Scanners.Settings.Instance; } set { Scanners.Settings.Instance = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

		#endregion


		#region class BaseClass
		public abstract class BaseClass : INotifyPropertyChanged
		{
			protected volatile bool raisePropertyChanged = true;
			protected volatile bool propertyChanged = false;
			
			public event PropertyChangedEventHandler PropertyChanged;

			public abstract void CopyFrom(Settings source);

            public enum SubfolderNameBasedOn
            {
                IllName,
                TransactionName
            }

            public enum ExportNameBasedOn
            {
                IllName,
                TransactionName
            }

			#region RaisePropertyChanged()
			public void RaisePropertyChanged(string propertyName)
			{
				if (this.raisePropertyChanged)
				{
					if (PropertyChanged != null)
					{
						if (propertyName != null && (propertyName.StartsWith("get_") || propertyName.StartsWith("set_")))
							PropertyChanged(this, new PropertyChangedEventArgs(propertyName.Substring(4)));
						else
							PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
					}

					this.propertyChanged = false;
				}
				else
					this.propertyChanged = true;
			}
			#endregion

			#region RaisePropertyChanged()
			public void RaisePropertyChanged(object sender, string propertyName)
			{
				if (this.raisePropertyChanged)
				{
					if (PropertyChanged != null)
					{
						if (propertyName != null && (propertyName.StartsWith("get_") || propertyName.StartsWith("set_")))
							propertyName = propertyName.Substring(4);

						PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
					}

					this.propertyChanged = false;
				}
				else
					this.propertyChanged = true;
			}
			#endregion

			#region SetProperty()
			public void SetProperty(ref bool property, bool value, string propertyName)
			{
				if (property != value)
				{
					property = value;
					RaisePropertyChanged(propertyName);
				}
			}

			public void SetProperty(ref string property, string value, string propertyName)
			{
				if (property != value)
				{
					property = value;
					RaisePropertyChanged(propertyName);
				}
			}

			public void SetProperty(ref int property, int value, string propertyName)
			{
				if (property != value)
				{
					property = value;
					RaisePropertyChanged(propertyName);
				}
			}
			
			public void SetProperty(ref uint property, uint value, string propertyName)
			{
				if (property != value)
				{
					property = value;
					RaisePropertyChanged(propertyName);
				}
			}

			public void SetProperty(ref ushort property, ushort value, string propertyName)
			{
				if (property != value)
				{
					property = value;
					RaisePropertyChanged(propertyName);
				}
			}

			public void SetProperty(ref BscanILL.Scan.FileFormat property, BscanILL.Scan.FileFormat value, string propertyName)
			{
				if (property != value)
				{
					property = value;
					RaisePropertyChanged(propertyName);
				}
			}
			#endregion

		}
		#endregion

		#region class GeneralClass
		[Serializable]
		public class GeneralClass : BaseClass
		{
			string	version;
			string	serverLicenseFilesUri;
			bool	kicImportEnabled = false;
			string	kicImportDir = @"c:\KICImport";
			int		keepImagesFor = 30;
			string	_sqliteConnectionString ;
			string _articlesDirectory;
            PdfColorDepth _pdfImportColorDepth = PdfColorDepth.Auto;
            int _pdfImportDpi = 300;
            bool _forceDefaultPdfParams = false;
            bool multiArticleSupportEnabled = false;
            bool previewWindowEnabled = false;
            bool exportDialogEnabled = true;
            OcrEngineProfile ocrEngineProfile = OcrEngineProfile.Speed;

            public enum PdfColorDepth
            {
                Auto,
                Bitonal,
                Grayscale,
                Color
            }

            public enum OcrEngineProfile
            {
                Speed,
                Accuracy                
            }

			#region constructor
			public GeneralClass()
			{
				this.version = "0.0.0";			                
                //this.serverLicenseFilesUri = "https://license.imageaccess.com/SoftDongles/Scanners/";  // 01-15-2019 per Keith SoftDongles virtual directory was broken
                this.serverLicenseFilesUri = "https://license.imageaccess.com/Scanners/";                //he fixed it so it should work again but for safety I remove this subfolder from path in case it breaks again. this path should work always                                                                                                   

				_sqliteConnectionString = string.Format("Data Source=\"{0}\\Data\\BscanILLData.db3\"", this.AppDataDir);
				_articlesDirectory = Path.Combine(this.AppDataDir, "Articles");
			}
			#endregion

			#region Version
			public string Version
			{
				get { return this.version; }
				set { this.version = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
			}
			#endregion

			#region ServerLicenseFilesUri
			public string ServerLicenseFilesUri
			{
				get { return this.serverLicenseFilesUri; }
				set { this.serverLicenseFilesUri = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
			}
			#endregion

			#region PdfImportColor
			public PdfColorDepth PdfImportColor 
			{
				get 
				{
                    return _pdfImportColorDepth;
				}
				set
				{
                    if (_pdfImportColorDepth != value)
					{
                        _pdfImportColorDepth = value;
						RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
					}
				}
			}
			#endregion

            #region PdfImportColor
            public OcrEngineProfile OcrEngProfile
            {
                get
                {
                    return ocrEngineProfile;
                }
                set
                {
                    if (ocrEngineProfile != value)
                    {
                        ocrEngineProfile = value;
                        RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            #endregion

            #region PdfImportDpi
            public int PdfImportDpi
            {
                get
                {
                    return _pdfImportDpi;
                }
                set
                {
                    if (_pdfImportDpi != value)
                    {
                        _pdfImportDpi = value;
                        RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            #endregion

            #region ForceDefaultPdfParams
            public bool ForceDefaultPdfParams
            {
                get
                {
                    return _forceDefaultPdfParams;
                }
                set
                {
                    if (_forceDefaultPdfParams != value)
                    {
                        _forceDefaultPdfParams = value;
                        RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            #endregion

			#region ArticlesDir
			/// <summary>
			/// this.AppDataDir + @"\Articles";
			/// </summary>
			public string ArticlesDir 
			{
				get 
				{
					//string imageDir = Path.Combine(@"\\CORP-SRV\T_Drive\Jirka", "Articles");
					Directory.CreateDirectory(_articlesDirectory);
					return _articlesDirectory;
				}
				set
				{
					if (_articlesDirectory != value)
					{
						_articlesDirectory = value;
						RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
					}
				}
			}
			#endregion

			#region DatabaseFilePath
			/// <summary>
			/// Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\DLSG\BscanILL\Data\BscanILLData.db3";
			/// </summary>
			public string DatabaseFilePath
			{
				get { return Path.Combine(this.AppDataDir, @"Data\BscanILLData.db3"); }
			}
			#endregion

			#region AppDataDir
			/// <summary>
			/// Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\DLSG\BscanILL";
			/// </summary>
			public string AppDataDir
			{
				get 
				{
					string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"DLSG\BscanILL");
					Directory.CreateDirectory(path);
					return path;
				}
			}
			#endregion

			#region SQLiteConnectionString
			public string SQLiteConnectionString
			{
				get { return _sqliteConnectionString; }
				set
				{
					if (this._sqliteConnectionString != value)
					{
						this._sqliteConnectionString = value;
						RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
					}
				}
			}
			#endregion

			#region TempDir
			/// <summary>
			/// this.AppDataDir + @"\temp";
			/// </summary>
			public string TempDir
			{
				get 
				{ 
					string path = Path.Combine(this.AppDataDir, "temp");
					Directory.CreateDirectory(path);
					return path;
				}
			}
			#endregion

            #region ErrLogDir
            /// <summary>
            /// this.AppDataDir + @"\Log";
            /// </summary>
            public string ErrLogDir
            {
                get
                {
                    string path = Path.Combine(this.AppDataDir, "ErrorLog");
                    Directory.CreateDirectory(path);
                    return path;
                }
            }
            #endregion

			#region LicenseDir
			/// <summary>
			/// this.AppDataDir + @"\License";
			/// </summary>
			public string LicenseDir
			{
				get
				{
					string path = Path.Combine(this.AppDataDir, "License");
					Directory.CreateDirectory(path);
					return path;
				}
			}
			#endregion

            #region OCRLicenseDir
            /// <summary>
            /// this.AppDataDir + @"\OCRLicense";
            /// location of Abbyy local license file
            /// </summary>
            public string OCRLicenseDir
            {
                get
                {
                    string path = Path.Combine(this.AppDataDir, "OCRLicense");
                    Directory.CreateDirectory(path);
                    return path;
                }
            }
            #endregion

            #region GsDir
            /// <summary>
            /// Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\DLSG\BscanILL\GS";
            /// </summary>
            public string GsDir
            {
                get
                {
                    string path = Path.Combine(this.AppDataDir, @"GS");
                    //Directory.CreateDirectory(path);
                    return path;
                }
            }
            #endregion

			#region IrisBinDir
			/// <summary>
			/// Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\DLSG\BscanILL\IRIS\bin";
			/// </summary>
			public string IrisBinDir
			{
				get
				{
					string path = Path.Combine(this.AppDataDir, @"IRIS\bin");
					//Directory.CreateDirectory(path);
					return path;
				}
			}
			#endregion

			#region IrisResourcesDir
			/// <summary>
			/// Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\DLSG\BscanILL\IRIS\resources";
			/// </summary>
			public string IrisResourcesDir
			{
				get
				{
					string path = Path.Combine(this.AppDataDir, @"IRIS\resources");
					//Directory.CreateDirectory(path);
					return path;
				}				
			}
			#endregion

			#region KicImportEnabled
			public bool KicImportEnabled
			{
				get { return this.kicImportEnabled; }
				set { this.kicImportEnabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
			}
			#endregion

            #region MultiArticleSupportEnabled
            public bool MultiArticleSupportEnabled
			{
                get { return this.multiArticleSupportEnabled; }
                set { this.multiArticleSupportEnabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
			}
			#endregion

            #region PreviewWindowEnabled
            public bool PreviewWindowEnabled
			{
                get { return this.previewWindowEnabled; }
                set { this.previewWindowEnabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
			}
			#endregion

            #region ExportDialogEnabled
            public bool ExportDialogEnabled
			{
                get { return this.exportDialogEnabled; }
                set { this.exportDialogEnabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
			}
			#endregion

			#region KicImportDir
			public string KicImportDir
			{
				get { return this.kicImportDir; }
				set { this.kicImportDir = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
			}
			#endregion

			#region KeepArticlesFor
			public int KeepArticlesFor
			{
				get { return this.keepImagesFor; }
				set
				{
					if (this.keepImagesFor != value)
					{
						this.keepImagesFor = value;
						RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
					}
				}
			}
			#endregion


			#region CopyFrom()
			public override void CopyFrom(Settings source)
			{
				this.Version = source.General.Version;
				this.ServerLicenseFilesUri = source.General.ServerLicenseFilesUri;
				this.KicImportEnabled = source.General.KicImportEnabled;
                this.MultiArticleSupportEnabled = source.General.MultiArticleSupportEnabled;
                this.PreviewWindowEnabled = source.General.PreviewWindowEnabled;                
                this.ExportDialogEnabled = source.General.ExportDialogEnabled;                     
				this.KicImportDir = source.General.KicImportDir;
				this.KeepArticlesFor = source.General.KeepArticlesFor;
				this.ArticlesDir = source.General.ArticlesDir;
                this.PdfImportColor = source.General.PdfImportColor;
                this.PdfImportDpi = source.General.PdfImportDpi;
                this.ForceDefaultPdfParams = source.General.ForceDefaultPdfParams;
                this.OcrEngProfile = source.General.OcrEngProfile;                 
			}
			#endregion
		}
		#endregion

		#region class IllClass
		[Serializable]
		public class IllClass : BaseClass
		{
			string			pullslipsDirectory = @"c:\Pullslips";

			public IllClass()
			{
			}

			public string PullslipsDirectory
			{
				get { return this.pullslipsDirectory; }
				set 
				{
					if (this.pullslipsDirectory != value)
					{
						this.pullslipsDirectory = value;
						RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
					}
				}
			}


			#region CopyFrom()
			public override void CopyFrom(Settings source)
			{
				this.PullslipsDirectory = source.ILL.PullslipsDirectory;
			}
			#endregion

		}
		#endregion
	
		#region class ImageTreatmentClass
		[Serializable]
		public class ImageTreatmentClass : BaseClass
		{
			public AutoImageTreatmentClass		AutoImageTreatment { get; set; }
			public ManualImageTreatmentClass	ManualImageTreatment { get; set; }


			#region constructor
			public ImageTreatmentClass()
			{
				this.AutoImageTreatment = new AutoImageTreatmentClass();
				this.ManualImageTreatment = new ManualImageTreatmentClass();
			}
			#endregion


			#region class AutoImageTreatmentClass
			[Serializable]
			public class AutoImageTreatmentClass : BaseClass
			{
				public ContentLocationClass ContentLocation		{ get; set; }
				public ImageDependencyClass ImageDependency { get; set; }

				#region constructor
				public AutoImageTreatmentClass()
				{
					this.ContentLocation = new ContentLocationClass();
					this.ImageDependency = new ImageDependencyClass();
				}
				#endregion

				#region class ContentLocationClass
				[Serializable]
				public class ContentLocationClass : BaseClass
				{
					double contentOffsetInches = 0.2;

					#region constructor
					public ContentLocationClass()
					{
					}
					#endregion

					#region ContentOffsetInches
					public double ContentOffsetInches
					{
						get { return this.contentOffsetInches; }
						set { this.contentOffsetInches = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
					}
					#endregion

					#region CopyFrom()
					public override void CopyFrom(Settings source)
					{
						this.ContentOffsetInches = source.ImageTreatment.AutoImageTreatment.ContentLocation.ContentOffsetInches;
					}
					#endregion
				}
				#endregion

				#region class ImageDependencyClass
				[Serializable]
				public class ImageDependencyClass : BaseClass
				{
					double dependencyHorizontalToleranceInches = 0.0;
					double dependencyVerticalToleranceInches = 0.0;
					bool setDependencyAutomatically = false;

					#region constructor
					public ImageDependencyClass()
					{
					}
					#endregion

					#region SetDependencyAutomatically
					public bool SetDependencyAutomatically
					{
						get { return this.setDependencyAutomatically; }
						set { this.setDependencyAutomatically = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
					}
					#endregion

					#region DependencyHorizontalToleranceInches
					public double DependencyHorizontalToleranceInches
					{
						get { return this.dependencyHorizontalToleranceInches; }
						set { this.dependencyHorizontalToleranceInches = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
					}
					#endregion

					#region DependencyVerticalToleranceInches
					public double DependencyVerticalToleranceInches
					{
						get { return this.dependencyVerticalToleranceInches; }
						set { this.dependencyVerticalToleranceInches = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
					}
					#endregion

					#region CopyFrom()
					public override void CopyFrom(Settings source)
					{
						this.SetDependencyAutomatically = source.ImageTreatment.AutoImageTreatment.ImageDependency.SetDependencyAutomatically;
						this.DependencyHorizontalToleranceInches = source.ImageTreatment.AutoImageTreatment.ImageDependency.DependencyHorizontalToleranceInches;
						this.DependencyVerticalToleranceInches = source.ImageTreatment.AutoImageTreatment.ImageDependency.DependencyVerticalToleranceInches;
					}
					#endregion
				}
				#endregion

				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.ContentLocation.CopyFrom(source);
					this.ImageDependency.CopyFrom(source);
				}
				#endregion
			}
			#endregion

			#region class ManualImageTreatmentClass
			[Serializable]
			public class ManualImageTreatmentClass : BaseClass
			{
				#region constructor
				public ManualImageTreatmentClass()
				{
				}
				#endregion

				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
				}
				#endregion
			}
			#endregion


			#region CopyFrom()
			public override void CopyFrom(Settings source)
			{
				this.AutoImageTreatment.CopyFrom(source);
				this.ManualImageTreatment.CopyFrom(source);
			}
			#endregion

		}
		#endregion

		#region class ExportClass
		[Serializable]
		public class ExportClass : BaseClass
		{
			public PdfClass Pdf { get; set; }
			public AudioClass Audio { get; set; }

			public EmailClass Email { get; set; }
			public EmailClientClass DlsgClient { get; set; }
			public PrinterClass Printer { get; set; }
			public SharedDiskClass SharedDisk { get; set; }
			public SaveOnDiskClass SaveOnDisk { get; set; }
			public CloudClass Cloud { get; set; }
			public ArielClass Ariel { get; set; }
			public ILLiadClass ILLiad { get; set; }
			//public FtpServerClass	FtpServer	{ get; set; }
			public OdysseyClass Odyssey { get; set; }
			public FtpClass FtpServer { get; set; }
			public FtpDirClass FtpDirectory { get; set; }
			public ArticleExchangeClass ArticleExchange { get; set; }
			public TipasaClass Tipasa { get; set; }
			public WorldShareILLClass WorldShareILL { get; set; }
			public RapidoClass Rapido { get; set; }

			bool advancedMultiTiff = false;
			bool advancedMultiPdf = true;
			bool advancedMultiText = false;
			bool advancedMultiAudio = true;
			bool advancedSearchablePdf = true;
			bool isPdfaDefault = false;

			#region constructor
			public ExportClass()
			{
				this.Pdf = new PdfClass();
				this.Audio = new AudioClass();

				this.Email = new EmailClass();
				this.DlsgClient = new EmailClientClass();
				this.Printer = new PrinterClass();
				this.SharedDisk = new SharedDiskClass();
				this.SaveOnDisk = new SaveOnDiskClass();
				this.Cloud = new CloudClass();
				this.Ariel = new ArielClass();
				this.ILLiad = new ILLiadClass();
				this.Odyssey = new OdysseyClass();
				this.FtpServer = new FtpClass();
				this.FtpDirectory = new FtpDirClass();
				this.ArticleExchange = new ArticleExchangeClass();
				this.Tipasa = new TipasaClass();
				this.WorldShareILL = new WorldShareILLClass();
				this.Rapido = new RapidoClass();

				this.advancedMultiTiff = false;
				this.advancedMultiPdf = true;
				this.advancedMultiText = false;
				this.advancedMultiAudio = true;
				this.advancedSearchablePdf = true;
				this.isPdfaDefault = false;
			}
			#endregion


			public bool AdvancedMultiTiff { get { return advancedMultiTiff; } set { SetProperty(ref advancedMultiTiff, value, MethodBase.GetCurrentMethod().Name); } }
			public bool AdvancedMultiPdf { get { return advancedMultiPdf; } set { SetProperty(ref advancedMultiPdf, value, MethodBase.GetCurrentMethod().Name); } }
			public bool AdvancedMultiText { get { return advancedMultiText; } set { SetProperty(ref advancedMultiText, value, MethodBase.GetCurrentMethod().Name); } }
			public bool AdvancedMultiAudio { get { return advancedMultiAudio; } set { SetProperty(ref advancedMultiAudio, value, MethodBase.GetCurrentMethod().Name); } }
			public bool AdvancedSearchablePdf { get { return advancedSearchablePdf; } set { SetProperty(ref advancedSearchablePdf, value, MethodBase.GetCurrentMethod().Name); } }
			public bool IsPdfaDefault { get { return isPdfaDefault; } set { SetProperty(ref isPdfaDefault, value, MethodBase.GetCurrentMethod().Name); } }


			#region CopyFrom()
			public override void CopyFrom(Settings source)
			{
				this.AdvancedMultiTiff = source.Export.AdvancedMultiTiff;
				this.AdvancedMultiPdf = source.Export.AdvancedMultiPdf;
				this.AdvancedMultiText = source.Export.AdvancedMultiText;
				this.AdvancedMultiAudio = source.Export.AdvancedMultiAudio;
				this.AdvancedSearchablePdf = source.Export.AdvancedSearchablePdf;
				this.IsPdfaDefault = source.Export.IsPdfaDefault;

				this.Pdf.CopyFrom(source);
				this.Audio.CopyFrom(source);

				this.Email.CopyFrom(source);
				this.DlsgClient.CopyFrom(source);
				this.Printer.CopyFrom(source);
				this.SharedDisk.CopyFrom(source);
				this.SaveOnDisk.CopyFrom(source);
				this.Cloud.CopyFrom(source);
				this.Ariel.CopyFrom(source);
				this.ILLiad.CopyFrom(source);
				this.Odyssey.CopyFrom(source);
				this.FtpServer.CopyFrom(source);
				this.FtpDirectory.CopyFrom(source);
				this.ArticleExchange.CopyFrom(source);
				this.Tipasa.CopyFrom(source);
				this.WorldShareILL.CopyFrom(source);
				this.Rapido.CopyFrom(source);
			}
			#endregion

			#region class PdfClass
			[Serializable]
			public class PdfClass : BaseClass
			{
				public int Resolution { get; set; }

				public PdfClass()
				{
					this.Resolution = 150;
				}

				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.Resolution = source.Export.Pdf.Resolution;
				}
				#endregion

			}
			#endregion

			#region class AudioClass
			[Serializable]
			public class AudioClass : BaseClass
			{
				public bool Enabled { get; set; }
				public string DefaultVoice { get; set; }

				public AudioClass()
				{
					this.Enabled = true;
					this.DefaultVoice = "";

					SpeechSynthesizer speechEngine = new SpeechSynthesizer();
					ReadOnlyCollection<InstalledVoice> voices = speechEngine.GetInstalledVoices();

					foreach (InstalledVoice voice in voices)
						if (voice.Enabled)
						{
							this.DefaultVoice = voice.VoiceInfo.Name;
							break;
						}
				}

				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.Enabled = source.Export.Audio.Enabled;
					this.DefaultVoice = source.Export.Audio.DefaultVoice;
				}
				#endregion

			}
			#endregion

			#region class EmailClientClass
			[Serializable]
			public class EmailClientClass : BaseClass
			{
				bool enabled = true;
				string smtpServer = "smtp.imageaccess.com";
				ushort port = 25;
				bool sslEncryption = false;
				bool defaultCredentials = true;
				string from = "Bscan ILL Site " + Environment.MachineName + " <BscanILL-Operations@imageaccess.com>";
				int sizeLimitInMB = 13;

				public EmailClientClass()
				{
				}

				public bool Enabled { get { return this.enabled; } set { SetProperty(ref this.enabled, value, MethodBase.GetCurrentMethod().Name); } }
				public string SmtpServer { get { return this.smtpServer; } set { SetProperty(ref this.smtpServer, value, MethodBase.GetCurrentMethod().Name); } }
				public ushort Port { get { return this.port; } set { SetProperty(ref this.port, value, MethodBase.GetCurrentMethod().Name); } }
				public bool SslEncryption { get { return this.sslEncryption; } set { SetProperty(ref this.sslEncryption, value, MethodBase.GetCurrentMethod().Name); } }
				public bool DefaultCredentials { get { return this.defaultCredentials; } set { SetProperty(ref this.defaultCredentials, value, MethodBase.GetCurrentMethod().Name); } }
				public string From { get { return this.from; } set { SetProperty(ref this.from, value, MethodBase.GetCurrentMethod().Name); } }
				public int SizeLimitInMB { get { return this.sizeLimitInMB; } set { SetProperty(ref this.sizeLimitInMB, value, MethodBase.GetCurrentMethod().Name); } }


				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.raisePropertyChanged = false;

					this.Enabled = source.Export.DlsgClient.Enabled;
					this.SmtpServer = source.Export.DlsgClient.SmtpServer;
					this.Port = source.Export.DlsgClient.Port;
					this.SslEncryption = source.Export.DlsgClient.SslEncryption;
					this.DefaultCredentials = source.Export.DlsgClient.DefaultCredentials;
					this.From = source.Export.DlsgClient.From;
					this.SizeLimitInMB = source.Export.DlsgClient.SizeLimitInMB;

					this.raisePropertyChanged = true;

					if (this.propertyChanged)
						RaisePropertyChanged(this, null);
				}
				#endregion

			}
			#endregion

			#region class EmailClass
			[Serializable]
			public class EmailClass : EmailClientClass
			{
				public enum EmailMethodBasedOn
				{
					HTTP,
					SMTP,
					Both   //try first HTTP, if validation fails ->try smtp method -> was turned off this method to match KIC gui
				}

				bool exportEnabled = true;
				string username = "";
				string password = "";
				string subject = "Images from Bscan ILL.";
				string body = "";

				bool emailValidation = true;
				bool updateILLiad = false;
				bool changeRequestToFinished = false;
				public BscanILL.Scan.FileFormat fileFormat = Scan.FileFormat.Pdf;
				ExportNameBasedOn exportNameBase = ExportNameBasedOn.IllName;
				EmailMethodBasedOn emailDeliveryType = EmailMethodBasedOn.HTTP;
				Settings.GeneralClass.PdfColorDepth fileExportColorMode = Settings.GeneralClass.PdfColorDepth.Auto;
				int fileExportQuality = 85;

				public EmailClass()
				{
					this.SmtpServer = "";
					this.Port = 25;
					this.SslEncryption = false;
					this.DefaultCredentials = true;
					this.From = "";
					this.SizeLimitInMB = 20;
				}

				public bool ExportEnabled { get { return this.exportEnabled; } set { SetProperty(ref this.exportEnabled, value, MethodBase.GetCurrentMethod().Name); } }
				public bool IsExportEnabled { get { return this.Enabled && this.exportEnabled; } }
				public string Username { get { return this.username; } set { SetProperty(ref this.username, value, MethodBase.GetCurrentMethod().Name); } }
				public string Password { get { return this.password; } set { SetProperty(ref this.password, value, MethodBase.GetCurrentMethod().Name); } }
				public string Subject { get { return this.subject; } set { SetProperty(ref this.subject, value, MethodBase.GetCurrentMethod().Name); } }
				public string Body { get { return this.body; } set { SetProperty(ref this.body, value, MethodBase.GetCurrentMethod().Name); } }

				public bool EmailValidation { get { return this.emailValidation; } set { SetProperty(ref this.emailValidation, value, MethodBase.GetCurrentMethod().Name); } }
				public bool UpdateILLiad { get { return this.updateILLiad; } set { SetProperty(ref this.updateILLiad, value, MethodBase.GetCurrentMethod().Name); ; } }
				public bool ChangeRequestToFinished { get { return this.changeRequestToFinished; } set { SetProperty(ref this.changeRequestToFinished, value, MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Scan.FileFormat FileFormat { get { return this.fileFormat; } set { SetProperty(ref this.fileFormat, value, MethodBase.GetCurrentMethod().Name); } }
				public ExportNameBasedOn ExportNameBase { get { return this.exportNameBase; } set { this.exportNameBase = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public EmailMethodBasedOn EmailDeliveryType { get { return this.emailDeliveryType; } set { this.emailDeliveryType = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public Settings.GeneralClass.PdfColorDepth FileExportColorMode { get { return this.fileExportColorMode; } set { this.fileExportColorMode = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public int FileExportQuality { get { return this.fileExportQuality; } set { this.fileExportQuality = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.raisePropertyChanged = false;

					this.Enabled = source.Export.Email.Enabled;
					this.SmtpServer = source.Export.Email.SmtpServer;
					this.Port = source.Export.Email.Port;
					this.SslEncryption = source.Export.Email.SslEncryption;
					this.EmailValidation = source.Export.Email.EmailValidation;
					this.DefaultCredentials = source.Export.Email.DefaultCredentials;
					this.From = source.Export.Email.From;
					this.SizeLimitInMB = source.Export.Email.SizeLimitInMB;

					this.ExportEnabled = source.Export.Email.ExportEnabled;
					this.Username = source.Export.Email.Username;
					this.Password = source.Export.Email.Password;
					this.Subject = source.Export.Email.Subject;
					this.Body = source.Export.Email.Body;

					this.EmailValidation = source.Export.Email.EmailValidation;
					this.UpdateILLiad = source.Export.Email.UpdateILLiad;
					this.ChangeRequestToFinished = source.Export.Email.ChangeRequestToFinished;
					this.FileFormat = source.Export.Email.FileFormat;
					this.ExportNameBase = source.Export.Email.ExportNameBase;
					this.EmailDeliveryType = source.Export.Email.EmailDeliveryType;
					this.FileExportColorMode = source.Export.Email.FileExportColorMode;
					this.FileExportQuality = source.Export.Email.FileExportQuality;

					this.raisePropertyChanged = true;

					if (this.propertyChanged)
						RaisePropertyChanged(this, null);
				}
				#endregion
			}
			#endregion

			#region class PrinterClass
			[Serializable]
			public class PrinterClass : BaseClass
			{
				PrinterProfiles printerProfiles = new PrinterProfiles();
				BscanILL.Export.Printing.Functionality printFunctionality = BscanILL.Export.Printing.Functionality.Xps;


				#region constructor
				public PrinterClass()
				{
					printerProfiles.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Profiles_CollectionChanged);
				}
				#endregion


				#region Enabled
				public bool Enabled
				{
					get { return printerProfiles.Count > 0; }
					set { RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
				}
				#endregion

				#region PrinterProfiles
				public PrinterProfiles PrinterProfiles
				{
					get { return this.printerProfiles; }
					set
					{
						this.printerProfiles = value;

						RaisePropertyChanged("Enabled");
						RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
					}
				}
				#endregion

				#region PrintFunctionality
				public BscanILL.Export.Printing.Functionality PrintFunctionality
				{
					get { return this.printFunctionality; }
					set
					{
						this.printFunctionality = value;
						RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
					}
				}
				#endregion


				#region Profiles_CollectionChanged()
				void Profiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
				{
					RaisePropertyChanged("Enabled");
					RaisePropertyChanged("PrinterProfiles");
				}
				#endregion

				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.PrinterProfiles = source.Export.Printer.PrinterProfiles;
					this.PrintFunctionality = source.Export.Printer.PrintFunctionality;
				}
				#endregion
			}
			#endregion

			#region class FtpClass
			[Serializable]
			public class FtpClass : BaseClass
			{
				bool enabled = true;
				ObservableCollection<BscanILL.Export.FTP.FtpLogin> ftpProfiles = new ObservableCollection<BscanILL.Export.FTP.FtpLogin>();
				bool sendConfirmationEmail = false;
				bool updateILLiad = false;
				bool changeRequestToFinished = false;
				BscanILL.Scan.FileFormat fileFormat = BscanILL.Scan.FileFormat.Pdf;
				bool saveToSubfolder = false;
				SubfolderNameBasedOn subFolderNameBase = SubfolderNameBasedOn.IllName;
				ExportNameBasedOn exportNameBase = ExportNameBasedOn.TransactionName;
				Settings.GeneralClass.PdfColorDepth fileExportColorMode = Settings.GeneralClass.PdfColorDepth.Auto;
				int fileExportQuality = 85;

				public FtpClass()
				{
				}

				public bool Enabled { get { return this.enabled; } set { this.enabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public ObservableCollection<BscanILL.Export.FTP.FtpLogin> FtpProfiles { get { return this.ftpProfiles; } set { this.ftpProfiles = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool SendConfirmationEmail { get { return this.sendConfirmationEmail; } set { this.sendConfirmationEmail = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool UpdateILLiad { get { return this.updateILLiad; } set { this.updateILLiad = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool ChangeRequestToFinished { get { return this.changeRequestToFinished; } set { this.changeRequestToFinished = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Scan.FileFormat ExportFileFormat { get { return this.fileFormat; } set { this.fileFormat = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool SaveToSubfolder { get { return this.saveToSubfolder; } set { this.saveToSubfolder = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public SubfolderNameBasedOn SubFolderNameBase { get { return this.subFolderNameBase; } set { this.subFolderNameBase = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public ExportNameBasedOn ExportNameBase { get { return this.exportNameBase; } set { this.exportNameBase = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public Settings.GeneralClass.PdfColorDepth FileExportColorMode { get { return this.fileExportColorMode; } set { this.fileExportColorMode = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public int FileExportQuality { get { return this.fileExportQuality; } set { this.fileExportQuality = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.Enabled = source.Export.FtpServer.Enabled;
					this.FtpProfiles = source.Export.FtpServer.FtpProfiles;
					this.SendConfirmationEmail = source.Export.FtpServer.SendConfirmationEmail;
					this.UpdateILLiad = source.Export.FtpServer.UpdateILLiad;
					this.ChangeRequestToFinished = source.Export.FtpServer.ChangeRequestToFinished;
					this.ExportFileFormat = source.Export.FtpServer.ExportFileFormat;
					this.SaveToSubfolder = source.Export.FtpServer.SaveToSubfolder;
					this.SubFolderNameBase = source.Export.FtpServer.SubFolderNameBase;
					this.ExportNameBase = source.Export.FtpServer.ExportNameBase;
					this.FileExportColorMode = source.Export.FtpServer.FileExportColorMode;
					this.FileExportQuality = source.Export.FtpServer.FileExportQuality;
				}
				#endregion
			}
			#endregion

			#region class SharedDiskClass
			[Serializable]
			public class SharedDiskClass : BaseClass
			{
				bool enabled = false;

				public string CentralStationUrl { get; set; }
				public string ImagesDirectory { get; set; }
				public ushort CentralStationPort { get; set; }
				public bool WindowsCredentials { get; set; }
				public string Domain { get; set; }
				public string Username { get; set; }
				public string Password { get; set; }
				public string Path { get; set; }

				public bool Enabled
				{
					get { return this.enabled; }
					set
					{
						this.enabled = value;
						RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
					}
				}

				public SharedDiskClass()
				{
					this.Path = @"C:\BscanILLStorage\SharedDisk";
					this.CentralStationUrl = "localhost";
					this.ImagesDirectory = @"c:\MobileILL";
					this.CentralStationPort = 1233;
					this.WindowsCredentials = true;
					this.Domain = "";
					this.Username = "";
					this.Password = "";
				}

				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.Enabled = source.Export.SharedDisk.Enabled;
					this.CentralStationUrl = source.Export.SharedDisk.CentralStationUrl;
					this.ImagesDirectory = source.Export.SharedDisk.ImagesDirectory;
					this.CentralStationPort = source.Export.SharedDisk.CentralStationPort;
					this.WindowsCredentials = source.Export.SharedDisk.WindowsCredentials;
					this.Domain = source.Export.SharedDisk.Domain;
					this.Username = source.Export.SharedDisk.Username;
					this.Password = source.Export.SharedDisk.Password;
					this.Path = source.Export.SharedDisk.Path;
				}
				#endregion
			}
			#endregion

			#region class SaveOnDiskClass
			[Serializable]
			public class SaveOnDiskClass : BaseClass
			{
				bool enabled = true;
				string exportDirPath = @"c:\BscanILL-Export";
				bool updateILLiad = false;
				bool changeRequestToFinished = false;
				BscanILL.Scan.FileFormat fileFormat = BscanILL.Scan.FileFormat.Pdf;
				ActionBeforeExport beforeExport = ActionBeforeExport.KeepExistingFiles;
				bool saveToSubfolder = true;
				SubfolderNameBasedOn subFolderNameBase = SubfolderNameBasedOn.IllName;
				ExportNameBasedOn exportNameBase = ExportNameBasedOn.TransactionName;
				Settings.GeneralClass.PdfColorDepth fileExportColorMode = Settings.GeneralClass.PdfColorDepth.Auto;
				int fileExportQuality = 85;

				public SaveOnDiskClass()
				{
				}

				public enum ActionBeforeExport
				{
					KeepExistingFiles,
					CleanExportDir
				}

				/*                
								public enum SubfolderNameBasedOn
								{
									IllName,
									TransactionName
								}

								public enum ExportNameBasedOn
								{
									IllName,
									TransactionName
								}
				*/

				public bool Enabled { get { return this.enabled; } set { this.enabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string ExportDirPath { get { return this.exportDirPath; } set { this.exportDirPath = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool UpdateILLiad { get { return this.updateILLiad; } set { this.updateILLiad = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool ChangeRequestToFinished { get { return this.changeRequestToFinished; } set { this.changeRequestToFinished = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Scan.FileFormat ExportFileFormat { get { return this.fileFormat; } set { this.fileFormat = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public ActionBeforeExport BeforeExport { get { return this.beforeExport; } set { this.beforeExport = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool SaveToSubfolder { get { return this.saveToSubfolder; } set { this.saveToSubfolder = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public SubfolderNameBasedOn SubFolderNameBase { get { return this.subFolderNameBase; } set { this.subFolderNameBase = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public ExportNameBasedOn ExportNameBase { get { return this.exportNameBase; } set { this.exportNameBase = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public Settings.GeneralClass.PdfColorDepth FileExportColorMode { get { return this.fileExportColorMode; } set { this.fileExportColorMode = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public int FileExportQuality { get { return this.fileExportQuality; } set { this.fileExportQuality = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.Enabled = source.Export.SaveOnDisk.Enabled;
					this.ExportDirPath = source.Export.SaveOnDisk.ExportDirPath;
					this.UpdateILLiad = source.Export.SaveOnDisk.UpdateILLiad;
					this.ChangeRequestToFinished = source.Export.SaveOnDisk.ChangeRequestToFinished;
					this.ExportFileFormat = source.Export.SaveOnDisk.ExportFileFormat;
					this.BeforeExport = source.Export.SaveOnDisk.BeforeExport;
					this.SaveToSubfolder = source.Export.SaveOnDisk.SaveToSubfolder;
					this.SubFolderNameBase = source.Export.SaveOnDisk.SubFolderNameBase;
					this.ExportNameBase = source.Export.SaveOnDisk.ExportNameBase;
					this.FileExportColorMode = source.Export.SaveOnDisk.FileExportColorMode;
					this.FileExportQuality = source.Export.SaveOnDisk.FileExportQuality;
				}
				#endregion
			}
			#endregion

			#region class CloudClass
			[Serializable]
			public class CloudClass : BaseClass
			{
				bool googleDocsEnabled = false;
				bool qrCodeEnabled = false;
				string qrCodeUploadUrl = @"http://QRUploader.dlsg.net/Images/";
				string qrCodeDownloadUrl = @"http://dlsg.net/qr/";


				#region constructor
				public CloudClass()
				{
				}
				#endregion

				#region Enabled
				public bool Enabled
				{
					get { return this.GoogleDocsEnabled || this.QrCodeEnabled; }
				}
				#endregion

				#region GoogleDocsEnabled
				public bool GoogleDocsEnabled
				{
					get { return this.googleDocsEnabled; }
					set
					{
						this.googleDocsEnabled = value;
						RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
					}
				}
				#endregion

				#region QrCodeEnabled
				public bool QrCodeEnabled
				{
					get { return this.qrCodeEnabled; }
					set
					{
						this.qrCodeEnabled = value;
						RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
					}
				}
				#endregion

				#region QrCodeUploadUrl
				/// <summary>
				/// always ends with '/'
				/// </summary>
				public string QrCodeUploadUrl
				{
					get { return this.qrCodeUploadUrl; }
					set
					{
						this.qrCodeUploadUrl = value;

						if (qrCodeUploadUrl.Length > 0 && qrCodeUploadUrl[qrCodeUploadUrl.Length - 1] != '/')
							qrCodeUploadUrl += "/";

						RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
					}
				}
				#endregion

				#region QrCodeDownloadUrl
				/// <summary>
				/// always ends ends with '/'
				/// </summary>
				public string QrCodeDownloadUrl
				{
					get { return this.qrCodeDownloadUrl; }
					set
					{
						this.qrCodeDownloadUrl = value;

						if (qrCodeDownloadUrl.Length > 0 && qrCodeDownloadUrl[qrCodeDownloadUrl.Length - 1] != '/')
							qrCodeDownloadUrl += "/";

						RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
					}
				}
				#endregion

				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.GoogleDocsEnabled = source.Export.Cloud.GoogleDocsEnabled;
					this.QrCodeEnabled = source.Export.Cloud.QrCodeEnabled;
					this.QrCodeUploadUrl = source.Export.Cloud.QrCodeUploadUrl;
					this.QrCodeDownloadUrl = source.Export.Cloud.QrCodeDownloadUrl;
				}
				#endregion
			}
			#endregion

			#region class ArielClass
			[Serializable]
			public class ArielClass : BaseClass
			{
				bool enabled = true;
				int majorVersion = 4;
				int minorVersion = 1;
				string executable = "";
				bool updateILLiad = false;
				bool changeRequestToFinished = false;
				bool updateILLiadNegativeIDs = true; //false;
				List<string> ipAddressesToNotUpdateILLiad = new List<string>();
				ExportNameBasedOn exportNameBase = ExportNameBasedOn.IllName;

				public ArielClass()
				{
				}

				public bool Enabled
				{
					get { return this.enabled; }
					set { this.enabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
				}

				public int MajorVersion
				{
					get { return this.majorVersion; }
					set { this.majorVersion = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
				}

				public int MinorVersion
				{
					get { return this.minorVersion; }
					set { this.minorVersion = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
				}

				public string Executable
				{
					get { return this.executable; }
					set { this.executable = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
				}

				public bool UpdateILLiad
				{
					get { return this.updateILLiad; }
					set { this.updateILLiad = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
				}

				public bool ChangeRequestToFinished
				{
					get { return this.changeRequestToFinished; }
					set { this.changeRequestToFinished = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
				}

				public bool UpdateILLiadNegativeIDs
				{
					//force true to update ILLiad when negative doc numbers
					//get { return this.updateILLiadNegativeIDs; }
					//set { this.updateILLiadNegativeIDs = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
					get { return true; }
					set
					{
						if (!this.updateILLiadNegativeIDs)
						{
							this.updateILLiadNegativeIDs = true;
						}
						RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
					}
				}

				public List<string> IpAddressesToNotUpdateILLiad
				{
					get { return this.ipAddressesToNotUpdateILLiad; }
					set { this.ipAddressesToNotUpdateILLiad = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
				}

				public ExportNameBasedOn ExportNameBase { get { return this.exportNameBase; } set { this.exportNameBase = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.Enabled = source.Export.Ariel.Enabled;
					this.MajorVersion = source.Export.Ariel.MajorVersion;
					this.MinorVersion = source.Export.Ariel.MinorVersion;
					this.Executable = source.Export.Ariel.Executable;
					this.UpdateILLiad = source.Export.Ariel.UpdateILLiad;
					this.ChangeRequestToFinished = source.Export.Ariel.ChangeRequestToFinished;
					this.UpdateILLiadNegativeIDs = source.Export.Ariel.UpdateILLiadNegativeIDs;
					this.IpAddressesToNotUpdateILLiad = source.Export.Ariel.IpAddressesToNotUpdateILLiad;
					this.ExportNameBase = source.Export.Ariel.ExportNameBase;
				}
				#endregion
			}
			#endregion

			#region class ILLiadClass
			[Serializable]
			public class ILLiadClass : BaseClass
			{
				bool enabled = true;
				ILLiadVersion version = ILLiadVersion.Version8_1_4_0;
				bool exportToOdysseyHelper = true;
				string odysseyHelperDir = @"c:\OdysseyHelper";

				bool sqlEnabled = false;
				string sqlServerUri = "";
				ushort sqlPort = 1433;
				string sqlDatabaseName = "";
				bool sqlWindowsCredentials = true;
				string sqlUsername = "sa";
				byte[] sqlPassword = new byte[0];
				List<string> sqlRequestsToDownload = new List<string>();
				bool sqlLoadArticles = true;
				bool sqlLoadLoans = true;
				ExportNameBasedOn exportNameBase = ExportNameBasedOn.TransactionName;

				TnOrientation pullslipTnOrientation = TnOrientation.VerticalOrHorizontal;
				System.Drawing.Rectangle pullslipTnZone = new System.Drawing.Rectangle(0, 0, 600, 1000);
				int pullslipTnMin = 100000;
				int pullslipTnMax = 999999;
				int updateExtraMenuItems = 0;

				BscanILL.Export.ExportType preferredExportPP1 = BscanILL.Export.ExportType.None;
				BscanILL.Export.ExportType preferredExportPP2 = BscanILL.Export.ExportType.None;
				BscanILL.Export.ExportType preferredExportPP3 = BscanILL.Export.ExportType.None;
				BscanILL.Export.ExportType preferredExportPN1 = BscanILL.Export.ExportType.None;
				BscanILL.Export.ExportType preferredExportPN2 = BscanILL.Export.ExportType.None;
				BscanILL.Export.ExportType preferredExportPN3 = BscanILL.Export.ExportType.None;
				BscanILL.Export.ExportType preferredExportNP1 = BscanILL.Export.ExportType.None;
				BscanILL.Export.ExportType preferredExportNP2 = BscanILL.Export.ExportType.None;
				BscanILL.Export.ExportType preferredExportNP3 = BscanILL.Export.ExportType.None;
				BscanILL.Export.ExportType preferredExportNN1 = BscanILL.Export.ExportType.None;
				BscanILL.Export.ExportType preferredExportNN2 = BscanILL.Export.ExportType.None;
				BscanILL.Export.ExportType preferredExportNN3 = BscanILL.Export.ExportType.None;

				BscanILL.Scan.FileFormat exportFileFormat = BscanILL.Scan.FileFormat.Tiff; // was ImageFormat

				Settings.GeneralClass.PdfColorDepth fileExportColorMode = Settings.GeneralClass.PdfColorDepth.Auto;
				int fileExportQuality = 85;

				#region constructor
				public ILLiadClass()
				{
				}
				#endregion

				#region enum TnOrientation
				[Serializable]
				public enum TnOrientation
				{
					Vertical,
					Horizontal,
					VerticalOrHorizontal
				}
				#endregion


				public bool Enabled { get { return enabled; } set { enabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public ILLiadVersion Version { get { return version; } set { version = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool ExportToOdysseyHelper { get { return exportToOdysseyHelper; } set { exportToOdysseyHelper = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string OdysseyHelperDir { get { return odysseyHelperDir; } set { odysseyHelperDir = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				public bool SqlEnabled { get { return sqlEnabled; } set { sqlEnabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string SqlServerUri { get { return sqlServerUri; } set { sqlServerUri = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public ushort SqlPort { get { return sqlPort; } set { sqlPort = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string SqlDatabaseName { get { return sqlDatabaseName; } set { sqlDatabaseName = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool SqlWindowsCredentials { get { return sqlWindowsCredentials; } set { sqlWindowsCredentials = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string SqlUsername { get { return sqlUsername; } set { sqlUsername = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public byte[] SqlPassword { get { return sqlPassword; } set { sqlPassword = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public List<string> SqlRequestsToDownload { get { return sqlRequestsToDownload; } set { sqlRequestsToDownload = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool SqlLoadArticles { get { return sqlLoadArticles; } set { sqlLoadArticles = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool SqlLoadLoans { get { return sqlLoadLoans; } set { sqlLoadLoans = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				public TnOrientation PullslipTnOrientation { get { return pullslipTnOrientation; } set { pullslipTnOrientation = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public System.Drawing.Rectangle PullslipTnZone { get { return pullslipTnZone; } set { pullslipTnZone = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public int PullslipTnMin { get { return pullslipTnMin; } set { pullslipTnMin = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public int PullslipTnMax { get { return pullslipTnMax; } set { pullslipTnMax = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public int UpdateExtraMenuItems { get { return updateExtraMenuItems; } set { updateExtraMenuItems = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				public BscanILL.Export.ExportType PreferredExportPP1 { get { return preferredExportPP1; } set { preferredExportPP1 = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Export.ExportType PreferredExportPP2 { get { return preferredExportPP2; } set { preferredExportPP2 = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Export.ExportType PreferredExportPP3 { get { return preferredExportPP3; } set { preferredExportPP3 = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Export.ExportType PreferredExportPN1 { get { return preferredExportPN1; } set { preferredExportPN1 = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Export.ExportType PreferredExportPN2 { get { return preferredExportPN2; } set { preferredExportPN2 = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Export.ExportType PreferredExportPN3 { get { return preferredExportPN3; } set { preferredExportPN3 = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Export.ExportType PreferredExportNP1 { get { return preferredExportNP1; } set { preferredExportNP1 = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Export.ExportType PreferredExportNP2 { get { return preferredExportNP2; } set { preferredExportNP2 = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Export.ExportType PreferredExportNP3 { get { return preferredExportNP3; } set { preferredExportNP3 = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Export.ExportType PreferredExportNN1 { get { return preferredExportNN1; } set { preferredExportNN1 = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Export.ExportType PreferredExportNN2 { get { return preferredExportNN2; } set { preferredExportNN2 = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Export.ExportType PreferredExportNN3 { get { return preferredExportNN3; } set { preferredExportNN3 = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				public BscanILL.Scan.FileFormat ExportFileFormat { get { return exportFileFormat; } set { exportFileFormat = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public ExportNameBasedOn ExportNameBase { get { return this.exportNameBase; } set { this.exportNameBase = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public Settings.GeneralClass.PdfColorDepth FileExportColorMode { get { return this.fileExportColorMode; } set { this.fileExportColorMode = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public int FileExportQuality { get { return this.fileExportQuality; } set { this.fileExportQuality = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				#region SqlPasswordText
				[XmlIgnore]
				public string SqlPasswordText
				{
					get
					{
						if (this.SqlPassword != null && this.SqlPassword.Length > 0)
						{
							byte[] sqlPasswordArray = new byte[this.SqlPassword.Length];

							for (int i = 0; i < this.SqlPassword.Length; i++)
								sqlPasswordArray[i] = (byte)this.SqlPassword[i];

							return DecryptText(sqlPasswordArray, false);
						}
						else
							return "";
					}
					set
					{
						if (value.Length > 0)
							this.SqlPassword = EncryptText(value, false);
						else
							this.SqlPassword = new byte[0];
					}
				}
				#endregion


				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.Enabled = source.Export.ILLiad.Enabled;
					this.Version = source.Export.ILLiad.Version;
					this.ExportToOdysseyHelper = source.Export.ILLiad.ExportToOdysseyHelper;
					this.OdysseyHelperDir = source.Export.ILLiad.OdysseyHelperDir;

					this.SqlEnabled = source.Export.ILLiad.SqlEnabled;
					this.SqlServerUri = source.Export.ILLiad.SqlServerUri;
					this.SqlPort = source.Export.ILLiad.SqlPort;
					this.SqlDatabaseName = source.Export.ILLiad.SqlDatabaseName;
					this.SqlWindowsCredentials = source.Export.ILLiad.SqlWindowsCredentials;
					this.SqlUsername = source.Export.ILLiad.SqlUsername;
					this.SqlPassword = source.Export.ILLiad.SqlPassword;
					this.SqlRequestsToDownload = source.Export.ILLiad.SqlRequestsToDownload;
					this.SqlLoadArticles = source.Export.ILLiad.SqlLoadArticles;
					this.SqlLoadLoans = source.Export.ILLiad.SqlLoadLoans;

					this.PullslipTnOrientation = source.Export.ILLiad.PullslipTnOrientation;
					this.PullslipTnZone = source.Export.ILLiad.PullslipTnZone;
					this.PullslipTnMin = source.Export.ILLiad.PullslipTnMin;
					this.PullslipTnMax = source.Export.ILLiad.PullslipTnMax;
					this.UpdateExtraMenuItems = source.Export.ILLiad.UpdateExtraMenuItems;

					this.PreferredExportPP1 = source.Export.ILLiad.PreferredExportPP1;
					this.PreferredExportPP2 = source.Export.ILLiad.PreferredExportPP2;
					this.PreferredExportPP3 = source.Export.ILLiad.PreferredExportPP3;
					this.PreferredExportPN1 = source.Export.ILLiad.PreferredExportPN1;
					this.PreferredExportPN2 = source.Export.ILLiad.PreferredExportPN2;
					this.PreferredExportPN3 = source.Export.ILLiad.PreferredExportPN3;
					this.PreferredExportNP1 = source.Export.ILLiad.PreferredExportNP1;
					this.PreferredExportNP2 = source.Export.ILLiad.PreferredExportNP2;
					this.PreferredExportNP3 = source.Export.ILLiad.PreferredExportNP3;
					this.PreferredExportNN1 = source.Export.ILLiad.PreferredExportNN1;
					this.PreferredExportNN2 = source.Export.ILLiad.PreferredExportNN2;
					this.PreferredExportNN3 = source.Export.ILLiad.PreferredExportNN3;

					this.ExportFileFormat = source.Export.ILLiad.ExportFileFormat;
					this.ExportNameBase = source.Export.ILLiad.ExportNameBase;
					this.FileExportColorMode = source.Export.ILLiad.FileExportColorMode;
					this.FileExportQuality = source.Export.ILLiad.FileExportQuality;
				}
				#endregion
			}
			#endregion

			#region class FtpDirClass
			[Serializable]
			public class FtpDirClass : BaseClass
			{
				bool enabled = true;
				string ftpAddress = "";
				string exportDirectoryPath = "";
				bool sendConfirmationEmail = false;
				bool saveToSubfolder = false;
				SubfolderNameBasedOn subFolderNameBase = SubfolderNameBasedOn.IllName;
				ExportNameBasedOn exportNameBase = ExportNameBasedOn.TransactionName;
				bool updateILLiad = false;
				bool changeRequestToFinished = false;
				BscanILL.Scan.FileFormat fileFormat = BscanILL.Scan.FileFormat.Pdf;
				Settings.GeneralClass.PdfColorDepth fileExportColorMode = Settings.GeneralClass.PdfColorDepth.Auto;
				int fileExportQuality = 85;

				public FtpDirClass()
				{
				}

				public bool Enabled { get { return this.enabled; } set { this.enabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string FtpAddress { get { return this.ftpAddress; } set { this.ftpAddress = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string ExportDirectoryPath { get { return this.exportDirectoryPath; } set { this.exportDirectoryPath = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool SendConfirmationEmail { get { return this.sendConfirmationEmail; } set { this.sendConfirmationEmail = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool SaveToSubfolder { get { return this.saveToSubfolder; } set { this.saveToSubfolder = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public SubfolderNameBasedOn SubFolderNameBase { get { return this.subFolderNameBase; } set { this.subFolderNameBase = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public ExportNameBasedOn ExportNameBase { get { return this.exportNameBase; } set { this.exportNameBase = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool UpdateILLiad { get { return this.updateILLiad; } set { this.updateILLiad = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool ChangeRequestToFinished { get { return this.changeRequestToFinished; } set { this.changeRequestToFinished = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Scan.FileFormat ExportFileFormat { get { return this.fileFormat; } set { this.fileFormat = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public Settings.GeneralClass.PdfColorDepth FileExportColorMode { get { return this.fileExportColorMode; } set { this.fileExportColorMode = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public int FileExportQuality { get { return this.fileExportQuality; } set { this.fileExportQuality = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				[System.Xml.Serialization.XmlIgnoreAttribute]
				public DirectoryInfo ExportDirectory { get { return new DirectoryInfo(this.ExportDirectoryPath); } set { this.ExportDirectoryPath = value.FullName; } }


				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.Enabled = source.Export.FtpDirectory.Enabled;
					this.FtpAddress = source.Export.FtpDirectory.FtpAddress;
					this.ExportDirectoryPath = source.Export.FtpDirectory.ExportDirectoryPath;
					this.SendConfirmationEmail = source.Export.FtpDirectory.SendConfirmationEmail;
					this.SaveToSubfolder = source.Export.FtpDirectory.SaveToSubfolder;
					this.SubFolderNameBase = source.Export.FtpDirectory.SubFolderNameBase;
					this.ExportNameBase = source.Export.FtpDirectory.ExportNameBase;
					this.UpdateILLiad = source.Export.FtpDirectory.UpdateILLiad;
					this.ChangeRequestToFinished = source.Export.FtpDirectory.ChangeRequestToFinished;
					this.ExportFileFormat = source.Export.FtpDirectory.ExportFileFormat;
					this.FileExportColorMode = source.Export.FtpDirectory.FileExportColorMode;
					this.FileExportQuality = source.Export.FtpDirectory.FileExportQuality;
				}
				#endregion

			}
			#endregion

			#region class OdysseyClass
			[Serializable]
			public class OdysseyClass : BaseClass
			{
				bool enabled = true;
				string exportDirPath = @"c:\bscanill\ExportedImages";
				BscanILL.Scan.FileFormat exportFileFormat = BscanILL.Scan.FileFormat.Pdf;
				ExportNameBasedOn exportNameBase = ExportNameBasedOn.TransactionName;
				Settings.GeneralClass.PdfColorDepth fileExportColorMode = Settings.GeneralClass.PdfColorDepth.Auto;
				int fileExportQuality = 85;

				public OdysseyClass()
				{
				}

				public bool Enabled { get { return this.enabled; } set { this.enabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string ExportDirPath { get { return this.exportDirPath; } set { this.exportDirPath = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Scan.FileFormat ExportFileFormat { get { return this.exportFileFormat; } set { this.exportFileFormat = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public ExportNameBasedOn ExportNameBase { get { return this.exportNameBase; } set { this.exportNameBase = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public Settings.GeneralClass.PdfColorDepth FileExportColorMode { get { return this.fileExportColorMode; } set { this.fileExportColorMode = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public int FileExportQuality { get { return this.fileExportQuality; } set { this.fileExportQuality = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				[System.Xml.Serialization.XmlIgnoreAttribute]
				public DirectoryInfo ExportDir { get { return new DirectoryInfo(this.ExportDirPath); } set { this.ExportDirPath = value.FullName; } }


				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.Enabled = source.Export.Odyssey.Enabled;
					this.ExportDirPath = source.Export.Odyssey.ExportDirPath;
					this.ExportFileFormat = source.Export.Odyssey.ExportFileFormat;
					this.ExportNameBase = source.Export.Odyssey.ExportNameBase;
					this.FileExportColorMode = source.Export.Odyssey.FileExportColorMode;
					this.FileExportQuality = source.Export.Odyssey.FileExportQuality;
				}
				#endregion
			}
			#endregion

			#region class ArticleExchangeClass
			[Serializable]
			public class ArticleExchangeClass : BaseClass
			{
				bool enabled = true;
				string wsKey = "4fesz3B9mtZwHmBDDnTSsE5M43uPJHSRjEDSKkQkUh2NG9zgnKYhNc3lDaeh57oPAlpU2KUShAqSMNqR";
				byte[] secretBuffer = new byte[32] { 199, 239, 9, 189, 78, 1, 141, 145, 76, 73, 86, 150, 66, 19, 20, 209, 223, 24, 71, 170, 51, 215, 176, 53, 179, 113, 39, 130, 206, 9, 91, 68 };
				string autho = "";
				byte[] passwordBuffer = new byte[0];
				string principalId = "";
				string principalDns = "";
				string requesterInstSymbol = "";
				string confirmEmailAddress = null;
				bool resetConfirmEmailAddress = false;
				bool updateILLiad = true;
				bool changeRequestToFinished = true;
				BscanILL.Scan.FileFormat fileFormat = BscanILL.Scan.FileFormat.Pdf;
				ObservableCollection<string> confirmationRecipients = new ObservableCollection<string>();
				ExportNameBasedOn exportNameBase = ExportNameBasedOn.IllName;
				Settings.GeneralClass.PdfColorDepth fileExportColorMode = Settings.GeneralClass.PdfColorDepth.Auto;
				int fileExportQuality = 85;

				string subject = "BscanILL Article Exchange Delivery Notification";
				string body = "To retrieve your requested document {ID} posted by BscanILL:" + Environment.NewLine + Environment.NewLine + "Follow: {URL}" + Environment.NewLine + "Passwd: {PASS}";


				#region constructor
				public ArticleExchangeClass()
				{
				}
				#endregion


				public bool Enabled { get { return this.enabled; } set { this.enabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string WSKey { get { return this.wsKey; } }
				public byte[] SecretBuffer { get { return this.secretBuffer; } }
				public string Autho { get { return this.autho; } set { this.autho = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public byte[] PasswordBuffer { get { return this.passwordBuffer; } set { this.passwordBuffer = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string PrincipalId { get { return this.principalId; } set { this.principalId = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string PrincipalDns { get { return this.principalDns; } set { this.principalDns = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string RequesterInstSymbol { get { return this.requesterInstSymbol; } set { this.requesterInstSymbol = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string ConfirmationEmailAddress { get { return this.confirmEmailAddress; } set { this.confirmEmailAddress = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool ResetConfirmEmailAddress { get { return this.resetConfirmEmailAddress; } set { this.resetConfirmEmailAddress = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool UpdateILLiad { get { return this.updateILLiad; } set { this.updateILLiad = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public bool ChangeRequestToFinished { get { return this.changeRequestToFinished; } set { this.changeRequestToFinished = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Scan.FileFormat ExportFileFormat { get { return this.fileFormat; } set { this.fileFormat = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public ObservableCollection<string> ConfirmationRecipients { get { return this.confirmationRecipients; } set { this.confirmationRecipients = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public ExportNameBasedOn ExportNameBase { get { return this.exportNameBase; } set { this.exportNameBase = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				public string Subject { get { return this.subject; } set { SetProperty(ref this.subject, value, MethodBase.GetCurrentMethod().Name); } }
				public string Body { get { return this.body; } set { SetProperty(ref this.body, value, MethodBase.GetCurrentMethod().Name); } }
				public Settings.GeneralClass.PdfColorDepth FileExportColorMode { get { return this.fileExportColorMode; } set { this.fileExportColorMode = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public int FileExportQuality { get { return this.fileExportQuality; } set { this.fileExportQuality = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				#region Secret
				/*[XmlIgnore]
				public SecureString Secret
				{
					get
					{
						try { return BscanILL.Misc.DataProtector.GetSecureString(this.SecretBuffer); }
						catch (Exception) { return new SecureString(); }
					}
					set { this.SecretBuffer = BscanILL.Misc.DataProtector.GetArray(value); RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
				}*/
				#endregion

				#region Password
				[XmlIgnore]
				public SecureString Password
				{
					get
					{
						try { return BscanILL.Misc.DataProtector.GetSecureString(this.PasswordBuffer); }
						catch (Exception) { return new SecureString(); }
					}
					set { this.PasswordBuffer = BscanILL.Misc.DataProtector.GetArray(value); RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
				}
				#endregion


				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.Enabled = source.Export.ArticleExchange.Enabled;
					//this.WSKey = source.Export.ArticleExchange.WSKey;
					//this.SecretBuffer = source.Export.ArticleExchange.SecretBuffer;
					this.Autho = source.Export.ArticleExchange.Autho;
					this.PasswordBuffer = source.Export.ArticleExchange.PasswordBuffer;
					this.PrincipalId = source.Export.ArticleExchange.PrincipalId;
					this.PrincipalDns = source.Export.ArticleExchange.PrincipalDns;
					this.RequesterInstSymbol = source.Export.ArticleExchange.RequesterInstSymbol;
					this.ConfirmationEmailAddress = source.Export.ArticleExchange.ConfirmationEmailAddress;
					this.ResetConfirmEmailAddress = source.Export.ArticleExchange.ResetConfirmEmailAddress;
					this.Subject = source.Export.ArticleExchange.Subject;
					this.Body = source.Export.ArticleExchange.Body;
					this.ExportNameBase = source.Export.ArticleExchange.ExportNameBase;
					this.ExportFileFormat = source.Export.ArticleExchange.ExportFileFormat;

					this.ConfirmationRecipients.Clear();
					foreach (string str in source.Export.ArticleExchange.ConfirmationRecipients)
						this.ConfirmationRecipients.Add(str);

					this.UpdateILLiad = source.Export.ArticleExchange.UpdateILLiad;
					this.ChangeRequestToFinished = source.Export.ArticleExchange.ChangeRequestToFinished;
					this.FileExportColorMode = source.Export.ArticleExchange.FileExportColorMode;
					this.FileExportQuality = source.Export.ArticleExchange.FileExportQuality;
				}
				#endregion
			}
			#endregion

			#region class TipasaClass
			[Serializable]
			public class TipasaClass : BaseClass
			{
				bool enabled = true;
				string instSymbol = "";
				ExportNameBasedOn exportNameBase = ExportNameBasedOn.IllName;  //oclcRequestID=ILL is mandatory field in Tipasa, that is why we should favor ILL based naming of sent document                
				BscanILL.Scan.FileFormat fileFormat = BscanILL.Scan.FileFormat.Pdf;
				Settings.GeneralClass.PdfColorDepth fileExportColorMode = Settings.GeneralClass.PdfColorDepth.Auto;
				int fileExportQuality = 85;

				#region constructor
				public TipasaClass()
				{
				}
				#endregion


				public bool Enabled { get { return this.enabled; } set { this.enabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string InstSymbol { get { return this.instSymbol; } set { this.instSymbol = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public ExportNameBasedOn ExportNameBase { get { return this.exportNameBase; } set { this.exportNameBase = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Scan.FileFormat ExportFileFormat { get { return this.fileFormat; } set { this.fileFormat = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public Settings.GeneralClass.PdfColorDepth FileExportColorMode { get { return this.fileExportColorMode; } set { this.fileExportColorMode = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public int FileExportQuality { get { return this.fileExportQuality; } set { this.fileExportQuality = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.Enabled = source.Export.Tipasa.Enabled;
					this.InstSymbol = source.Export.Tipasa.InstSymbol;
					this.ExportNameBase = source.Export.Tipasa.ExportNameBase;
					this.ExportFileFormat = source.Export.Tipasa.ExportFileFormat;
					this.FileExportColorMode = source.Export.Tipasa.FileExportColorMode;
					this.FileExportQuality = source.Export.Tipasa.FileExportQuality;
				}
				#endregion
			}
			#endregion

			#region class WorldShareILLClass
			[Serializable]
			public class WorldShareILLClass : BaseClass
			{
				bool enabled = true;
				string instSymbol = "";
				ExportNameBasedOn exportNameBase = ExportNameBasedOn.IllName;  //oclcRequestID=ILL is mandatory field in WorldShareILL, that is why we should favor ILL based naming of sent document                
				BscanILL.Scan.FileFormat fileFormat = BscanILL.Scan.FileFormat.Pdf;
				Settings.GeneralClass.PdfColorDepth fileExportColorMode = Settings.GeneralClass.PdfColorDepth.Auto;
				int fileExportQuality = 85;

				#region constructor
				public WorldShareILLClass()
				{
				}
				#endregion


				public bool Enabled { get { return this.enabled; } set { this.enabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string InstSymbol { get { return this.instSymbol; } set { this.instSymbol = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public ExportNameBasedOn ExportNameBase { get { return this.exportNameBase; } set { this.exportNameBase = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Scan.FileFormat ExportFileFormat { get { return this.fileFormat; } set { this.fileFormat = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public Settings.GeneralClass.PdfColorDepth FileExportColorMode { get { return this.fileExportColorMode; } set { this.fileExportColorMode = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public int FileExportQuality { get { return this.fileExportQuality; } set { this.fileExportQuality = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.Enabled = source.Export.WorldShareILL.Enabled;
					this.InstSymbol = source.Export.WorldShareILL.InstSymbol;
					this.ExportNameBase = source.Export.WorldShareILL.ExportNameBase;
					this.ExportFileFormat = source.Export.WorldShareILL.ExportFileFormat;
					this.FileExportColorMode = source.Export.WorldShareILL.FileExportColorMode;
					this.FileExportQuality = source.Export.WorldShareILL.FileExportQuality;
				}
				#endregion
			}
			#endregion

			#region class RapidoClass
			[Serializable]
			public class RapidoClass : BaseClass
			{
				bool enabled = true;
				string apiKey = "";
				ExportNameBasedOn exportNameBase = ExportNameBasedOn.IllName;  //oclcRequestID=ILL is mandatory field in Tipasa, that is why we should favor ILL based naming of sent document                
				BscanILL.Scan.FileFormat fileFormat = BscanILL.Scan.FileFormat.Pdf;
				Settings.GeneralClass.PdfColorDepth fileExportColorMode = Settings.GeneralClass.PdfColorDepth.Auto;
				int fileExportQuality = 85;

				#region constructor
				public RapidoClass()
				{
				}
				#endregion

				public bool Enabled { get { return this.enabled; } set { this.enabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public string ApiKey { get { return this.apiKey; } set { this.apiKey = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public ExportNameBasedOn ExportNameBase { get { return this.exportNameBase; } set { this.exportNameBase = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public BscanILL.Scan.FileFormat ExportFileFormat { get { return this.fileFormat; } set { this.fileFormat = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public Settings.GeneralClass.PdfColorDepth FileExportColorMode { get { return this.fileExportColorMode; } set { this.fileExportColorMode = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
				public int FileExportQuality { get { return this.fileExportQuality; } set { this.fileExportQuality = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

				#region CopyFrom()
				public override void CopyFrom(Settings source)
				{
					this.Enabled = source.Export.Rapido.Enabled;
					this.ApiKey = source.Export.Rapido.ApiKey;
					this.ExportNameBase = source.Export.Rapido.ExportNameBase;
					this.ExportFileFormat = source.Export.Rapido.ExportFileFormat;
					this.FileExportColorMode = source.Export.Rapido.FileExportColorMode;
					this.FileExportQuality = source.Export.Rapido.FileExportQuality;
				}
				#endregion
			}

			#endregion

		}
        #endregion

        #region class StatsAndNotifsClass
        [Serializable]
		public class StatsAndNotifsClass : BaseClass
		{
			public List<DateTime> TimesToSend			{ get; set; }

			public string	AdditionalStatsRecipients	{ get; set; }
			public string	AdditionalAdminRecipients	{ get; set; }

			public bool		LogToEventLog				{ get; set; }
			public bool		LogToLogFile				{ get; set; }
			public bool		SendToDLSG					{ get; set; }

			public string DlsgManagementEmail	{ get { return "BscanILL-Management@imageaccess.com"; } }
			public string DlsgAdminsEmail		{ get { return "BscanILL-Admins@imageaccess.com"; } }
			public string DlsgDevelopersEmail	{ get { return "BscanILL-Operations@imageaccess.com"; } }
			public string DlsgLicenseEmail		{ get { return "BscanILL-Admins@imageaccess.com"; } }


			#region constructor
			public StatsAndNotifsClass()
			{
				this.TimesToSend = new List<DateTime>();

				this.AdditionalStatsRecipients = "";
				this.AdditionalAdminRecipients = "";

				this.LogToEventLog = false;
				this.LogToLogFile = true;
				this.SendToDLSG = true;
			}
			#endregion

			#region TimesToSendStr
			public string TimesToSendStr
			{
				get
				{
					string timesToSendStr = TimesToSend.Count > 0 ? TimesToSend[0].ToString("HH:mm") : "23:30";

					for (int i = 1; i < TimesToSend.Count; i++)
						timesToSendStr += ", " + TimesToSend[i].ToString("HH:mm");

					return timesToSendStr;
				}
			}
			#endregion


			#region ParseRecipients()
			public static List<string> ParseRecipients(string recipients)
			{
				string[] recipientsArray = recipients.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
				List<string> recipientsList = new List<string>();

				foreach (string recipient in recipientsArray)
					if (recipient.Trim().Length >  0 && BscanILL.Export.Email.Email.IsValidEmail(recipient.Trim()) && recipientsList.Contains(recipient) == false)
						recipientsList.Add(recipient.Trim());

				return recipientsList;
			}
			#endregion

			#region ParseTimes()
			public static List<DateTime> ParseTimes(string timesLine)
			{
				string[] times = timesLine.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
				List<DateTime> timesList = new List<DateTime>();
				DateTime dateTime;

				foreach (string time in times)
					if (DateTime.TryParseExact(time.Trim(), "HH:mm", new System.Globalization.CultureInfo("en-US"), System.Globalization.DateTimeStyles.None, out dateTime))
						timesList.Add(dateTime);

				return timesList;
			}
			#endregion

			#region CopyFrom()
			public override void CopyFrom(Settings source)
			{
				this.TimesToSend = source.StatsAndNotifs.TimesToSend;

				this.AdditionalStatsRecipients = source.StatsAndNotifs.AdditionalStatsRecipients;
				this.AdditionalAdminRecipients = source.StatsAndNotifs.AdditionalAdminRecipients;

				this.LogToEventLog = source.StatsAndNotifs.LogToEventLog;
				this.LogToLogFile = source.StatsAndNotifs.LogToLogFile;
				this.SendToDLSG = source.StatsAndNotifs.SendToDLSG;
			}
			#endregion
		
		}
		#endregion

		#region class UserInterfaceClass
		[Serializable]
		public class UserInterfaceClass : BaseClass
		{

			public UserInterfaceClass()
			{
			}

			#region CopyFrom()
			public override void CopyFrom(Settings source)
			{
			}
			#endregion

		}
		#endregion

		#region class LicensingClass
		public class LicensingClass : BaseClass
		{
			bool ocrEnabled = false;
			bool audioEnabled = false;
			bool addditionalScannerEnabled = false;
			bool rapidoEnabled = false;
			
			public LicensingClass()
			{
			}

			[XmlIgnore]
			public bool OcrEnabled
			{
				get { return ocrEnabled; }
				set 
				{		
					this.ocrEnabled = value;
					RaisePropertyChanged(this, MethodBase.GetCurrentMethod().Name);
				}
			}

			[XmlIgnore]
			public bool AudioEnabled
			{
				get { return audioEnabled; }
				set
				{
					this.audioEnabled = value;
					RaisePropertyChanged(this, MethodBase.GetCurrentMethod().Name);
				}
			}

			[XmlIgnore]
			public bool AddidionalScannerEnabled
			{
				get { return addditionalScannerEnabled; }
				set
				{
					this.addditionalScannerEnabled = value;
					RaisePropertyChanged(this, MethodBase.GetCurrentMethod().Name);
				}
			}

			[XmlIgnore]
			public bool RapidoEnabled
			{
				get { return rapidoEnabled; }
				set
				{
					this.rapidoEnabled = value;
					RaisePropertyChanged(this, MethodBase.GetCurrentMethod().Name);
				}
			}


			#region CopyFrom()
			public override void CopyFrom(Settings source)
			{
				this.OcrEnabled = source.Licensing.OcrEnabled;
				this.AudioEnabled = source.Licensing.AudioEnabled;
				this.AddidionalScannerEnabled = source.Licensing.AddidionalScannerEnabled;
				this.RapidoEnabled = source.Licensing.RapidoEnabled;
			}
			#endregion

		}
		#endregion

		#region class FormsProcessingClass
		public class FormsProcessingClass : BaseClass
		{
			string		bsaFile;
			string		scriptFile;
			string		trainingName;


			#region constructor
			public FormsProcessingClass()
			{
				this.bsaFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\DLSG\BscanILL\FormsProcessing\Apps\ILLScan.bsa";
				this.scriptFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\DLSG\BscanILL\FormsProcessing\Lib\Default\Default.bsa";
				this.trainingName = "Default";
			}
			#endregion


			public FileInfo		BsaFile			{ get { return new FileInfo(bsaFile); } }
			public FileInfo		ScriptFile		{ get { return new FileInfo(scriptFile); } }

			#region BsaFilePath
			public string BsaFilePath
			{
				get { return bsaFile; }
				set
				{
					this.bsaFile = value;
					RaisePropertyChanged(this, MethodBase.GetCurrentMethod().Name);
				}
			}
			#endregion

			#region ScriptFilePath
			public string ScriptFilePath
			{
				get { return scriptFile; }
				set
				{
					this.scriptFile = value;
					RaisePropertyChanged(this, MethodBase.GetCurrentMethod().Name);
				}
			}
			#endregion

			#region TrainingName
			public string TrainingName
			{
				get { return trainingName; }
				set
				{
					this.trainingName = value;
					RaisePropertyChanged(this, MethodBase.GetCurrentMethod().Name);
				}
			}
			#endregion


			#region CopyFrom()
			public override void CopyFrom(Settings source)
			{
				this.BsaFilePath = source.FormsProcessing.BsaFilePath;
				this.ScriptFilePath = source.FormsProcessing.ScriptFilePath;
				this.TrainingName = source.FormsProcessing.TrainingName;
			}
			#endregion
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Instance
		static public Settings Instance
		{
			get 
			{
				if (Settings.instance != null)
					return Settings.instance;

				if (File.Exists(settingsFile))
				{
					try
					{
						using (FileStream stream = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							XmlSerializer serializer = new XmlSerializer(typeof(BscanILL.SETTINGS.Settings));

							Settings.instance = (BscanILL.SETTINGS.Settings)serializer.Deserialize(stream);

							Scanners.Settings.Instance = Settings.instance.Scanner;
							return Settings.instance;
						}
					}
					catch (Exception ex)
					{
						throw new Exception(string.Format("Can't read settings from '{0}'!", settingsFile) + Environment.NewLine + Environment.NewLine + BscanILL.Misc.Misc.GetErrorMessage(ex));
					}
				}
				else
				{
					Settings.instance = new Settings();
										
					return Settings.instance;
				}
			} 
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Save()
		public void Save()
		{
			new FileInfo(settingsFile).Directory.Create();
			
			this.General.Version = BscanILL.SETTINGS.Settings.GetVersionString();
			string tmpFile = settingsFile + ".tmp";

			using (FileStream stream = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(BscanILL.SETTINGS.Settings));

				serializer.Serialize(stream, this);
			}

			if (File.Exists(settingsFile))
				File.Delete(settingsFile);

			File.Move(tmpFile, settingsFile);
		}
		#endregion

		#region GetVersionString()
		public static string GetVersionString()
		{
			Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
		}
		#endregion

		#region EncryptText()
		public static byte[] EncryptText(string text, bool currentDomainUserOnly)
		{
			System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
			byte[] textArray = encoding.GetBytes(text);
			byte[] entrophy = encoding.GetBytes(Environment.MachineName);
			BscanILL.Misc.DataProtector dp;

			if (currentDomainUserOnly)
				dp = new BscanILL.Misc.DataProtector(BscanILL.Misc.DataProtector.Store.USE_USER_STORE);
			else
				dp = new BscanILL.Misc.DataProtector(BscanILL.Misc.DataProtector.Store.USE_MACHINE_STORE);

			byte[] decodedText = dp.Encrypt(textArray, entrophy);

			return decodedText;
		}
		#endregion

		#region DecryptText()
		public static string DecryptText(byte[] encodedArray, bool currentDomainUserOnly)
		{
			System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
			byte[] entrophy = encoding.GetBytes(Environment.MachineName);
			//byte[] decodedTextArray = encoding.GetBytes(decodedText);
			BscanILL.Misc.DataProtector dp;

			if (currentDomainUserOnly)
				dp = new BscanILL.Misc.DataProtector(BscanILL.Misc.DataProtector.Store.USE_USER_STORE);
			else
				dp = new BscanILL.Misc.DataProtector(BscanILL.Misc.DataProtector.Store.USE_MACHINE_STORE);

			byte[] decodedArray = dp.Decrypt(encodedArray, entrophy);

			return encoding.GetString(decodedArray);
		}
		#endregion

		#region Clone()
		public BscanILL.SETTINGS.Settings Clone()
		{
			BscanILL.SETTINGS.Settings settings = new Settings();

			settings.General.CopyFrom(this);
			settings.ILL.CopyFrom(this);
			settings.ImageTreatment.CopyFrom(this);
			settings.Export.CopyFrom(this);
			settings.StatsAndNotifs.CopyFrom(this);
			settings.UserInterface.CopyFrom(this);
			settings.Licensing.CopyFrom(this);
			settings.FormsProcessing.CopyFrom(this);
			settings.Scanner.CopyFrom(this.Scanner);

			return settings;
		}
		#endregion

		#region ApplySettings()
		public void ApplySettings(BscanILL.SETTINGS.Settings source, bool copyToGlobal)
		{
			this.General.CopyFrom(source);
			this.ILL.CopyFrom(source);
			this.ImageTreatment.CopyFrom(source);
			this.Export.CopyFrom(source);
			this.StatsAndNotifs.CopyFrom(source);
			this.UserInterface.CopyFrom(source);
			this.Licensing.CopyFrom(source);
			this.FormsProcessing.CopyFrom(source);
			this.Scanner.CopyFrom(source.Scanner);

			if (copyToGlobal)
			{
				//BscanILL.SETTINGS.Settings.Instance.ApplySettings(source);
				Scanners.Settings.Instance.CopyFrom(source.Scanner);
				//ClickDLL.Settings.SettingsClick.Instance.CopyFrom(source.Scanner.ClickScanner.Settings);
			}
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region RaisePropertyChanged
		public void RaisePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, e);
		}

		public void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				if (propertyName != null && (propertyName.StartsWith("get_") || propertyName.StartsWith("set_")))
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName.Substring(4)));
				else
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion
	
		#endregion


	}

}

