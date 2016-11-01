using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Direct2D1;

using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using System.Windows.Forms;

namespace DxLib
{
    public class D2DFigure
    {

        SharpDX.Direct3D11.Device _device;
        Texture2D _backBuffer;
        RenderTargetView _backBufferView;
        SwapChain _swapChain;
        

        public SharpDX.Direct2D1.Factory Factory2D { get; private set; }
        public RenderTarget RenderTarget2D { get; private set; }
        private Bitmap _bitmap;

        public SharpDX.Direct3D11.Device Device
        {
            get
            {
                return _device;
            }
        }

        public Texture2D BackBuffer
        {
            get
            {
                return _backBuffer;
            }
        }

        public RenderTargetView BackBufferView
        {
            get
            {
                return _backBufferView;
            }
        }

        void Initialize(Form form)
        {
            var DisplayHandle = form.Handle;
            // SwapChain description
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(400, 400,
                                        new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = DisplayHandle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create Device and SwapChain
            SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, new[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 }, desc, out _device, out _swapChain);

            // Ignore all windows events
            SharpDX.DXGI.Factory factory = _swapChain.GetParent<SharpDX.DXGI.Factory>();
            factory.MakeWindowAssociation(DisplayHandle, WindowAssociationFlags.IgnoreAll);

            // New RenderTargetView from the backbuffer
            _backBuffer = Texture2D.FromSwapChain<Texture2D>(_swapChain, 0);

            _backBufferView = new RenderTargetView(_device, _backBuffer);
        }

        public D2DFigure(Form form)
        {
            Initialize(form);

            Factory2D = new SharpDX.Direct2D1.Factory();

            using (var surface = BackBuffer.QueryInterface<Surface>())
            {
                RenderTarget2D = new RenderTarget(Factory2D, surface,
                                                  new RenderTargetProperties(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied)));
            }
            RenderTarget2D.AntialiasMode = AntialiasMode.PerPrimitive;

            _bitmap = LoadFromFile(RenderTarget2D, "C:\\Users\\kishima\\Source\\Repos\\TestVS\\TestVS\\test.png");

        }

        public Bitmap LoadFromFile(RenderTarget renderTarget, string file)
        {
            // Loads from file using System.Drawing.Image
            using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(file))
            {
                var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var bitmapProperties = new BitmapProperties(new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
                var size = new Size2(bitmap.Width, bitmap.Height);

                // Transform pixels from BGRA to RGBA
                int stride = bitmap.Width * sizeof(int);
                using (var tempStream = new DataStream(bitmap.Height * stride, true, true))
                {
                    // Lock System.Drawing.Bitmap
                    var bitmapData = bitmap.LockBits(sourceArea, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                    // Convert all pixels 
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        int offset = bitmapData.Stride * y;
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            // Not optimized 
                            byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            int rgba = R | (G << 8) | (B << 16) | (A << 24);
                            tempStream.Write(rgba);
                        }

                    }
                    bitmap.UnlockBits(bitmapData);
                    tempStream.Position = 0;

                    return new Bitmap(renderTarget, size, tempStream, stride, bitmapProperties);
                }
            }
        }

        public void Draw()
        {
            BeginDraw();
            RenderTarget2D.DrawBitmap(_bitmap, 1.0f, BitmapInterpolationMode.Linear);
            EndDraw();

        }

        void BeginDraw()
        {
            RenderTarget2D.BeginDraw();
        }

        void EndDraw()
        {
            RenderTarget2D.EndDraw();

        }
    }
}
