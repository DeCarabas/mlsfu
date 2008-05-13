//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Tools.MapBrowser
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    public class MapDocument
    {
        ObservableCollection<MapItem> items = new ObservableCollection<MapItem>();

        public MapDocument()
        {
            Items.Add(new MapItem()
            {
                Latitude = 47.639073,
                Longitude = -122.133712,
                Content = new Button() { Content = "Building 42" }
            });
            Items.Add(new MapItem()
            {
                Latitude = 34.0540,
                Longitude = -118.2370,
                Content = new Button() { Content = "LA" }
            });
        }

        public ObservableCollection<MapItem> Items { get { return items; } }
    }

    public class MapItem : ContentControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty LatitudeProperty =
            DependencyProperty.Register(
                "Latitude",
                typeof(double),
                typeof(MapItem));

        public static readonly DependencyProperty LongitudeProperty =
            DependencyProperty.Register(
                "Longitude",
                typeof(double),
                typeof(MapItem));

        public double Latitude
        {
            get { return (double)GetValue(LatitudeProperty); }
            set { SetValue(LatitudeProperty, value); }
        }

        public double Longitude
        {
            get { return (double)GetValue(LongitudeProperty); }
            set { SetValue(LongitudeProperty, value); }
        }

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
