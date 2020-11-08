using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Net;



    public class ImageResizer
    {
        private int allowedFileSizeInByte;
        private string sourcePath;
        private string destinationPath;

        //String sourcefilepath = “@absolutepath”; // e.g. “d:/test.docx”
        String ftpurl; 
        String ftpusername; 
        String ftppassword; 
        public ImageResizer(int allowedSize, string sourcePath, string destinationPath, string ftpurl, string ftpusername, string ftppassword)
        {
            allowedFileSizeInByte = allowedSize;
            this.sourcePath = sourcePath;
            this.destinationPath = destinationPath;
            this.ftpurl = ftpurl;
            this.ftpusername = ftpusername;
            this.ftppassword = ftppassword;
        }

        public void ScaleImage()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (FileStream fs = new FileStream(sourcePath, FileMode.Open))
                {
                    Bitmap bmp = (Bitmap)Image.FromStream(fs);
                    SaveTemporary(bmp, ms, 100);

                    while (ms.Length < 0.9 * allowedFileSizeInByte || ms.Length > allowedFileSizeInByte)
                    {
                        double scale = Math.Sqrt((double)allowedFileSizeInByte / (double)ms.Length);
                        ms.SetLength(0);
                        bmp = ScaleImage(bmp, scale);
                        SaveTemporary(bmp, ms, 100);
                    }

                    if (bmp != null)
                    {
                        bmp.Dispose();
                    }
                    SaveImageToFile(ms);
                    fs.Close();
                }
                ms.Close();
                
            }
        }

        private void SaveImageToFile(MemoryStream ms)
        {
            byte[] data = ms.ToArray();


            using (FileStream fs = new FileStream(destinationPath, FileMode.Create))
            {
                //fs.Write(data, 0, data.Length);

                string ftpfullpath = ftpurl;
                FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(ftpfullpath);
                ftp.Credentials = new NetworkCredential(ftpusername, ftppassword);

                ftp.KeepAlive = true;
                ftp.UseBinary = true;
                ftp.Method = WebRequestMethods.Ftp.UploadFile;

                //FileStream fs2 = File.OpenRead(destinationPath);
                //byte[] buffer = new byte[fs2.Length];
                //fs2.Read(buffer, 0, buffer.Length);
                //fs2.Close();

                Stream ftpstream = ftp.GetRequestStream();
                ftpstream.Write(data, 0, data.Length);
                ftpstream.Close();
                fs.Close();

            }
            ms.Close();

        }
        private void SaveTemporary(Bitmap bmp, MemoryStream ms, int quality)
        {
            EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            var codec = GetImageCodecInfo();
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            if (codec != null)
                bmp.Save(ms, codec, encoderParams);
            else
                bmp.Save(ms, GetImageFormat());
        }

        public Bitmap ScaleImage(Bitmap image, double scale)
        {
            int newWidth = (int)(image.Width * scale);
            int newHeight = (int)(image.Height * scale);

            Bitmap result = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                g.DrawImage(image, 0, 0, result.Width, result.Height);
            }
            return result;
        }

        private ImageCodecInfo GetImageCodecInfo()
        {
            FileInfo fi = new FileInfo(sourcePath);

            switch (fi.Extension)
            {
                case ".bmp": return ImageCodecInfo.GetImageEncoders()[0];
                case ".jpg":
                case ".jpeg": return ImageCodecInfo.GetImageEncoders()[1];
                case ".gif": return ImageCodecInfo.GetImageEncoders()[2];
                case ".tiff": return ImageCodecInfo.GetImageEncoders()[3];
                case ".png": return ImageCodecInfo.GetImageEncoders()[4];
                default: return null;
            }
        }

        private ImageFormat GetImageFormat()
        {
            FileInfo fi = new FileInfo(sourcePath);

            switch (fi.Extension)
            {
                case ".jpg": return ImageFormat.Jpeg;
                case ".bmp": return ImageFormat.Bmp;
                case ".gif": return ImageFormat.Gif;
                case ".png": return ImageFormat.Png;
                case ".tiff": return ImageFormat.Tiff;
                default: return ImageFormat.Png;
            }
        }
    }

