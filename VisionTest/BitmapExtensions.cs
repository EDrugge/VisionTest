using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace VisionTest
{
	public static class BitmapExtensions
	{
		public static void SaveJpg100(this Bitmap bmp, string filename)
		{            
			EncoderParameters encoderParameters = new EncoderParameters(1);
			encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
			bmp.Save(filename, GetEncoder(ImageFormat.Jpeg), encoderParameters);
		}

		public static void SaveJpg100(this Bitmap bmp, Stream stream)
		{
			EncoderParameters encoderParameters = new EncoderParameters(1);
			encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
			bmp.Save(stream, GetEncoder(ImageFormat.Jpeg), encoderParameters);
		}

		public static ImageCodecInfo GetEncoder(ImageFormat format)
		{
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

			foreach (ImageCodecInfo codec in codecs)
			{
				if (codec.FormatID == format.Guid)
				{
					return codec;
				}
			}

			return null;
		}
	}
}
