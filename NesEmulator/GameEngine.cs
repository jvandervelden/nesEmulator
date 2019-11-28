using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The idea is based on One Loan Coders console/pixel game engine: https://github.com/OneLoneCoder/videos
/// </summary>
namespace TestPGE
{
    public abstract partial class GameEngine : Game
    {
        public const int TICKS_PER_MILLISECOND = 1000000;
        public const int TICKS_PER_SECOND = 10000000;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        protected enum KeyState
        {
            HELD,
            PRESSED,
            OPEN
        }

        private class SubDisplayInfo
        {
            public Display Display { get; set; } = null;
            public Graphics Graphics { get; set; } = null;
            public int PixelWidth { get; set; } = 1;
            public int PixelHeight { get; set; } = 1;
            public int Width { get; set; } = 1;
            public int Height { get; set; } = 1;
        }

        private Dictionary<string, SubDisplayInfo> _subDisplay;
        private int _pixelWidth = 1;
        private int _pixelHeight = 1;
        private int _width = 1;
        private int _height = 1;

        protected Dictionary<Keys, KeyState> interestedKeys = new Dictionary<Keys, KeyState>();

        protected Random _rand = new Random((int)DateTime.Now.Ticks + (int)(DateTime.Now.Ticks >> 32));

        private System.Timers.Timer fpsTimer = new System.Timers.Timer();

        protected int ScreenWidth { get { return _width; } }
        protected int ScreenHeight { get { return _height; } }

        protected int PixelWidth { get { return _pixelWidth; } }
        protected int PixelHeight { get { return _pixelHeight; } }

        public GameEngine(int width, int height, int pw, int ph) : base ()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _width = width;
            _height = height;
            _pixelWidth = pw;
            _pixelHeight = ph;

            graphics.PreferredBackBufferWidth = width * pw;
            graphics.PreferredBackBufferHeight = height * ph;
            IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            Start();
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState currentState = Keyboard.GetState();


            foreach (Keys interestedKey in interestedKeys.Keys.ToArray())
            {
                if (currentState.IsKeyDown(interestedKey))
                    interestedKeys[interestedKey] = KeyState.HELD;
                else if (interestedKeys[interestedKey] != KeyState.OPEN)
                    interestedKeys[interestedKey]++;
            }

            OnUserUpdate(gameTime.ElapsedGameTime.Ticks);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            DrawFrame(gameTime.ElapsedGameTime.Ticks);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            IsRunning = false;
            Dispose();
            base.OnExiting(sender, args);
        }

        public void Start()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            _subDisplay = new Dictionary<string, SubDisplayInfo>();

            Console.WindowWidth = Math.Min(105, Console.LargestWindowWidth);
            Console.WindowHeight = Math.Min(68, Console.LargestWindowHeight);
            Console.SetBufferSize(Math.Max(Console.BufferWidth, 190), Math.Max(Console.BufferHeight, 67));
            Console.CursorVisible = false;
            
            IsRunning = true;

