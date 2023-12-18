using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Printing;

namespace BscanILL.Export.Printing
{
	class PrintTestSheetPaginator : DocumentPaginator
	{
		PageMediaSize pageMediaSize;
		System.Windows.Thickness margin;
		System.Windows.Size pageSize;

		public PrintTestSheetPaginator(PageMediaSize pageMediaSize, System.Windows.Thickness margin)
		{
			this.pageMediaSize = pageMediaSize;
			this.margin = margin;

			double availableW = pageMediaSize.Width.Value - margin.Left - margin.Right;
			double availableH = pageMediaSize.Height.Value - margin.Top - margin.Bottom;
			pageSize = new System.Windows.Size(availableW, availableH);
		}

		public override DocumentPage GetPage(int pageNumber)
		{
			PrintPageElement pageElement = new PrintPageElement(pageSize.Width / 96.0, pageSize.Height / 96.0);

			double marginX = pageMediaSize.Width.Value - pageSize.Width;
			double marginY = pageMediaSize.Height.Value - pageSize.Height;

			pageElement.Margin = new System.Windows.Thickness(marginX / 2, marginY / 2,	marginX / 2, marginY / 2);

			pageElement.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			pageElement.VerticalAlignment = System.Windows.VerticalAlignment.Center;
			pageElement.Measure(new System.Windows.Size(pageMediaSize.Width.Value, pageMediaSize.Height.Value));
			pageElement.Arrange(new System.Windows.Rect(0, 0, pageMediaSize.Width.Value, pageMediaSize.Height.Value));

			DocumentPage documentPage = new DocumentPage(pageElement);

			return documentPage;
		}

		public override bool IsPageCountValid
		{
			get { return true; }
		}

		public override int PageCount
		{
			get { return 1; }
		}

		public override System.Windows.Size PageSize
		{
			get
			{
				return pageSize;
			}
			set
			{
				this.pageSize = value;
			}
		}

		public override IDocumentPaginatorSource Source
		{
			get { return null; }
		}
	
	}
}
