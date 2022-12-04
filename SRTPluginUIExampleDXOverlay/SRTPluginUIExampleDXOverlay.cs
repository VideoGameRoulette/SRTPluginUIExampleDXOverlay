using GameOverlay.Drawing;
using GameOverlay.Windows;
using SRTPluginBase;
using SRTExampleProvider64;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace SRTPluginUIExampleDXOverlay
{
    public class SRTPluginUIExampleDXOverlay : PluginBase, IPluginUI
    {
        internal static PluginInfo _Info = new PluginInfo();
        public override IPluginInfo Info => _Info;
        public string RequiredProvider => "SRTExampleProvider64";
        private IPluginHostDelegates hostDelegates;
        private IGameMemoryExample gameMemory;

        // DirectX Overlay-specific.
        private OverlayWindow _window;
        private Graphics _graphics;
        private SharpDX.Direct2D1.WindowRenderTarget _device;

        private Font _consolasBold;

        private SolidBrush _black;
        private SolidBrush _white;
        private SolidBrush _red;
        private SolidBrush _yellow_500;
        private SolidBrush _slate_800;
        private SolidBrush _slate_900;
        private SolidBrush _sky_500;

        private SharpDX.Direct2D1.Bitmap _money;
        private SharpDX.Direct2D1.Bitmap _kudos;
        private SharpDX.Direct2D1.Bitmap _liberty;
        private SharpDX.Direct2D1.Bitmap _utility;
        private SharpDX.Direct2D1.Bitmap _morality;

        public PluginConfiguration config;
        private Process GetProcess() => Process.GetProcessesByName("TRIANGLE_STRATEGY-Win64-Shipping")?.FirstOrDefault();
        private Process gameProcess;
        private IntPtr gameWindowHandle;

        [STAThread]
        public override int Startup(IPluginHostDelegates hostDelegates)
        {
            this.hostDelegates = hostDelegates;
            config = LoadConfiguration<PluginConfiguration>();

            gameProcess = GetProcess();
            if (gameProcess == default)
                return 1;
            gameWindowHandle = gameProcess.MainWindowHandle;

            DEVMODE devMode = default;
            devMode.dmSize = (short)Marshal.SizeOf<DEVMODE>();
            PInvoke.EnumDisplaySettings(null, -1, ref devMode);

            // Create and initialize the overlay window.
            _window = new OverlayWindow(0, 0, devMode.dmPelsWidth, devMode.dmPelsHeight);
            _window?.Create();

            // Create and initialize the graphics object.
            _graphics = new Graphics()
            {
                MeasureFPS = false,
                PerPrimitiveAntiAliasing = false,
                TextAntiAliasing = true,
                UseMultiThreadedFactories = false,
                VSync = false,
                Width = _window.Width,
                Height = _window.Height,
                WindowHandle = _window.Handle
            };
            _graphics?.Setup();

            // Get a refernence to the underlying RenderTarget from SharpDX. This'll be used to draw portions of images.
            _device = (SharpDX.Direct2D1.WindowRenderTarget)typeof(Graphics).GetField("_device", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_graphics);

            _consolasBold = _graphics?.CreateFont(config.StringFontName, config.FontSize, true);

            _white = _graphics?.CreateSolidBrush(255, 255, 255);
            _red = _graphics?.CreateSolidBrush(255, 0, 0);
            _yellow_500 = _graphics?.CreateSolidBrush(245, 158, 11, 255);
            _sky_500 = _graphics?.CreateSolidBrush(14, 165, 233, 255);
            _slate_800 = _graphics?.CreateSolidBrush(30, 41, 59, 100);
            _slate_900 = _graphics?.CreateSolidBrush(15, 23, 42, 255);
            _black = _graphics?.CreateSolidBrush(0, 0, 0, 100);

            _money = ImageLoader.LoadBitmap(_device, Properties.Resources.Money);
            _kudos = ImageLoader.LoadBitmap(_device, Properties.Resources.Kudos);
            _liberty = ImageLoader.LoadBitmap(_device, Properties.Resources.LIberty);
            _utility = ImageLoader.LoadBitmap(_device, Properties.Resources.Utility);
            _morality = ImageLoader.LoadBitmap(_device, Properties.Resources.Morality);
            return 0;
        }

        public override int Shutdown()
        {
            SaveConfiguration(config);

            _black?.Dispose();
            _white?.Dispose();
            _red?.Dispose();
            _slate_800?.Dispose();
            _slate_900?.Dispose();
            _sky_500?.Dispose();
            _yellow_500?.Dispose();

            _consolasBold?.Dispose();
            _money?.Dispose();
            _kudos?.Dispose();
            _liberty?.Dispose();
            _utility?.Dispose();
            _morality?.Dispose();

            _device = null; // We didn't create this object so we probably shouldn't be the one to dispose of it. Just set the variable to null so the reference isn't held.
            _graphics?.Dispose(); // This should technically be the one to dispose of the _device object since it was pulled from this instance.
            _graphics = null;
            _window?.Dispose();
            _window = null;

            gameProcess?.Dispose();
            gameProcess = null;

            return 0;
        }

        public int ReceiveData(object gameMemory)
        {
            this.gameMemory = (IGameMemoryExample)gameMemory;
            _window?.PlaceAbove(gameWindowHandle);
            _window?.FitTo(gameWindowHandle, true);

            try
            {
                _graphics?.BeginScene();
                _graphics?.ClearScene();
                if (config.ScalingFactor != 1f)
                    _device.Transform = new SharpDX.Mathematics.Interop.RawMatrix3x2(config.ScalingFactor, 0f, 0f, config.ScalingFactor, 0f, 0f);
                DrawOverlay();
                if (config.ScalingFactor != 1f)
                    _device.Transform = new SharpDX.Mathematics.Interop.RawMatrix3x2(1f, 0f, 0f, 1f, 0f, 0f);
            }
            catch (Exception ex)
            {
                hostDelegates.ExceptionMessage.Invoke(ex);
            }
            finally
            {
                _graphics?.EndScene();
            }

            return 0;
        }

        private float GetStringSize(string str)
        {
            return (float)_graphics?.MeasureString(_consolasBold, _consolasBold.FontSize, str).X;
        }

        private float AlignRight(string s, float x)
        {
            return (x + 170f) - GetStringSize(s);
        }

        private void DrawTextBlock(ref float dx, ref float dy, string label, string val, SolidBrush color)
        {
            _graphics?.DrawText(_consolasBold, _consolasBold.FontSize, _yellow_500, dx, dy += 24f, label);
            var dx2 = dx + GetStringSize(label);
            _graphics?.DrawText(_consolasBold, _consolasBold.FontSize, color, AlignRight(val, dx), dy, val);
        }

        private void DrawTextBlockRow(ref float dx, ref float dy, string label, string val, SolidBrush color)
        {
            float marginX = 40f;
            _graphics?.DrawText(_consolasBold, _consolasBold.FontSize, _yellow_500, dx, dy, label);
            var dx2 = dx + GetStringSize(label);
            _graphics?.DrawText(_consolasBold, _consolasBold.FontSize, color, dx2, dy, val);
            dx += GetStringSize(label) + GetStringSize(val) + marginX;
        }

        private void DrawTextValue(ref float dx, ref float dy, string val, SolidBrush color)
        {
            float marginX = 40f;
            _graphics?.DrawText(_consolasBold, _consolasBold.FontSize, color, dx, dy, val);
            dx += GetStringSize(val) + marginX;
        }

        private void DrawImage(ref float dx, ref float dy, SharpDX.Direct2D1.Bitmap bm)
        {
            SharpDX.Mathematics.Interop.RawRectangleF imageRegion;
            imageRegion = new SharpDX.Mathematics.Interop.RawRectangleF(0, 0, 64f, 64f);
            SharpDX.Mathematics.Interop.RawRectangleF drawRegion;
            drawRegion = new SharpDX.Mathematics.Interop.RawRectangleF(dx, dy - 8f, dx + 32f, dy + 24f);
            _device?.DrawBitmap(bm, drawRegion, 1f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear, imageRegion);
            dx += 32f;
        }

        private void DrawImageWithLabel(ref float dx, ref float dy, string val, SharpDX.Direct2D1.Bitmap bm, SolidBrush color)
        {
            DrawImage(ref dx, ref dy, bm);
            DrawTextValue(ref dx, ref dy, val, color);
        }

        private void DrawOverlay()
        {
            var dict = new Dictionary<string, string>()
            {
                {config.MoneyString, gameMemory.Money.ToString()},
                {config.KudosString, gameMemory.Kudos.ToString()},
                {config.LibertyString, gameMemory.Liberty.ToString()},
                {config.UtilityString, gameMemory.Utility.ToString()},
                {config.MoralityString, gameMemory.Morality.ToString()}
            };

            if (!config.ShowMoney) dict.Remove(config.MoneyString);
            if (!config.ShowKudos) dict.Remove(config.KudosString);
            if (!config.ShowConvictions)
            {
                dict.Remove(config.LibertyString);
                dict.Remove(config.UtilityString);
                dict.Remove(config.MoralityString);
            }

            float offsetX = config.PositionX + 15f;
            float offsetY = config.PositionY + (20f - (_consolasBold.FontSize / 2));
            float textWidth = 0f;

            foreach (var item in dict)
                textWidth += GetStringSize(item.Key) + GetStringSize(item.Value);

            textWidth += (15f * 2) + (40f * (dict.Count - 1));
            var center = 960f - (textWidth / 2);
            _graphics?.FillRectangle(_black, center, config.PositionY, center + textWidth, config.PositionY + 40f);
            offsetX = center + 10f;

            foreach (var item in dict)
            {
                if (item.Key == config.MoneyString)
                {
                    if (!config.ShowMoney) continue;
                    DrawImageWithLabel(ref offsetX, ref offsetY, item.Value, _money, _white);
                    continue;
                }
                if (item.Key == config.KudosString)
                {
                    if (!config.ShowKudos) continue;
                    DrawImageWithLabel(ref offsetX, ref offsetY, item.Value, _kudos, _white);
                    continue;
                }
                if (!config.ShowConvictions) continue;
                if (item.Key == config.LibertyString)
                {
                    DrawImageWithLabel(ref offsetX, ref offsetY, item.Value, _liberty, _white);
                    continue;
                }
                if (item.Key == config.UtilityString)
                {
                    DrawImageWithLabel(ref offsetX, ref offsetY, item.Value, _utility, _white);
                    continue;
                }
                if (item.Key == config.MoralityString)
                {
                    DrawImageWithLabel(ref offsetX, ref offsetY, item.Value, _morality, _white);
                    continue;
                }
            }
        }

    }

}
