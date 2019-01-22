﻿using Microsoft.Extensions.Logging;
using System;

namespace ISO8583Net.Field
{
    public abstract class ISOComponent
    {
        private readonly ILogger _logger;

        internal ILogger Logger { get { return _logger; } }

        protected int m_number;

        protected String m_value;

        public ISOComponent(ILogger logger, int number)
        {
            _logger = logger;

            m_number = number;
        }

        public ISOComponent(ILogger logger, int number, String value)
        {
            _logger = logger;

            m_value = value;

            m_number = number;
        }

        public abstract String GetValue();

        public abstract void SetValue(String value);

        public abstract override String ToString();

        public abstract void SetFieldValue(int fieldNumber, String fieldValue);

        //public abstract void SetFieldValue(int fieldNumber, int subFieldNumber, String fieldValue);

        public abstract void SetFieldValue(int fieldNumber, String tag, String tagValue);

        public abstract String GetFieldValue(int fieldNumber);

        public abstract string GetFieldValue(int fieldNumber, int subField);

        public abstract string GetFieldValue(int fieldNumber, String tag, String tagValue);

        public abstract void Trace();
       
    }
}
