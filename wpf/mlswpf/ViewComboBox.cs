//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Tools.MapBrowser
{
    using System.Windows.Controls;

    public class ViewComboBox : ComboBox
    {
        public ViewComboBox()
        {
            ItemsSource = new MapStyle[] { MapStyle.Satalite, MapStyle.Roads, MapStyle.Both };
        }
    }
}
