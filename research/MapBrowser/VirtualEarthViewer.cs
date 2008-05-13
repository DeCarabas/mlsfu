//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Tools.MapBrowser
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Threading;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Linq;
    using System.Linq;
    using System.Net;
    using System.IO;
    using System.Windows.Input;
    using System.Text;

    public class VirtualEarthViewer : VirtualEarthBase
    {
        WorldCoordinate _upperLeftAtDown;
        WorldCoordinate _viewAtDown;
        Point _mouseDownPixel;

        double _wheelDelta;


        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _wheelDelta += e.Delta;
            if (Math.Abs(_wheelDelta) >= 120)
            {
                int zoomSnaps = (int)(_wheelDelta / 120.0);
                int zoom = ZoomLevel + zoomSnaps;
                ZoomLevel = Math.Max(Math.Min(zoom, 18), 1);
                _wheelDelta -= zoomSnaps * 120.0;
            }
            base.OnMouseWheel(e);
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            base.OnMouseLeftButtonUp(e);
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Mouse.Capture(this, CaptureMode.SubTree);
            _upperLeftAtDown = new WorldCoordinate() { Latitude = CornerLatitude, Longitude = CornerLongitude };
            _viewAtDown = GetView();
            _mouseDownPixel = e.GetPosition(this);
            if (e.ClickCount == 2)
            {
                LookAt(ClientToWorld(_mouseDownPixel));
                ZoomLevel = Math.Min(ZoomLevel + 1, 18);
            }
            base.OnMouseLeftButtonDown(e);
        }
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point mouseCurrentPixel = e.GetPosition(this);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double xDelta = _mouseDownPixel.X - mouseCurrentPixel.X;
                double yDelta = _mouseDownPixel.Y - mouseCurrentPixel.Y;

                if (ViewMode == WorldPanelViewMode.Center)
                {
                    Point offset = Translator.CoordinateToPixels(_viewAtDown);
                    WorldCoordinate newWorld = Translator.PixelsToCoordinates(new Point(xDelta + offset.X, yDelta + offset.Y));

                    ViewLatitude = newWorld.Latitude;
                    ViewLongitude = newWorld.Longitude;
                }
                else
                {
                    Point offset = Translator.CoordinateToPixels(_upperLeftAtDown);
                    WorldCoordinate newWorld = Translator.PixelsToCoordinates(new Point(xDelta + offset.X, yDelta + offset.Y));

                    CornerLatitude = newWorld.Latitude;
                    CornerLongitude = newWorld.Longitude;
                }
            }

            base.OnMouseMove(e);
        }
    }
    public class VirtualEarthMap : VirtualEarthBase
    {
        public static readonly DependencyProperty MapStyleProperty =
            DependencyProperty.Register("MapStyle", typeof(MapStyle), typeof(VirtualEarthMap), new FrameworkPropertyMetadata(MapStyle.Roads));

        public MapStyle MapStyle { get { return (MapStyle)GetValue(MapStyleProperty); } set { SetValue(MapStyleProperty, value); } }

        Dictionary<TileCoordinate, UIElement> displays = new Dictionary<TileCoordinate, UIElement>();
        Queue<Func<object>> workQueue = new Queue<Func<object>>();

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ZoomLevelProperty
                || e.Property == MapStyleProperty)
            {
                workQueue.Clear();
                displays.Clear();
                
                InvalidateMeasure();
            }
            else if (e.Property == CornerLatitudeProperty
                || e.Property == CornerLongitudeProperty)
            {
                InvalidateMeasure();
            }
            base.OnPropertyChanged(e);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Children.Clear();
            //VisualOperations.GetChildren(this).Clear();

            if (double.IsPositiveInfinity(constraint.Width)) { constraint.Width = 500; }
            if (double.IsPositiveInfinity(constraint.Height)) { constraint.Height = 500; }

            double optimalRows = constraint.Height / (double)VETSHelper.TileSizePixels;
            double optimalColumns = constraint.Width / (double)VETSHelper.TileSizePixels;

            int neededRows = (int)(Math.Ceiling(optimalRows + 1.0));
            int neededColumns = (int)(Math.Ceiling(optimalColumns + 1.0));

            WorldCoordinate upperLeft = new WorldCoordinate() { Latitude = CornerLatitude, Longitude = CornerLongitude };

            var pixels = Translator.CoordinateToPixels(upperLeft);
            int tileX, tileY;
            VirtualEarthTileSystem.PixelXYToTileXY((int)pixels.X,(int) pixels.Y, out tileX, out tileY);
            TileCoordinate tileStart = new TileCoordinate(tileX, tileY);

            Dictionary<TileCoordinate, UIElement> oldDisplays = displays;
            displays = new Dictionary<TileCoordinate, UIElement>();

            int maxTiles = VETSHelper.MaxTiles(ZoomLevel);

            for (int y = tileStart.Y; y < Math.Min(tileStart.Y + neededRows, maxTiles); y++)
            {
                for (int x = tileStart.X; x < Math.Min(tileStart.X + neededColumns, maxTiles); x++)
                {
                    TileCoordinate cur = new TileCoordinate(x, y);
                    UIElement child = null;
                    if (!oldDisplays.ContainsKey(cur))
                    {
                        child = CreateChild(cur);
                        if (child != null)
                        {
                            displays[cur] = child;
                        }
                    }
                    else
                    {
                        child = displays[cur] = oldDisplays[cur];
                    }

                    if (child != null)
                    {
                        Children.Add(child);
                        //VisualOperations.GetChildren(this).Add(child);
                        child.Measure(constraint);
                    }
                }
            }
            if (workQueue.Count > 0)
            {
                workQueue.Dequeue().BeginInvoke(null, null);
            }

            return constraint;
        }

        UIElement CreateChild(TileCoordinate tile)
        {
            string ft;
            ft = ".jpg";
            string ms = "a";
            switch (MapStyle)
            {
                case MapStyle.Roads:
                    ms = "r";
                    ft = ".png";
                    break;
                case MapStyle.Satalite:
                    ms = "a";
                    ft = ".jpg";
                    break;
                case MapStyle.Both:
                    ms = "h";
                    ft = ".jpg";
                    break;
            }
            string qk = VirtualEarthTileSystem.TileXYToQuadKey(tile.X, tile.Y, ZoomLevel);
            string fullpath = "http://r0.ortho.tiles.virtualearth.net/tiles/" + ms + qk + ft + "?g=1";
            Image img = new Image();
            img.Stretch = Stretch.None;
            img.Height = img.Width = VETSHelper.TileSizePixels;
            img.SnapsToDevicePixels = true;
            SetImageSource(img, fullpath);

            int absX, absY;
            VirtualEarthTileSystem.TileXYToPixelXY(tile.X, tile.Y, out absX, out absY);
            Point absolute = new Point(absX, absY);
            WorldCoordinate wc = Translator.PixelsToCoordinates(absolute);
            SetLatitude(img, wc.Latitude);
            SetLongitude(img, wc.Longitude);
            return img;
        }

        void SetImageSource(Image img, string fullpath)
        {
            workQueue.Enqueue(((Func<object>)delegate()
            {
                MemoryStream t = new MemoryStream();
                try
                {
                    Stream image = new WebClient().OpenRead(fullpath);
                    byte[] buffer = new byte[4096];
                    int written = 0;
                    do
                    {
                        written = image.Read(buffer, 0, buffer.Length);
                        t.Write(buffer, 0, written);
                    } while (written > 0);
                    t.Seek(0, SeekOrigin.Begin);
                }
                catch { return null; }

                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    (DispatcherOperationCallback)delegate(object o)
                    {
                        BitmapImage b = new BitmapImage();
                        b.BeginInit();
                        b.StreamSource = t;
                        b.EndInit();
                        img.Source = b;
                        if (workQueue.Count > 0)
                        {
                            workQueue.Dequeue().BeginInvoke(null, null);
                        }
                        return null;
                    }, null);
                return null;
            }));
        }
    }
    public class VirtualEarthBase : WorldPanel
    {
        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
            "ZoomLevel",
            typeof(int),
            typeof(VirtualEarthBase),
            new FrameworkPropertyMetadata(2));

        public int ZoomLevel { get { return (int)GetValue(ZoomLevelProperty); } set { SetValue(ZoomLevelProperty, value); } }

        public VirtualEarthBase()
        {
            ClipToBounds = true;
            this.RequestBringIntoView += new RequestBringIntoViewEventHandler(VirtualEarthBase_RequestBringIntoView);
        }

        void VirtualEarthBase_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            UIElement target = e.TargetObject as UIElement;
            while (target != null
                && target != this
                && (GetLatitude(target) == 0.0
                || GetLongitude(target) == 0.0))
            {
                target = VisualTreeHelper.GetParent(target) as UIElement;
            }
            if (target != null && target != this)
            {
                double lat = GetLatitude(target);
                double lon = GetLongitude(target);

                WorldCoordinate bottomRight = ClientToWorld(new Point(ActualWidth, ActualHeight));

                if (lat > CornerLatitude || lat < bottomRight.Latitude
                    || lon < CornerLongitude || lon > bottomRight.Longitude)
                {
                    ViewLatitude = lat;
                    ViewLongitude = lon;
                }
            }

        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ZoomLevelProperty)
            {
                UpdateMapSize(ZoomLevel);
            }
            base.OnPropertyChanged(e);
        }
    }
    public enum MapStyle
    {
        Satalite,
        Roads,
        Both
    }
    public class ViewComboBox : ComboBox
    {
        public ViewComboBox()
        {
            ItemsSource = new MapStyle[] { MapStyle.Satalite, MapStyle.Roads, MapStyle.Both };
        }
    }

    static class VETSHelper
    {
        internal const int TileSizePixels = 256;

        internal static int MaxTiles(int quadLevel)
        {
            return (int)Math.Pow(2.0, (double)quadLevel);
        }
    }
    struct TileCoordinate
    {
        int _x, _y;
        public int X { get { return _x; } set { _x = value; } }
        public int Y { get { return _y; } set { _y = value; } }

        public TileCoordinate(int x, int y) { _x = x; _y = y; }

        public override bool Equals(object obj)
        {
            return ((TileCoordinate)obj).X == X && ((TileCoordinate)obj).Y == Y;
        }
        public override int GetHashCode()
        {
            return _x.GetHashCode() + (_y.GetHashCode() << 8);
        }
    }
}
