using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AlpineBitsTestClient
{
    class AlpineBitsResponse
    {
        public string ResponseBody { get; set; }
        public string ResponseHeaders { get; set; }
        public string StatusCode { get; set; }
        public string Encoding { get; set; }
    }

    public class AlpineBitsServer
    {
        public string ServerURL = "";
        public string UserName = "";
        public string Password = "";
        public string X_AlpineBits_ClientID = "";
        public string X_AlpineBits_ProtocolVersion = "b";
        public bool InvokeZipped = false;
        public bool AcceptResponseGZIPEncoded = true;
        public string HotelCode = "";
        public string HotelName = "";
    }

    class AlpineBitsRequest
    {
        /// <summary>WriteMsgInForm
        /// 
        /// </summary>
        /// <param name="sMsg"></param>
        /// <param name="sAddMsg"></param>
        /// <param name="writeNewfile"></param>
        /// <param name="iStatus">0=SUCCESS, 1=WARNING, 2=ERROR</param>
        public static void LogXMLFile(string XML, string filename = "")
        {

            //*** Eintrag in LOG-Datei schreiben
            try
            {
                DateTime oDta = new DateTime(System.DateTime.Now.Ticks);

                string LogPath = AppDomain.CurrentDomain.BaseDirectory + @"\Log\" + DateTime.Now.Year.ToString() + @"\" + DateTime.Now.Month.ToString() + @"\" + DateTime.Now.Day.ToString() + @"\";
                if (Directory.Exists(LogPath) == false)
                    Directory.CreateDirectory(LogPath);

                string sLogFile = LogPath+ filename.Replace(':','_') +".xml";
                // Write the dedicated LogFile in a dedicated direcotry
                File.WriteAllText(sLogFile, XML);

            }
            catch (Exception k)
            {
                System.Diagnostics.Debug.WriteLine(k.Message.ToString());
            }
        }
        public static AlpineBitsResponse ProcessRequest(AlpineBitsServer CallServer, string CallAction, string CallParam = "")
        {
            var StatusCode = "";
            var Response = new AlpineBitsResponse();
            if (CallParam.Length > 0)
            {
                CallParam = CallParam.Replace("{HOTELCODE}", CallServer.HotelCode);
                CallParam = CallParam.Replace("{HOTELNAME}", "Hotel AlpineBits");
                CallParam = CallParam.Replace("{YEAR}", DateTime.Now.Year.ToString());
            }
            // OTA_INVENTORY
            var content = new MultipartFormDataContent(Guid.NewGuid().ToString());
            //Note: it's a best practice to use double-quotes, even when the name doesn't contain spaces:
            content.Add(new StringContent(CallAction), "\"action\"");
            string RequestXML = CallParam;
            if (CallAction != "getVersion" && CallAction != "getCapabilities")
                content.Add(new StringContent(CallParam), "\"request\"");
            HttpResponseMessage result;

            // Client: acceptEncoding gzIP
            HttpClientHandler clHandler = new HttpClientHandler();
            if (CallServer.AcceptResponseGZIPEncoded)
                clHandler.AutomaticDecompression = DecompressionMethods.GZip;

            var headersRequest = "";
            var headersResponse = "";
            using (var client = new HttpClient(clHandler))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(string.Format("{0}:{1}", CallServer.UserName, CallServer.Password))));
                client.DefaultRequestHeaders.Add("X-AlpineBits-ClientProtocolVersion", CallServer.X_AlpineBits_ProtocolVersion);
                client.DefaultRequestHeaders.Add("X-AlpineBits-ClientID", CallServer.X_AlpineBits_ClientID);
                if (CallServer.InvokeZipped)
                    result = client.PostAsync(CallServer.ServerURL, new CompressedContent(content, "gzip")).Result;
                else
                {
                    //                    client.DefaultRequestHeaders.TransferEncoding.Add(new TransferCodingHeaderValue("chunked"));
                    //                    client.DefaultRequestHeaders.TransferEncoding.Add(new TransferCodingHeaderValue("gzip"));
                    result = client.PostAsync(CallServer.ServerURL, content).Result;
                }
                headersResponse = result.Headers.ToString()+result.Content.Headers.ToString();
                Response.Encoding = result.Content.Headers.ContentType.ToString();

                headersRequest = client.DefaultRequestHeaders.ToString();
                StatusCode = result.StatusCode.ToString("D") +" " + result.StatusCode.ToString(); 
            }
            Response.StatusCode = StatusCode;
            Response.ResponseHeaders = headersResponse;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                Response.ResponseBody = result.Content.ReadAsStringAsync().Result;
            }
            else
            {
                Response.ResponseBody = result.Content.ReadAsStringAsync().Result;
//                Response.ResponseBody = @"Request failed. Status code: " + result.StatusCode + " " + result.StatusCode.ToString("D") + ". Message Content: " + result.Content.ReadAsStringAsync().Result;
            }

            return Response;

        }
    }
}
