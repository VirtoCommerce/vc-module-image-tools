using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace VirtoCommerce.ImageTools.ImageAbstractions
{
    /// <summary>
    /// Image resize library
    /// </summary>
    public class DefaultImageResizer : IImageResizer
    {
        /// <summary>
        /// Scale image by given percent
        /// </summary>
        public virtual Image<Rgba32> ScaleByPercent(Image<Rgba32> image, int percent)
        {
            var nPercent = (float)percent / 100;
            var newWidth = (int)(image.Width * nPercent);
            var newHeight = (int)(image.Height * nPercent);

            var result = image.Clone(ctx =>
             {
                 ctx.Resize(newWidth, newHeight);
             });

            return result;
        }

        /// <summary>
        /// Resize image vertically with keeping it aspect rate.
        /// </summary>
        public virtual Image<Rgba32> FixedHeight(Image<Rgba32> image, int height, Rgba32 background)
        {
            var options = new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size { Height = height, Width = image.Width }
            };

            var result = image.Clone(ctx =>
            {
                ctx.Resize(options);
                ctx.BackgroundColor(background);
            });

            return result;
        }

        /// <summary>
        /// Resize image horizontally with keeping it aspect rate
        /// </summary>
        public virtual Image<Rgba32> FixedWidth(Image<Rgba32> image, int width, Rgba32 background)
        {
            var options = new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size { Height = image.Height, Width = width }
            };

            var result = image.Clone(ctx =>
            {
                ctx.Resize(options);
                ctx.BackgroundColor(background);
            });

            return result;
        }

        /// <summary>
        /// Resize image.
        /// Original image will be resized proportionally to fit given Width, Height.
        /// Original image will not be cropped.
        /// If the original image has an aspect ratio different from thumbnail then thumbnail will contain empty spaces (top and bottom or left and right). 
        /// The empty spaces will be filled with given color.
        /// </summary>
        public virtual Image<Rgba32> FixedSize(Image<Rgba32> image, int width, int height, Rgba32 background)
        {
            var options = new ResizeOptions
            {
                Mode = ResizeMode.Pad,
                Size = new Size { Height = height, Width = width }
            };

            var result = image.Clone(ctx =>
            {
                ctx.Resize(options);
                ctx.BackgroundColor(background);
            });

            return result;
        }

        /// <summary>
        /// Resize and trim excess.
        /// The image will have given size
        /// </summary>
        public virtual Image<Rgba32> Crop(Image<Rgba32> image, int width, int height, AnchorPosition anchor)
        {
            var options = new ResizeOptions
            {
                Mode = ResizeMode.Crop,
                Size = new Size { Height = height, Width = width },
                Position = GetAnchorPositionMode(anchor)
            };

            var result = image.Clone(ctx =>
            {
                ctx.Resize(options);
            });

            return result;
        }

        protected virtual Image<Rgba32> Transform(Image<Rgba32> original, Rectangle source, Rectangle destination, Size canvasSize, Rgba32? backgroundColor)
        {
            if (!backgroundColor.HasValue)
            {
                backgroundColor = Color.Transparent;
            }

            var result = new Image<Rgba32>(new Configuration(), canvasSize.Width, canvasSize.Height, backgroundColor.Value);
            result.Metadata.HorizontalResolution = original.Metadata.HorizontalResolution;
            result.Metadata.VerticalResolution = original.Metadata.VerticalResolution;

            var imgToDraw = original.Clone(ctx =>
            {
                ctx.Crop(source);
                ctx.Resize(destination.Size);
                ctx.BackgroundColor(backgroundColor.Value);
            });

            result.Mutate(ctx =>
            {
                ctx.DrawImage(imgToDraw, destination.Location, new GraphicsOptions() { Antialias = true });
            });

            return result;
        }

        private AnchorPositionMode GetAnchorPositionMode(AnchorPosition anchorPosition)
        {
            var ancorPositionMap = new Dictionary<AnchorPosition, AnchorPositionMode>
            {
                { AnchorPosition.TopLeft, AnchorPositionMode.TopLeft },
                { AnchorPosition.TopCenter, AnchorPositionMode.Top},
                { AnchorPosition.TopRight, AnchorPositionMode.TopRight},
                { AnchorPosition.CenterLeft, AnchorPositionMode.Left},
                { AnchorPosition.Center, AnchorPositionMode.Center},
                { AnchorPosition.CenterRight,AnchorPositionMode.Right},
                { AnchorPosition.BottomLeft, AnchorPositionMode.BottomLeft},
                { AnchorPosition.BottomCenter,AnchorPositionMode.Bottom },
                { AnchorPosition.BottomRight, AnchorPositionMode.BottomRight}
            };
            if (!ancorPositionMap.ContainsKey(anchorPosition))
            {
                throw new ArgumentOutOfRangeException($"AnchorPosition {anchorPosition} not supported.");
            }
            return ancorPositionMap[anchorPosition];
        }
    }
}
