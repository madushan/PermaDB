namespace Perma.Engine
{
    internal class RecordStorage
    {
        readonly CellStorage _storage;
        const int KNextCellId = 0;
        const int KRecordLength = 1;
        const int KCellContentLength = 2;
        const int KPreviousBlockId = 3;
        const int KIsDeleted = 4;

        public RecordStorage(CellStorage storage)
        {
            _storage = storage;
        }

        internal virtual byte[] Find(uint recordId)
        {
            using (Cell cell =_storage.Find(recordId))
            {
                if (cell == null) return null;

                if (cell.GetHeader(KIsDeleted) == 1L) return null;

                if (0L != cell.GetHeader(KPreviousBlockId)) return null;

                long totalRecordSize = cell.GetHeader(KRecordLength);

                byte[] data = new byte[totalRecordSize];
                int byteRead = 0;

                Cell currentCell = cell;
                while (true)
                {
                    uint nextCellId;
                    using (currentCell)
                    {
                        long thisCellContentLength = currentCell.GetHeader(KCellContentLength);

                        currentCell.Read(destination:data,destinationOffset:byteRead,sourceOffset:0,count:(int)thisCellContentLength);
                        byteRead += (int)thisCellContentLength;

                        nextCellId = (uint)currentCell.GetHeader(KNextCellId);
                        if (nextCellId == 0)
                            return data;
                    }

                    currentCell = _storage.Find(nextCellId);
                }
            }
        }

        internal virtual uint Create()
        {
            using (Cell firstCell = AllocateCell())
            {
                return firstCell.Id;
            }
        }

        internal virtual uint Create(byte[] data)
        {
            using (Cell firstCell = AllocateCell())
            {
                uint returnId = firstCell.Id;
                
                int dataWritten = 0;
                int dataTobeWritten = data.Length;
                firstCell.SetHeader(KRecordLength, dataTobeWritten);
                
                if (dataTobeWritten == 0)
                {
                    return returnId;
                }
                
                Cell currentCell = firstCell;
                while (dataWritten < dataTobeWritten)
                {
                    Cell nextCell = null;

                    using (currentCell)
                    {
                        int thisWrite = (int)Math.Min(_storage.CellContentSize, dataTobeWritten - dataWritten);
                        currentCell.Write(data, dataWritten, 0, thisWrite);
                        currentCell.SetHeader(KCellContentLength, (long)thisWrite);
                        dataWritten += thisWrite;
                        
                        if (dataWritten < dataTobeWritten)
                        {
                            nextCell = AllocateCell();
                            bool success = false;
                            try
                            {
                                nextCell.SetHeader(KPreviousBlockId, currentCell.Id);
                                currentCell.SetHeader(KNextCellId, nextCell.Id);
                                success = true;
                            }
                            finally
                            {
                                if ((!success) && (nextCell != null))
                                {
                                    nextCell.Dispose();
                                    nextCell = null;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    } 
                    
                    if (nextCell != null)
                    {
                        currentCell = nextCell;
                    }
                }
                return returnId;
            }
        }

        Cell AllocateCell()
        {
            uint reusableCellId;
            Cell newCell;
            newCell = _storage.CreateNew();

            return newCell;
        }

        bool TryFindFreeCell(out uint cellId)
        {
            cellId = 0;
            Cell lastCell, secondLastCell;
            GetSpaceTrackingBlock(out lastCell,out secondLastCell);

            using (lastCell)
            using (secondLastCell)
            {
                long currentCellContentLength = lastCell.GetHeader(KCellContentLength);
                if (currentCellContentLength == 0)
                {
                    if (secondLastCell == null)
                    {
                        return false; 
                    }

                    cellId = ReadUInt32FromTrailingContent(secondLastCell);

                    secondLastCell.SetHeader(KCellContentLength,secondLastCell.GetHeader(KCellContentLength)-4);
                    AppendUInt32ToContent(secondLastCell,lastCell.Id);

                    secondLastCell.SetHeader(KCellContentLength,secondLastCell.GetHeader(KCellContentLength)+4);
                    secondLastCell.SetHeader(KNextCellId,0);
                    lastCell.SetHeader(KPreviousBlockId,0);

                    return true;
                }
                else
                {
                    cellId = ReadUInt32FromTrailingContent(lastCell);
                    lastCell.SetHeader(KCellContentLength,currentCellContentLength - 4);

                    return true;
                }
            }

        }


        void AppendUInt32ToContent(Cell cell, uint value)
        {
            long contentLength = cell.GetHeader(KCellContentLength);
            
            cell.Write(source: LittleEndian.GetBytes(value), sourceOffset: 0, destinationOffset: (int)contentLength, count: 4);
        }
        
        uint ReadUInt32FromTrailingContent(Cell cell)
        {
            byte[] buffer = new byte[4];
            long contentLength = cell.GetHeader(KCellContentLength);
            
            cell.Read(destination: buffer, destinationOffset: 0, sourceOffset: (int)contentLength - 4, count: 4);
            return LittleEndian.GetUInt32(buffer);
        }


        internal void GetSpaceTrackingBlock(out Cell lastCell, out Cell secondLastCell)
        {
            lastCell = null;
            secondLastCell = null;

            IList<Cell> cells = FindCells(0);
        }

        List<Cell> FindCells(uint recordId)
        {
            List<Cell> cells = new List<Cell>();
            bool success = false;

            try
            {
                uint currentCellId = recordId;

                do
                {
                    Cell cell = _storage.Find(currentCellId);
                    if (cell == null)
                    {
                        if (currentCellId == 0)
                        {
                            cell = _storage.CreateNew();
                        }
                    }
                    cells.Add(cell);
                        
                    currentCellId = (uint)cell.GetHeader(KNextCellId);
                } while (currentCellId != 0);

                success = true;
                return cells;
            }
            finally
            {
                if (!success)
                {
                    foreach (Cell cell in cells)
                    {
                        cell.Dispose();
                    }
                }
            }
        }
    }
}