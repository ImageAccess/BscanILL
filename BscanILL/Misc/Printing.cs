using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;

namespace BscanILL.Misc
{
    class Printing
    {
        private BscanILL.Hierarchy.IllImage currentImageToPrint = null;
        private BscanILL.Hierarchy.IllPage currentPageToPrint = null;
        private BscanILL.Hierarchy.Article currentArticleToPrint = null;
        private BscanILL.Hierarchy.SessionBatch currentBatchToPrint = null;
        private int minImageCount = 0;
        private int maxImageCount = 0;
        private PrintRange rangeToPrint ;
        private int pageIndex;
        private int rangeFrom = 0;
        private int rangeTill = 0;
        private PrintMode modePrint = PrintMode.Image;
        private bool printingPages = false;

        #region enum PrintMode
        private enum PrintMode
        {
            None,
            Image ,
            Article ,
            Batch
        }
        #endregion

        //CONSTRUCTOR
        #region constructor
        public Printing(BscanILL.Hierarchy.SessionBatch batch, BscanILL.Hierarchy.Article article, BscanILL.Hierarchy.IllPage illPage)
        {
            printingPages = true;
            currentPageToPrint = illPage;
            currentBatchToPrint = batch;
            currentArticleToPrint = article;
            SetMaxCounts();
        }

        public Printing(BscanILL.Hierarchy.SessionBatch batch , BscanILL.Hierarchy.Article article ,BscanILL.Hierarchy.IllImage illImage)
        {
            printingPages = false;
            currentImageToPrint = illImage;
            currentBatchToPrint = batch;
            currentArticleToPrint = article;
            SetMaxCounts();
        }

        private void SetMaxCounts()
        {           
            maxImageCount = 0;
            if (currentBatchToPrint != null)
            {
                foreach (BscanILL.Hierarchy.Article oneArticle in currentBatchToPrint.Articles)
                {
                    if (printingPages)
                    {
                        maxImageCount += oneArticle.GetPages(true).Count;
                    }
                    else
                    {
                        maxImageCount += oneArticle.GetScans(true).Count;                        
                    }                    
                }
            }
            else
            {
                if (printingPages)
                {
                    maxImageCount += currentArticleToPrint.GetPages(true).Count;
                }
                else
                {
                    maxImageCount += currentArticleToPrint.GetScans(true).Count;
                }
            }

            if (maxImageCount > 0)
            {
                minImageCount = 1;
            }
            else
            {
                minImageCount = 0;
            }
        }
        #endregion

        //PUBLIC METHODS
        #region public methods        
        public void PrintPageDialog()
        {
            PrintDialog();
        }
        #endregion

        //PRIVATE METHODS
        #region private methods

        private bool IsPrinterInstalled( string printerName)
        {
            //  to get names of available printers                                    
            foreach ( string name_of_printer in PrinterSettings.InstalledPrinters)
            {
                if( string.Compare(name_of_printer, printerName, true) == 0 )
                {
                    return true;
                }                
            }
            return false;
        }

        private void CheckPrinterSupport()
        {
            PrinterSettings nastaveni = new PrinterSettings();
            if (nastaveni.CanDuplex)
            {
                System.Windows.MessageBox.Show("can duplex");
                nastaveni.Duplex = System.Drawing.Printing.Duplex.Default;
            }
            else
            {
                System.Windows.MessageBox.Show("cannot duplex");
                nastaveni.Duplex = Duplex.Simplex;
            }

            if (nastaveni.IsDefaultPrinter)
            {
                System.Windows.MessageBox.Show(nastaveni.PrinterName + "is default printer");
            }
            else
            {
                System.Windows.MessageBox.Show(nastaveni.PrinterName + "is not default printer");
            }
        }

        private void PrinterPreviewDialog()
        {
            System.Windows.Forms.PrintPreviewDialog previewDlg = new System.Windows.Forms.PrintPreviewDialog();
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += PrintPage;

            PrinterSettings nastaveni = new PrinterSettings();

            pd.PrinterSettings = nastaveni;
            previewDlg.Document = pd;
            previewDlg.ShowDialog();
        }

        private void PrinterSetupDialog()
        {
            System.Windows.Forms.PageSetupDialog setupDialog = new System.Windows.Forms.PageSetupDialog();
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += PrintPage;

            setupDialog.Document = pd;
            if (setupDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pd.Print();
            }
        }

