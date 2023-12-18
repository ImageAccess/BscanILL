using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Printing;
using System.Windows.Documents;

namespace BscanILL.Export.Printing
{
	class PrintPaginator : DocumentPaginator, IDisposable
	{
		List<PrintPageElement>							pageElements = new List<PrintPageElement>();

		List<System.IO.FileInfo>	images;
		PageMediaSize				pageMediaSize;
		System.Windows.Thickness	margin;
		System.Windows.Size			pageSize;

		public PrintPaginator(List<System.IO.FileInfo> images, PageMediaSize pageMediaSize, System.Windows.Thickness margin)
		{
			this.images = images;
			this.pageMediaSize = pageMediaSize;
			this.margin = margin;

			double availableW = pageMediaSize.Width.Value - margin.Left - margin.Right;
			double availableH = pageMediaSize.Height.Value - margin.Top - margin.Bottom;
			pageSize = new System.Windows.Size(availableW, availableH);
		}

		public void Dispose()
		{
			foreach (PrintPageElement pageElement in pageElements)
				pageElement.UpdateLayout();
		}

		public override System.Windows.Documents.DocumentPage GetPage(int pageNumber)
		{
			PrintPageElement pageElement;
			
			if(images[pageNumber] != null)
				pageElement = new PrintPageElement(images[pageNumber], pageSize.Width / 96.0, pageSize.Height / 96.0);
			else
				pageElement = new PrintPageElement();

			double marginX = pageMediaSize.Width.Value - pageSize.Width;
			double marginY = pageMediaSize.Height.Value - pageSize.Height;

			pageElement.Margin = new System.Windows.Thickness(marginX / 2, marginY / 2,	marginX / 2, marginY / 2);

			pageElement.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			pageElement.VerticalAlignment = System.Windows.VerticalAlignment.Center;
			pageElement.Measure(new System.Windows.Size(pageMediaSize.Width.Value, pageMediaSize.Height.Value));
			pageElement.Arrange(new System.Windows.Rect(0, 0, pageMediaSize.Width.Value, pageMediaSize.Height.Value));

			System.Windows.Documents.DocumentPage documentPage = new System.Windows.Documents.DocumentPage(pageElement);
			pageElements.Add(pageElement);

			return documentPage;
		}

		public override bool IsPageCountValid
		{
			get { return true; }
		}

		public override int PageCount
		{
			get { return this.images.Count; }
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
