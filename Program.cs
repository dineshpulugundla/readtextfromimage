using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace ReadTextFromImage
{
    class Program
    {
        private const string Endpoint = "https://southcentralus.api.cognitive.microsoft.com/";

        static void Main(string[] args)
        {
            var image = "P_1.jpg";
            var client = new HttpClient { BaseAddress = new Uri(Endpoint) };
            ExtractText(client, image).Wait();
        }

       private static async Task ExtractText(HttpClient client, string image)
        {
            using (var imageStream = File.OpenRead(image))
            {
                var reqBody = new StreamContent(imageStream);
                reqBody.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "e0b2b71bc1ec404a8833583d0a75b3e3");

                var uri = "https://southcentralus.api.cognitive.microsoft.com/vision/v1.0/ocr" + "?language=unk&detectOrientation=true";

                byte[] imgArray = ConvertStreamToByteArray(imageStream);
                try
                {
                    using (ByteArrayContent content = new ByteArrayContent(imgArray))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        var temp = await client.PostAsync(uri, content);
                        var rspBody = await temp.Content.ReadAsStringAsync();
                        var rspJson = JToken.Parse(rspBody);
                        JObject obj = JObject.Parse(rspBody);
                        JToken sec = obj["regions"];
                        string numberPlate = string.Empty;
                        foreach (JToken token in sec)
                        {
                            JToken jToken = token["lines"].First["words"];
                            foreach (JToken item in jToken)
                            {
                                numberPlate += item["text"].ToString();
                            }
                            
                        }
                        Console.WriteLine(numberPlate);
                        
                    }
                }
                catch (Exception ex)
                {

                }

            }
        }
        private static byte[] ConvertStreamToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
