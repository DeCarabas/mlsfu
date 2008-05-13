//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Tools.MapBrowser
{
    using System.Windows;
    using System.Windows.Controls;

    public class WorldPanel : Panel
    {
        public static readonly DependencyProperty LatitudeProperty =
            DependencyProperty.RegisterAttached("Latitude",
                                                typeof(double),
                                                typeof(WorldPanel));

        public static readonly DependencyProperty LongitudeProperty =
            DependencyProperty.RegisterAttached("Longitude",
                                                typeof(double),
                                                typeof(WorldPanel));

        public static double GetLatitude(UIElement target) { return (double)target.GetValue(LatitudeProperty); }
        public static void SetLatitude(UIElement target, double value) { target.SetValue(LatitudeProperty, value); }
        public static double GetLongitude(UIElement target) { return (double)target.GetValue(LongitudeProperty); }
        public static void SetLongitude(UIElement target, double value) { target.SetValue(LongitudeProperty, value); }

        public static readonly DependencyProperty CornerLatitudeProperty = DependencyProperty.Register(
            "CornerLatitude",
            typeof(double),
            typeof(WorldPanel),
            new FrameworkPropertyMetadata(85.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty CornerLongitudeProperty = DependencyProperty.Register(
            "CornerLongitude",
            typeof(double),
            typeof(WorldPanel),
            new FrameworkPropertyMetadata(-180.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ViewModeProperty = DependencyProperty.Register(
            "ViewMode",
            typeof(WorldPanelViewMode),
            typeof(WorldPanel),
            new FrameworkPropertyMetadata(WorldPanelViewMode.Center));
        public static readonly DependencyProperty ViewLatitudeProperty = DependencyProperty.Register(
            "ViewLatitude",
            typeof(double),
            typeof(WorldPanel),
            new FrameworkPropertyMetadata(double.NaN));
        public static readonly DependencyProperty ViewLongitudeProperty = DependencyProperty.Register(
            "ViewLongitude",
            typeof(double),
            typeof(WorldPanel),
            new FrameworkPropertyMetadata(double.NaN));

        public WorldPanelViewMode ViewMode { get { return (WorldPanelViewMode)GetValue(ViewModeProperty); } set { SetValue(ViewModeProperty, value); } }
        public double CornerLatitude { get { return (double)GetValue(CornerLatitudeProperty); } set { SetValue(CornerLatitudeProperty, value); } }
        public double CornerLongitude { get { return (double)GetValue(CornerLongitudeProperty); } set { SetValue(CornerLongitudeProperty, value); } }
        public double ViewLatitude { get { return (double)GetValue(ViewLatitudeProperty); } set { SetValue(ViewLatitudeProperty, value); } }
        public double ViewLongitude { get { return (double)GetValue(ViewLongitudeProperty); } set { SetValue(ViewLongitudeProperty, value); } }

        // max:
        //double _metersPerPixel = 81920.0;
        int mapSize = 2;

        WorldCoordinate _upperLeft = WorldCoordinate.FromLatitudeLongitude(85, -180);
        WorldCoordinateTranslator _translator;

        public WorldPanel()
        {
        }

        public void LookAt(WorldCoordinate location)
        {
            ViewLatitude = location.Latitude;
            ViewLongitude = location.Longitude;
        }

        protected WorldCoordinate GetView()
        {
            object lat = ReadLocalValue(ViewLatitudeProperty);
            object lon = ReadLocalValue(ViewLongitudeProperty);
            WorldCoordinate center = ClientToWorld(new Point(ActualWidth / 2, ActualHeight / 2));

            if (lat != DependencyProperty.UnsetValue)
            {
                center.Latitude = (double)lat;
            }
            if (lon != DependencyProperty.UnsetValue)
            {
                center.Longitude = (double)lon;
            }
            return center;
        }

        void SetViewOnCenter()
        {
            if (!double.IsNaN(ViewLatitude) && !double.IsNaN(ViewLongitude))
            {
                WorldCoordinate lookAt = new WorldCoordinate() { Latitude = ViewLatitude, Longitude = ViewLongitude };
                lookAt.Clamp();

                Point absoluteLoc = Translator.CoordinateToPixels(lookAt);
                Point newCorner = new Point(absoluteLoc.X - ActualWidth / 2, absoluteLoc.Y - ActualHeight / 2);

                WorldCoordinate desiredUpperLeft = Translator.PixelsToCoordinates(newCorner);

                CornerLatitude = desiredUpperLeft.Latitude;
                CornerLongitude = desiredUpperLeft.Longitude;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            DependencyProperty dp = e.Property;
            if (dp == ViewLatitudeProperty
                || dp == ViewLongitudeProperty)
            {
                SetViewOnCenter();
            }
            else if (dp == CornerLatitudeProperty
                || dp == CornerLongitudeProperty)
            {
                if (CornerLongitude < -180)
                {
                    CornerLongitude = -180;
                }
                else if (CornerLongitude > 180)
                {
                    CornerLongitude = 180;
                }
                if (CornerLatitude < -85)
                {
                    CornerLatitude = -85;
                }
                else if (CornerLatitude > 85)
                {
                    CornerLatitude = 85;
                }
                _upperLeft = new WorldCoordinate() { Longitude = CornerLongitude, Latitude = CornerLatitude };
                InvalidateArrange();
            }
            base.OnPropertyChanged(e);
        }

        protected void UpdateMapSize(int mapSize)
        {
            this.mapSize = mapSize;
            _translator = null;
            if (ViewMode == WorldPanelViewMode.Center)
            {
                SetViewOnCenter();
            }
            InvalidateMeasure();
        }

        protected WorldCoordinateTranslator Translator
        {
            get
            {
                if (_translator == null)
                {
                    _translator = new WorldCoordinateTranslator() { MapSize = mapSize };
                }
                return _translator;
            }
        }

        protected WorldCoordinate ClientToWorld(Point value)
        {
            Point offset = Translator.CoordinateToPixels(_upperLeft);
            WorldCoordinate world = Translator.PixelsToCoordinates(new Point(value.X + offset.X, value.Y + offset.Y));
            return world;
        }
        protected Point WorldToClient(WorldCoordinate value)
        {
            Point virtualLocation = Translator.CoordinateToPixels(value);
            Point offset = Translator.CoordinateToPixels(_upperLeft);
            return new Point(virtualLocation.X - offset.X, virtualLocation.Y - offset.Y);
        }

        protected override Size MeasureOverride(Size availableSize)
        {

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
            }
            if (double.IsPositiveInfinity(availableSize.Width)) { availableSize.Width = 500; }
            if (double.IsPositiveInfinity(availableSize.Height)) { availableSize.Height = 500; }
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                var wc = new WorldCoordinate()
                {
                    Latitude = GetLatitude(child),
                    Longitude = GetLongitude(child)
                };

                Point client = WorldToClient(wc);
                child.Arrange(new Rect(client, child.DesiredSize));
            }
            return finalSize;
        }

    }
    public class WorldCoordinateTranslator
    {
        public int MapSize { get; set; }

        public WorldCoordinate PixelsToCoordinates(Point pixel)
        {
            WorldCoordinate latLong = new WorldCoordinate();
            double lat, lon;
            VirtualEarthTileSystem.PixelXYToLatLong((int)pixel.X, (int)pixel.Y, MapSize, out lat, out lon);
            latLong.Latitude = lat;
            latLong.Longitude = lon;
            return latLong;
        }

        public Point CoordinateToPixels(WorldCoordinate coord)
        {
            int x, y;
            VirtualEarthTileSystem.LatLongToPixelXY(coord.Latitude, coord.Longitude, MapSize, out x, out y);
            return new Point(x, y);
        }
    }
    public enum WorldPanelViewMode
    {
        Center,
        TopLeft
    }
    public struct WorldCoordinate
    {
        double _latitude;
        double _longitude;

        public double Latitude { get { return _latitude; } set { _latitude = value; } }
        public double Longitude { get { return _longitude; } set { _longitude = value; } }

        public Point ToPoint()
        {
            return new Point(Longitude, Latitude);
        }

        public static WorldCoordinate FromLatitudeLongitude(double latitude, double longitude)
        {
            return new WorldCoordinate() { Longitude = longitude, Latitude = latitude };
        }

        public static WorldCoordinate FromPoint(Point pt)
        {
            return new WorldCoordinate() { Longitude = pt.X, Latitude = pt.Y };
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", Latitude, Longitude);
        }

        public void Clamp()
        {
            if (_latitude < -85) _latitude = -85;
            if (_latitude > 85) _latitude = 85;
            if (_longitude < -180) _longitude = -180;
            if (_longitude > 180) _longitude = 180;
        }
    }
}
