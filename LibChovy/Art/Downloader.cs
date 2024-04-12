using GameBuilder.Pops;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibChovy.Art
{
    public class Downloader
    {
        private const string coverApi = "https://raw.githubusercontent.com/xlenore/psx-covers/main/covers/";
        private static HttpClient httpClient = new HttpClient();

        public static async Task<byte[]> DownloadCover(PSInfo game)
        {
            string discIdDash = game.DiscId.Substring(0, 4) + "-" + game.DiscId.Substring(4, 5);

            return await DownloadCover(discIdDash);
        }

        public static async Task<byte[]> DownloadCover(string gameId)
        {
            try
            {
                byte[] data = await httpClient.GetByteArrayAsync(coverApi + gameId + ".jpg");
                using (Image coverImage = Image.Load(data))
                {
                    using (Image psnBorder = Image.Load(Resources.ICON0))
                    {

                        //coverImage.Mutate(x => x.Crop(new Rectangle(80, 0, coverImage.Width - 80, coverImage.Height)));
                        coverImage.Mutate(x => x.Resize(58, 58));
                        psnBorder.Mutate(x => x.DrawImage(coverImage, new Point(13, 11), 1.0f));

                        using (MemoryStream png = new MemoryStream())
                        {
                            await psnBorder.SaveAsPngAsync(png);
                            return png.ToArray();
                        }
                    }
                }

            }
            catch (Exception) { return Resources.ICON0; }
        }

    }
}
