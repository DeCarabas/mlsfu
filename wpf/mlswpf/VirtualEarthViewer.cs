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
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;

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
}
