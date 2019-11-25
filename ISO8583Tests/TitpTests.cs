using ISO8583Net.Field;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using ISO8583Net.Utilities;
using System.Linq;
using ISO8583Net.Packager;
using Microsoft.Extensions.Logging;
using ISO8583Net.Message;

namespace ISO8583Tests
{
    public class TitpTests
    {
        private ILogger logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<BitmapTests>();



        [Theory]
        [InlineData("4598890000000004")]
        [InlineData("45988900000000041")]
        [InlineData("459889000000000")]
        [InlineData("4598890000000004123")]
        [InlineData("459889000000")]
        public void CardNumberIsEncodedCorrectly(string cardNumber)
        {
            var mPackager = new ISOMessagePackager(logger, "titp.xml"); // initialize from default visa packager that is embeded as a resource in the library
            var m = new ISOMessage(logger, mPackager);
            m.Set(0, "0100");
            m.Set(2, cardNumber);
            m.Set(3, "000000");
            var packed = m.Pack();
            m.UnPack(packed);
            var hexBytes = ISOUtils.Bytes2Hex(packed);
            //                         header(TPDU)   msgtype  bitmap
            string expectedHexBytes = "0000000000" + "0100" + "6000000000000000";

            if (cardNumber.Length % 2 != 0)
            {
                expectedHexBytes += cardNumber.Length.ToString().PadLeft(2, '0') + cardNumber + "F" + "000000";
            }
            else
            {
                expectedHexBytes += cardNumber.Length.ToString().PadLeft(2, '0') + cardNumber + "000000";
            }
            
            Assert.Equal(cardNumber, m.GetFieldValue(2));
            Assert.Equal(expectedHexBytes, hexBytes);
        }

        [Theory]
        [InlineData("123456")]
        [InlineData("12345678")]
        public void TerminalIdIsEncodedCorrectly(string terminalId)
        {
            var mPackager = new ISOMessagePackager(logger, "titp.xml"); // initialize from default visa packager that is embeded as a resource in the library
            var m = new ISOMessage(logger, mPackager);
            m.Set(0, "0100");
            m.Set(2, "4598890000000004");
            m.Set(3, "000000");
            m.Set(41, terminalId);
            var packed = m.Pack();            
            var hexBytes = ISOUtils.Bytes2Hex(packed);
            m.UnPack(packed);
            //                         header(TPDU)   msgtype  bitmap
            string expectedHexBytes = "0000000000" + "0100" + "6000000000800000" + "164598890000000004" + "000000" + ToASCIIHexString(terminalId.PadRight(8, ' '));


            Assert.Equal(terminalId.PadRight(8, ' '), m.GetFieldValue(41));
            Assert.Equal(expectedHexBytes, hexBytes);
        }

        private string ToASCIIHexString(string value)
        {
            string result = "";
            for (int i = 0; i < value.Length; i++)
            {
                result += Convert.ToString((byte)value[i], 16);
            }

            return result;
        }
    }
}
