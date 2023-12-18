using System;
using System.Collections;
using System.Collections.Specialized ;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging ;
//using System.Data;
using System.IO ;
using System.Net ;
using System.Text ;
using System.Diagnostics ;
using System.Configuration ;
using System.Net.Cache;



namespace Scanners
{
	/// <summary>
	/// Summary description for BookEyeControl.
	/// </summary>
	public static class Scanner
	{
		//static variables
		static ImageCodecInfo		tiffImageCodecInfo = Scanner.GetEncoderInfo(ImageFormat.Tiff);
		static ImageCodecInfo		jpegImageCodecInfo = Scanner.GetEncoderInfo(ImageFormat.Jpeg);
		
		static EncoderParameters	tiffBwEncoderParams = GenerateTiffBwEncoderParams();
		static EncoderParameters	tiffGrayEncoderParams = GenerateTiffGrayEncoderParams();
		static EncoderParameters	tiffColorEncoderParams = GenerateTiffColorEncoderParams();
		static EncoderParameters	jpegEncoderParams = GenerateJpegEncoderParams();

		//PUBLIC PROPERTIES
		#region public properties
		public static ImageCodecInfo	TiffImageCodecInfo { get { return tiffImageCodecInfo; } }
		public static ImageCodecInfo	JpegImageCodecInfo { get { return jpegImageCodecInfo; } }
		
		public static EncoderParameters TiffBwEncoderParams { get { return tiffBwEncoderParams; } }
		public static EncoderParameters TiffGrayEncoderParams { get { return tiffGrayEncoderParams; } }
		public static EncoderParameters TiffColorEncoderParams { get { return tiffColorEncoderParams; } }
		public static EncoderParameters JpegEncoderParams { get { return jpegEncoderParams; } }

		#endregion

		//PUBLIC METHODS
		#region public methods

		#region GetEncoderInfo()
		public static ImageCodecInfo GetEncoderInfo(String mimeType)
		{
			int j;
			ImageCodecInfo[] encoders;
			encoders = ImageCodecInfo.GetImageEncoders();
			for(j = 0; j < encoders.Length; ++j)
			{
				if(encoders[j].MimeType == mimeType)
					return encoders[j];
			}
			return null;
		}

		public static ImageCodecInfo GetEncoderInfo(ImageFormat imageFormat)
		{
			ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

			foreach(ImageCodecInfo encoder in encoders)
			{
				if(encoder.FormatID == imageFormat.Guid)
					return encoder;
			}

			return null;
		}
		#endregion

		#region PingDevice()
		public static void PingDevice(Scanners.PingDeviceReceiver receiver, string ip, bool wakeOnLan)
		{
			//string ip = Scanners.Settings.Instance.S2NScanner.Ip;
			
			using (System.Net.NetworkInformation.Ping png = new System.Net.NetworkInformation.Ping())
			{
				try
				{
					System.Net.NetworkInformation.PingReply reply = png.Send(ip, 5000);
					
					if (reply.Status != System.Net.NetworkInformation.IPStatus.Success)
					{
						if (wakeOnLan == false || WakeOnLan(receiver) == false)						
							throw new ScannersEx("Can't connect to the S2N Scanner " + ip + "! ");
					}
				}
				catch (System.Net.NetworkInformation.PingException)
				{
					if (wakeOnLan == false || WakeOnLan(receiver) == false)
						throw new ScannersEx("Can't connect to the S2N Scanner " + ip + "! ");
				}
				catch
				{
					throw new ScannersEx("Can't connect to the S2N Scanner " + ip + "! ");
				}
			}
		}
		#endregion
	
		#endregion

		//PRIVATE METHODS
		#region private methods

		#region GenerateTiffBwEncoderParams()
		private static EncoderParameters GenerateTiffBwEncoderParams()
		{
			EncoderParameters encoderParams = new EncoderParameters(2);
			encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionCCITT4);
			encoderParams.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 1L);
			return encoderParams;
		}
		#endregion

		#region GenerateTiffGrayEncoderParams()
		private static EncoderParameters GenerateTiffGrayEncoderParams()
		{
			EncoderParameters encoderParams = new EncoderParameters(2);
			encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionLZW);
			encoderParams.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 8L);
			return encoderParams;
		}
		#endregion

		#region GenerateTiffColorEncoderParams()
		private static EncoderParameters GenerateTiffColorEncoderParams()
		{
			EncoderParameters encoderParams = new EncoderParameters(2);
			encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionLZW);
			encoderParams.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 24L);
			return encoderParams;
		}
		#endregion

		#region GenerateJpegEncoderParams()
		private static EncoderParameters GenerateJpegEncoderParams()
		{
			EncoderParameters encoderParams = new EncoderParameters(1);
			encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 80L);
			return encoderParams;
		}
		#endregion

		#region WakeOnLan()
		public static bool WakeOnLan(Scanners.PingDeviceReceiver receiver)
		{
			try
			{
				Scanners.Settings.S2NScannerClass s = Scanners.Settings.Instance.S2NScanner;

				if (s.WakeOnLAN && s.WakeOnLanMacAddress.Length == 12)
					return Scanners.WakeOnLAN.WakeS2NOnLan(s.WakeOnLanMacAddress, s.Ip, receiver);
				else
					return false;
			}
			catch
			{
				return false;
			}
		}
		#endregion
	
		#endregion

	}

}
