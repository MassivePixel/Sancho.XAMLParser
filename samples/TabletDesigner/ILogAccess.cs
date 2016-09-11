// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

namespace TabletDesigner
{
    public interface ILogAccess
    {
        void Clear();
        string Log { get; }
    }
}
