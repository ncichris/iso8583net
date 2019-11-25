using Microsoft.Extensions.Logging;
using System;

namespace ISO8583Net.Header
{
    /// <summary>
    /// 
    /// </summary>
    public class ISOHeaderTITP : ISOHeader
    {
        /// <summary>
        /// 
        /// </summary>
        public byte[] Payload { get; set; } = new byte[5];
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public ISOHeaderTITP(ILogger logger) : base(logger)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int Length()
        {
            return Payload.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packedBytes"></param>
        /// <param name="index"></param>
        public override void Pack(byte[] packedBytes, ref int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        public override void SetMessageLength(int length)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        public override void SetValue(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override void UnPack(byte[] packedBytes, ref int index)
        {
            throw new NotImplementedException();
        }
    }
}