        //PageIsGoodToPrint 
        // parce selectedPages string (for example  "1,2,8,12-15") and compare with page index
        private bool PageIsGoodToPrint( int pageIndexToCompare, string selectedPagesSpecification)
        {
            bool isGood = false;
            bool wasDash = false;
            string pageFrom = "";
            string pageTill = "";
            for(int i = 0; i < selectedPagesSpecification.Length; i++)
            {
                if((selectedPagesSpecification[i] >= '0' ) && (selectedPagesSpecification[i] >= '9') )
                {
                    if( ! wasDash )
                    {
                        pageFrom = pageFrom + selectedPagesSpecification[i];
                    }
                    else
                    {
                        pageTill = pageTill + selectedPagesSpecification[i];
                    }
                }
                else
                if (selectedPagesSpecification[i] == '-')
                {
                    wasDash = true;
                }
                else
                if (selectedPagesSpecification[i] == ',')
                {
                    int pageFromInt = 0;
                    if (pageFrom.Length > 0)
                    {
                        pageFromInt = int.Parse(pageFrom);
                    }
                    int pageTillInt = 0;
                    if (pageTill.Length > 0)
                    {
                        pageTillInt = int.Parse(pageTill);
                    }

                    if(pageTillInt == 0)
                    {
                        pageTillInt = pageFromInt;
                    }

                    if((pageFromInt > 0) && (pageIndexToCompare >= pageFromInt) && (pageIndexToCompare <= pageTillInt) )
                    {
                        isGood = true;
                        break;
                    }

                    pageFrom = "";
                    pageTill = "";
                }
                else
                if (selectedPagesSpecification[i] == ' ')
                {

                }
                else
                {
                    //wrong character in format - fail
                    break;
                }

            }

            if(pageFrom.Length > 0)
            {
                int pageFromInt = 0;
                if (pageFrom.Length > 0)
                {
                    pageFromInt = int.Parse(pageFrom);
                }
                int pageTillInt = 0;
                if (pageTill.Length > 0)
                {
                    pageTillInt = int.Parse(pageTill);
                }

                if (pageTillInt == 0)
                {
                    pageTillInt = pageFromInt;
                }

                if ((pageFromInt > 0) && (pageIndexToCompare >= pageFromInt) && (pageIndexToCompare <= pageTillInt))
                {
                    isGood = true;                 
                }
            }

            return isGood;
        }

        private string GetImageToPrintPath()
        {
            string path = "";
            int currentIndex = 0;            
            
            if (currentBatchToPrint != null)
            {                
                for (int i = 0; i < currentBatchToPrint.Articles.Count; i++)
                {
                       if (printingPages)
                       {
                           Hierarchy.IllPages pages = currentBatchToPrint.Articles[i].GetPages(true);
                           if (pageIndex <= (currentIndex + pages.Count))
                           {
                               //found    
                               if( ((pageIndex - currentIndex - 1) >= 0) && (( pageIndex - currentIndex - 1 ) < pages.Count ) )
                               {
                                   path = pages[pageIndex - currentIndex - 1].FilePath.FullName;
                               }
                               break;
                           }
                           else
                           {
                               currentIndex += pages.Count;
                           }
                       }
                       else
                       {
                           Hierarchy.IllImages images = currentBatchToPrint.Articles[i].GetScans(true);
                           if (pageIndex <= (currentIndex + images.Count))
                           {
                               //found     
                               if (((pageIndex - currentIndex - 1) >= 0) && ((pageIndex - currentIndex - 1) < images.Count))
                               {
                                   path = images[pageIndex - currentIndex - 1].FilePath.FullName;
                               }
                               break;
                           }
                           else
                           {
                               currentIndex += images.Count;
                           }
                       }                                      
                }
            }
            else
            if (currentArticleToPrint != null)
            {               
                    if (printingPages)
                    {
                        Hierarchy.IllPages pages = currentArticleToPrint.GetPages(true);
                        if (((pageIndex - 1) >= 0) && ((pageIndex - 1) < pages.Count))
                        {
                            path = pages[(pageIndex - 1)].FilePath.FullName;
                        }
                    }
                    else
                    {            
                        Hierarchy.IllImages images = currentArticleToPrint.GetScans(true);
                        if (((pageIndex - 1) >= 0) && ((pageIndex - 1) < images.Count))
                        {
                            path = images[(pageIndex - 1)].FilePath.FullName;
                        }
                    }                
            }

            return path;
        }

