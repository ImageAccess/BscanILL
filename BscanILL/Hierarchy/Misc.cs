using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.Hierarchy
{
	public delegate void IllImageHnd(BscanILL.Hierarchy.IllImage illImage);
	public delegate void ImageAddingEventHnd(BscanILL.Hierarchy.IllImage illImage);
	public delegate void ImageInsertingEventHnd(int index, BscanILL.Hierarchy.IllImage illImage);
	public delegate void ImageRemovingEventHnd(BscanILL.Hierarchy.IllImage illImage);

	public delegate void IllPageHnd(BscanILL.Hierarchy.IllPage illImage);
	public delegate void IllPageAddingEventHnd(BscanILL.Hierarchy.IllPage illImage);
	public delegate void IllPageInsertingEventHnd(int index, BscanILL.Hierarchy.IllPage illImage);
	public delegate void IllPageRemovingEventHnd(BscanILL.Hierarchy.IllPage illImage);

	public delegate void ClearingEventHnd();
	public delegate void DisposingEventHnd();

	
	public enum ImageType
	{
		Scan,
		Page
	}

	#region enum ArticleStatus
	public enum ArticleStatus : byte
	{
		Active = BscanILLData.Models.ArticleStatus.Active,
		Creating = BscanILLData.Models.ArticleStatus.Creating,
		Deleted = BscanILLData.Models.ArticleStatus.Deleted
	}
	#endregion

	#region enum ScanStatus
	public enum ScanStatus : byte
	{
		Active = BscanILLData.Models.ScanStatus.Active,
		Creating = BscanILLData.Models.ScanStatus.Creating,
		Pullslip = BscanILLData.Models.ScanStatus.Pullslip,
		Deleted = BscanILLData.Models.ScanStatus.Deleted
	}
	#endregion

	#region enum PageStatus
	public enum PageStatus : byte
	{
		Active = BscanILLData.Models.PageStatus.Active,
		Creating = BscanILLData.Models.PageStatus.Creating,
		Deleted = BscanILLData.Models.PageStatus.Deleted
	}
	#endregion

	#region enum ExportStatus
	public enum ExportStatus : byte
	{
		Created =BscanILLData.Models.ExportStatus.Created,
		Successfull=BscanILLData.Models.ExportStatus.Successfull,
		Error = BscanILLData.Models.ExportStatus.Error
	}
	#endregion

	#region enum ExportFileStatus
	public enum ExportFileStatus : byte
	{
		Active = BscanILLData.Models.ExportFileStatus.Active,
		Creating = BscanILLData.Models.ExportFileStatus.Creating,
		Deleted = BscanILLData.Models.ExportFileStatus.Deleted
	}
	#endregion

}
