namespace Perma.Engine
{
    public class Cell:IDisposable
    {
        private readonly byte[] _firstSector;
        readonly long[] _headers = new long[6];
        readonly Stream _stream;
        private readonly CellStorage _storage;
        bool _isFirstSectorDirty = false;
        bool _isDisposed = false;
        
        public uint Id { get; set; }

        internal Cell(CellStorage storage,uint id, byte[] firstSector,Stream stream)
        {
            _storage = storage;
            _firstSector = firstSector;
            _stream = stream;
            Id = id;
            FetchHeaders(firstSector);
        }

        void FetchHeaders(byte[] firstSector)
        {
            byte[] headerBytes = BufferReader.ReadHeader(firstSector);

            byte[] headerSlice = new byte[8];

            for (int i = 0; i < _headers.Length; i++)
            {
                Array.Copy(headerBytes,i*8, headerSlice, 0,8);

                _headers[i] = LittleEndian.GetInt64(headerSlice);
            }
        }

        internal long GetHeader(int headerIndex)
        {
            return _headers[headerIndex];
        }

        internal void SetHeader(int headerIndex, long value)
        {
            _headers[headerIndex] = value;
            BufferWriter.WriteBuffer((long)value,_firstSector,headerIndex*8);
            _isFirstSectorDirty = true;
        }

        internal void Read(byte[] destination, int destinationOffset, int sourceOffset, int count)
        {
            int dataCopied = 0;
            bool copyFromFirstSector = (CommonConstants.HeaderSize+sourceOffset) < CommonConstants.CellSize;
            if (copyFromFirstSector)
            {
                int toBeCopied = Math.Min(CommonConstants.CellSize - CommonConstants.HeaderSize - sourceOffset, count);
                Buffer.BlockCopy(
                        src:_firstSector,
                        srcOffset:CommonConstants.HeaderSize+sourceOffset,
                        dst:destination,
                        dstOffset:destinationOffset,
                        count:toBeCopied
                    );
                dataCopied += toBeCopied;
            }

            if (dataCopied < count)
            {
                if (copyFromFirstSector)
                {
                    _stream.Position = (Id*CommonConstants.CellSize) + CommonConstants.CellSize;
                }
                else
                {
                    _stream.Position = (Id*CommonConstants.CellSize) + CommonConstants.HeaderSize + sourceOffset;
                }
            }

            while (dataCopied < count)
            {
                int bytesToRead = Math.Min(CommonConstants.CellSize, count - dataCopied);
                int thisRead = _stream.Read(destination, destinationOffset + dataCopied, bytesToRead);
                dataCopied += thisRead;
            }
        }

        public void Write(byte[] source, int sourceOffset, int destinationOffset, int count)
        {
            if ((_storage.CellHeaderSize + destinationOffset) < _storage.DiskSectorSize)
            {
                int thisWrite = Math.Min(count, _storage.DiskSectorSize - _storage.CellHeaderSize - destinationOffset);
                Buffer.BlockCopy(src: source
                    , srcOffset: sourceOffset
                    , dst: _firstSector
                    , dstOffset: _storage.CellHeaderSize + destinationOffset
                    , count: thisWrite);
                _isFirstSectorDirty = true;
            }
            
            if ((_storage.CellHeaderSize + destinationOffset + count) > _storage.DiskSectorSize)
            {
                this._stream.Position = (Id * _storage.CellSize)
                                        + Math.Max(_storage.DiskSectorSize, _storage.CellHeaderSize + destinationOffset);
                
                int d = _storage.DiskSectorSize - (_storage.CellHeaderSize + destinationOffset);
                if (d > 0)
                {
                    destinationOffset += d;
                    sourceOffset += d;
                    count -= d;
                }
                
                int written = 0;
                while (written < count)
                {
                    var bytesToWrite = (int)Math.Min(4096, count - written);
                    this._stream.Write(source, sourceOffset + written, bytesToWrite);
                    this._stream.Flush();
                    written += bytesToWrite;
                }
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                _isDisposed = true;

                if (_isFirstSectorDirty)
                {
                    _stream.Position = (Id * _storage.CellSize);
                    _stream.Write(_firstSector, 0, 4096);
                    _stream.Flush();
                    _isFirstSectorDirty = false;
                }
            }
        }

        ~Cell()
        {
            Dispose(false);
        }
    }
}