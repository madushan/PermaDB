﻿namespace Perma.Engine
{
    public class Database:IDisposable
    {
        bool _disposed = false;
        readonly Stream _databaseStream;
        readonly RecordStorage _dbRecords;
        readonly StringSerializer _stringSerializer = new();
        private readonly string _databasePath = Directory.GetCurrentDirectory();

        public Database(string databaseName)
        {
            string path = Path.Combine(_databasePath, databaseName);
            _databaseStream = new FileStream(
                path,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None,
                CommonConstants.CellSize,
                useAsync: true);
            _dbRecords =
                new RecordStorage(
                    new CellStorage(_databaseStream));

        }

        public uint Insert(string row)
        {
            uint recordId = _dbRecords.Create(_stringSerializer.Serialize(row));
            return recordId;
        }

        public string FindById(uint id)
        {
            byte[] data = _dbRecords.Find(id);
            return _stringSerializer.Deserializer(data);
        }
        
        ~Database()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _databaseStream.Dispose();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}