            if (!OnUserCreate())
            {
                IsRunning = false;
                Exit();
            }
        }

        private Display CreateDisplay(int width, int height, int pw, int ph)
        {
            Display display = new Display();

            display.MinimumSize = new Size(width * pw + 17, height * ph + 40);
            display.MaximumSize = display.MinimumSize;
            display.MaximizeBox = false;

            display.Show();

            return display;
        }

        protected void CloseSubDisplay(string displayId)
        {
            if (_subDisplay.ContainsKey(displayId))
            {
                _subDisplay[displayId].Display.Close();
                _subDisplay.Remove(displayId);
            }
        }

        protected void CreateSubDisplay(int width, int height, string displayId)
        {
            CreateSubDisplay(width, height, _pixelWidth, _pixelHeight, displayId);
        }

        protected void CreateSubDisplay(int width, int height, int pw, int ph, string displayId)
        {
            Display subDisplay = CreateDisplay(width, height, pw, ph);
            Graphics subGraphics = subDisplay.CreateGraphics();

            subDisplay.Message = displayId;
            subDisplay.DisplayId = displayId;
            subDisplay.FormClosing += SubDisplay_FormClosing;

            subGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            
            _subDisplay.Add(displayId, new SubDisplayInfo() {
                Display = subDisplay,
                Graphics = subGraphics,
                PixelWidth = pw,
                PixelHeight = ph,
                Width = width,
                Height = height
            });
        }

        private void SubDisplay_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (_subDisplay.Count > 0)
            {
                _subDisplay.Remove(((Display)sender).DisplayId);
            }
        }

        private long lastFireTime;
        private bool updatePass;

        protected bool IsRunning { get; private set; }

        private void MainLoop(object data)
        {
            updatePass = true;

            while (IsRunning && updatePass)
            {
                long currentTime = DateTime.Now.Ticks;
                long elapsedTicks = currentTime - lastFireTime;

                updatePass = this.OnUserUpdate(elapsedTicks);

                if (elapsedTicks != 0)
                    lastFireTime = currentTime;
            }
        }

        protected abstract bool OnUserCreate();

        protected abstract bool OnUserUpdate(long elapsedTicks);

        protected abstract bool DrawFrame(long elapsedTicks);

        private SubDisplayInfo GetDisplay(string display)
        {
            if (_subDisplay.ContainsKey(display))
                return _subDisplay[display];

            return null;
        }

        protected void Clear(System.Drawing.Color bgColor, int? display = null)
        {
            GraphicsDevice.Clear(new Microsoft.Xna.Framework.Color((uint)bgColor.ToArgb()));
        }

        protected void Draw(int x, int y, SimpleTexture image)
        {
            spriteBatch.Draw(image.ToXnaTexture(GraphicsDevice), new Microsoft.Xna.Framework.Rectangle(x * _pixelWidth, y * _pixelHeight, image.Width * _pixelWidth, image.Height * _pixelHeight), Microsoft.Xna.Framework.Color.White);
        }

        protected void Draw(int x, int y, SimpleTexture image, string display)
        {
            SubDisplayInfo displayInfo = GetDisplay(display);

            displayInfo?.Graphics.DrawImage(image.ToBitMap(), 
                    x * displayInfo.PixelWidth, 
                    y * displayInfo.PixelHeight, 
                    image.Width * displayInfo.PixelWidth, 
                    image.Height * displayInfo.PixelHeight);
        }

        protected void Draw(int x, int y, Microsoft.Xna.Framework.Color color)
        {
            var tex = new Texture2D(GraphicsDevice, 1, 1);

            tex.SetData(new Microsoft.Xna.Framework.Color[] { color });
            spriteBatch.Draw(tex, new Microsoft.Xna.Framework.Rectangle(x * _pixelWidth, y * _pixelHeight, _pixelWidth, _pixelHeight), Microsoft.Xna.Framework.Color.White);
        }

        protected void Draw(int x, int y, System.Drawing.Color color, string display)
        {
            SubDisplayInfo displayInfo = GetDisplay(display);

            displayInfo?.Graphics.FillRectangle(new SolidBrush(color), new System.Drawing.Rectangle(x * _pixelWidth, y * _pixelHeight, _pixelWidth, _pixelHeight));
        }

        protected void DrawRect(int x, int y, int width, int height, System.Drawing.Color color, bool fill = true, string display = null)
        {
            SubDisplayInfo displayInfo = GetDisplay(display);

            if (displayInfo == null) return;

            var pen = new Pen(color);
            var brush = new SolidBrush(color);
            var rectangle = new System.Drawing.Rectangle(x * _pixelWidth, y * _pixelHeight, width * _pixelWidth, height * _pixelHeight);

            if (fill) displayInfo?.Graphics.FillRectangle(brush, rectangle);
            else displayInfo?.Graphics.DrawRectangle(pen, rectangle);
        }

        protected void DrawString(int x, int y, String message, System.Drawing.Color color, string display)
        {
            SubDisplayInfo displayInfo = GetDisplay(display);
            displayInfo?.Graphics.DrawString(message, new Font(FontFamily.GenericMonospace, 10), new SolidBrush(color), x, y);
        }

        protected void DrawLine(int x1, int y1, int x2, int y2, System.Drawing.Color color, string display)
        {
            double slope = (x1 - x2) == 0 ? 1 : (y1 - y2) / (x1 - x2);
            double b = y1 - slope * x1;

            if (Math.Abs(slope) > 1)
            {
                int sY = Math.Min(y1, y2);
                int eY = Math.Max(y1, y2);

                for (int ty = sY; ty <= eY; ty++)
                {
                    double tx = Math.Round((ty - b) / slope);
                    Draw((int)Math.Round(tx), ty, color, display);
                }
            }
            else
            {
                int sX = Math.Min(x1, x2);
                int eX = Math.Max(x1, x2);

                for (int tx = sX; tx <= eX; tx++)
                {
                    double ty = Math.Round(slope * tx + b);
                    Draw(tx, (int)Math.Round(ty), color, display);
                }
            }
        }

        protected void DrawWireFrameModel(List<Vec2d> modelCoordinates, double x, double y, double r, double s, System.Drawing.Color color, string display)
	    {
            // Create translated model vector of coordinate pairs
            List<Vec2d> transformedCoordinates = new List<Vec2d>();
            int verts = modelCoordinates.Count;

		    // Rotate
		    for (int i = 0; i < verts; i++)
		    {
                transformedCoordinates.Add(Vec2d.XY(0, 0));
                
                transformedCoordinates[i].X = modelCoordinates[i].X * Math.Cos(r) - modelCoordinates[i].Y * Math.Sin(r);
                transformedCoordinates[i].Y = modelCoordinates[i].X * Math.Sin(r) + modelCoordinates[i].Y * Math.Cos(r);

                transformedCoordinates[i].X = transformedCoordinates[i].X * s;
                transformedCoordinates[i].Y = transformedCoordinates[i].Y * s;

                transformedCoordinates[i].X = transformedCoordinates[i].X + x;
			    transformedCoordinates[i].Y = transformedCoordinates[i].Y + y;
		    }

		    // Draw Closed Polygon
		    for (int i = 0; i<verts + 1; i++)
		    {
			    int j = (i + 1);
                DrawLine((int)Math.Round(transformedCoordinates[i % verts].X), (int)Math.Round(transformedCoordinates[i % verts].Y),
                    (int)Math.Round(transformedCoordinates[j % verts].X), (int)Math.Round(transformedCoordinates[j % verts].Y), color, display);
		    }
	    }
    }
}
