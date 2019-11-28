using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPGE
{
    public class SimpleTexture
    {
        public Microsoft.Xna.Framework.Color[] Data { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public SimpleTexture(int width, int height) : this(new Microsoft.Xna.Framework.Color[width * height], width, height) { }

        private Texture2D _generatedTexture = null;
        private Bitmap _generatedImage = null;

        public SimpleTexture(Microsoft.Xna.Framework.Color[] data, int width, int height)
        {
            Data = data;
            Width = width;
            Height = height;

            if (width * height != data.Length)
                throw new ArgumentOutOfRangeException($"Data does not have the right amount of pixels {data.Length} for a {width} x {height} texture.");
        }

        public Texture2D ToXnaTexture(GraphicsDevice graphicsDevice)
        {
            if (_generatedTexture == null)
                _generatedTexture = new Texture2D(graphicsDevice, Width, Height);

            UInt32[] data = new UInt32[Data.Length];

            for (int i = 0; i < Data.Length; i++)
                data[i] = Data[i].PackedValue;

            _generatedTexture.SetData(data);

            return _generatedTexture;
        }

        public Bitmap ToBitMap()
        {
            if (_generatedImage == null)
                _generatedImage = new Bitmap(Width, Height);

            UInt32[] data = new UInt32[Data.Length];

            for (int i = 0; i < Data.Length; i++)
                _generatedImage.SetPixel(i % Width, i / Width, System.Drawing.Color.FromArgb(Data[i].A, Data[i].R, Data[i].G, Data[i].B));

            return _generatedImage;
        }

        public void Combine(int x, int y, SimpleTexture simpleTexture)
        {
            for (int yIdx = 0; yIdx < simpleTexture.Height; yIdx++)
                Array.Copy(simpleTexture.Data, yIdx * simpleTexture.Width, Data, x + y * Height, simpleTexture.Width);
        }
    }
}
