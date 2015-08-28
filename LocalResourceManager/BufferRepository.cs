using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using System.IO.MemoryMappedFiles;

using PheonixRt.DataContracts;


namespace LocalResourceManager
{
    public static class BufferRepository
    {

        public static string _blockPath = @"C:\RD\Blocks";

        public static SharedBuffer CreateBuffer(Guid id, Type elementType, long bufferSize)
        {
            var sb = new SharedBuffer();
            sb.Id = id;
            sb.ElementTypeString = elementType.FullName;
            sb.ElementSize = Marshal.SizeOf(elementType);
            sb.ElementCount = bufferSize;

            if (_blockPath != string.Empty)
            {
                var fileStream = System.IO.File.Create(string.Format(@"{0}\{1}", _blockPath, id.ToString()), 
                    (int)(bufferSize * sb.ElementSize), System.IO.FileOptions.DeleteOnClose);

                var mmf = MemoryMappedFile.CreateFromFile(fileStream, id.ToString(), 
                    (int)(bufferSize * sb.ElementSize),
                    MemoryMappedFileAccess.ReadWrite, null, System.IO.HandleInheritability.None, false);

                _cacheBuffers.TryAdd(id, sb);
                _cacheMmfs.TryAdd(id, mmf);
                _cacheFileStreams.TryAdd(id, fileStream);
            }
            else
            { 
                var mmf = MemoryMappedFile.CreateNew(id.ToString(), 
                    bufferSize * sb.ElementSize);
                _cacheBuffers.TryAdd(id, sb);
                _cacheMmfs.TryAdd(id, mmf);
            }

            return sb;
        }

        public static SharedBuffer GetBuffer(Guid id)
        {
            return _cacheBuffers[id];
        }

        public static void ReadFile(Guid id)
        {
#if USE_CACHED_FS
            System.IO.FileStream fs;
            if (_cacheFileStreams.TryGetValue(id, out fs))
            {
                fs.Seek(0, System.IO.SeekOrigin.Begin);
                for (int n = 0; n < fs.Length / PAGE_SIZE; n++)
                { 
                    lock(_buffer)
                    { 
                        fs.Read(_buffer, 0, (int)PAGE_SIZE);
                    }
                }
                System.Console.WriteLine("Done reading file");
            }
#elif USED_STREAM
            MemoryMappedFile mmf;
            if (_cacheMmfs.TryGetValue(id, out mmf))
            {
                using (var mmvs = mmf.CreateViewStream())
                { 
                    for (int n = 0; n < mmvs.Length / PAGE_SIZE; n++)
                    {
                        lock (_buffer)
                        {
                            mmvs.Read(_buffer, 0, (int)PAGE_SIZE);
                        }
                    }
                }
            }
#endif
        }

        const long PAGE_SIZE = 4096;
        static byte[] _buffer = new byte[PAGE_SIZE];

        public static void FreeBuffer(Guid id)
        {
            SharedBuffer sb;
            _cacheBuffers.TryRemove(id, out sb);

            MemoryMappedFile mmf;
            _cacheMmfs.TryRemove(id, out mmf);
            if (mmf != null)
            { 
                mmf.SafeMemoryMappedFileHandle.Close();
                mmf.Dispose();
            }

            if (_cacheFileStreams.ContainsKey(id))
            {
                System.IO.FileStream fs;
                _cacheFileStreams.TryRemove(id, out fs);
                fs.Close();
                fs.Dispose();
            }
        }

        public static int GetCount()
        {
            return _cacheBuffers.Count;
        }

        public static double GetSizeBytes()
        {
            var sumSizes = from sb in _cacheBuffers.Values
                           select sb.ElementCount * sb.ElementSize;
            return sumSizes.Sum();
        }

        static ConcurrentDictionary<Guid, SharedBuffer> _cacheBuffers =
            new ConcurrentDictionary<Guid, SharedBuffer>();

        static ConcurrentDictionary<Guid, MemoryMappedFile> _cacheMmfs =
            new ConcurrentDictionary<Guid, MemoryMappedFile>();

        static ConcurrentDictionary<Guid, System.IO.FileStream> _cacheFileStreams =
            new ConcurrentDictionary<Guid, System.IO.FileStream>();
    }
}
