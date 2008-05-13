//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Tools.MapBrowser
{
    using System;

    static class VETSHelper
    {
        internal const int TileSizePixels = 256;

        internal static int MaxTiles(int quadLevel)
        {
            return (int)Math.Pow(2.0, (double)quadLevel);
        }
    }
}
