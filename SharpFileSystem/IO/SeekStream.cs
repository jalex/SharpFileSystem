using System;
using System.IO;

namespace SharpFileSystem.IO {

    /// <summary>
    /// SeekStream allows seeking on non-seekable streams by buffering read data. 
    /// </summary>
    public class SeekStream : Stream {
        readonly MemoryStream _innerStream;
        readonly int _bufferSize;
        readonly byte[] _buffer;


        public delegate void DataWrittenHandler();
        public DataWrittenHandler DataWritten;

        public Stream BaseStream { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SeekStream(Stream baseStream, int bufferSize = 81920) : base() {
            _innerStream = new MemoryStream();
            _bufferSize = bufferSize;
            _buffer = new byte[_bufferSize];

            this.BaseStream = baseStream;
        }

        int ReadChunk() {
            int thisRead, read = 0;
            long pos = _innerStream.Position;
            do {
                thisRead = BaseStream.Read(_buffer, 0, _bufferSize - read);
                _innerStream.Write(_buffer, 0, thisRead);
                read += thisRead;
            } while(read < _bufferSize && thisRead > 0);
            _innerStream.Position = pos;
            return read;
        }

        void FastForwardWrite(long position = -1) {
            _innerStream.Position = 0;
            while((position == -1 || position > this.Length || true) && WriteChunk() > 0) {
                // fast-forward
            }
        }

        void FastForward(long position = -1) {
            while((position == -1 || position > this.Length) && ReadChunk() > 0) {
                // fast-forward
            }
        }

        int WriteChunk() {
            int thisWrite, write = 0;
            long pos = BaseStream.Position;
            do {
                thisWrite = _innerStream.Read(_buffer, 0, _bufferSize - write);
                BaseStream.Write(_buffer, 0, thisWrite);

                write += thisWrite;
            } while(write < _bufferSize && thisWrite > 0);
            BaseStream.Position = pos;
            return write;
        }

        #region Strem members

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => BaseStream.CanRead;

        public override bool CanWrite => true;

        public override long Position {
            get => _innerStream.Position;
            set {
                if(value > BaseStream.Position) FastForward(value);
                _innerStream.Position = value;
            }
        }

        public override void Flush() {
            BaseStream.Flush();
        }


        public override int Read(byte[] buffer, int offset, int count) {
            FastForward(offset + count);
            return _innerStream.Read(buffer, offset, count);
        }

        public override int ReadByte() {
            FastForward(this.Position + 1);
            return base.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            long pos = -1;
            if(origin == SeekOrigin.Begin) {
                pos = offset;
            } else
            if(origin == SeekOrigin.Current) {
                pos = _innerStream.Position + offset;
            }
            FastForward(pos);
            return _innerStream.Seek(offset, origin);
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if(!disposing) {
                _innerStream.Dispose();
                BaseStream.Dispose();
            }
        }

        public override long Length => _innerStream.Length;

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            _innerStream.Write(buffer, offset, count);
            FastForwardWrite(offset + count);
            DataWritten?.Invoke();
        }

        public override void WriteByte(byte value) {
            _innerStream.WriteByte(value);
            FastForwardWrite(this.Position + 1);
            DataWritten?.Invoke();
        }

        #endregion
    }
}
