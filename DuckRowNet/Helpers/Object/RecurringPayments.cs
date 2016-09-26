using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using com.paypal.sdk.services;
using com.paypal.sdk.profiles;
using com.paypal.sdk.util;
using System.Web.Configuration;
using System.Net;
using System.IO;
using System.Text;

namespace DuckRowNet.Helpers.Object
{
    

    public class RecurringPayments
    {
        private string APIUsername;
        private string APIPassword;
        private string APISignature;

        private const string SIGNATURE = "SIGNATURE";
        private const string PWD = "PWD";
        private const string ACCT = "ACCT";

        private string Subject = "";
        private string BNCode = "PP-ECWizard";

        private string pendpointurl = "https://api-3t.paypal.com/nvp";
        private const string CVV2 = "CVV2";

        //HttpWebRequest Timeout specified in milliseconds
        private const int Timeout = 50000;
        private static readonly string[] SECURED_NVPS = new string[] { ACCT, CVV2, SIGNATURE, PWD };

        public RecurringPayments()
        {
            this.APIUsername = PayPal.Profile.ApiUsername;
            this.APIPassword = PayPal.Profile.ApiPassword;
            this.APISignature = PayPal.Profile.ApiSignature;
        }

        public bool SetExpressCheckout(string companyName, string amt, ref string token, ref string retMsg, string returnURL, string cancelURL)
        {
            string host = "www.paypal.com";
            string desc = "Subscription for " + companyName + " on Qwerty Time.";

            var hostUrl = HttpContext.Current.Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
            returnURL = hostUrl + returnURL;
            cancelURL = hostUrl + cancelURL;
            if (PayPal.Profile.Environment == "sandbox")
            {
                pendpointurl = "https://api-3t.sandbox.paypal.com/nvp";
                host = "www.sandbox.paypal.com";
            }

            NVPCodec encoder = new NVPCodec();
            encoder["VERSION"] = "51.0";
            encoder["METHOD"] = "SetExpressCheckout";
            encoder["RETURNURL"] = returnURL;
            encoder["CANCELURL"] = cancelURL;
            encoder["AMT"] = amt;
            //encoder["PAYMENTACTION"] = "SALE";
            encoder["CURRENCYCODE"] = "EUR";
            encoder["NOSHIPPING"] = "1";
            encoder["L_BILLINGTYPE0"] = "RecurringPayments";
            encoder["L_BILLINGAGREEMENTDESCRIPTION0"] = desc;

            string pStrrequestforNvp = encoder.Encode();
            string pStresponsenvp = HttpCall(pStrrequestforNvp);

            NVPCodec decoder = new NVPCodec();
            decoder.Decode(pStresponsenvp);

            string strAck = decoder["ACK"].ToLower();
            if (strAck != null && (strAck == "success" || strAck == "successwithwarning"))
            {
                token = decoder["TOKEN"];
                string ECURL = "https://" + host + "/cgi-bin/webscr?cmd=_express-checkout" + "&token=" + token;
                retMsg = ECURL;
                return true;
            }
            else
            {
                retMsg = "ErrorCode=" + decoder["L_ERRORCODE0"] + "&" +
                    "Desc=" + decoder["L_SHORTMESSAGE0"] + "&" +
                    "Desc2=" + decoder["L_LONGMESSAGE0"];
                return false;
            }
        }

        /// This returns true
        public bool GetExpressCheckoutDetails(string token, ref string PayerId, ref string retMsg)
        {
            if (PayPal.Profile.Environment == "sandbox")
            {
                pendpointurl = "https://api-3t.sandbox.paypal.com/nvp";
            }
            NVPCodec encoder = new NVPCodec();
            encoder["METHOD"] = "GetExpressCheckoutDetails";
            encoder["TOKEN"] = token;

            string pStrrequestforNvp = encoder.Encode();
            string pStresponsenvp = HttpCall(pStrrequestforNvp);

            NVPCodec decoder = new NVPCodec();
            decoder.Decode(pStresponsenvp);

            string strAck = decoder["ACK"].ToLower();
            if (strAck != null && (strAck == "success" || strAck == "successwithwarning"))
            {
                retMsg = pStresponsenvp;
                return true;
            }
            else
            {
                retMsg = "ErrorCode=" + decoder["L_ERRORCODE0"] + "&" +
                    "Desc=" + decoder["L_SHORTMESSAGE0"] + "&" +
                    "Desc2=" + decoder["L_LONGMESSAGE0"];
                return false;
            }
        }

