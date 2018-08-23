using System;
using System.IO;

namespace SharpFileSystem.IO {

    public class BoundStream : Stream {
        readonly Stream _stream;
        readonly long _length;
        long _position = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public BoundStream(Stream stream, long length) {
            _stream = stream;
            _length = length;
        }

        #region Stream members

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override void Flush() {
            _stream.Flush();
        }

        public override long Length => _length;

        public override long Position {
            get => _position;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            if(count > (int)(_length - _position)) count = (int)(_length - _position);
            if(count == 0) return 0;
            int result = _stream.Read(buffer, offset, count);
            _position += result;
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            _stream.Write(buffer, offset, count);
        }

        public override void Close() {
            _stream.Close();
        }

        #endregion
    }
}
