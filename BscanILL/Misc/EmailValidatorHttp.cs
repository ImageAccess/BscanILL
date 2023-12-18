using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KssFolderAPIClientNS;

namespace BscanILL.Misc
{
    class EmailValidatorHttp
    {
        private const string LiveBasePath = @"https://q8x.us/Api/";

        #region ValidateEmailHttp()
        
        public static bool ValidateEmailHttp(string address)
        {            
            bool result = false;
             
            string returnValue = "";
            
            KssFolderAPIClient client = null;
             
            client = new KssFolderAPIClient (LiveBasePath);

            returnValue = "";
            returnValue = client.KicValidateEmailEx(address);
            if( returnValue == "Passed")
            {
                result = true;
            }
            else
            {
                result = false;
            }
              
            return result;             
        }
          
        #endregion
    }
}
