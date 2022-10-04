namespace Perma.Engine
{
    internal class CellStorage
    {
        readonly Stream _stream;
        private readonly Dictionary<uint, Cell> _cells = new();
        internal int DiskSectorSize { get; }
        internal int CellSize { get; }
        internal int CellHeaderSize { get; }
        internal int CellContentSize { get; }

        public CellStorage(Stream storage)
        {
            DiskSectorSize = CommonConstants.CellSize;
            CellSize = CommonConstants.CellSize;
            CellHeaderSize = CommonConstants.HeaderSize;
            CellContentSize = CommonConstants.CellSize - CommonConstants.HeaderSize;
            _stream = storage;
        }

        internal Cell Find(uint id)
        {
            if (_cells.ContainsKey(id))
            {
                return _cells[id];
            }
            long cellPosition = id * CellSize;
            if ((cellPosition + CellSize) > _stream.Length)
            {
                return null;
            }
            byte[] firstSector = new byte[DiskSectorSize];
            _stream.Position = id* CellSize;
            _stream.Read(firstSector, 0, DiskSectorSize);

            Cell cell = new Cell(this, id, firstSector, _stream);
            OnCellInitialized(cell);
            return cell;
        }

        internal Cell CreateNew()
        {
            uint cellId = (uint)Math.Ceiling((double)_stream.Length / (double)CellSize);
            _stream.SetLength((long)((cellId*CellSize) + CellSize));
            _stream.Flush();

            Cell cell = new Cell(this, cellId, new byte[DiskSectorSize], _stream);
            OnCellInitialized(cell);
            return cell;
        }

        protected virtual void OnCellInitialized(Cell cell)
        {
            _cells[cell.Id] = cell;
        }
    }
}