        public bool CreateRecurringPaymentsProfileCode(string companyName, string token, string amount, string profileDate, string billingPeriod, string billingFrequency, ref string retMsg)
        {
            NVPCallerServices caller = new NVPCallerServices();
            IAPIProfile profile = ProfileFactory.createSignatureAPIProfile();
            profile.APIUsername = this.APIUsername;
            profile.APIPassword = this.APIPassword;
            profile.APISignature = this.APISignature;
            profile.Environment = "sandbox";
            caller.APIProfile = profile;
            //string host = "www.paypal.com";
            if (PayPal.Profile.Environment == "sandbox")
            {
                pendpointurl = "https://api-3t.sandbox.paypal.com/nvp";
                //host = "www.sandbox.paypal.com";
            }

            string desc = "Subscription for " + companyName + " on Qwerty Time.";

            NVPCodec encoder = new NVPCodec();
            encoder["VERSION"] = "51.0";

            // Add request-specific fields to the request.
            encoder["METHOD"] = "CreateRecurringPaymentsProfile";
            encoder["TOKEN"] = token;
            encoder["AMT"] = amount;
            encoder["CURRENCYCODE"] = "EUR";
            encoder["PROFILESTARTDATE"] = profileDate; //Date format from server expects Ex: 2006-9-6T0:0:0
            encoder["BILLINGPERIOD"] = billingPeriod;
            encoder["BILLINGFREQUENCY"] = billingFrequency;
            encoder["L_BILLINGTYPE0"] = "RecurringPayments";
            encoder["DESC"] = desc;
            encoder["L_BILLINGAGREEMENTDESCRIPTION0"] = desc; //"Subscription for Qwerty Time";

            // Execute the API operation and obtain the response.
            string pStrrequestforNvp = encoder.Encode();
            string pStresponsenvp = caller.Call(pStrrequestforNvp);

            NVPCodec decoder = new NVPCodec();
            decoder.Decode(pStresponsenvp);
            //return decoder["ACK"];
            string strAck = decoder["ACK"];
            bool success = false;
            if (strAck != null && (strAck == "Success" || strAck == "SuccessWithWarning"))
            {
                retMsg = pStresponsenvp;
                success = true; // check decoder["result"]
            }
            else
            {
                success = false;
            }

            //StringBuilder buffer = new StringBuilder();
            //for (int i = 0; i < decoder.Keys.Count; i++)
            //{
            //    buffer.AppendFormat("{0}: {1}", decoder.Keys[i], decoder.GetValues(i).Aggregate((vals, val) => vals + "----" + val));
            //}
            //retMsg = buffer.ToString();

            return success;// returns false
        }

        public bool GetRecurringPaymentDetails(string profileID, ref string retMsg)
        {
            if (PayPal.Profile.Environment == "sandbox")
            {
                pendpointurl = "https://api-3t.sandbox.paypal.com/nvp";
            }
            NVPCodec encoder = new NVPCodec();
            encoder["METHOD"] = "GetRecurringPaymentsProfileDetails";
            encoder["PROFILEID"] = profileID;

            string pStrrequestforNvp = encoder.Encode();
            string pStresponsenvp = HttpCall(pStrrequestforNvp);

            NVPCodec decoder = new NVPCodec();
            decoder.Decode(pStresponsenvp);

            string strAck = decoder["ACK"].ToLower();
            if (strAck != null && (strAck == "success" || strAck == "successwithwarning"))
            {
                retMsg = pStresponsenvp;
                return true;
            }
            else
            {
                retMsg = "ErrorCode=" + decoder["L_ERRORCODE0"] + "&" +
                    "Desc=" + decoder["L_SHORTMESSAGE0"] + "&" +
                    "Desc2=" + decoder["L_LONGMESSAGE0"];
                return false;
            }
        }

        public bool CheckRecurringPayment(CompanyDetails companyDetails)
        {
            Subscription sub = new Subscription(companyDetails);
            if (!String.IsNullOrEmpty(sub.ProfileID))
            {
                if (PayPal.Profile.Environment == "sandbox")
                {
                    pendpointurl = "https://api-3t.sandbox.paypal.com/nvp";
                }
                NVPCodec encoder = new NVPCodec();
                encoder["METHOD"] = "GetRecurringPaymentsProfileDetails";
                encoder["PROFILEID"] = sub.ProfileID;

                string pStrrequestforNvp = encoder.Encode();
                string pStresponsenvp = HttpCall(pStrrequestforNvp);

                NVPCodec decoder = new NVPCodec();
                decoder.Decode(pStresponsenvp);

                string strAck = decoder["ACK"].ToLower();
                if (strAck != null && (strAck == "success" || strAck == "successwithwarning"))
                {
                    //retMsg = pStresponsenvp;
                    if (decoder["STATUS"] == "Active")
                    {
                        return true;
                    }
                }
                else
                {
                    //retMsg = "ErrorCode=" + decoder["L_ERRORCODE0"] + "&" +
                    //    "Desc=" + decoder["L_SHORTMESSAGE0"] + "&" +
                    //    "Desc2=" + decoder["L_LONGMESSAGE0"];
                    return false;
                }
            }
            else
            {
                return false;
            }


            return true;
        }

