using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BscanILLData.Models
{
	#region enum ExportType
	public enum ExportType : byte
	{
		Ftp = 0x00,
		Ariel = 0x01,
		ILLiad = 0x02,
		Email = 0x03,
		Odyssey = 0x04,
		SaveOnDisk = 0x05,
		ArielPatron = 0x06,
		FtpDir = 0x07,
		Print = 0x08,
		ArticleExchange = 0x09,
        Tipasa = 0x0A,
        WorldShareILL = 0x0B,
		Rapido = 0x0C,
		None = 0xFF
		//Lending = Ariel | ArielPatron | Odyssey | Email | Ftp | SaveOnDisk,
		//DocDelivery = ILLiad
	}
	#endregion

	#region enum ArticleStatus
	public enum ArticleStatus : byte
	{
		Active = 0,
		Creating = 1,
		Deleted = 255
	}
	#endregion

	#region enum ScanStatus
	public enum ScanStatus : byte
	{
		Active = 0,
		Creating = 1,
		Pullslip = 2,
		Deleted = 255
	}
	#endregion

	#region enum PageStatus
	public enum PageStatus : byte
	{
		Active = 0,
		Creating = 1,
		Deleted = 255
	}
	#endregion

	#region enum ExportStatus
	public enum ExportStatus : byte
	{
		Created = 0,
		Successfull = 1,
		Error = 2
	}
	#endregion

	#region enum ExportFileStatus
	public enum ExportFileStatus : byte
	{
		Active = 0,
		Creating = 1,
		Deleted = 255
	}
	#endregion

	#region enum ColorMode
	public enum ColorMode
	{
		Color = 1,
		Grayscale = 2,
		Bitonal = 3,
		Unknown = 5
	}
	#endregion

	#region enum ScanFileFormat
	public enum ScanFileFormat : byte
	{
		Jpeg = 1,
		Tiff = 2,
		Png = 4
	}
	#endregion

	#region enum ExportFileFormat
	public enum ExportFileFormat : byte
	{
		Jpeg = ScanFileFormat.Jpeg,
		Tiff = ScanFileFormat.Tiff,
		Pdf = ScanFileFormat.Png,
		Png = 8,
		SPdf = 16,
		Text = 32,
		Audio = 64,
		Auto = 128,
		Unknown = 0XFF
	}
	#endregion







	

	#region enum SqlEngine
	public enum SqlEngine : byte
	{
		SQLite			= 0x01,
		MsSqlServer2012 = 0x02
	}
	#endregion

	#region enum UserState
	public enum UserState : byte
	{
		Active = 1
	}
	#endregion

	#region enum UserType
	[Serializable]
	public enum UserType : byte
	{
		Normal = 0,
		ILL = 1,
		Faculty = 2,
		Error = 99
	}
	#endregion

	#region enum SessionState
	public enum SessionState : byte
	{
		Open	= 0x01,
		Closed	= 0x02
	}
	#endregion



	#region enum MetricsActionType
	public enum MetricsActionType : int
	{
		Unknown = 0,

		TotalScanTime = 1,
		ScannerScanTime = 2,
		ItTime = 3,
		ItFindPagesSplitter = 4,
		ChangeSettingsTime = 16,

		ModifyTime = 32,

		IdleTimeBetweenScans = 64
	}
	#endregion

	#region enum MetricsFlags
	[Flags]
	public enum MetricsFlags : int
	{
		NoFlag = 0,
		ScanSingle = 1,
		ScanSplit = 2,
		ScanBook = 4,
		ScanTurbo = 8,

		ItCrop = 64,
		ItDeskew = 128,
		ItBookfold = 256,
		ItFingers = 512,
		ItBackgroundFlattener = 1024
	}
	#endregion

	#region enum AdminMessageCode
	public enum AdminMessageCode : int
	{
		Unknown = 0,
	}
	#endregion
	
	#region enum AdminMessagePriority
	public enum AdminMessagePriority : byte
	{
		Unknown = 0,
		Low = 4,
		Medium = 8,
		High = 12
	}
	#endregion


}
