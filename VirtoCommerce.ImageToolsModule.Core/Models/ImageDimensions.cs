using System.Drawing;

namespace VirtoCommerce.ImageToolsModule.Core.Models
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
            get { return new Size { Width = Width, Height = Height }; }
        }
    }
}
