//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Tools.MapBrowser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;

    public class VirtualEarthMap : VirtualEarthBase
    {
        public static readonly DependencyProperty MapStyleProperty =
            DependencyProperty.Register(
                "MapStyle",
                typeof(MapStyle),
                typeof(VirtualEarthMap),
                new FrameworkPropertyMetadata(MapStyle.Roads));

        Dictionary<TileCoordinate, UIElement> displays = new Dictionary<TileCoordinate, UIElement>();
        Queue<Func<object>> workQueue = new Queue<Func<object>>();

        public MapStyle MapStyle
        {
            get { return (MapStyle)GetValue(MapStyleProperty); }
            set { SetValue(MapStyleProperty, value); }
        }

        UIElement CreateChild(TileCoordinate tile)
        {
            string ft = ".jpg";
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
            VirtualEarthTileSystem.PixelXYToTileXY((int)pixels.X, (int)pixels.Y, out tileX, out tileY);
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
}
