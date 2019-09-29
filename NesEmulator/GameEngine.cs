using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

/// <summary>
/// The idea is based on One Loan Coders console/pixel game engine: https://github.com/OneLoneCoder/videos
/// </summary>
namespace TestPGE
{
    public abstract class GameEngine
    {
        public const int TICKS_PER_MILLISECOND = 1000000;
        public const int TICKS_PER_SECOND = 10000000;

        protected enum KeyState
        {
            HELD,
            PRESSED,
            OPEN
        }

        private Form1 _mainDisplay;
        private Graphics _graphics;
        private int _pixelWidth = 1;
        private int _pixelHeight = 1;
        private int _width = 1;
        private int _height = 1;
        private Thread loopThread;

        protected readonly ConcurrentDictionary<Keys, KeyState> _keyState = new ConcurrentDictionary<Keys, KeyState>();

        protected Random _rand = new Random((int)DateTime.Now.Ticks + (int)(DateTime.Now.Ticks >> 32));

        private System.Timers.Timer fpsTimer = new System.Timers.Timer();

        protected int ScreenWidth { get { return _width; } }
        protected int ScreenHeight { get { return _height; } }

        public GameEngine(int width, int height, int pw, int ph)
        {
            _width = width;
            _height = height;            
            _pixelWidth = pw;
            _pixelHeight = ph;
        }

        public void Start()
        {
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
                _keyState.TryAdd(key, KeyState.OPEN);

            _mainDisplay = new Form1();

            Console.WindowWidth = Math.Min(105, Console.LargestWindowWidth);
            Console.WindowHeight = Math.Min(68, Console.LargestWindowHeight);
            Console.SetBufferSize(Math.Max(Console.BufferWidth, 190), Math.Max(Console.BufferHeight, 67));
            Console.CursorVisible = false;

            _mainDisplay.MinimumSize = new Size(_width * _pixelWidth + 17, _height * _pixelHeight + 40);
            _mainDisplay.MaximumSize = _mainDisplay.MinimumSize;
            _mainDisplay.MaximizeBox = false;
            _mainDisplay.KeyDown += _mainDisplay_KeyDown;
            _mainDisplay.KeyUp += _mainDisplay_KeyUp;

            _mainDisplay.Show();

            _mainDisplay.FormClosing += _drawControl_FormClosing;

            _graphics = _mainDisplay.CreateGraphics();

            _graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

            if (OnUserCreate())
            {
                loopThread = new Thread(this.MainLoop);
                lastFireTime = DateTime.Now.Ticks;
                loopThread.Start();

                fpsTimer.Elapsed += FpsTimer_Elapsed;
                fpsTimer.Interval = 1000;
                fpsTimer.Start();
            }
        }

        private void _mainDisplay_KeyUp(object sender, KeyEventArgs e)
        {
            _keyState.AddOrUpdate(e.KeyCode, (k) => KeyState.PRESSED, (k, ks) => KeyState.PRESSED);
        }

        private void _mainDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            _keyState.AddOrUpdate(e.KeyCode, (k) => KeyState.HELD, (k, ks) => KeyState.HELD);
        }

        private void ClearPressedKeys()
        {
            foreach (Keys key in _keyState.Keys)
            {
                _keyState.TryUpdate(key, KeyState.OPEN, KeyState.PRESSED);
            }
        }

        private void FpsTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this._mainDisplay.SetText($"FPS: {fps.ToString("N2")}");
        }

        private void _drawControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.run = false;
            this.loopThread.Join();

            Application.Exit();
        }

        private long lastFireTime;
        private bool run;
        private bool updatePass;
        private double fps = 0;

        private void MainLoop(object data)
        {
            run = true;
            updatePass = true;

            while (run && updatePass)
            {
                long currentTime = DateTime.Now.Ticks;
                double elapsedTime = (currentTime - lastFireTime) / (double)TICKS_PER_SECOND;

                fps = 1d / elapsedTime;

                updatePass = this.OnUserUpdate(elapsedTime);

                lastFireTime = currentTime;

                ClearPressedKeys();
            } 
        }

        protected abstract bool OnUserCreate();
        protected abstract bool OnUserUpdate(double elapsedTime);
        
        protected void Clear(Color bgColor)
        {
            _graphics.Clear(bgColor);
        }

        protected void Draw(int x, int y, Color color)
        {
            _graphics.FillRectangle(new SolidBrush(color), new Rectangle(x * _pixelWidth, y * _pixelHeight, _pixelWidth, _pixelHeight));
        }

        protected void DrawRect(int x, int y, int width, int height, Color color, bool fill = true)
        {
            var pen = new Pen(color);
            var brush = new SolidBrush(color);
            var rectangle = new Rectangle(x * _pixelWidth, y * _pixelHeight, width * _pixelWidth, height * _pixelHeight);

            if (fill) _graphics.FillRectangle(brush, rectangle);
            else _graphics.DrawRectangle(pen, rectangle);
        }

        protected void DrawLine(int x1, int y1, int x2, int y2, Color color)
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

        protected void DrawWireFrameModel(List<Vec2d> modelCoordinates, double x, double y, double r, double s, Color color)
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
