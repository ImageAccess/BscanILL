using System;
using System.IO;

namespace BscanILL.Export.Ariel
{
	/// <summary>
	/// Summary description for IAriel.
	/// </summary>
	public interface IAriel
	{
		event ProgressChangedHandle ProgressChanged;
		event ProgressCommentHandle ProgressComment;
		
		//bool StartAriel();
		void ExportArticle(ExportUnit exportUnit);
		void ExportArticleToPatron(ExportUnit exportUnit);
	}

}
