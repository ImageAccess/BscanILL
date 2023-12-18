using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ARSoft.Tools.Net.Dns;

namespace BscanILL.Misc
{
    class EmailValidator
    {
        public class SmtpDirect
        {
            /// <summary>
            /// Get / Set the name of the SMTP mail server
            /// </summary>
            private enum SMTPResponse : int
            {
                CONNECT_SUCCESS = 220,
                GENERIC_SUCCESS = 250,
                DATA_SUCCESS = 354,
                QUIT_SUCCESS = 221

            }
            public static bool CheckMailbox(string server, string to, string from)
            {
                IPHostEntry IPhst = Dns.GetHostEntry(server);
                IPEndPoint endPt = new IPEndPoint(IPhst.AddressList[0], 25);
                Socket s = new Socket(endPt.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                s.Connect(endPt);
                if (!Check_Response(s, SMTPResponse.CONNECT_SUCCESS))
                {
                    s.Close();
                    return false;
                }

                Senddata(s, string.Format("HELO {0}\r\n", Dns.GetHostName()));
                if (!Check_Response(s, SMTPResponse.GENERIC_SUCCESS))
                {
                    s.Close();
                    return false;
                }

                Senddata(s, string.Format("MAIL From: {0}\r\n", from));
                if (!Check_Response(s, SMTPResponse.GENERIC_SUCCESS))
                {
                    s.Close();
                    return false;
                }

                Senddata(s, string.Format("RCPT TO: {0}\r\n", to));
                if (!Check_Response(s, SMTPResponse.GENERIC_SUCCESS))
                {
                    s.Close();
                    return false;
                }

                Senddata(s, "QUIT\r\n");
                Check_Response(s, SMTPResponse.QUIT_SUCCESS);
                s.Close();
                return true;
            }
            private static void Senddata(Socket s, string msg)
            {

                byte[] _msg = Encoding.ASCII.GetBytes(msg);
                s.Send(_msg, 0, _msg.Length, SocketFlags.None);
            }

            public static string LastResponse = "";
            private static bool Check_Response(Socket s, SMTPResponse response_expected)
            {
                string sResponse;
                int response;
                byte[] bytes = new byte[1024];
                while (s.Available == 0)
                {
                    System.Threading.Thread.Sleep(100);
                }
                s.Receive(bytes, 0, s.Available, SocketFlags.None);
                sResponse = Encoding.ASCII.GetString(bytes);
                sResponse = sResponse.TrimEnd(new[] { '\r', '\n', '\0' });
                response = Convert.ToInt32(sResponse.Substring(0, 3));
                LastResponse = sResponse;
                if (response != (int)response_expected)
                {
                    return false;
                }
                return true;
            }
        }



        // PUBLIC METHODS
        #region public methods

