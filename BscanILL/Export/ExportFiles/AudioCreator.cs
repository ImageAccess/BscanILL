using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Yeti.MMedia;
using Yeti.MMedia.Mp3;
using WaveLib;
using System.Speech.Synthesis;
using System.Collections.ObjectModel;
using System.IO;
using System.Drawing;
using BscanILL.Misc;


namespace BscanILL.Export.ExportFiles
{
	public class AudioCreator : ExportFilesBasics
	{
		SpeechSynthesizer speechEngine = new SpeechSynthesizer();
		InstalledVoice defaultVoice = null;
		object locker = new object();

//  IRIS
#if IRIS_ENGINE
		Iris iris = Iris.Instance;
#else
        Abbyy abbyy = Abbyy.Instance;
#endif
		delegate List<BscanILL.Export.AudioZoning.ZonesImage> GetAudioZonesWinFormHnd(System.Windows.Window parentForm, BscanILL.Hierarchy.IllPages illImages);
		delegate List<BscanILL.Export.AudioZoning.ZonesImage> GetAudioZonesWpfHnd(System.Windows.Window parentForm, BscanILL.Hierarchy.IllPages illImages);
		GetAudioZonesWpfHnd dlgGetAudioZonesWpf;        

		#region constructor
        public AudioCreator()
		{            
			ReadOnlyCollection<InstalledVoice> voices = speechEngine.GetInstalledVoices();

			if (voices.Count == 0)
				throw new Exception("No voices installed!");

			foreach (InstalledVoice voice in voices)
				if (voice.Enabled)
					defaultVoice = voice;

			if (defaultVoice == null)
				throw new Exception("There is not an enabled voice!");

//IRIS
#if IRIS_ENGINE
                this.iris.ProgressChanged += delegate(double progress) { Progress_Changed(progress); };
                this.iris.DescriptionChanged += delegate(string description) { Description_Changed(description); };
#else      
                this.abbyy.ProgressChanged += delegate(double progress) { Progress_Changed(progress); };
                this.abbyy.DescriptionChanged += delegate(string description) { Description_Changed(description); };      
#endif
			this.dlgGetAudioZonesWpf += new GetAudioZonesWpfHnd(GetAudioZonesWpfTU);
		}
		#endregion

		#region class AudioType
		public class AudioType
		{
			public readonly string Voice;
			public readonly int Speed;
			public readonly int Volume;
			public readonly int BitRate;
			public readonly bool PerDocument;

			public AudioType(string voice, int speed, int volume, int bitRate, bool perDocument)
			{
				this.Voice = voice;
				this.Speed = speed;
				this.Volume = volume;
				this.BitRate = bitRate;
				this.PerDocument = perDocument;
			}
		}
		#endregion

		//PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose()
		{
			if (this.speechEngine != null)
			{
				this.speechEngine.Dispose();
				this.speechEngine = null;
			}
		}
		#endregion

		#region GetAudioZones()
		public List<BscanILL.Export.AudioZoning.ZonesImage> GetAudioZones(System.Windows.Window parentForm, BscanILL.Hierarchy.IllPages illImages)
		{
            if (parentForm != null )
            {
			   return (List<BscanILL.Export.AudioZoning.ZonesImage>)parentForm.Dispatcher.Invoke(this.dlgGetAudioZonesWpf, parentForm, illImages);
            }
            else
            {
                return null ;
            }
		}
		#endregion

		#region CreateAudio()
		public void CreateAudio(BscanILL.Hierarchy.IllPage illPage, FileInfo destFile, AudioType audioType, bool updateProgressBar)
		{
			BscanILL.Hierarchy.IllPages illImages = new BscanILL.Hierarchy.IllPages();
			illImages.Add(illPage);

            CreateAudio(illImages, destFile, audioType, updateProgressBar);
		}

        public void CreateAudio(BscanILL.Export.AudioZoning.ZonesImage audioZoneImage, FileInfo destFile, AudioType audioType, bool updateProgressBar)
		{
			List<BscanILL.Export.AudioZoning.ZonesImage> audioZoneImages = new List<BscanILL.Export.AudioZoning.ZonesImage>();
			audioZoneImages.Add(audioZoneImage);

            CreateAudio(audioZoneImages, destFile, audioType, updateProgressBar);
		}

