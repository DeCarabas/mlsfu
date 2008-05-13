//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Tools.MapBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.IO;
    using System.Windows.Data;
    using System.Globalization;
    using System.Windows.Media.Imaging;
    using System.Runtime.InteropServices;
    using System.Windows.Input;
    using System.Windows.Shell;
    using System.Windows.Media;
    using System.Windows.Interop;
    using Microsoft.Win32;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Collections.ObjectModel;

    [ComponentPart(CreationPolicy.Factory)]
    [Export("{OzSuite}Map")]
    [ExportProperty("{OzSuite}DisplayName", "map")]
    [ExportProperty("{OzSuite}Category", "Application")]
    public class MapDocument
    {
        ObservableCollection<MapItem> items = new ObservableCollection<MapItem>();

        public MapDocument()
        {
            Items.Add(new MapItem() { Latitude = 47.639073, Longitude = -122.133712, Content = new Button() { Content = "Building 42" } });
            Items.Add(new MapItem() { Latitude = 34.0540, Longitude = -118.2370, Content = new Button() { Content = "LA" } });
        }

        public ObservableCollection<MapItem> Items { get { return items; } }
    }

    public class MapItem : ContentControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty LatitudeProperty =
            DependencyProperty.Register("Latitude",
                                                typeof(double),
                                                typeof(MapItem));

        public static readonly DependencyProperty LongitudeProperty =
            DependencyProperty.Register("Longitude",
                                                typeof(double),
                                                typeof(MapItem));

        public double Latitude { get { return (double)GetValue(LatitudeProperty); } set { SetValue(LatitudeProperty, value); } }
        public double Longitude { get { return (double)GetValue(LongitudeProperty); } set { SetValue(LongitudeProperty, value); } }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == LatitudeProperty) { Notify("Latitude"); }
            if (e.Property == LongitudeProperty) { Notify("Longitude"); }
        }
        void Notify(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
