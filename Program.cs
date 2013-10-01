namespace ByteArray
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// This class implements a byte array, such that, memory is not 
    /// wasted when data size is less than a byte
    /// </summary>
    /// <remarks>
    /// This implementation has a defect where it fails to 
    /// split the data bits and store it in adjacent byte array.
    /// Same applies for get data
    /// </remarks>
    public class ByteArray
    {
        private readonly byte[] array;

        private readonly int setMask;

        private readonly int getMask;

        private readonly int elementSize;

        private readonly int arraySize;

        private const int ByteLength = 8;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="elementSize">element size</param>
        /// <param name="arraySize">array size</param>
        public ByteArray(int elementSize, int arraySize)
        {
            Contract.Assert(elementSize <= ByteLength);

            this.arraySize = arraySize;
            this.elementSize = elementSize;

            this.array = new byte[(arraySize * elementSize / ByteLength) + 1];

            // Set MSB as 1's based on element size
            this.setMask = (int)(256 - (Math.Pow(2, (ByteLength - elementSize))));

            // Set LSB as 1's based on element size
            this.getMask = (1 << this.elementSize) - 1;
        }

        /// <summary>
        /// Set the byte data
        /// </summary>
        /// <param name="loc">data location</param>
        /// <param name="data">data in byte</param>
        public void Set(int loc, byte data)
        {
            if (loc > arraySize)
            {
                throw new IndexOutOfRangeException();
            }

            // calculate location offset
            var offSet = loc * this.elementSize;

            // calculate data shift count
            var mod = offSet % ByteLength;

            // calculate byte array index
            var index = offSet / 8;

            // right shift the source data 
            var source = data >> mod;

            // right shift the mask to clear not needed source data
            var mask = setMask >> mod;

            // set the data
            array[index] = (byte)((this.array[index] & ~mask) | (source & mask));
        }

        /// <summary>
        /// Get the byte data based on location
        /// </summary>
        /// <param name="loc">data location</param>
        /// <returns>byte data</returns>
        public byte Get(int loc)
        {
            if (loc > arraySize)
            {
                throw new IndexOutOfRangeException();
            }

            // calculate location offset
            var offSet = loc * this.elementSize;

            // Calculate the number of shifts required in the result byte array
            var mod = (offSet % 8) + this.elementSize;

            // calculate byte array index
            var index = offSet / 8;

            // Get the byte which contains the result
            byte result = this.array[index];

            // Circular shift the result data
            result = (byte)((result << mod) | (result >> 8 - mod));

            // Use getMask to remove unwanted data from result
            return (byte)(result & this.getMask);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Online decimal/binary converters
            // http://www.binaryhexconverter.com/binary-to-decimal-converter
            // http://www.binaryhexconverter.com/decimal-to-binary-converter

            // Few References: 
            // http://graphics.stanford.edu/~seander/bithacks.html#CopyIntegerSign
            // http://en.wikipedia.org/wiki/Bit_manipulation
            // Hacker's Delight book 

            var array = new ByteArray(4, 5);

            array.Set(0, 0);    // 00000000
            array.Set(1, 174);  // 10101110
            array.Set(2, 233);  // 11101001
            array.Set(3, 232);  // 11101000
            array.Set(2, 174);  // 10101110

            var r1 = array.Get(0); // 0  -> 00000000
            var r2 = array.Get(1); // 10 -> 00001010
            var r3 = array.Get(2); // 10 -> 00001010
            var r4 = array.Get(3); // 14 -> 00001110
        }
    }
}
