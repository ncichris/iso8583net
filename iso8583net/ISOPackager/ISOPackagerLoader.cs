﻿using ISO8583Net.Header;
using ISO8583Net.Interpreter;
using ISO8583Net.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace ISO8583Net.Packager
{
    /// <summary>
    /// Packager Loader
    /// </summary>
    public class ISOPackagerLoader
    {
        private int m_totalFields = 0;

        private readonly ILogger _logger;
        /// <summary>
        /// Create a new packager based on an embedded file or an actual file on disk
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="fileName">File name on disk, or embedded resource file name</param>
        /// <param name="msgFieldPackager"></param>
        public ISOPackagerLoader(ILogger logger, string fileName, ref ISOMessageFieldsPackager msgFieldPackager)
        {
            _logger = logger;

            XmlReader reader = null;
            
            try
            {
                var embeddedResource = typeof(ISOPackagerLoader).GetTypeInfo().Assembly.GetManifestResourceStream("ISO8583Net.ISODialects." + fileName);
                reader = XmlReader.Create(embeddedResource);
            }
            catch (Exception)
            {
                //Resource Not found try with a file           
            }

            if (reader != null)
            {
                ReadDefinition(ref msgFieldPackager, reader);
            }
            else
            {
                if (File.Exists(fileName))
                {
                    if (_logger.IsEnabled(LogLevel.Trace)) _logger.LogTrace("Loading packager definition from [" + fileName + "]");

                    reader = XmlReader.Create(fileName);
                    ReadDefinition(ref msgFieldPackager, reader);
                }
                else
                {
                    _logger.LogError(string.Format("Filename [{0}] does not exist", fileName));

                    throw new Exception(string.Format("Filename[{0}] does not exist", fileName));
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="msgFieldPackager"></param>
        public ISOPackagerLoader(ILogger logger, ref ISOMessageFieldsPackager msgFieldPackager)
        {
            _logger = logger;
            if (_logger.IsEnabled(LogLevel.Trace)) _logger.LogTrace("Loading packager definition from build-in resource");

            // load from embeded resource visa.xml

            Stream stream = typeof(ISOPackagerLoader).GetTypeInfo().Assembly.GetManifestResourceStream("ISO8583Net.ISODialects.visa.xml");

            var reader = XmlReader.Create(stream);

            ReadDefinition(ref msgFieldPackager, reader);
        }

        private void ReadDefinition(ref ISOMessageFieldsPackager msgFieldPackager, XmlReader reader)
        {
            ISOMessageTypesPackager isoMessageTypesPackager = null;
            string headerPackager = null;
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "isopackager":
                            string attribute = reader["totalfields"];
                            m_totalFields = Int32.Parse(attribute);
                            m_totalFields += 1;
                            headerPackager = reader["headerpackager"];
                            break;

                        case "messages":                            
                            isoMessageTypesPackager = LoadMessageTypes(reader);
                            break;

                        case "isofields":
                            msgFieldPackager = LoadISOMessageFieldsPackager(reader, 0);
                            msgFieldPackager.HeaderPackager = headerPackager;
                            msgFieldPackager.SetMessageTypesPackager(isoMessageTypesPackager);
                            msgFieldPackager.SetStorageClass(Type.GetType("ISO8583Net.Field.ISOMessageFields"));
                            break;
                    }
                }
            }
        }
         /// <summary>
         /// 
         /// </summary>
         /// <param name="headerPackagerName"></param>
         /// <param name="logger"></param>
         /// <returns></returns>
        public static (ISOHeader Header, ISOHeaderPackager Packager) GetMessageHeaderAndPackager(string headerPackagerName, ILogger logger)
        {
            ISOHeader header;
            ISOHeaderPackager packager;
            switch (headerPackagerName)
            {
                case "ISOHeaderTITPPackager":
                    packager = new ISOHeaderTITPPackager(logger);
                    header = new ISOHeaderTITP(logger);
                    break;
                case "ISOHeaderVisaPackager":
                    packager = new ISOHeaderVisaPackager(logger);
                    header = new ISOHeaderVisa(logger);
                    break;
                default:
                    throw new ArgumentException($"Invalid header packager name: {headerPackagerName}");

            }
            return (header, packager);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private ISOMessageTypesPackager LoadMessageTypes(XmlReader reader)
        {
            ISOMessageTypesPackager msgTypesPackager = new ISOMessageTypesPackager(_logger, m_totalFields);

            if (reader.ReadToDescendant("message"))
            {
                do
                {
                    ISOMsgTypePackager msgTypeDefinition = new ISOMsgTypePackager(_logger, m_totalFields);

                    // Search for the attribute name on this current node.

                    string attribute = reader["type"];

                    if (attribute != null)
                    {
                        msgTypeDefinition.messageTypeIdentifier = attribute;
                    }

                    attribute = reader["name"];

                    if (attribute != null)
                    {
                        msgTypeDefinition.messageTypeName = attribute;
                    }

                    attribute = reader["desc"];

                    if (attribute != null)
                    {
                        msgTypeDefinition.messageTypeDescription = attribute;
                    }

                    // read the rest of attributes from 0 to total_dialect fields

                    for (int field = 0; field <= m_totalFields; ++field)
                    {
                        string attributeName = "f" + field.ToString("D3");

                        attribute = reader[attributeName];

                        switch (attribute)
                        {
                            case "C":
                                msgTypeDefinition.m_conBitmap.SetBit(field);
                                break;

                            case "M":
                                msgTypeDefinition.m_manBitmap.SetBit(field);
                                break;

                            case "O":
                                msgTypeDefinition.m_optBitmap.SetBit(field); 
                                break;

                            default:
                                msgTypeDefinition.m_manBitmap.SetBit(field);

                                msgTypeDefinition.m_optBitmap.SetBit(field);
                                break;
                        }
                    }

                    msgTypesPackager.Add(msgTypeDefinition.messageTypeIdentifier, msgTypeDefinition);

                } while (reader.ReadToNextSibling("message"));
            }

            return msgTypesPackager;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private ISOFieldPackager LoadISOFieldPackager(XmlReader reader)
        {
            ISOFieldDefinition fieldDefinition = new ISOFieldDefinition();

            ISOFieldPackager fieldPackager = new ISOFieldPackager(_logger);

            // Search for the attribute name on this current node.

            string attribute = reader["number"];

            if (attribute != null)
            {
                fieldPackager.SetFieldNumber(Int32.Parse(attribute));
            }

            attribute = reader["name"];

            if (attribute != null)
            {
                fieldDefinition.name = attribute;
            }

            attribute = reader["length"];

            if (attribute != null)
            {
                fieldDefinition.length = Int32.Parse(attribute);
            }

            attribute = reader["lengthlength"];

            if (attribute != null)
            {
                fieldDefinition.lengthLength = Int32.Parse(attribute);
            }

            attribute = reader["lengthformat"];

            if (attribute != null)
            {
                switch (attribute)
                {
                    case "FIXED":
                        fieldDefinition.lengthFormat = ISOFieldLengthFormat.FIXED;
                        break;

                    case "VAR":
                        fieldDefinition.lengthFormat = ISOFieldLengthFormat.VAR;
                        break;
                }
            }

            attribute = reader["lengthcoding"];

            if (attribute != null)
            {
                switch (attribute)
                {
                    case "ASCII":
                        fieldDefinition.lengthCoding = ISOFieldCoding.ASCII;
                        break;

                    case "BCD":
                        fieldDefinition.lengthCoding = ISOFieldCoding.BCD;
                        break;

                    case "BCDU":
                        fieldDefinition.lengthCoding = ISOFieldCoding.BCDU;
                        break;

                    case "EBCDIC":
                        fieldDefinition.lengthCoding = ISOFieldCoding.EBCDIC;
                        break;

                    case "BIN":
                        fieldDefinition.lengthCoding = ISOFieldCoding.BIN;
                        break;
                }
            }

            attribute = reader["lengthpadding"];

            if (attribute != null)
            {
                switch (attribute)
                {
                    case "LEFT":
                        fieldDefinition.lengthPadding = ISOFieldPadding.LEFT;
                        break;

                    case "RIGHT":
                        fieldDefinition.lengthPadding = ISOFieldPadding.RIGHT;
                        break;

                    case "NONE":
                        fieldDefinition.lengthPadding = ISOFieldPadding.NONE;
                        break;
                }
            }

            attribute = reader["contentformat"];

            if (attribute != null)
            {
                switch (attribute)
                {
                    case "A":
                        fieldDefinition.content = ISOFieldContent.A;
                        break;

                    case "AN":
                        fieldDefinition.content = ISOFieldContent.AN;
                        break;

                    case "ANS":
                        fieldDefinition.content = ISOFieldContent.ANS;
                        break;

                    case "AS":
                        fieldDefinition.content = ISOFieldContent.AS;
                        break;

                    case "N":
                        fieldDefinition.content = ISOFieldContent.N;
                        break;

                    case "NS":
                        fieldDefinition.content = ISOFieldContent.NS;
                        break;

                    case "HD":
                        fieldDefinition.content = ISOFieldContent.HD;
                        break;

                    case "TRACK2":
                        fieldDefinition.content = ISOFieldContent.Z;
                        break;

                    case "Z":
                        fieldDefinition.content = ISOFieldContent.Z;
                        break;
                }
            }

            attribute = reader["contentcoding"];

            if (attribute != null)
            {
                switch (attribute)
                {
                    case "ASCII":
                        fieldDefinition.contentCoding = ISOFieldCoding.ASCII;
                        break;

                    case "BCD":
                        fieldDefinition.contentCoding = ISOFieldCoding.BCD;
                        // Always N the content since nothing else is possible
                        fieldDefinition.content = ISOFieldContent.N; 
                        break;

                    case "BCDU":
                        fieldDefinition.contentCoding = ISOFieldCoding.BCDU;
                        // Always N the content since nothing else is possible
                        fieldDefinition.content = ISOFieldContent.N;
                        break;

                    case "EBCDIC":
                        fieldDefinition.contentCoding = ISOFieldCoding.EBCDIC;
                        break;

                    case "BIN":
                        // Always HD the content since nothing else is possible
                        fieldDefinition.content = ISOFieldContent.HD;
                        fieldDefinition.contentCoding = ISOFieldCoding.BIN;
                        break;

                    case "Z":
                        fieldDefinition.contentCoding = ISOFieldCoding.Z;
                        break;

                }
            }

            attribute = reader["contentpaddingcharacter"];
            if (!string.IsNullOrEmpty(attribute))
            {
                if (fieldDefinition.contentCoding == ISOFieldCoding.ASCII || fieldDefinition.contentCoding == ISOFieldCoding.ASCII)
                {

                    //save as ascii character
                    fieldDefinition.contentPaddingCharacter = (byte)attribute[0];
                }
                else
                {
                    //padding character should be a hex digit 0-F
                    //save the numeric value of it
                    fieldDefinition.contentPaddingCharacter = Convert.ToByte(attribute, 16);
                }
                
            }
            attribute = reader["contentpadding"];

            if (attribute != null)
            {
                switch (attribute)
                {
                    case "LEFT":
                        fieldDefinition.contentPadding = ISOFieldPadding.LEFT;    
                        break;

                    case "RIGHT":
                        fieldDefinition.contentPadding = ISOFieldPadding.RIGHT;
                        break;

                    case "NONE":
                        fieldDefinition.contentPadding = ISOFieldPadding.NONE;
                        break;
                }
            }

            attribute = reader["desc"];

            if (attribute != null)
            {
                fieldDefinition.description = attribute;
            }

            fieldPackager.SetFieldDefinition(fieldDefinition);
            return fieldPackager;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="fieldNumber"></param>
        /// <returns></returns>
        private ISOMessageFieldsPackager LoadISOMessageFieldsPackager(XmlReader reader, int fieldNumber)
        {
            ISOMessageFieldsPackager msgFieldPackager = new ISOMessageFieldsPackager(_logger, fieldNumber, m_totalFields); 

            if (reader.ReadToDescendant("isofield"))
            {
                do
                {
                    int fldNumber = int.Parse(reader["number"]);

                    string packager       = reader["packager"];
                    string storageclass   = reader["storageclass"];
                    string iscomposite    = reader["composite"];
                    string isointerpreter = reader["interpreter"];

                    if (_logger.IsEnabled(LogLevel.Trace)) _logger.LogTrace("Field Number: " + fldNumber.ToString().PadLeft(3, '0') + " Name: " + reader["name"] + " Description: " + reader["desc"]);
                                                                                                
                    switch (packager)
                    {
                        case "ISOFieldBitmapSubFieldsPackager":

                            int totalFields = Int32.Parse(reader["totalfields"]);

                            totalFields += 1;

                            if (_logger.IsEnabled(LogLevel.Trace)) _logger.LogTrace("Field Number: " + fldNumber.ToString().PadLeft(3,'0') + " is of [[[<--ISOMessageFieldsPackager-->]]], SubFields follow:");

                            ISOFieldPackager fPackager = LoadISOFieldPackager(reader);

                            ISOFieldBitmapSubFieldsPackager newMsgFieldPackager = LoadISOMessageSubFieldsPackager(reader, fldNumber);

                            newMsgFieldPackager.SetISOFieldDefinition(fPackager.GetISOFieldDefinition());

                            newMsgFieldPackager.SetStorageClass(Type.GetType("ISO8583Net.Field.ISOFieldBitmapSubFields"));

                            msgFieldPackager.Add(newMsgFieldPackager, newMsgFieldPackager.GetFieldNumber());

                            newMsgFieldPackager.totalFields=totalFields;

                            break;

                        default:

                            ISOFieldPackager fieldPackager = LoadISOFieldPackager(reader);

                            if (storageclass == null)
                            {
                                fieldPackager.SetStorageClass(Type.GetType("ISO8583Net.Field.ISOField"));
                            }
                            else
                            {
                                fieldPackager.SetStorageClass(Type.GetType("ISO8583Net." + storageclass));
                            }

                            if (iscomposite == null || iscomposite=="N" || iscomposite=="n" || iscomposite=="No" || iscomposite=="no")
                            {
                                fieldPackager.SetComposite(false);
                            }
                            else
                            {
                                fieldPackager.SetComposite(true);
                            }

                            switch (isointerpreter)
                            {
                                //case "ISOEMVTagInterpreter":
                                //    fieldPackager.SetISOInterpreter(new ISOEMVTagInterpreter(Logger));
                                //    break;

                                case "ISOIndexedValueInterpreter":
                                    ISOIndexedValueInterpreter isoIndexedValueInterpreter = LoadISOIndexedValueInterpreter(reader);
                                    fieldPackager.SetISOInterpreter(isoIndexedValueInterpreter);
                                    break;

                            }

                            msgFieldPackager.Add(fieldPackager, fieldPackager.GetFieldNumber());

                            break;
                    }
                } while (reader.ReadToNextSibling("isofield"));
            }
            return msgFieldPackager;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="fieldNumber"></param>
        /// <returns></returns>
        private ISOFieldBitmapSubFieldsPackager LoadISOMessageSubFieldsPackager(XmlReader reader, int fieldNumber)
        {
            ISOFieldBitmapSubFieldsPackager msgFieldPackager = new ISOFieldBitmapSubFieldsPackager(_logger, fieldNumber, m_totalFields);

            if (reader.ReadToDescendant("isofield"))
            {
                do
                {
                    int fldNumber = int.Parse(reader["number"]);

                    string packager = reader["packager"];
                    string storageclass = reader["storageclass"];
                    string iscomposite = reader["composite"];
                    string isointerpreter = reader["interpreter"];

                    if (_logger.IsEnabled(LogLevel.Trace)) _logger.LogTrace("Field Number: " + fldNumber.ToString().PadLeft(3, '0') + " Name: " + reader["name"] + " Description: " + reader["desc"]);

                    switch (packager)
                    {
                        case "ISOFieldBitmapSubFieldsPackager":

                            int totalFields = Int32.Parse(reader["totalfields"]);

                            totalFields += 1;

                            if (_logger.IsEnabled(LogLevel.Trace)) _logger.LogTrace("Field Number: " + fldNumber.ToString().PadLeft(3, '0') + " is of [[[<--ISOMessageFieldsPackager-->]]], SubFields follow:");

                            ISOFieldPackager fPackager = LoadISOFieldPackager(reader);

                            ISOFieldBitmapSubFieldsPackager newMsgFieldPackager = LoadISOMessageSubFieldsPackager(reader, fldNumber);

                            newMsgFieldPackager.SetISOFieldDefinition(fPackager.GetISOFieldDefinition());

                            newMsgFieldPackager.SetStorageClass(Type.GetType("ISO8583Net.Field.ISOFieldBitmapSubFields"));

                            msgFieldPackager.Add(newMsgFieldPackager, newMsgFieldPackager.GetFieldNumber());

                            newMsgFieldPackager.totalFields=totalFields;

                            break;

                        default:

                            ISOFieldPackager fieldPackager = LoadISOFieldPackager(reader);

                            if (storageclass == null)
                            {
                                fieldPackager.SetStorageClass(Type.GetType("ISO8583Net.Field.ISOField"));
                            }
                            else
                            {
                                fieldPackager.SetStorageClass(Type.GetType("ISO8583Net." + storageclass));
                            }

                            if (iscomposite == null || iscomposite == "N" || iscomposite == "n" || iscomposite == "No" || iscomposite == "no")
                            {
                                fieldPackager.SetComposite(false);
                            }
                            else
                            {
                                fieldPackager.SetComposite(true);
                            }

                            switch (isointerpreter)
                            {
                                //case "ISOEMVTagInterpreter":
                                //    fieldPackager.SetISOInterpreter(new ISOEMVTagInterpreter(Logger));
                                //    break;

                                case "ISOIndexedValueInterpreter":
                                    ISOIndexedValueInterpreter isoIndexedValueInterpreter = LoadISOIndexedValueInterpreter(reader);
                                    fieldPackager.SetISOInterpreter(isoIndexedValueInterpreter);
                                    break;

                            }

                            msgFieldPackager.Add(fieldPackager, fieldPackager.GetFieldNumber());

                            break;
                    }
                } while (reader.ReadToNextSibling("isofield"));
            }
            return msgFieldPackager;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private ISOIndexedValueInterpreter LoadISOIndexedValueInterpreter(XmlReader reader)
        {
            ISOIndexedValueInterpreter isoIndexedValueInterpreter = new ISOIndexedValueInterpreter(_logger);

            if (reader.ReadToDescendant("interpreter"))
            {
                do
                {
                    string index = reader["index"];
                    string length = reader["length"];
                    string desc = reader["desc"];

                    if (reader.ReadToDescendant("value"))
                    {
                        isoIndexedValueInterpreter.AddIndexLength(Int32.Parse(index), Int32.Parse(length));

                        Dictionary<string, string> dic = new Dictionary<string, string>();

                        dic.Add("", desc);

                        do
                        {
                            string value = reader["value"];
                            desc         = reader["desc"];

                            dic.Add(value, desc);

                        } while (reader.ReadToNextSibling("value"));

                        isoIndexedValueInterpreter.AddIndexValueDescriptionDic(Int32.Parse(index),dic);
                    } 
                } while (reader.ReadToNextSibling("interpreter"));
            }

            return isoIndexedValueInterpreter;
        }
    }
}
