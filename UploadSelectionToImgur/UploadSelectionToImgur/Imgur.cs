using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace UploadSelectionToImgur
{
    class Imgur
    {
        string ClientId { get; set; }

        public Imgur(string clientId)
        {
            this.ClientId = clientId;
        }

        public async Task<string> Upload(Bitmap image)
        {
            var imgConverter = new ImageConverter();
            var imgBytes = (byte[])imgConverter.ConvertTo(image, typeof(byte[]));

            using (var client = new HttpClient())
            {
                var requestContent = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(imgBytes);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                requestContent.Add(imageContent, "image");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", this.ClientId);

                var response = await client.PostAsync("https://api.imgur.com/3/upload", requestContent);
                var responseText = await response.Content.ReadAsStringAsync();

                return responseText.Substring(15, 7);
            }
        }

        public void OpenImageInBrowser(string imageId)
        {
            Process.Start("http://imgur.com/" + imageId);
        }
    }
}
