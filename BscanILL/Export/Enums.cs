using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.Export
{
	
	public enum ExportType : byte
	{
		Ftp = BscanILLData.Models.ExportType.Ftp,
		Ariel = BscanILLData.Models.ExportType.Ariel,
		ILLiad = BscanILLData.Models.ExportType.ILLiad,
		Email = BscanILLData.Models.ExportType.Email,
		Odyssey = BscanILLData.Models.ExportType.Odyssey,
		SaveOnDisk = BscanILLData.Models.ExportType.SaveOnDisk,
		ArielPatron = BscanILLData.Models.ExportType.ArielPatron,
		FtpDir = BscanILLData.Models.ExportType.FtpDir,
		Print = BscanILLData.Models.ExportType.Print,
		ArticleExchange = BscanILLData.Models.ExportType.ArticleExchange,
        Tipasa = BscanILLData.Models.ExportType.Tipasa,
        WorldShareILL = BscanILLData.Models.ExportType.WorldShareILL,
		Rapido = BscanILLData.Models.ExportType.Rapido,
		None = BscanILLData.Models.ExportType.None
		//Lending = Ariel | ArielPatron | Odyssey | Email | Ftp | SaveOnDisk,
		//DocDelivery = ILLiad
	}

}
