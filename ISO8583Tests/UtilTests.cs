using Xunit;
using ISO8583Net.Utilities;

namespace ISO8583Tests
{
    public class UtilTests
    {
        [Fact]
        public void TestLookup32()
        {
            var first = CreateLookup32();
            var second = CreateLookup32Char();
            for (int i = 0; i < first.Length; i++)
            {
                Assert.Equal((char)first[i], second[i]);
            }
        }
        private static uint[] CreateLookup32()
        {
            var result = new uint[256];

            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");

                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }

            return result;
        }
        private static char[] CreateLookup32Char()
        {
            var result = new char[256];

            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");

                result[i] = (char)(s[0] + (s[1] << 16));
            }

            return result;
        }
        [Fact]
        public void Ascii2BcdWorks()
        {

            byte[] buffer = new byte[50];
            byte[] oldbuffer = new byte[50];
            int currentIndex = 0;
            ISOUtils.Ascii2Bcd("12341234", buffer, ref currentIndex, ISO8583Net.Types.ISOFieldPadding.LEFT);
            currentIndex = 0;
            ISOUtils.Ascii2BcdOld("12341234", oldbuffer, ref currentIndex, ISO8583Net.Types.ISOFieldPadding.LEFT);
            Compare(buffer, oldbuffer, currentIndex);
            currentIndex = 0;
            string asciiResult = ISOUtils.Bcd2Ascii(buffer, ref currentIndex, ISO8583Net.Types.ISOFieldPadding.LEFT, "12341234".Length);
            Assert.Equal("12341234", asciiResult);

            currentIndex = 0;
            ISOUtils.Ascii2Bcd("12341234", buffer, ref currentIndex, ISO8583Net.Types.ISOFieldPadding.RIGHT);
            currentIndex = 0;
            ISOUtils.Ascii2BcdOld("12341234", oldbuffer, ref currentIndex, ISO8583Net.Types.ISOFieldPadding.RIGHT);
            Compare(buffer, oldbuffer, currentIndex);
            currentIndex = 0;
            asciiResult = ISOUtils.Bcd2Ascii(buffer, ref currentIndex, ISO8583Net.Types.ISOFieldPadding.RIGHT, "12341234".Length);
            Assert.Equal("12341234", asciiResult);

            currentIndex = 0;
            ISOUtils.Ascii2Bcd("1234123", buffer, ref currentIndex, ISO8583Net.Types.ISOFieldPadding.LEFT);
            currentIndex = 0;
            ISOUtils.Ascii2BcdOld("1234123", oldbuffer, ref currentIndex, ISO8583Net.Types.ISOFieldPadding.LEFT);
            Compare(buffer, oldbuffer, currentIndex);
            currentIndex = 0;
            asciiResult = ISOUtils.Bcd2Ascii(buffer, ref currentIndex, ISO8583Net.Types.ISOFieldPadding.LEFT, "1234123".Length);
            Assert.Equal("1234123", asciiResult);


            currentIndex = 0;
            ISOUtils.Ascii2Bcd("1234123", buffer, ref currentIndex, ISO8583Net.Types.ISOFieldPadding.RIGHT);
            currentIndex = 0;
            ISOUtils.Ascii2BcdOld("1234123", oldbuffer, ref currentIndex, ISO8583Net.Types.ISOFieldPadding.RIGHT);
            Compare(buffer, oldbuffer, currentIndex);
            currentIndex = 0;
            asciiResult = ISOUtils.Bcd2Ascii(buffer, ref currentIndex, ISO8583Net.Types.ISOFieldPadding.RIGHT, "1234123".Length);
            Assert.Equal("1234123", asciiResult);
        }

        private static void Compare(byte[] newbuf, byte[] oldbuf, int length)
        {
            for (int i = 0; i < length; i++)
            {
                Assert.Equal(newbuf[i], oldbuf[i]);                    
            }
        }
    }
}