        private void SetPageIndexRange(ref int minPageIndex, ref int maxPageIndex, ref int printJobMinRange, ref int printJobMaxRange)
        {
            minPageIndex = maxPageIndex = printJobMinRange = printJobMaxRange = 0;
            if (modePrint != PrintMode.None)
            {
                if (modePrint != PrintMode.Image)
                {
                    //for all pages or range set min and max pages based on if Batch or just article loaded 
                    if (modePrint == PrintMode.Batch)
                    {
                        printJobMinRange = minImageCount;
                        printJobMaxRange = maxImageCount;

                        minPageIndex = minImageCount - 1;
                        maxPageIndex = maxImageCount;
                    }
                    else
                    {
                        //print article
                        if(currentBatchToPrint != null)
                        {
                            bool foundBegin = false;
                            
                            for ( int i = 0; i < currentBatchToPrint.Articles.Count; i++ )
                            {
                                if ( ! foundBegin )
                                {
                                    if (currentArticleToPrint.Id == currentBatchToPrint.Articles[i].Id)
                                    {
                                        //found the article to print in the batch
                                        printJobMaxRange = printJobMinRange;
                                       
                                        if (printingPages)
                                        {                                          
                                            printJobMaxRange += currentBatchToPrint.Articles[i].GetPages(true).Count;
                                        }
                                        else
                                        {                                            
                                            printJobMaxRange += currentBatchToPrint.Articles[i].GetScans(true).Count;
                                        }                                        

                                        printJobMinRange++;                                        

                                        minPageIndex = printJobMinRange - 1;
                                        maxPageIndex = printJobMaxRange;

                                        foundBegin = true;
                                        break;
                                    }
                                    else
                                    {                                        
                                        if (printingPages)
                                        {                                           
                                            printJobMinRange += currentBatchToPrint.Articles[i].GetPages(true).Count;
                                        }
                                        else
                                        {                                           
                                            printJobMinRange += currentBatchToPrint.Articles[i].GetScans(true).Count;
                                        }
                                    }
                                }                               
                            }
                        }
                        else
                        {
                            printJobMinRange = minImageCount;
                            printJobMaxRange = maxImageCount;

                            minPageIndex = minImageCount - 1;
                            maxPageIndex = maxImageCount;
                        }
                    }

                }
            }
        }

