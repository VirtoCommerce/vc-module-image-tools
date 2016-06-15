using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    public class ImageDimensions
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Rectangle Rectangle
        {
            get { return new Rectangle(X, Y, Width, Height); }
        }

        public Size Size
        {
            get { return new Size { Width = this.Width, Height = this.Height }; }
        }
    }

    /// <summary>
    /// Image resize library
    /// </summary>
    public class ImageResizer: IImageResizer
	{
        public enum Dimensions 
		{
			Width,
			Height
		}
        public enum AnchorPosition
		{
			Top,
			Center,
			Bottom,
			Left,
			Right
		}

        /// <summary>
        /// Scale image by given percent
        /// </summary>
        public Image ScaleByPercent(Image image, int Percent)
		{
			float nPercent = ((float)Percent/100);

            var source = new ImageDimensions { Width = image.Width, Height = image.Height };
            var destination = new ImageDimensions { Width = (int)(source.Width * nPercent), Height = (int)(source.Height * nPercent)};

            return Transform(image, source, destination, destination.Size, null);
		}

        public Image ConstrainProportions(Image image, int size, Dimensions dimension)
		{
            var source = new ImageDimensions { Width = image.Width, Height = image.Height };
            var destination = new ImageDimensions();

			float nPercent = 0;

			switch(dimension)
			{
				case Dimensions.Width:
					nPercent = ((float)size/(float)source.Width);
					break;
				default:
					nPercent = ((float)size/(float)source.Height);
					break;
			}
				
			destination.Width = (int)(source.Width * nPercent);
			destination.Height = (int)(source.Height * nPercent);

            return Transform(image, source, destination, destination.Size, null);

		}

        /// <summary>
        /// Resize image.
        /// Original image will be resized proportionally to fit given Width, Height .
        /// Original image will not be cropped.
        /// If the original image has an aspect ratio different from thumbnail then thumbnail will contain empty spaces (top and bottom or left and right). 
        /// The empty spaces will be filled with given color.
        /// </summary>
        public Image FixedSize(Image image, int width, int height, Color color)
		{
            var source = new ImageDimensions { Width = image.Width, Height = image.Height };
            var destination = new ImageDimensions();

			float nPercent = 0;
			float nPercentW = ((float)width / (float)source.Width);
			float nPercentH = ((float)height / (float)source.Height);

			//if we have to pad the height pad both the top and the bottom
			//with the difference between the scaled height and the desired height
			if(nPercentH < nPercentW)
			{
				nPercent = nPercentH;
                destination.X = (int)((width - (source.Width * nPercent))/2);
			}
			else
			{
				nPercent = nPercentW;
                destination.Y = (int)((height - (source.Height * nPercent))/2);
			}
		
			destination.Width = (int)(source.Width * nPercent);
			destination.Height = (int)(source.Height * nPercent);

			return Transform(image, source, destination, new Size { Height = height, Width = width }, color); 
		}


        /// <summary>
        /// Crop image
        /// </summary>
        public Image Crop(Image image, int width, int height, AnchorPosition anchor)
		{
            var source = new ImageDimensions { Width = image.Width, Height = image.Height };
            var destination = new ImageDimensions();


			float nPercent = 0;
			float nPercentW = 0;
			float nPercentH = 0;

			nPercentW = ((float)width/(float)source.Width);
			nPercentH = ((float)height/(float)source.Height);

			if(nPercentH < nPercentW)
			{
				nPercent = nPercentW;
				switch(anchor)
				{
					case AnchorPosition.Top:
                        destination.Y = 0;
						break;
					case AnchorPosition.Bottom:
                        destination.Y = (int)(height - (source.Height * nPercent));
						break;
					default:
                        destination.Y = (int)((height - (source.Height * nPercent))/2);
						break;
				}				
			}
			else
			{
				nPercent = nPercentH;
				switch(anchor)
				{
					case AnchorPosition.Left:
                        destination.X = 0;
						break;
					case AnchorPosition.Right:
                        destination.X = (int)(width - (source.Width * nPercent));
						break;
					default:
                        destination.X = (int)((width - (source.Width * nPercent))/2);
						break;
				}			
			}

            destination.Width = (int)(source.Width * nPercent);
            destination.Height = (int)(source.Height * nPercent);

            return Transform(image, source, destination, new Size { Height = height, Width = width }, null);
		}

        private Image Transform(Image original, ImageDimensions source, ImageDimensions destination, Size canvasSize, Color? backgroundColor)
        {
            Bitmap bitmap = new Bitmap(canvasSize.Width, canvasSize.Height, PixelFormat.Format24bppRgb);
            bitmap.SetResolution(original.HorizontalResolution, original.VerticalResolution);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                if (backgroundColor != null)
                {
                    graphics.Clear(backgroundColor.Value);
                }
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(original, destination.Rectangle, source.Rectangle, GraphicsUnit.Pixel);
            }
            return bitmap;
        }


    }

}
