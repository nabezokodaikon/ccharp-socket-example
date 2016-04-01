using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLib.Command
{
    public sealed class ExampleCommand
    {
        public static readonly int COMMAND_SIZE_DATA_SIZE = BitConverter.GetBytes(0).Length;
        public static readonly Encoding ENCODE = Encoding.GetEncoding("shift-jis");

        public string Contents { get; private set; }

        public ExampleCommand(string contents)
        {
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }

            this.Contents = contents;
        }

        public ExampleCommand(byte[] bin)
        {
            if (bin == null)
            {
                throw new ArgumentNullException("bin");
            }

            if (bin.Length < ExampleCommand.COMMAND_SIZE_DATA_SIZE)
            {
                throw new ArgumentException(string.Format(
                    "{0}バイト以上のバイナリデータを指定してください。",
                    ExampleCommand.COMMAND_SIZE_DATA_SIZE),
                    "bin");
            }

            var commandSizeBuffer = bin.Take(ExampleCommand.COMMAND_SIZE_DATA_SIZE).ToArray();
            var commandSize = BitConverter.ToInt32(commandSizeBuffer, 0);
            if (bin.Length != commandSize)
            {
                throw new ArgumentException(string.Format(
                    "バイナリデータのサイズが、コマンドサイズと一致しません。バイナリデータサイズ = {0}, コマンドサイズ = {1}",
                    bin.Length, ExampleCommand.COMMAND_SIZE_DATA_SIZE),
                    "bin");
            }

            var contentsSize = commandSize - ExampleCommand.COMMAND_SIZE_DATA_SIZE;
            var contentsBuffer = bin.Skip(ExampleCommand.COMMAND_SIZE_DATA_SIZE).Take(contentsSize).ToArray();
            this.Contents = ExampleCommand.ENCODE.GetString(contentsBuffer);
        }

        public byte[] ToBinary()
        {
            var contentsBuffer = ExampleCommand.ENCODE.GetBytes(this.Contents);
            var commandSize = contentsBuffer.Length;
            var commandSizeBuffer = BitConverter.GetBytes(commandSize);
            var bin = new List<byte>();
            bin.AddRange(commandSizeBuffer);
            bin.AddRange(contentsBuffer);
            return bin.ToArray();
        }
    }
}