        private void PrintDialog()
        {
            try
            {
                System.Windows.Forms.PrintDialog printDialog = new System.Windows.Forms.PrintDialog();

                BscanILL.UI.Dialogs.PrintOptionDlg printOptionDialog = new UI.Dialogs.PrintOptionDlg();
                
                if (currentBatchToPrint == null)
                {
                    printOptionDialog.currentBatch.IsEnabled = false;
                }
                
                modePrint = PrintMode.None;
                if (printOptionDialog.ShowDialog() == true)
                {
                    if (printOptionDialog.currentImg.IsChecked == true)
                    {
                        modePrint = PrintMode.Image;                        
                        printDialog.AllowSomePages = false;                        
                    }
                    else
                    if (printOptionDialog.currentArticle.IsChecked == true)
                    {
                        modePrint = PrintMode.Article;                        
                        printDialog.AllowSomePages = true;                        
                    }
                    else
                    if (printOptionDialog.currentBatch.IsChecked == true)
                    {
                        modePrint = PrintMode.Batch;                        
                        printDialog.AllowSomePages = true;
                        
                    }
                }

                if (modePrint != PrintMode.None)
                {
                    int minPageIndex = 0;
                    int maxPageIndex = 0;
                    int printJobMinRange = 0;
                    int printJobMaxRange = 0;

                    if (modePrint != PrintMode.Image)
                        SetPageIndexRange(ref minPageIndex, ref maxPageIndex, ref printJobMinRange, ref printJobMaxRange);

                    printDialog.AllowCurrentPage = true;
                    printDialog.AllowSelection = false;
                    printDialog.AllowPrintToFile = false;
                    printDialog.UseEXDialog = true;

                    PrintDocument pd = new PrintDocument();

                    pd.PrintPage += PrintPage;
                    if ( (modePrint != PrintMode.Image) &&(printJobMinRange != printJobMaxRange) )
                    {                        
                        pd.PrinterSettings.PrintRange = PrintRange.AllPages;                     
                    }
                    else
                    {
                        pd.PrinterSettings.PrintRange = PrintRange.CurrentPage;
                    }

                    pd.PrinterSettings.FromPage = printJobMinRange; // minImageCount;
                    pd.PrinterSettings.ToPage = printJobMaxRange; // maxImageCount;

                    pd.PrinterSettings.MinimumPage = printJobMinRange; // minImageCount;
                    pd.PrinterSettings.MaximumPage = printJobMaxRange; // maxImageCount;

                    printDialog.Document = pd;

                    rangeToPrint = PrintRange.AllPages;
                    rangeFrom = 0;
                    rangeTill = 0;
                    pageIndex = 0;

                    bool goPrint = false;
                    if (printOptionDialog.ForcePrint)
                    {
                        //print without the dialog 'current/all/page selection' printing dialog
                        if (modePrint != PrintMode.Image)
                        {
                            //print all pages from article/batch
                            rangeToPrint = PrintRange.AllPages;
                            rangeFrom = printJobMinRange;
                            rangeTill = printJobMaxRange;
                            pageIndex = rangeFrom - 1;
                        }
                        else
                        {
                            //print current page
                            rangeToPrint = PrintRange.CurrentPage;
                        }
                        goPrint = true;
                    }
                    else
                    {
                      if (printDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                      {
                        //seems that collate and copies parameter is handeled by the printing dialog itself and we do not have to loop through amount of copies and take care of collate (page order)...
                        //bool collate = printDialog.PrinterSettings.Collate;
                        //int copies = printDialog.PrinterSettings.Copies;
                        if (modePrint != PrintMode.Image)
                        {
                            if (printDialog.PrinterSettings.PrintRange == PrintRange.Selection)
                            {
                                //print specific pages  (1,2,8  or 2-6) // actually I do not know what this settings stands for becasue specification '3' or '2-3' is handled by PrintRange.SomePages
                                rangeToPrint = PrintRange.Selection;
                                pageIndex = minImageCount - 1;
                            }
                            else
                            if (printDialog.PrinterSettings.PrintRange == PrintRange.SomePages)
                            {
                                //print Range of pages
                                rangeToPrint = PrintRange.SomePages;
                                rangeFrom = printDialog.PrinterSettings.FromPage;
                                rangeTill = printDialog.PrinterSettings.ToPage;
                                pageIndex = rangeFrom - 1;
                            }
                            else
                            if (printDialog.PrinterSettings.PrintRange == PrintRange.AllPages)
                            {
                                //print all pages
                                rangeToPrint = PrintRange.AllPages;
                                rangeFrom = printJobMinRange;
                                rangeTill = printJobMaxRange;
                                pageIndex = rangeFrom - 1;
                            }
                            else
                            if (printDialog.PrinterSettings.PrintRange == PrintRange.CurrentPage)
                            {
                                //print current page
                                rangeToPrint = PrintRange.CurrentPage;
                            }
                        }
                        else
                        {
                            //print current page
                            rangeToPrint = PrintRange.CurrentPage;
                        }
                        goPrint = true;
                      }
                    }

                    if (goPrint)
                    {
                        pd.Print();
                        //PrintPage_Test();  //to run smae code but without printing
                    }
                }
                                             
            }
            catch( Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }


        private void PrintPage_Test()
        {
            bool tmpB = true ;
            while (tmpB)
            {
                tmpB = PrintPage_Tmp();
            }
        }

        private bool PrintPage_Tmp()
        {
            bool eHasMorePages = false;
                System.Drawing.Image img = null;                
                //System.Drawing.Point loc = new System.Drawing.Point(0, 0);

                string currentPath = "";

                pageIndex++;

                switch (rangeToPrint)
                {
                    case PrintRange.AllPages:
                    case PrintRange.SomePages:
                        {
                            //print Range of pages
                            if ((pageIndex >= rangeFrom) && (pageIndex <= rangeTill))
                            {
                                currentPath = GetImageToPrintPath();

                                if (currentPath.Length > 0)
                                {
                                    //pull slip                                
                                    img = System.Drawing.Image.FromFile(currentPath);
                                    if (pageIndex < rangeTill)
                                    {
                                        eHasMorePages = true;
                                    }
                                    else
                                    {
                                        eHasMorePages = false;
                                    }
                                }
                                else
                                {
                                    eHasMorePages = false;
                                }
                            }
                            else
                            {
                                eHasMorePages = false;
                            }
                        }
                        break;

                    case PrintRange.Selection:
                        {
                            //print specific pages  (1,2,8  or 2-6)
                            /*
                                                        if ((pageIndex >= minImageCount) && (pageIndex <= maxImageCount))
                                                        {
                                                            string selectedPages = "1,2,8";  //just for testing currently I do not know how to gather the string value with psecified pages from dialog control
                                                            if (PageIsGoodToPrint(pageIndex, selectedPages))
                                                            {
                                                                if (pageIndex == 1)
                                                                {
                                                                    //pull slip                                
                                                                    img = System.Drawing.Image.FromFile(currentArticleToPrint.Pullslip.FilePath.FullName);
                                                                }
                                                                else
                                                                {
                                                                    if (((pageIndex - 2) < currentArticleToPrint.Scans.Count) && ((pageIndex - 2) >= 0))
                                                                    {
                                                                        img = System.Drawing.Image.FromFile(currentArticleToPrint.Scans[(pageIndex - 2)].FilePath.FullName);
                                                                    }
                                                                }
                                                            }

                                                            e.HasMorePages = true;
                                                        }
                                                        else
                                                        {
                                                            e.HasMorePages = false;
                                                        }
                            */
                            eHasMorePages = false;
                        }
                        break;

                    case PrintRange.CurrentPage:
                        {
                            //print current page
                            if (printingPages)
                            {
                                img = System.Drawing.Image.FromFile(currentPageToPrint.FilePath.FullName);
                            }
                            else
                            {
                                 img = System.Drawing.Image.FromFile(currentImageToPrint.FilePath.FullName);
                            }
                            eHasMorePages = false;
                        }
                        break;

                    default:
                        {
                            eHasMorePages = false;
                        }
                        break;
                }

                if (img != null)
                {
                    //////e.Graphics.DrawImage(img, loc);
                    img.Dispose();
                }
                return eHasMorePages;
        }


        private void PrintPage(object o, PrintPageEventArgs e)
        {            
            if (e.Cancel == false)
            {
                System.Drawing.Image img = null;
                
                System.Drawing.Point loc = new System.Drawing.Point(0, 0);

                string currentPath = "";                

                pageIndex++;

                switch (rangeToPrint)
                {
                    case PrintRange.AllPages:
                    case PrintRange.SomePages:
                        {
                            //print Range of pages
                            if ((pageIndex >= rangeFrom) && (pageIndex <= rangeTill))
                            {
                                currentPath = GetImageToPrintPath();

                                if (currentPath.Length > 0)
                                {
                                    //pull slip                                
                                    img = System.Drawing.Image.FromFile(currentPath);
                                    if (pageIndex < rangeTill)
                                    {
                                        e.HasMorePages = true;
                                    }
                                    else
                                    {
                                        e.HasMorePages = false;
                                    }
                                }
                                else
                                {                                   
                                    e.HasMorePages = false;                                   
                                }
                            }
                            else
                            {
                                e.HasMorePages = false;
                            }
                        }
                        break;

                    case PrintRange.Selection:
                        {
                            //print specific pages  (1,2,8  or 2-6)
/*
                            if ((pageIndex >= minImageCount) && (pageIndex <= maxImageCount))
                            {
                                string selectedPages = "1,2,8";  //just for testing currently I do not know how to gather the string value with psecified pages from dialog control
                                if (PageIsGoodToPrint(pageIndex, selectedPages))
                                {
                                    if (pageIndex == 1)
                                    {
                                        //pull slip                                
                                        img = System.Drawing.Image.FromFile(currentArticleToPrint.Pullslip.FilePath.FullName);
                                    }
                                    else
                                    {
                                        if (((pageIndex - 2) < currentArticleToPrint.Scans.Count) && ((pageIndex - 2) >= 0))
                                        {
                                            img = System.Drawing.Image.FromFile(currentArticleToPrint.Scans[(pageIndex - 2)].FilePath.FullName);
                                        }
                                    }
                                }

                                e.HasMorePages = true;
                            }
                            else
                            {
                                e.HasMorePages = false;
                            }
*/
                            e.HasMorePages = false;
                        }
                        break;

                    case PrintRange.CurrentPage:
                        {
                            //print current page
                            if (printingPages)
                            {
                                img = System.Drawing.Image.FromFile(currentPageToPrint.FilePath.FullName);
                            }
                            else
                            {
                                img = System.Drawing.Image.FromFile(currentImageToPrint.FilePath.FullName);
                            }
                            e.HasMorePages = false;
                        }
                        break;

                    default:
                        {
                            e.HasMorePages = false;
                        }
                        break;
                }

                if (img != null)
                {
                    e.Graphics.DrawImage(img, loc);
                    img.Dispose();
                }

            }
            else
            {
                e.HasMorePages = false;
            }
        }
        #endregion
    }
}
