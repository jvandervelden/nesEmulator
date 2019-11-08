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

        //private Display _mainDisplay;
        private List<Display> _subDisplay;
        //private Graphics _graphics;
        private List<Graphics> _subGraphics;
        private int _pixelWidth = 1;
        private int _pixelHeight = 1;
        private int _width = 1;
        private int _height = 1;
        //private Thread loopThread;

        protected Dictionary<Keys, bool> interestedKeys = new Dictionary<Keys, bool>();

        protected readonly ConcurrentDictionary<Keys, KeyState> _keyState = new ConcurrentDictionary<Keys, KeyState>();

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
                interestedKeys[interestedKey] = currentState.IsKeyDown(interestedKey);
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);
            spriteBatch.Begin();

            OnUserUpdate(gameTime.ElapsedGameTime.Ticks);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        public void Start()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //foreach (Keys key in Enum.GetValues(typeof(Keys)))
            //    _keyState.TryAdd(key, KeyState.OPEN);

            //_mainDisplay = CreateDisplay(_width, _height, _pixelWidth, _pixelHeight);
            //_mainDisplay.KeyDown += _mainDisplay_KeyDown;
            //_mainDisplay.KeyUp += _mainDisplay_KeyUp;

            //_mainDisplay.FormClosing += _drawControl_FormClosing;

            _subDisplay = new List<Display>();

            Console.WindowWidth = Math.Min(105, Console.LargestWindowWidth);
            Console.WindowHeight = Math.Min(68, Console.LargestWindowHeight);
            Console.SetBufferSize(Math.Max(Console.BufferWidth, 190), Math.Max(Console.BufferHeight, 67));
            Console.CursorVisible = false;

            //_graphics = _mainDisplay.CreateGraphics();
            _subGraphics = new List<Graphics>();

            //_graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

            run = true;

            if (OnUserCreate())
            {
                //loopThread = new Thread(this.MainLoop);
                //loopThread.Priority = ThreadPriority.Highest;
                //lastFireTime = DateTime.Now.Ticks;
                //loopThread.Start();

                //fpsTimer.Elapsed += FpsTimer_Elapsed;
                //fpsTimer.Interval = 1000;
                //fpsTimer.Start();
            }
            else
            {
                run = false;
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

        protected int CreateSubDisplay(int width, int height, string caption)
        {
            return CreateSubDisplay(width, height, _pixelWidth, _pixelHeight, caption);
        }

        protected int CreateSubDisplay(int width, int height, int pw, int ph, string caption)
        {
            Display subDisplay = CreateDisplay(width, height, _pixelWidth, _pixelHeight);
            Graphics subGraphics = subDisplay.CreateGraphics();

            subDisplay.Message = caption;
            subDisplay.DisplayId = _subDisplay.Count;
            subDisplay.FormClosing += SubDisplay_FormClosing;

            subGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

            _subDisplay.Add(subDisplay);
            _subGraphics.Add(subDisplay.CreateGraphics());

            return subDisplay.DisplayId.Value;
        }

        private void SubDisplay_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (_subDisplay.Count > 0)
            {
                _subDisplay.RemoveAt(((Display)sender).DisplayId.Value);
                _subGraphics.RemoveAt(((Display)sender).DisplayId.Value);
            }
        }

        //private void _mainDisplay_KeyUp(object sender, KeyEventArgs e)
        //{
            //_keyState.AddOrUpdate(e.KeyCode, (k) => KeyState.PRESSED, (k, ks) => KeyState.PRESSED);
        //}

        //private void _mainDisplay_KeyDown(object sender, KeyEventArgs e)
        //{
            //_keyState.AddOrUpdate(e.KeyCode, (k) => KeyState.HELD, (k, ks) => KeyState.HELD);
        //}

        private void ClearPressedKeys()
        {
            //foreach (Keys key in _keyState.Keys)
            //{
            //    _keyState.TryUpdate(key, KeyState.OPEN, KeyState.PRESSED);
            //}
        }

        private void FpsTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //this._mainDisplay.SetText($"FPS: {fps.ToString("N2")}");
        }

        //private void _drawControl_FormClosing(object sender, FormClosingEventArgs e)
        //{
            //this.run = false;

            //_graphics = null;

            //for (int i = _subDisplay.Count - 1; i >= 0; i--)
            //    _subDisplay[i].Close();

            //this.loopThread.Join();

            //Application.Exit();
        //}

        private long lastFireTime;
        private bool run;
        private bool updatePass;
        private long fps = 0;
        private long cyclesInFrame = 0;

        protected bool IsRunning { get { return run; } }

        private void MainLoop(object data)
        {
            updatePass = true;

            while (run && updatePass)
            {
                //cyclesInFrame++;

                long currentTime = DateTime.Now.Ticks;
                long elapsedTicks = currentTime - lastFireTime;

                //if (elapsedTicks > TimeSpan.TicksPerMillisecond)
                //{
                //    fps = cyclesInFrame * 1000;
                //    cyclesInFrame = 0;
                //}

                updatePass = this.OnUserUpdate(elapsedTicks);

                if (elapsedTicks != 0)
                    lastFireTime = currentTime;

                //ClearPressedKeys();
            }
        }

        protected abstract bool OnUserCreate();

        protected abstract bool OnUserUpdate(long elapsedTicks);

        private Graphics GetDisplay(int? display = null)
        {
            if (display.HasValue)
            {
                if (display.Value < _subGraphics.Count)
                    return _subGraphics[display.Value];
                else
                    throw new NoGraphicsException();
            }

            throw new NoGraphicsException();
        }

        protected void Clear(System.Drawing.Color bgColor, int? display = null)
        {
            GraphicsDevice.Clear(new Microsoft.Xna.Framework.Color((uint)bgColor.ToArgb()));
        }

        protected void Draw(int x, int y, SimpleTexture image, int? display = null)
        {
            spriteBatch.Draw(image.ToXnaTexture(GraphicsDevice), new Microsoft.Xna.Framework.Rectangle(x * _pixelWidth, y * _pixelHeight, image.Width * _pixelWidth, image.Height * _pixelHeight), Microsoft.Xna.Framework.Color.White);
            //spriteBatch.Draw(image.ToXnaTexture(GraphicsDevice), new Microsoft.Xna.Framework.Rectangle(x, y, image.Width, image.Height), Microsoft.Xna.Framework.Color.White);
        }

        protected void Draw(int x, int y, Image image, int? display = null)
        {
            GetDisplay(display).DrawImage(image, x * _pixelWidth, y * _pixelHeight, image.Width * _pixelWidth, image.Height * _pixelHeight);
        }

        protected void Draw(int x, int y, Microsoft.Xna.Framework.Color color)
        {
            var tex = new Texture2D(GraphicsDevice, 1, 1);

            tex.SetData(new Microsoft.Xna.Framework.Color[] { color });
            spriteBatch.Draw(tex, new Microsoft.Xna.Framework.Rectangle(x * _pixelWidth, y * _pixelHeight, _pixelWidth, _pixelHeight), Microsoft.Xna.Framework.Color.White);
        }

        protected void Draw(int x, int y, System.Drawing.Color color, int? display = null)
        {
            GetDisplay(display).FillRectangle(new SolidBrush(color), new System.Drawing.Rectangle(x * _pixelWidth, y * _pixelHeight, _pixelWidth, _pixelHeight));
        }

        protected void DrawRect(int x, int y, int width, int height, System.Drawing.Color color, bool fill = true, int? display = null)
        {
            var pen = new Pen(color);
            var brush = new SolidBrush(color);
            var rectangle = new System.Drawing.Rectangle(x * _pixelWidth, y * _pixelHeight, width * _pixelWidth, height * _pixelHeight);

            if (fill) GetDisplay(display).FillRectangle(brush, rectangle);
            else GetDisplay(display).DrawRectangle(pen, rectangle);
        }

        protected void DrawString(int x, int y, String message, System.Drawing.Color color, int? display = null)
        {
            GetDisplay(display).DrawString(message, new Font(FontFamily.GenericMonospace, 10), new SolidBrush(color), x, y);
        }

        protected void DrawLine(int x1, int y1, int x2, int y2, System.Drawing.Color color)
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
                    Draw((int)Math.Round(tx), ty, color);
                }
            }
            else
            {
                int sX = Math.Min(x1, x2);
                int eX = Math.Max(x1, x2);

                for (int tx = sX; tx <= eX; tx++)
                {
                    double ty = Math.Round(slope * tx + b);
                    Draw(tx, (int)Math.Round(ty), color);
                }
            }
        }

        protected void DrawWireFrameModel(List<Vec2d> modelCoordinates, double x, double y, double r, double s, System.Drawing.Color color)
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
                    (int)Math.Round(transformedCoordinates[j % verts].X), (int)Math.Round(transformedCoordinates[j % verts].Y), color);
		    }
	    }
    }
}
