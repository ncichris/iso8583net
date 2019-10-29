﻿using ISO8583Net.Packager;
using ISO8583Net.Types;
using ISO8583Net.Utilities;
using Microsoft.Extensions.Logging;
using System;

namespace ISO8583Net.Header
{
    /// <summary>
    /// 
    /// </summary>
    public class ISOHeaderVisa : ISOHeader
    {
        public int m_length = 22;

        public string h01_HeaderLength { get; set; }                     // Byte 1         header len bytes   -   2HD

        public string h02_HeaderFlagAndFormat { get; set; }              // Byte 2         1B 8N Bit string   -   2HD

        public string h03_TextFormat { get; set; }                       // Byte 3         1B Binary          -   2HD

        public string h04_TotalMessageLength { get; set; }               // Bytes 4-5      2B Binary          -   4HD                                                          

        public string h05_DestinationStationId { get; set; }             // Bytes 6-8      3B 6 Numeric BCD   -   6N

        public string h06_SourceStationId { get; set; }                  // Bytes 9-11     3B 6 Numeric BCD   -   6N

        public string h07_RoundTripControlInformation { get; set; }      // Byte 12        1B 8Bit string     -   2HD

        public string h08_BaseIFlag { get; set; }                        // Bytes 13-14    2B 16Bit string    -   4HD

        public string h09_MessageStatusFlag { get; set; }                // Bytes 15-17    3B 24Bit string    -   6HD

        public string h10_BatchNumber { get; set; }                      // Bytes 18       1B Binary          -   2HD

        public string h11_Reserved { get; set; }                         // Byte 19-21     3B Binary          -   6HD

        public string h12_UserInformation { get; set; }                  // Byte 22        1B Binary          -   2HD

        public string h13_Bitmap { get; set; }                           // Byyte 23-24    2B Binary          -   4HD

        public string h14_RejectedGroupData { get; set; }                // Byyte 25-26    2B Binary          -   4HD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public ISOHeaderVisa(ILogger logger) : base (logger)
        {
            h01_HeaderLength = "00";
            h02_HeaderFlagAndFormat = "00";
            h03_TextFormat = "00";
            h04_TotalMessageLength = "0000";
            h05_DestinationStationId = "000000";
            h06_SourceStationId = "000000";
            h07_RoundTripControlInformation = "00";
            h08_BaseIFlag = "0000";
            h09_MessageStatusFlag = "000000";
            h10_BatchNumber = "00";
            h11_Reserved = "000000";
            h12_UserInformation = "00";
            h13_Bitmap = "0000";
            h14_RejectedGroupData = "0000";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="isoHeaderPackager"></param>
        public ISOHeaderVisa(ILogger logger, ISOHeaderPackager isoHeaderPackager) : base (logger)
        {
            h01_HeaderLength = "00";
            h02_HeaderFlagAndFormat = "00";
            h03_TextFormat = "00";
            h04_TotalMessageLength = "0000";
            h05_DestinationStationId = "000000";
            h06_SourceStationId = "000000";
            h07_RoundTripControlInformation = "00";
            h08_BaseIFlag = "0000";
            h09_MessageStatusFlag = "000000";
            h10_BatchNumber = "00";
            h11_Reserved = "000000";
            h12_UserInformation = "00";
            h13_Bitmap = "0000";
            h14_RejectedGroupData = "0000";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int Length()
        {
            return m_length;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        public override void SetMessageLength(int length)
        {
            // provision for leading zeros during conversion of length indicator
            h04_TotalMessageLength = (length).ToString("X4");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        public override void SetValue(byte[] bytes)
        {
            // Unpack should check for existense of Header Field 13 always
            int index = 0;

            //if (Logger.IsEnabled(LogLevel.Information)) Logger.LogInformation("Unpacking VISA Header");

            //Read header length
            string lenHex = ISOUtils.Bytes2Hex(bytes, ref index, 1);

            m_length = ISOUtils.Hex2Bytes(lenHex)[0]; 

            h02_HeaderFlagAndFormat = ISOUtils.Bytes2Hex(bytes, ref index, 1);

            h03_TextFormat = ISOUtils.Bytes2Hex(bytes, ref index, 1);

            h04_TotalMessageLength = ISOUtils.Bytes2Hex(bytes, ref index, 2);

            h05_DestinationStationId = ISOUtils.Bcd2Ascii(bytes, ref index, ISOFieldPadding.LEFT, 6);

            h06_SourceStationId = ISOUtils.Bcd2Ascii(bytes, ref index, ISOFieldPadding.LEFT, 6);

            h07_RoundTripControlInformation = ISOUtils.Bytes2Hex(bytes, ref index, 1);

            h08_BaseIFlag = ISOUtils.Bytes2Hex(bytes, ref index, 2);

            h09_MessageStatusFlag = ISOUtils.Bytes2Hex(bytes, ref index, 3);

            h10_BatchNumber = ISOUtils.Bytes2Hex(bytes, ref index, 1);

            h11_Reserved = ISOUtils.Bytes2Hex(bytes, ref index, 3);

            h12_UserInformation = ISOUtils.Bytes2Hex(bytes, ref index, 1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packedData"></param>
        /// <param name="index"></param>
        public override void Pack(byte[] packedData, ref int index)
        {
            // should never be called, I ll deal with it later
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packedBytes"></param>
        /// <param name="index"></param>
        public override void UnPack(byte[] packedBytes, ref int index)
        {
            // should never be called, I ll deal with it later
            throw new NotImplementedException();
        }
    }
}
