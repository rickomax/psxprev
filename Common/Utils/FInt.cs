using System;

namespace PSXPrev.Common.Utils
{
    public struct FInt
    {
        public long RawValue;
        public const int SHIFT_AMOUNT = 12; //12 is 4096

        public const long One = 1 << SHIFT_AMOUNT;
        public const int OneI = 1 << SHIFT_AMOUNT;
        public static FInt OneF = FInt.Create(1, true);

        #region Constructors
        public static FInt Create(long StartingRawValue, bool UseMultiple)
        {
            FInt fInt;
            fInt.RawValue = StartingRawValue;
            if (UseMultiple)
                fInt.RawValue = fInt.RawValue << SHIFT_AMOUNT;
            return fInt;
        }
        public static FInt Create(double DoubleValue)
        {
            FInt fInt;
            DoubleValue *= (double)One;
            fInt.RawValue = (int)Math.Round(DoubleValue);
            return fInt;
        }
        #endregion

        public int IntValue
        {
            get { return (int)(this.RawValue >> SHIFT_AMOUNT); }
        }

        public int ToInt()
        {
            return (int)(this.RawValue >> SHIFT_AMOUNT);
        }

        public double ToDouble()
        {
            return (double)this.RawValue / (double)One;
        }

        public FInt Inverse
        {
            get { return FInt.Create(-this.RawValue, false); }
        }

        #region FromParts
        /// <summary>
        /// Create a fixed-int number from parts.  For example, to create 1.5 pass in 1 and 500.
        /// </summary>
        /// <param name="PreDecimal">The number above the decimal.  For 1.5, this would be 1.</param>
        /// <param name="PostDecimal">The number below the decimal, to three digits.  
        /// For 1.5, this would be 500. For 1.005, this would be 5.</param>
        /// <returns>A fixed-int representation of the number parts</returns>
        public static FInt FromParts(int PreDecimal, int PostDecimal)
        {
            FInt f = FInt.Create(PreDecimal, true);
            if (PostDecimal != 0)
                f.RawValue += (FInt.Create(PostDecimal) / 1000).RawValue;

            return f;
        }
        #endregion

        #region *
        public static FInt operator *(FInt one, FInt other)
        {
            FInt fInt;
            fInt.RawValue = (one.RawValue * other.RawValue) >> SHIFT_AMOUNT;
            return fInt;
        }

        public static FInt operator *(FInt one, int multi)
        {
            return one * (FInt)multi;
        }

        public static FInt operator *(int multi, FInt one)
        {
            return one * (FInt)multi;
        }
        #endregion

        #region /
        public static FInt operator /(FInt one, FInt other)
        {
            FInt fInt;
            fInt.RawValue = (one.RawValue << SHIFT_AMOUNT) / (other.RawValue);
            return fInt;
        }

        public static FInt operator /(FInt one, int divisor)
        {
            return one / (FInt)divisor;
        }

        public static FInt operator /(int divisor, FInt one)
        {
            return (FInt)divisor / one;
        }
        #endregion

        #region %
        public static FInt operator %(FInt one, FInt other)
        {
            FInt fInt;
            fInt.RawValue = (one.RawValue) % (other.RawValue);
            return fInt;
        }

        public static FInt operator %(FInt one, int divisor)
        {
            return one % (FInt)divisor;
        }

        public static FInt operator %(int divisor, FInt one)
        {
            return (FInt)divisor % one;
        }
        #endregion

        #region +
        public static FInt operator +(FInt one, FInt other)
        {
            FInt fInt;
            fInt.RawValue = one.RawValue + other.RawValue;
            return fInt;
        }

        public static FInt operator +(FInt one, int other)
        {
            return one + (FInt)other;
        }

        public static FInt operator +(int other, FInt one)
        {
            return one + (FInt)other;
        }
        #endregion

        #region -
        public static FInt operator -(FInt one, FInt other)
        {
            FInt fInt;
            fInt.RawValue = one.RawValue - other.RawValue;
            return fInt;
        }

        public static FInt operator -(FInt one, int other)
        {
            return one - (FInt)other;
        }

        public static FInt operator -(int other, FInt one)
        {
            return (FInt)other - one;
        }
        #endregion

        #region ==
        public static bool operator ==(FInt one, FInt other)
        {
            return one.RawValue == other.RawValue;
        }

        public static bool operator ==(FInt one, int other)
        {
            return one == (FInt)other;
        }

        public static bool operator ==(int other, FInt one)
        {
            return (FInt)other == one;
        }
        #endregion

        #region !=
        public static bool operator !=(FInt one, FInt other)
        {
            return one.RawValue != other.RawValue;
        }

        public static bool operator !=(FInt one, int other)
        {
            return one != (FInt)other;
        }

        public static bool operator !=(int other, FInt one)
        {
            return (FInt)other != one;
        }
        #endregion

        #region >=
        public static bool operator >=(FInt one, FInt other)
        {
            return one.RawValue >= other.RawValue;
        }

        public static bool operator >=(FInt one, int other)
        {
            return one >= (FInt)other;
        }

        public static bool operator >=(int other, FInt one)
        {
            return (FInt)other >= one;
        }
        #endregion

        #region <=
        public static bool operator <=(FInt one, FInt other)
        {
            return one.RawValue <= other.RawValue;
        }

        public static bool operator <=(FInt one, int other)
        {
            return one <= (FInt)other;
        }

        public static bool operator <=(int other, FInt one)
        {
            return (FInt)other <= one;
        }
        #endregion

        #region >
        public static bool operator >(FInt one, FInt other)
        {
            return one.RawValue > other.RawValue;
        }

        public static bool operator >(FInt one, int other)
        {
            return one > (FInt)other;
        }

        public static bool operator >(int other, FInt one)
        {
            return (FInt)other > one;
        }
        #endregion

        #region <
        public static bool operator <(FInt one, FInt other)
        {
            return one.RawValue < other.RawValue;
        }

        public static bool operator <(FInt one, int other)
        {
            return one < (FInt)other;
        }

        public static bool operator <(int other, FInt one)
        {
            return (FInt)other < one;
        }
        #endregion

        public static explicit operator int(FInt src)
        {
            return (int)(src.RawValue >> SHIFT_AMOUNT);
        }

        public static explicit operator FInt(int src)
        {
            return FInt.Create(src, true);
        }

        public static explicit operator FInt(long src)
        {
            return FInt.Create(src, true);
        }

        public static explicit operator FInt(ulong src)
        {
            return FInt.Create((long)src, true);
        }

        public static FInt operator <<(FInt one, int Amount)
        {
            return FInt.Create(one.RawValue << Amount, false);
        }

        public static FInt operator >>(FInt one, int Amount)
        {
            return FInt.Create(one.RawValue >> Amount, false);
        }

        public override bool Equals(object obj)
        {
            if (obj is FInt)
                return ((FInt)obj).RawValue == this.RawValue;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return RawValue.GetHashCode();
        }

        public override string ToString()
        {
            return this.RawValue.ToString();
        }
    }
}
