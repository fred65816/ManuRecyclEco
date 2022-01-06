using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ManuRecyEco.Models
{
    public class ImageCopy
    {
        public int Id { get; set; }

        [Required, Column(TypeName = "BLOB")]
        public Byte[] Data { get; set; }

        [Required, Column(TypeName = "varchar(5)")]
        public string Extension { get; set; }

        [NotMapped]
        public bool IsUsed { get; private set; }

        public ImageCopy()
        {
            IsUsed = false;
        }

        public void ImageToBlob(string filePath, string extension)
        {
            Extension = extension;

            Uri uri = new Uri(filePath, UriKind.RelativeOrAbsolute);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = uri;
            bitmapImage.DecodePixelHeight = 225;
            bitmapImage.EndInit();

            BitmapEncoder encoder;

            if (extension == ".jpg")
            {
                encoder = new JpegBitmapEncoder();
            } 
            else
            {
                encoder = new PngBitmapEncoder();            
            }

            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                Data = ms.ToArray();
            }

            IsUsed = true;
        }

        public ImageSource BlobToImage()
        {
            BitmapImage bitmapImage = new BitmapImage();
            MemoryStream ms = new MemoryStream(Data);
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();

            ImageSource imgsrc = bitmapImage as ImageSource;

            return imgsrc;
        }
    }
}