		/// <summary>
		/// creates audio files without any cropping
		/// </summary>
		/// <param name="illImages"></param>
		/// <param name="destFile"></param>
		/// <param name="audioType"></param>
        public void CreateAudio(BscanILL.Hierarchy.IllPages illImages, FileInfo destFile, AudioType audioType, bool updateProgressBar)
		{
			DirectoryInfo tempDir = new DirectoryInfo(destFile.Directory.FullName + @"\temp");
			try
			{
				string text = "";
				tempDir.Create();

				for (int i = 0; i < illImages.Count; i++)
				{
					FileInfo tempFile = null;

					try
					{
						tempFile = SaveImage(illImages[i], tempDir);

                        string imageText;
//IRIS   
#if IRIS_ENGINE
						  imageText = iris.Ocr(tempFile);  
#else                      
                          if (_settings.General.OcrEngProfile == BscanILL.SETTINGS.Settings.GeneralClass.OcrEngineProfile.Speed)
                          {
                            //Speed
                              this.abbyy.OcrProfile = "Document Conversion Speed";     //Document Archiving is recommended profile for PDF creation. For RTF or PDF text only is recommended Document Conversion profile
                          }
                          else
                          {
                            //Accuracy
                              this.abbyy.OcrProfile = "Document Conversion Accuracy";
                          }
                          imageText = abbyy.Ocr(tempFile);
#endif                        
						text += imageText;
					}
					catch (IllException)
					{
					}
					catch (Exception ex)
					{
						try { Notifications.Instance.Notify(this, Notifications.Type.Error, "AudioCreator, CreateAudio() 1: " + ex.Message, ex, tempFile); }
						catch { Notifications.Instance.Notify(this, Notifications.Type.Error, "AudioCreator, CreateAudio() 1: " + ex.Message, ex); }
					}

                    if (updateProgressBar)
					    Progress_Changed((i + 1.0F) / (illImages.Count + 1.0F));
				}

				if (text.Length > 0)
					CreateDerivative(audioType, destFile, text);
				else
					throw new IllException("No text found.");

                if( updateProgressBar )
				    Progress_Changed(1);
			}
			finally
			{
				try { tempDir.Delete(true); }
				catch { }
			}
		}

