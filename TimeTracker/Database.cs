using System;
using System.Collections.Generic;
using System.IO;
using ServiceStack.Text;
using ServiceStack.Common;

namespace TimeTracker
{
    class Database : IDisposable
    {
        private readonly string _path;
        public List<TimeRecord> TimeRecords { get; set; }

        private Database(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            _path = path;
            TimeRecords = new List<TimeRecord>();
        }

        public static Database Open(string path)
        {
            try
            {
                return
                    new Database(path).PopulateWith(TypeSerializer.DeserializeFromStream<Database>(File.OpenRead(path)));
            }
            catch
            {
                return new Database(path);
            }
        }

        public void Save()
        {
            TypeSerializer.SerializeToStream(this, File.OpenWrite(_path));
        }

        public void Dispose()
        {
            Save();
        }
    }
}