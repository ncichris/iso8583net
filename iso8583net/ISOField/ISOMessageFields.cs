﻿using ISO8583Net.Packager;
using ISO8583Net.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace ISO8583Net.Field
{
    /// <summary>
    /// 
    /// </summary>
    public class ISOMessageFields : ISOField
    {
        /// <summary>
        /// 
        /// </summary>
        protected ISOComponent[] m_isoFields;
        /// <summary>
        /// 
        /// </summary>

        protected ISOMessageFieldsPackager m_packager;
        /// <summary>
        /// 
        /// </summary>
        public override string value
        {
            get
            {
                StringBuilder strBuilder = new StringBuilder();

                int totalFields = m_packager.GetTotalFields();

                for (int i = 0; i < totalFields; i++)
                {
                    if (m_isoFields[i] != null)
                    {
                        string str = m_isoFields[i].value;

                        strBuilder.Append(str);
                    }
                }

                return strBuilder.ToString();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="packager"></param>
        /// <param name="fieldNumber"></param>
        public ISOMessageFields(ILogger logger, ISOMessageFieldsPackager packager, int fieldNumber) : base(logger, packager, fieldNumber)
        {
            m_packager = packager;

            m_isoFields = new ISOComponent[m_packager.GetTotalFields()];

            m_isoFields[1] = new ISOFieldBitmap(Logger, (ISOFieldPackager)packager.GetFieldPackager(1), 1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldNumber"></param>
        /// <param name="fieldValue"></param>
        public override void Set(int fieldNumber, string fieldValue)
        {
            if (m_isoFields[fieldNumber] != null)
            {
                m_isoFields[fieldNumber].value = fieldValue; // SetValue(fieldValue);
            }
            else
            {
                if (SetFieldPackager(fieldNumber))
                {
                    m_isoFields[fieldNumber].value = fieldValue; // SetValue(fieldValue);

                    if (fieldNumber > 0) 
                    {
                        ((ISOFieldBitmap)m_isoFields[1]).SetBit(fieldNumber); 
                    }
                }
                else
                {
                    Logger.LogError("Trying to set Field [" + fieldNumber + "] that dose not exist in packager definition file");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldNumber"></param>
        /// <param name="subFieldNumber"></param>
        /// <param name="fieldValue"></param>
        public void SetValue(int fieldNumber, int subFieldNumber, string fieldValue)
        {
            if (m_isoFields[fieldNumber] == null)
            {
                // field is not initialized yet in the dictionary so initialize it and set th

                if (SetFieldPackager(fieldNumber))
                {
                    ((ISOFieldBitmap)m_isoFields[1]).SetBit(fieldNumber);                    

                    m_isoFields[fieldNumber].Set(subFieldNumber, fieldValue);
                }
                else
                {
                    Logger.LogError("Trying to set SubField [" + subFieldNumber.ToString() + "] of Field [" + fieldNumber + "] that dose not exist in packager definition file");
                }
            }
            else
            {
                m_isoFields[fieldNumber].Set(subFieldNumber, fieldValue);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldNumber"></param>
        /// <returns></returns>
        public bool SetFieldPackager(int fieldNumber)
        {
            ISOPackager fieldPackager = m_packager.GetFieldPackager(fieldNumber);

            if (m_isoFields[fieldNumber] == null && fieldPackager != null) // field is not initialized and packager was intialzied from xml for this field
            {
                if (fieldPackager.IsComposite())
                {
                    // Logger.LogTrace("Field [" + fieldNumber.ToString().PadLeft(3, '0') + "] is composite    , set ISOPackager = ISOMessageSubFields");

                    m_isoFields[fieldNumber] = new ISOFieldBitmapSubFields(Logger, (ISOFieldBitmapSubFieldsPackager)fieldPackager, fieldNumber);

                    return true;
                }
                else
                {
                    // Logger.LogTrace("Field [" + fieldNumber.ToString().PadLeft(3, '0') + "] is NOT composite, set ISOPackager = ISOField");

                    m_isoFields[fieldNumber] = new ISOField(Logger, fieldPackager, fieldNumber);

                    return true;
                }
            }
            else
            {
                Logger.LogError("Field Packager was not initialized from XML Packager definition file");

                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldNumber"></param>
        /// <returns></returns>
        public ISOComponent GetField(int fieldNumber)
        {
            return m_isoFields[fieldNumber];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ISOComponent[] GetFields()
        {
            return m_isoFields;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldNumber"></param>
        /// <returns></returns>
        public override string GetFieldValue(int fieldNumber)
        {
            return m_isoFields[fieldNumber]?.value; 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldNumber"></param>
        /// <param name="subField"></param>
        /// <returns></returns>
        public override string GetFieldValue(int fieldNumber, int subField)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder msgFieldValues = new StringBuilder();
            var bitmap = m_isoFields[1] as ISOFieldBitmap;
            int[] setFields = bitmap.GetSetFields();
            
            for (int k = 0; k < setFields.Length; k++)
            {
                int i = setFields[k];
                if (m_isoFields[i] != null)
                {
                    msgFieldValues.Append(m_isoFields[i].ToString());
                }
            }
            //int length = m_packager.GetTotalFields();
            //for (int i = 0; i < length; i++)
            //{
            //    if (m_isoFields[i] != null && (bitmap.BitIsSet(i) || i==0 || i==1))
            //    {
            //        msgFieldValues.Append(m_isoFields[i].ToString());
            //    }
            //}
            return msgFieldValues.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Trace()
        {
            throw new NotImplementedException();
        }
    }
}
