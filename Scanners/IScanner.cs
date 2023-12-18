using System;
using System.IO ;
using System.Drawing ;
using System.Drawing.Imaging ;


namespace Scanners
{

	public delegate void PreviewScannedHnd(Bitmap preview);
	public delegate void ImagesScannedHnd(int operationId, Bitmap image1, Bitmap image2);
	public delegate void ImageScannedHnd(int operationId, Bitmap image);
	public delegate void FilesScannedHnd(int operationId, string file1, string file2);
	public delegate void FileScannedHnd(int operationId, string filePath);
	public delegate void OperationSuccessfullHnd(int operationId);
	public delegate void OperationErrorHnd(int operationId, Exception ex);
	public delegate void ProgressChangedHnd(string description, float progress);


	#region interface IScanner
	/// <summary>
	/// Summary description for IScanner.
	/// </summary>
	public interface IScanner
	{
		//properties
		Scanners.DeviceInfo		DeviceInfo	{ get;}
		Scanners.MODELS.Model	Model { get; }

		//Scanners.S2N.DocSizeType	DocSizeType { get; }
		//Scanners.ColorMode		ColorMode	{ get; }
		//short					Dpi			{ get; }
		//double					Brightness	{ get; }
		//double					Contrast	{ get; }

		//methods
		void		Dispose();
		//void		Reset(int operationId);
		/*void		ChangeSettings(int operationId, Scanners.S2N.DocSizeType docSize, Scanners.ColorMode colorMode, short dpi, double brightness, double contrast, bool autoFocus);
		void		Scan(int operationId, Scanners.S2N.DocSizeType docSize, Scanners.ColorMode colorMode, short dpi, double brightness, double contrast, bool autoFocus);
		void		ActivateScannerButtons();
		void		DeactivateScannerButtons();*/

		//events
		/*event Scanners.FileScannedHnd			ImageScanned;
		event Scanners.ImageScannedHnd			PreviewScanned; 
		event Scanners.OperationSuccessfullHnd	OperationSuccessfull;
		event Scanners.OperationErrorHnd		OperationError;*/
	}
	#endregion

	#region interface PingDeviceReceiver
	public interface PingDeviceReceiver
	{
		void PingDeviceProgressChanged(string description);
	}
	#endregion

	#region struct DeviceInfo
	[Serializable]
	public abstract class DeviceInfo
	{
		Scanners.MODELS.Model scannerModel;


		public DeviceInfo(Scanners.MODELS.Model scannerModel)
		{
			this.scannerModel = scannerModel;
		}

		public Scanners.MODELS.Model ScannerModel { get { return this.scannerModel; } }
		public abstract string SerialNumber { get; }
		public abstract string Firmware{get;}


		public override string ToString()
		{
			return "";
		}
	}
	#endregion

	#region struct DeviceInfoS2N
	[Serializable]
	public class DeviceInfoS2N : DeviceInfo
	{	
		string ip;
		string firmware;
		string hostname;
		string deviceName;
		string netMask;
		string gateway;
		string dhcp;

		public DeviceInfoS2N(string ip, Scanners.MODELS.Model scannerModel, string firmwareVersion, string hostname, string deviceName, string netMask, string gateway, string dhcp)
			: base(scannerModel)
		{
			this.ip = ip;
			this.firmware = firmwareVersion;
			this.hostname = hostname;
			this.deviceName = deviceName;
			this.netMask = netMask;
			this.gateway = gateway;
			this.dhcp = dhcp;
		}

		public override string SerialNumber { get { return this.hostname; } }
		public override string Firmware { get { return this.firmware; } }

		public string Ip {get{return this.ip;}}
		public string Hostname{get{return this.hostname;}}
		public string DeviceName{get{return this.deviceName;}}
		public string NetMask{get{return this.netMask;}}
		public string Gateway{get{return this.gateway;}}
		public string Dhcp{get{return this.dhcp;}}

		public override string ToString()
		{
			string str = "IP: " + Ip + Environment.NewLine;
			str += "DeviceType: " + this.ScannerModel.ScannerSubGroup.ToString() + Environment.NewLine;
			str += "FirmwareVersion: " + Firmware + Environment.NewLine;
			str += "Hostname: " + Hostname + Environment.NewLine;
			str += "DeviceName: " + DeviceName + Environment.NewLine;
			str += "NetMask: " + NetMask + Environment.NewLine;
			str += "Gateway: " + Gateway + Environment.NewLine;
			str += "Dhcp: " + Dhcp + Environment.NewLine;

			return str;
		}
	}
	#endregion

	#region struct DeviceInfoTwain
	[Serializable]
	public class DeviceInfoTwain : DeviceInfo
	{
		string serialNumber;
		string firmware;

		public DeviceInfoTwain(Scanners.MODELS.Model scannerModel, string serialNumber, string firmware)
			: base(scannerModel)
		{
			this.serialNumber = serialNumber;
			this.firmware = firmware;
		}

		public override string SerialNumber { get { return this.serialNumber; } }
		public override string Firmware { get { return this.firmware; } }

		public override string ToString()
		{
			string str = "";
			str += "Serial Number: " + serialNumber + Environment.NewLine;
			str += "Firmware: " + firmware + Environment.NewLine;

			return str;
		}
	}
	#endregion

	#region struct DeviceInfoRebel
	[Serializable]
	public class DeviceInfoRebel : DeviceInfo
	{
		string serialNumber;
		string firmware;

		public DeviceInfoRebel(Scanners.MODELS.Model scannerModel, string serialNumber, string firmware)
			: base(scannerModel)
		{
			this.serialNumber = serialNumber;
			this.firmware = firmware;
		}

		public override string SerialNumber { get { return this.serialNumber; } }
		public override string Firmware { get { return this.firmware; } }

		public override string ToString()
		{
			string str = "";
			str += "Serial Number: " + serialNumber + Environment.NewLine;
			str += "Firmware: " + firmware + Environment.NewLine;

			return str;
		}
	}
	#endregion


}
