using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using BscanILL.Export.AdditionalInfo;
using BscanILL.Hierarchy;

namespace BscanILL.Export
{
    public class ExportTempUnit : INotifyPropertyChanged
    {
        BscanILL.Hierarchy.Article article;
        IAdditionalInfo additionalInfo = null;
        //Export.ExportType exportType;
        BscanILLData.Models.Helpers.NewDbExport newDbExport;

        public event PropertyChangedEventHandler PropertyChanged ;

        #region constructor
        // public ExportTempUnit(Article article, BscanILLData.Models.Helpers.NewDbExport newDbExportTemp, Export.ExportType exportTypeTemp, IAdditionalInfo additionalInfoTemp)
        public ExportTempUnit(Article article, BscanILLData.Models.Helpers.NewDbExport newDbExportTemp, IAdditionalInfo additionalInfoTemp)
        {
            this.article = article;
            this.newDbExport = newDbExportTemp;
            //this.exportType = exportTypeTemp;
            this.additionalInfo = additionalInfoTemp;
        }
        #endregion

        // PUBLIC PROPERTIES
        #region public properties

        public Article Article { get { return this.article; } }
        public BscanILLData.Models.Helpers.NewDbExport NewDbExport { get { return this.newDbExport; } }
        public IAdditionalInfo AdditionalInfo { get { return this.additionalInfo; } }

        #endregion
        
        // PRIVATE METHODS
        #region private methods

        #region RaisePropertyChanged()>
        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                if (propertyName.StartsWith("get_") || propertyName.StartsWith("set_"))
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName.Substring(4)));
                else
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
  
        #endregion        
    }
}

