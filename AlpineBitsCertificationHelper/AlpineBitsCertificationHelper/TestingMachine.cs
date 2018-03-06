using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AlpineBitsTestClient
{

    class JsonPost
    {
        public string version { get; set; }
        public string msg { get; set; }
    }

    class TestingMachine
    {
       public static async Task<string> Call(string Version, string filename)
       {
            JsonPost reqObject = new JsonPost();
            reqObject.version = Version;
            reqObject.msg = File.ReadAllText(filename);
            return await CallRestService("https://development.alpinebits.org/validator", JsonConvert.SerializeObject(reqObject));
            /*var Response = new AlpineBitsResponse();
            var content = new MultipartFormDataContent(Guid.NewGuid().ToString());
            //Note: it's a best practice to use double-quotes, even when the name doesn't contain spaces:
            content.Add(new StringContent("201507b"), "\"version\"");
            content.Add(new StringContent(File.ReadAllText(filename)),"\"msg\"");

           // content.Add(new StreamContent(new MemoryStream(File.ReadAllBytes(filename))), "\"msg\"", "upload.xml");
            HttpResponseMessage result;

            // Client: acceptEncoding gzIP
            HttpClientHandler clHandler = new HttpClientHandler();
            using (var client = new HttpClient(clHandler))
            {
                  result = client.PostAsync("https://development.alpinebits.org/validator", content).Result;
            }
            dynamic stuff = JsonConvert.DeserializeObject(result.Content.ReadAsStringAsync().Result);
            string Result = stuff.result;

            return Result;*/
        }

        private static async Task<dynamic> CallRestService(string uri, string jsonstring, string method="POST")
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = method;

            using (var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync()))
            {
                streamWriter.Write(jsonstring);
                streamWriter.Flush();
                streamWriter.Close();
            }
            string result="";
            var httpResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                 result = await streamReader.ReadToEndAsync();
            }

            return result;
        }

    }
}