        public void CreateAudio(List<BscanILL.Export.AudioZoning.ZonesImage> audioZoneImages, FileInfo destFile, AudioType audioType, bool updateProgressBar)
		{
			DirectoryInfo tempDir = new DirectoryInfo(destFile.Directory.FullName + @"\temp");

			try
			{
				int counter = 1;
				string text = "";

				tempDir.Create();

				for (int i = 0; i < audioZoneImages.Count; i++)
				{
					FileInfo cropFile = null;

					try
					{
						BscanILL.Export.AudioZoning.ZonesImage zonesImage = audioZoneImages[i];

						using (Bitmap bitmap = new Bitmap(zonesImage.File.FullName))
						{
							for (int j = 0; j < zonesImage.Zones.Count; j++)
							{
								BscanILL.Export.AudioZoning.Zone zone = zonesImage.Zones[j];

								Rectangle rect = new Rectangle((int)(zone.X * bitmap.Width), (int)(zone.Y * bitmap.Height), (int)(zone.Width * bitmap.Width), (int)(zone.Height * bitmap.Height));

								if (rect.Width > 40 && rect.Height > 40)
								{
									Bitmap crop = ImageProcessing.ImageCopier.Copy(bitmap, rect);

									cropFile = new FileInfo(tempDir.FullName + @"\" + counter.ToString() + ".jpg");
									counter++;

									crop.Save(cropFile.FullName, System.Drawing.Imaging.ImageFormat.Jpeg);
									crop.Dispose();

									cropFile.Refresh();

                                    string imageText;
//IRIS                                  
#if IRIS_ENGINE  
                                        imageText = iris.Ocr(cropFile);                                    
#else
                                    if (_settings.General.OcrEngProfile == BscanILL.SETTINGS.Settings.GeneralClass.OcrEngineProfile.Speed)
                                    {
                                        //Speed
                                        this.abbyy.OcrProfile = "Document Conversion Speed";     //Document Archiving is recommended profile for PDF creation. For RTF or PDF text only is recommended Document Conversion profile
                                    }
                                    else
                                    {
                                        //Accuracy
                                        this.abbyy.OcrProfile = "Document Conversion Accuracy";
                                    }
                                    imageText = abbyy.Ocr(cropFile);
#endif                                    
									text += imageText;

                                    if (updateProgressBar)
									    Progress_Changed((((i + 1.0F) / (float)audioZoneImages.Count) + (((j + 1.0F) / (zonesImage.Zones.Count + 1.0F)) / (float)audioZoneImages.Count)));
								}
							}
						}
					}
					catch (Exception ex)
					{
						if ((ex is IllException) == false)
						{
							try { Notifications.Instance.Notify(this, Notifications.Type.Error, "AudioCreator, CreateAudio() 2: " + ex.Message, ex, cropFile); }
							catch { Notifications.Instance.Notify(this, Notifications.Type.Error, "AudioCreator, CreateAudio() 2: " + ex.Message, ex); }
						}
					}

					//Progress_Changed((i + 1.0F) / (audioZoneImages.Count + 1));
					//ProgressUnit_Finished((uint)i);
				}

				if (text.Length > 0)
					CreateDerivative(audioType, destFile, text);
				else
					throw new IllException("No text found.");

                if(updateProgressBar)
				    Progress_Changed(1);
			}
			finally
			{
				try { tempDir.Delete(true); }
				catch { }
			}
		}
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region GetAudioZonesWpfTU()
		private List<BscanILL.Export.AudioZoning.ZonesImage> GetAudioZonesWpfTU(System.Windows.Window parentForm, BscanILL.Hierarchy.IllPages illImages)
		{
			List<BscanILL.Export.AudioZoning.ZonesImage> audioZoneImages = null;

			/*if (illImages.Count > 0)
			{
				if (BscanILL.UI.Dialogs.AlertDlg.Show(parentForm, "You have selected Audio as one of the output files. Would you like to select specific areas of text and exclude any footers, headers, page numbers, charts and other miscellaneous text?",
					BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Question) == true)
				{
					audioZoneImages = BscanILL.Dialogs.AudioZonesDlg.Show(parentForm, illImages);
				}
			}*/            

			return audioZoneImages;
		}
		#endregion

		#region CreateDerivative()
		private void CreateDerivative(AudioType audioType, FileInfo destFile, string text)
		{
			try { speechEngine.SelectVoice(audioType.Voice); }
			catch { speechEngine.SelectVoice(defaultVoice.VoiceInfo.Name); }

			speechEngine.Rate = (int)audioType.Speed;
			speechEngine.Volume = audioType.Volume;

			FileInfo tempFile = new FileInfo(destFile.Directory.FullName + @"\" + Path.GetFileNameWithoutExtension(destFile.Name) + ".wav");

			speechEngine.SetOutputToWaveFile(tempFile.FullName);
			speechEngine.Speak(text);

			speechEngine.SetOutputToNull();

			ConvertToMp3(tempFile, destFile, (int)audioType.BitRate);

			tempFile.Delete();
		//////	Progress_Changed(1);
		}
		#endregion

		#region ConvertToMp3()
		void ConvertToMp3(FileInfo inFile, FileInfo outFile, int bitRate)
		{
			using (WaveStream InStr = new WaveStream(inFile.FullName))
			{
				Mp3WriterConfig m_Config = new Mp3WriterConfig(InStr.Format);
				//m_Config.Mp3Config.format = 
				m_Config.Mp3Config.format.lhv1.dwBitrate = Convert.ToUInt32(bitRate);
				m_Config.Mp3Config.format.mp3.bOriginal = Convert.ToInt32(bitRate);

				using (Mp3Writer writer = new Mp3Writer(new FileStream(outFile.FullName, FileMode.Create), m_Config))
				{
					byte[] buff = new byte[writer.OptimalBufferSize];
					int read = 0;
					int actual = 0;
					long total = InStr.Length;
					while ((read = InStr.Read(buff, 0, buff.Length)) > 0)
					{
						writer.Write(buff, 0, read);
						actual += read;
					}
				}
			}
		}
		#endregion

		#region CreateEmptyDir()
		private void CreateEmptyDir(DirectoryInfo dir)
		{
			dir.Refresh();

			if (dir.Exists)
				dir.Delete(true);

			dir.Create();
		}
		#endregion

		#region SaveImage()
		private static FileInfo SaveImage(BscanILL.Hierarchy.IllPage illPage, DirectoryInfo destDir)
		{
			BscanILL.Export.IP.ExportFileCreator creator = new BscanILL.Export.IP.ExportFileCreator();
			FileInfo file = new FileInfo(destDir.FullName + @"\" + illPage.FilePath.Name + ".tif");

			destDir.Create();

			creator.CreateExportFile(illPage.FilePath, file, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4),
				 Scanners.ColorMode.Bitonal, 200.0 / (double)illPage.FullImageInfo.DpiH);
			//file = illPage.GetExportFile(fileFormat, dpi.Value, warnings);

			file.Refresh();
			return file;
		}
		#endregion

		#endregion

	}
}
