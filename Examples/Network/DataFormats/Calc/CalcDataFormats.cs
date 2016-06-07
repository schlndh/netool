using Netool.Network.DataFormats;
using System;

namespace Examples.Network.DataFormats.Calc
{
    [Serializable]
    public class CalcData : IDataStream
    {
        private double op1;
        private double op2;
        private char operation;
        private StreamList stream = new StreamList();
        public double Op1 { get { return op1; } }
        public double Op2 { get { return op2; } }
        public char Operation { get { return operation; } }
        public long Length { get { return stream.Length; } }

        public CalcData(double op1, double op2, char operation)
        {
            this.op1 = op1;
            this.op2 = op2;
            this.operation = operation;
            stream.Add(new ByteArray(BitConverter.GetBytes(operation)));
            stream.Add(new ByteArray(BitConverter.GetBytes(op1)));
            stream.Add(new ByteArray(BitConverter.GetBytes(op2)));
            stream.Freeze();
        }

        public byte ReadByte(long index)
        {
            return stream.ReadByte(index);
        }

        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
            stream.ReadBytesToBuffer(buffer, start, length, offset);
        }

        public object Clone()
        {
            return this;
        }

        public override string ToString()
        {
            if(Operation == '=')
            {
                return Operation + Op1.ToString();
            }
            return Op1.ToString() + Operation + Op2.ToString();
        }
    }
}