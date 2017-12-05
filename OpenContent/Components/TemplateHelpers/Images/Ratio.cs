using System;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public class Ratio
    {
        private readonly float _ratio;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public float AsFloat => (float)Width / (float)Height;

        public Ratio(string ratioString)
        {
            Width = 1;
            Height = 1;
            var elements = ratioString.ToLowerInvariant().Split('x');
            if (elements.Length == 2)
            {
                if (int.TryParse(elements[0], out var leftPart) && int.TryParse(elements[1], out var rightPart))
                {
                    Width = leftPart;
                    Height = rightPart;
                }
            }
            _ratio = AsFloat;
        }
        public Ratio(int width, int height)
        {
            if (width < 1) throw new ArgumentOutOfRangeException("width", width, "should be 1 or larger");
            if (height < 1) throw new ArgumentOutOfRangeException("height", height, "should be 1 or larger");
            Width = width;
            Height = height;
            _ratio = AsFloat;
        }

        public void SetWidth(int newWidth)
        {
            Width = newWidth;
            Height = Convert.ToInt32(newWidth / _ratio);
        }
        public void SetHeight(int newHeight)
        {
            Width = Convert.ToInt32(newHeight * _ratio);
            Height = newHeight;
        }
        
        public bool IsSquare()
        {
            if (Width <= 0 || Height <= 0) return false;

            var ratio = Math.Round((decimal)Height / (decimal)Width, 1);
            return Math.Abs(1 - ratio) <= (decimal)0.1;
        }

        public bool IsPortrait()
        {
            if (Width <= 0 || Height <= 0) return false;
            if (IsSquare()) return false;

            return Height > Width;
        }

        public bool IsLandScape()
        {
            if (Width <= 0 || Height <= 0) return false;
            if (IsSquare()) return false;

            return Height < Width;
        }

        public void Rotate()
        {
            var h = Height;
            Height = Width;
            Width = h;
        }
    }
}