﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.utils
{
    public struct Dimension : IEquatable<Dimension>
    {
        public float Height;
        public float Width;

        #region IEquatable<Dimension> Members

        public bool Equals(Dimension other)
        {
            return Width == other.Width && Height == other.Height;
        }

        #endregion
    }
}