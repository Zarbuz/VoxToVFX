namespace Assets.VoxToVFXFramework.Scripts.Common
{
    public struct IntVector3
    {
        public IntVector3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static IntVector3 operator -(IntVector3 value)
        {
            return new IntVector3(-value.x, -value.y, -value.z);
        }
        public static IntVector3 operator -(IntVector3 value1, IntVector3 value2)
        {
            return new IntVector3(value1.x - value2.x, value1.y - value2.y, value1.z - value2.z);
        }
        public static IntVector3 operator *(int scaleFactor, IntVector3 value)
        {
            return new IntVector3(scaleFactor * value.x, scaleFactor * value.y, scaleFactor * value.z);
        }
        public static IntVector3 operator *(IntVector3 value, int scaleFactor)
        {
            return new IntVector3(value.x * scaleFactor, value.y * scaleFactor, value.z * scaleFactor);
        }
        public static IntVector3 operator *(IntVector3 value1, IntVector3 value2)
        {
            return new IntVector3(value1.x * value2.x, value1.y * value2.y, value1.z * value2.z);
        }
        public static IntVector3 operator /(IntVector3 value, int divider)
        {
            return new IntVector3(value.x / divider, value.y / divider, value.z / divider);
        }
        public static IntVector3 operator /(IntVector3 value1, IntVector3 value2)
        {
            return new IntVector3(value1.x / value2.x, value1.y / value2.y, value1.z / value2.z);
        }
        public static IntVector3 operator +(IntVector3 value1, IntVector3 value2)
        {
            return new IntVector3(value1.x + value2.x, value1.y + value2.y, value1.z + value2.z);
        }
        public static bool operator ==(IntVector3 value1, IntVector3 value2)
        {
            return value1.x == value2.x && value1.y == value2.y && value1.z == value2.z;
        }
        public static bool operator !=(IntVector3 value1, IntVector3 value2)
        {
            return value1.x != value2.x || value1.y != value2.y || value1.z != value2.z;
        }
        public static IntVector3 Max(IntVector3 value1, IntVector3 value2)
        {
            return new IntVector3(System.Math.Max(value1.x, value2.x), System.Math.Max(value1.y, value2.y), System.Math.Max(value1.z, value2.z));
        }
        public static IntVector3 Min(IntVector3 value1, IntVector3 value2)
        {
            return new IntVector3(System.Math.Min(value1.x, value2.x), System.Math.Min(value1.y, value2.y), System.Math.Min(value1.z, value2.z));
        }
        public static IntVector3 zero { get { return new IntVector3(0, 0, 0); } }
        public static IntVector3 one { get { return new IntVector3(1, 1, 1); } }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }
        public override bool Equals(System.Object obj)
        {
            if (!(obj is IntVector3)) return false;
            IntVector3 data = (IntVector3)obj;
            return this == data;
        }

        public int x, y, z;
    }
}
