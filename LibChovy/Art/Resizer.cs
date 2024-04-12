using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibChovy.Art
{
    public class Resizer
    {
        public static async Task<byte[]> LoadImage(string imagePath, int width=80, int height=80)
        {
            return await LoadImage(await File.ReadAllBytesAsync(imagePath), width, height);
        }
        public static async Task<byte[]> LoadImage(byte[] imageData, int width=80, int height=80)
        {
            using (Image img = Image.Load(imageData))
            {
                img.Mutate(x => x.Resize(width, height));
                using (MemoryStream png = new MemoryStream())
                {
                    await img.SaveAsPngAsync(png);
                    return png.ToArray();
                }
            }
        }
    }
}
