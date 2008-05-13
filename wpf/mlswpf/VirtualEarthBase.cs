//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Tools.MapBrowser
{
    using System.Windows;
    using System.Windows.Media;

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
}
