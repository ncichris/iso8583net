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
    
    public class PackUnpackTests
    {
        private ILogger logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<PackUnpackTests>();
        [Theory]
        [InlineData(new byte[] { 0x16 })]
        [InlineData(new byte[] { 0x16, 0x16 })]
        public void Bytes2IntWorks(byte[] data)
        {            
            int index = 0;
            int value = ISOUtils.Bytes2Int(data, ref index, data.Length * 2);
            if (data.Length == 1)
                Assert.Equal(0x16, value);
            if (data.Length == 2)
                Assert.Equal((0x16 * 0x100) + 0x16, value);
        }

        [Theory]
        [InlineData(new byte[] { 0x16 })]
        [InlineData(new byte[] { 0x16, 0x16 })]
        public void BytesBcd2IntWorks(byte[] data)
        {            
            int index = 0;
            int value = ISOUtils.BytesBcd2Int(data, ref index, data.Length * 2);
            
            if (data.Length == 1)
                Assert.Equal(16, value);
            if (data.Length == 2)
                Assert.Equal((16 * 100) + 16, value);
        }

        [Theory]
        [InlineData(0x16)]
        [InlineData(0x1616)]
        public void Int2BytesWorks(int data)
        {
            int index = 0;
            byte[] packedBytes = new byte[2]; 
            ISOUtils.Int2Bytes(data, packedBytes, ref index, data.ToString().Length);
            if (data.ToString().Length == 2)
                Assert.Equal(0x16, packedBytes[0]);
            if (data.ToString().Length == 4)
            {
                Assert.Equal(0x16, packedBytes[0]);
                Assert.Equal(0x16, packedBytes[1]);
            }
        }

        [Theory]
        [InlineData(16)]
        [InlineData(1616)]
        [InlineData(2345)]        
        public void Bcd2BytesWorks(int data)
        {
            int index = 0;
            byte[] packedBytes = new byte[2];
            ISOUtils.Bcd2Bytes(data, packedBytes, ref index, data.ToString().Length);
            if (data.ToString() == "16")
                Assert.Equal(0x16, packedBytes[0]);
            else if (data.ToString() == "1616")
            {
                Assert.Equal(0x16, packedBytes[0]);
                Assert.Equal(0x16, packedBytes[1]);
            }
            else
            {
                Assert.Equal(0x23, packedBytes[0]);
                Assert.Equal(0x45, packedBytes[1]);
            }
        }
        [Fact]
        public void ParseVisaMessage()
        {
            var mPackager = new ISOMessagePackager(logger); // initialize from default visa packager that is embeded as a resource in the library
            var m = new ISOMessage(logger, mPackager);
            /*
             * i000:[0110]
                i002:[4598890000000004]
                i003:[000000]
                i004:[000000004000]
                i007:[1028080759]
                i011:[000001]
                i012:[100759]
                i013:[1028]
                i019:[196]
                i025:[00]
                i032:[123456]
                i037:[000000000001]
                i039:[00]
                i041:[00002638]
                i042:[000000000219854]
                i049:[978]

             */
            m.Set(0, "0110");
            m.Set(2, "4598890000000004");
            m.Set(3, "000000");
            m.Set(4, "000000004000");
            m.Set(7, "1028080759");
            m.Set(11, "000001");
            m.Set(12, "100759");
            m.Set(13, "1028");
            m.Set(19, "196");            
            m.Set(25, "00");
            m.Set(32, "123456");
            m.Set(37, "000000000001");
            m.Set(39, "00");
            m.Set(41, "00002638");
            m.Set(42, "000000000219854");
            m.Set(49, "978");

            var messagePacked = m.Pack();

            var hexBytes = ISOUtils.Bytes2Hex(messagePacked);
            var newMessagePacked = ISOUtils.HexToByteArray(hexBytes);
            Assert.Equal(messagePacked, newMessagePacked);
            //                        "160000006D00000000000000000000000000000000000110723820810AC080001045988900000000040000000000000040001028080759000001100759102801960006123456F0F0F0F0F0F0F0F0F0F0F0F1F0F0F0F0F0F0F2F6F3F8F0F0F0F0F0F0F0F0F0F2F1F9F8F5F40978";
            string hexBytesExpected = "160000006D00000000000000000000000000000000000110723820810AC080001045988900000000040000000000000040001028080759000001100759102801960006123456F0F0F0F0F0F0F0F0F0F0F0F1F0F0F0F0F0F0F2F6F3F8F0F0F0F0F0F0F0F0F0F2F1F9F8F5F40978";
            Assert.Equal(hexBytesExpected, hexBytes);
            
            var newMessage = new ISOMessage(logger, mPackager);
            newMessage.UnPack(messagePacked);

            Assert.Equal(m.GetFieldValue(0), newMessage.GetFieldValue(0));
            Assert.Equal(m.GetFieldValue(2), newMessage.GetFieldValue(2));
            Assert.Equal(m.GetFieldValue(3), newMessage.GetFieldValue(3));
            Assert.Equal(m.GetFieldValue(4), newMessage.GetFieldValue(4));
            Assert.Equal(m.GetFieldValue(7), newMessage.GetFieldValue(7));
            Assert.Equal(m.GetFieldValue(11), newMessage.GetFieldValue(11));
            Assert.Equal(m.GetFieldValue(12), newMessage.GetFieldValue(12));
            Assert.Equal(m.GetFieldValue(13), newMessage.GetFieldValue(13));
            Assert.Equal(m.GetFieldValue(19), newMessage.GetFieldValue(19));            
            Assert.Equal(m.GetFieldValue(25), newMessage.GetFieldValue(25));
            Assert.Equal(m.GetFieldValue(32), newMessage.GetFieldValue(32));
            Assert.Equal(m.GetFieldValue(37), newMessage.GetFieldValue(37));
            Assert.Equal(m.GetFieldValue(39), newMessage.GetFieldValue(39));
            Assert.Equal(m.GetFieldValue(41), newMessage.GetFieldValue(41));
            Assert.Equal(m.GetFieldValue(42), newMessage.GetFieldValue(42));
            Assert.Equal(m.GetFieldValue(49), newMessage.GetFieldValue(49));

        }
    }
}