        public string GetRecurringPaymentStatus(Subscription sub)
        {
            if (!String.IsNullOrEmpty(sub.ProfileID))
            {
                if (PayPal.Profile.Environment == "sandbox")
                {
                    pendpointurl = "https://api-3t.sandbox.paypal.com/nvp";
                }
                NVPCodec encoder = new NVPCodec();
                encoder["METHOD"] = "GetRecurringPaymentsProfileDetails";
                encoder["PROFILEID"] = sub.ProfileID;

                string pStrrequestforNvp = encoder.Encode();
                string pStresponsenvp = HttpCall(pStrrequestforNvp);

                NVPCodec decoder = new NVPCodec();
                decoder.Decode(pStresponsenvp);

                string strAck = decoder["ACK"].ToLower();
                if (strAck != null && (strAck == "success" || strAck == "successwithwarning"))
                {
                    //retMsg = pStresponsenvp;
                    return decoder["STATUS"];
                }
                else
                {
                    //retMsg = "ErrorCode=" + decoder["L_ERRORCODE0"] + "&" +
                    //    "Desc=" + decoder["L_SHORTMESSAGE0"] + "&" +
                    //    "Desc2=" + decoder["L_LONGMESSAGE0"];
                    return "fail";
                }
            }
            return "fail";
        }

        public string HttpCall(string NvpRequest) //CallNvpServer 
        {
            string url = pendpointurl;
            string result = "ACK=fail";

            //To Add the credentials from the profile
            string strPost = NvpRequest + "&" + buildCredentialsNVPString();
            strPost = strPost + "&BUTTONSOURCE=" + HttpUtility.UrlEncode(BNCode);

            try
            {
                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                objRequest.Timeout = 50000;
                objRequest.Method = "POST";
                objRequest.ContentLength = strPost.Length;


                using (StreamWriter myWriter = new StreamWriter(objRequest.GetRequestStream()))
                {
                    myWriter.Write(strPost);
                }

                //Retrieve the Response returned from the NVP API call to PayPal
                HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();

                using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                //Logger.LogWarning("HttpCall: " + ex.Source, url, NvpRequest, "");

                /*
                if (log.IsFatalEnabled)
                {
                log.Fatal(e.Message, this);
                }*/
            }

            return result;
        }

        public bool CancelRecurringPayment(Subscription sub)
        {
            if (PayPal.Profile.Environment == "sandbox")
            {
                pendpointurl = "https://api-3t.sandbox.paypal.com/nvp";
            }
            NVPCodec encoder = new NVPCodec();
            encoder["METHOD"] = "ManageRecurringPaymentsProfileStatus";
            encoder["PROFILEID"] = sub.ProfileID;
            encoder["ACTION"] = "Cancel";
            encoder["NOTE"] = "Cancelled by user";

            string pStrrequestforNvp = encoder.Encode();
            string pStresponsenvp = HttpCall(pStrrequestforNvp);

            NVPCodec decoder = new NVPCodec();
            decoder.Decode(pStresponsenvp);

            string strAck = decoder["ACK"].ToLower();
            if (strAck != null && (strAck == "success" || strAck == "successwithwarning"))
            {
                return true;
            }
            else
            {
                string warning = "ErrorCode=" + decoder["L_ERRORCODE0"] + "&" +
                    "Desc=" + decoder["L_SHORTMESSAGE0"] + "&" +
                    "Desc2=" + decoder["L_LONGMESSAGE0"];
                //Logger.LogWarning("Recurring Payments Error: ", "", "", warning);
                return false;
            }


        }
        private string buildCredentialsNVPString()
        {
            NVPCodec codec = new NVPCodec();

            if (!String.IsNullOrEmpty(APIUsername))
                codec["USER"] = APIUsername;

            if (!String.IsNullOrEmpty(APIPassword))
                codec[PWD] = APIPassword;

            if (!String.IsNullOrEmpty(APISignature))
                codec[SIGNATURE] = APISignature;

            if (!String.IsNullOrEmpty(Subject))
                codec["SUBJECT"] = Subject;

            codec["VERSION"] = "51.0";

            return codec.Encode();
        }
    }

}