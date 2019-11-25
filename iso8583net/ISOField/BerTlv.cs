using ISO8583Net.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ISO8583Net.Field
{
    /// <summary>
    /// 
    /// </summary>
    public class BerTLV
    {
        /// <summary>
        /// Number of Objects in the TLV
        /// </summary>
        public int NbOfObjects
        {
            get { return elementPool.Count; }
        }

        BerTLVObject parent = null;

        private List<BerTLVObject> elementPool = new List<BerTLVObject>();

        /// <summary>
        /// Objects included in the TLV
        /// </summary>
        public List<BerTLVObject> ObjectList
        {
            get { return elementPool; }

            set { elementPool = value; }
        }

        private List<string> knownTags = null;

        /// <summary>
        /// List of known tags
        /// </summary>
        public List<string> KnownTags
        {
            set
            {
                knownTags = value;
            }
        }

        /// <summary>
        /// Parse a byte array containing the encoded TLV
        /// </summary>
        /// <param name="encodedTLV"></param>
        public void Parse(byte[] encodedTLV)
        {
            _Parse(encodedTLV);
        }

        /// <summary>
        /// Internal Parse
        /// </summary>
        /// <param name="encodedTLV"></param>
        /// <returns></returns>
        private int _Parse(byte[] encodedTLV)
        {
            string TLVTag;
            int TLVLen;
            byte[] TLVData;

            byte[] TLVTagBytes;
            int TagSize = 1;
            int LenSize;

            if ((encodedTLV[0] & 0x1F) == 0x1F)
            {
                TagSize++;
                for (int idx = 1; idx < encodedTLV.Length; ++idx)
                {
                    if ((encodedTLV[idx] & 0x80) == 0x80)
                        TagSize++;
                    else break;
                }
            }

            TLVTagBytes = new byte[TagSize];
            Array.Copy(encodedTLV, 0, TLVTagBytes, 0, TagSize);
            TLVTag = ISOUtils.Bytes2Hex(TLVTagBytes, TagSize);
            int tlvLenOffset = TagSize;

            if (encodedTLV[TagSize] < 128)
            {
                LenSize = 1;
                TLVLen = encodedTLV[TagSize];
            }
            else
            {
                LenSize = 1 + (0x7F & encodedTLV[tlvLenOffset]);
                byte[] lenTmp = new byte[4];

                int ofsetLenBytes = TagSize + 1;
                int nbLenBytes = LenSize - 1;

                if (nbLenBytes > 4) // 4 bytes is quite enaugh
                    throw new Exception("Length error in TLV package");

                for (int j = 0; j < nbLenBytes; ++j)
                    lenTmp[nbLenBytes - j - 1] = encodedTLV[ofsetLenBytes + j];

                TLVLen = BitConverter.ToInt32(lenTmp, 0);
            }

            TLVData = new byte[TLVLen];
            Array.Copy(encodedTLV, TagSize + LenSize, TLVData, 0, TLVLen);
            BerTLVObject newObj = new BerTLVObject(TLVTag, TLVData);
            newObj.Parent = parent;
            if (newObj.Parent != null)
                newObj.Parent.ChildList.Add(newObj);
            elementPool.Add(newObj);

            if (((byte)encodedTLV[0] & 32) == 32)
            {
                BerTLVObject oldParent = parent;
                parent = newObj;
                int TotalLen = TLVData.Length;
                int Index = 0;
                while ((TotalLen - Index) > 0)
                {
                    byte[] SubTLV = new byte[TotalLen - Index];
                    Array.Copy(TLVData, Index, SubTLV, 0, SubTLV.Length);
                    Index += this._Parse(SubTLV);
                }
                parent = oldParent;
            }

            return TagSize + LenSize + TLVData.Length;
        }

        /// <summary>
        /// Add a new object to the TLV
        /// </summary>
        /// <param name="tlvObj"></param>
        public void AddTLVObject(BerTLVObject tlvObj)
        {
            if (tlvObj == null)
                return;

            elementPool.Add(tlvObj);
        }

        /// <summary>
        /// Get first object with tag == tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public BerTLVObject GetFirstObject(string tag)
        {
            foreach (BerTLVObject obj in this.ObjectList)
            {
                if (obj.TagStr.Equals(tag))
                    return obj;
            }

            return null;
        }

        /// <summary>
        /// Get object at location index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public BerTLVObject GetObjectAt(int index)
        {
            if (index >= elementPool.Count)
                return null;

            return elementPool.ElementAt(index);
        }

        /// <summary>
        /// Get list of objects
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<BerTLVObject> GetObjectList(string tag)
        {
            List<BerTLVObject> tlvList = new List<BerTLVObject>();

            foreach (BerTLVObject tlvItem in elementPool)
            {
                if (tlvItem.TagStr.Equals(tag))
                    tlvList.Add(tlvItem);
            }

            return tlvList;
        }

        private bool isTagKnown(string tag)
        {
            if (knownTags == null)
                return true;

            if (knownTags.Contains(tag))
                return true;
            else
                return false;
        }
    }
}
