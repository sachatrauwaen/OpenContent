using System;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public class Ratio
    {
        private readonly float _ratio;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public float AsFloat
        {
            get { return (float)Width / (float)Height; }
        }

        public Ratio(string ratioString)
        {
            Width = 1;
            Height = 1;
            var elements = ratioString.ToLowerInvariant().Split('x');
            if (elements.Length == 2)
            {
                int leftPart;
                int rightPart;
                if (int.TryParse(elements[0], out leftPart) && int.TryParse(elements[1], out rightPart))
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
    }
}