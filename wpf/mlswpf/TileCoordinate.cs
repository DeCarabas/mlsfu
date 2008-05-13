//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Tools.MapBrowser
{
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