        #region ValidateEmail()
        public static bool ValidateEmail(string address)
        {
            bool result = false;
            string firstServer = "";
            try
            {
                // Step 1 - check if the email address look valid
                string regXExpression = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";
                if (Regex.IsMatch(address, regXExpression, RegexOptions.IgnoreCase))
                {
                    //Step 2 – Now we know the email address is a valid format, check the domain for a server to talk to...
                    string[] parts = address.Split('@');
                    if (parts.Length == 2)
                    {
                        DnsStubResolver resolver = new DnsStubResolver();
                        List<MxRecord> records = resolver.Resolve<MxRecord>(parts[1], RecordType.Mx);
                        if (records.Count > 0)
                        {
                            MxRecord first = records.First<MxRecord>();
                            firstServer = first.ExchangeDomainName.ToString();
                            //Step 3 – Now we know the mail server address so let us connect to it. You can connect to one of the exchanger addresses in the response from Step 2.
                            // Here is a list of sub steps...
                            //COMMAND:  telnet mx2.sub3.homie.mail.dreamhost.com 25
                            //RESPONSE: Connected to mx2.sub3.homie.mail.dreamhost.com. Escape character is ‘^]’. 220 homiemail-mx7.g.dreamhost.com ESMTP
                            //COMMAND:  helo hi
                            //RESPONSE: 250 homiemail-mx8.g.dreamhost.com
                            //COMMAND:  mail from: <postmaster@kicservices.com>
                            //RESPONSE: 250 2.1.0 Ok
                            //COMMAND:  rcpt to: <Address>
                            //RESPONSE: 550 5.1.1 <Address>: Recipient address rejected: User unknown in virtual alias table (or something other than an OK 250 response)
                            //COMMAND:  quit
                            //RESPONSE: 221 2.0.0 Bye

                            result = SmtpDirect.CheckMailbox(firstServer, address, "Postmaster@KICServices.com");
                        }

                        //note: if incorrect server name (wrong email address part after @ sign) then records.Count = 0
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                string msg = ex.Message;
                //in case no internet connection -> exception message equals "DNS request failed"
            }
            return result;
        }
        #endregion

        #region IsAddressValid()
        public static bool IsAddressValid(string address, BscanILL.Export.ExportType deliveryMethod, SETTINGS.Settings.ExportClass exportSettings, bool confirmEmailFtpServer, bool confirmEmailFtpDir)
        {
            bool addressValid = true;
            
            if(    (deliveryMethod == BscanILL.Export.ExportType.Email)
                || (deliveryMethod == BscanILL.Export.ExportType.ArticleExchange)
                || ((deliveryMethod == BscanILL.Export.ExportType.FtpDir) && confirmEmailFtpDir)                  
                || ((deliveryMethod == BscanILL.Export.ExportType.Ftp) &&  confirmEmailFtpServer)    )  
            {            
                            if ((address.Length > 0) && (string.Compare(address, "N/A") != 0))                
              {
                if (exportSettings.Email.EmailValidation)
                {                  
                  if (exportSettings.Email.EmailDeliveryType != SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.SMTP)        //merging Validation and delivery type control in settings GUI                   
                  {
                    if (!BscanILL.Misc.EmailValidatorHttp.ValidateEmailHttp(address))        //Http validation -> where we send the email address to our web server where the validation gets perform to overcome firewall issues with SMTP blocking at customer's network
                    {                       
                       if (exportSettings.Email.EmailDeliveryType == SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.Both)
                       {
                          //try also SMTP validation
                          if (!BscanILL.Misc.EmailValidator.ValidateEmail(address))
                          {
                             // both validation failed -> clear the OCR'ed value
                             //result.Address = "";

                             addressValid = false;
                          }
                       }
                       else
                       {
                          //validation failed -> clear the OCR'ed value
                          //result.Address = "";
                       
                          addressValid = false;
                       }
                    }
                  }
                  else
                  if (!BscanILL.Misc.EmailValidator.ValidateEmail(address))       //SMTP validation -> might be failing at customers because of the firewall
                  {                    
                    addressValid = false;
                  }
                }
              }

              if ((deliveryMethod == BscanILL.Export.ExportType.Email)
                      || (deliveryMethod == BscanILL.Export.ExportType.ArticleExchange)
                      || (deliveryMethod == BscanILL.Export.ExportType.Ariel)
//                      || ((deliveryMethod == BscanILL.Export.ExportType.FtpDir) && exportSettings.FtpDirectory.SendConfirmationEmail)                  
//                      || ((deliveryMethod == BscanILL.Export.ExportType.Ftp) &&  exportSettings.FtpServer.SendConfirmationEmail)    )
                      || ((deliveryMethod == BscanILL.Export.ExportType.FtpDir) && confirmEmailFtpDir)                  
                      || ((deliveryMethod == BscanILL.Export.ExportType.Ftp) &&  confirmEmailFtpServer)    )                
              {
                    if ((address.Length == 0) || (string.Compare(address, "N/A") == 0))
                    {
                        addressValid = false;  //set flag when email address empty when email, AE, Ariel of FtpDir delivery when address is mandatory field
                    }
              }
            }

            return addressValid;
        }
        #endregion

        #endregion
    }
}
