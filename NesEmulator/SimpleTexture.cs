using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPGE
{
    public class SimpleTexture
    {
        public Color[] Data { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public SimpleTexture(int width, int height) : this(new Color[width * height], width, height) { }

        private Texture2D _generatedTexture = null;

        public SimpleTexture(Color[] data, int width, int height)
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
    }
}
