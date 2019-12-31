using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Common.Collection
{
    public class PersistentCircularQueue<T>
    {
        private Queue<T> Queue { get; set; }
        private int Size { get; set; }
        private string filename { get; set; }

        public PersistentCircularQueue(int size, string name)
        {
            this.Size = size;
            this.Queue = new Queue<T>();

            var directory = Path.Combine(Path.GetTempPath(), "Nullifer");
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            this.filename = Path.Combine(directory, $"{name}.json");

            if (File.Exists(filename) == false)
                this.Save();
        }

        public bool Add(T value)
        {
            if (this.Queue.Contains(value))
                return false;

            this.Queue.Enqueue(value);

            if (this.Queue.Count > this.Size)
                this.Queue.Dequeue();

            this.Save();

            return true;
        }

        public void Load()
        {
            var json = File.ReadAllText(this.filename);
            this.Queue = Newtonsoft.Json.JsonConvert.DeserializeObject<Queue<T>>(json);
        }

        public void Save()
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(this.Queue);
            File.WriteAllText(this.filename, json);
        }
    }
}
