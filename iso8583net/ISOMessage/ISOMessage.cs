﻿using ISO8583Net.Field;
using ISO8583Net.Header;
using ISO8583Net.Packager;
using ISO8583Net.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace ISO8583Net.Message
{
    /// <summary>
    /// 
    /// </summary>
    public class ISOMessage : ISOComponent
    {
        /// <summary>
        /// 
        /// </summary>
        protected ISOMessagePackager m_isoMesssagePackager;
        /// <summary>
        /// 
        /// </summary>
        protected ISOMessageFields m_isoMessageFields;
        /// <summary>
        /// 
        /// </summary>
        protected ISOHeaderPackager m_isoHeaderPackager = null;
        /// <summary>
        /// 
        /// </summary>
        protected ISOHeader m_isoHeader = null;
        /// <summary>
        /// 
        /// </summary>
        protected int m_totalFields;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="messagePackager"></param>
        /// <param name="isoHeaderPackager"></param>
        //public ISOMessage(ILogger logger, ISOMessagePackager messagePackager, ISOHeaderPackager isoHeaderPackager) : base(logger, 0)
        //{
        //    m_isoMesssagePackager = messagePackager;

        //    m_isoMessageFields = new ISOMessageFields(Logger, m_isoMesssagePackager.GetISOMessageFieldsPackager(), 0);

        //    m_isoHeader = ISOPackagerLoader.GetMessageHeader(m_isoMesssagePackager.GetISOMessageFieldsPackager().HeaderPackager, logger);
        //    // based on isoHeaderPackager storage class initialize the correct ISOHeader
        //    //m_isoHeaderPackager = isoHeaderPackager;
        //    //m_isoHeader = new ISOHeaderVisa(Logger, m_isoHeaderPackager);

        //    m_totalFields = ((ISOMessagePackager)m_isoMesssagePackager).GetTotalFields();
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="isoMessagePackager"></param>
        public ISOMessage(ILogger logger, ISOMessagePackager isoMessagePackager) : base(logger, 0) 
        {
            m_isoMesssagePackager = isoMessagePackager;

            m_isoMessageFields = new ISOMessageFields(Logger, m_isoMesssagePackager.GetISOMessageFieldsPackager(), 0);

            var headerResult = ISOPackagerLoader.GetMessageHeaderAndPackager(isoMessagePackager.GetISOMessageFieldsPackager().HeaderPackager, Logger);
            m_isoHeader = headerResult.Header;
            m_isoHeaderPackager = headerResult.Packager;

            //m_isoHeaderPackager =  new ISOHeaderVisaPackager(Logger);
            // based on isoHeaderPackager storage class initialize the correct ISOHeader
            //m_isoHeader = new ISOHeaderVisa(Logger, m_isoHeaderPackager);

            m_totalFields = (m_isoMesssagePackager).GetTotalFields();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldNumber"></param>
        /// <param name="fieldValue"></param>
        public override void Set(int fieldNumber, string fieldValue)
        {
            if (fieldNumber >= 0 && fieldNumber <= m_totalFields) 
            {
                m_isoMessageFields.Set(fieldNumber, fieldValue);
            }
            else
            {
                Logger.LogError("Attempt to set value for an out of range field[", fieldNumber.ToString().PadLeft(3, ' ') + "]"); 
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldNumber"></param>
        /// <param name="subFieldNumber"></param>
        /// <param name="fieldValue"></param>
        public void Set(int fieldNumber, int subFieldNumber, string fieldValue)
        {
            if (fieldNumber >= 0 && fieldNumber <= m_totalFields && fieldNumber != 65 && fieldNumber != 129)
            {
                m_isoMessageFields.SetValue(fieldNumber, subFieldNumber, fieldValue);                
            }
            else
            {
                Logger.LogError("Attempt to set value for an out of range field[", fieldNumber.ToString().PadLeft(3, ' ') + "]");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldNumber"></param>
        /// <returns></returns>
        public override string GetFieldValue(int fieldNumber)
        {
            return m_isoMessageFields.GetFieldValue(fieldNumber);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldNumber"></param>
        /// <returns></returns>
        public ISOComponent GetField(int fieldNumber)
        {
            return m_isoMessageFields.GetField(fieldNumber);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldNumber"></param>
        /// <param name="subField"></param>
        /// <returns></returns>
        public override string GetFieldValue(int fieldNumber, int subField)
        {
            return m_isoMessageFields.GetFieldValue(subField);
        }
        
        /// <summary>
        /// Pack message using temporaty packedBytes, returns new byte array of final result
        /// </summary>
        /// <param name="packedBytes"></param>
        /// <returns></returns>
        protected byte[] Pack(byte[] packedBytes)
        {
            int index = 0;
            // if there is ISOMessage Header try pack it
            if (m_isoHeaderPackager != null && m_isoHeader != null)
            {
                //start packing the message after the header position
                index = m_isoHeader.Length();
            }
            // pack the isoMessage - is without message header
            m_isoMesssagePackager.Pack(m_isoMessageFields, packedBytes, ref index);

            // if there is ISOMessage Header try pack it
            if (m_isoHeaderPackager != null && m_isoHeader != null)
            {
                // set total message legnth in header
                m_isoHeader.SetMessageLength(index);

                // pack the isoHeader of the isoMessage
                int headerIndex = 0;
                m_isoHeaderPackager.Pack(m_isoHeader, packedBytes, ref headerIndex);                
            }
           
            return packedBytes.AsSpan(0, index).ToArray();

        }
        /// <summary>
        /// Pack message using an ArrayPool
        /// </summary>
        /// <returns></returns>
        public byte[] PackPooled()
        {
            byte[] tmpBuffer = System.Buffers.ArrayPool<byte>.Shared.Rent(2048);
            try
            {

                return Pack(tmpBuffer);
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(tmpBuffer);
            }

        }
        /// <summary>
        /// Pack Message
        /// </summary>
        /// <returns></returns>
        public byte[] Pack()
        {
            byte[] packedBytes = new byte[2048];

            return Pack(packedBytes);

        }

        /// <summary>
        /// Unpack Message
        /// </summary>
        /// <param name="packedBytes"></param>
        public void UnPack(ReadOnlySpan<byte> packedBytes)
        {
            int index = 0;

            // if there is ISOMessage Header then try unpack it
            if (m_isoHeaderPackager != null && m_isoHeader != null)
            {
                // unpack the isoHeader
                m_isoHeaderPackager.UnPack(m_isoHeader, packedBytes, ref index);
            }

            // unpack the isoMessage
            m_isoMesssagePackager.UnPack(m_isoMessageFields, packedBytes, ref index);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder msgFieldValues = new StringBuilder();

            msgFieldValues.Append("ISO Message Content: \n");

            msgFieldValues.Append(m_isoMessageFields.ToString());

            return msgFieldValues.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Trace()
        {
            m_isoMessageFields.Trace();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ISOMessage ToCommonISO()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isoMessage"></param>
        public void FromCommonISO(ISOMessage isoMessage)
        {
            throw new NotImplementedException();
        }
    }
}
