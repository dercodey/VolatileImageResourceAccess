using Microsoft.Win32.SafeHandles;
using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.Serialization;

namespace PheonixRt.DataContracts
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class SharedBuffer
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public Guid Id
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string ElementTypeString
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public int ElementSize
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public long ElementCount
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SafeMemoryMappedViewHandle GetHandle()
        {
            if (_mmf == null)
                _mmf = MemoryMappedFile.OpenExisting(Id.ToString());
            _mmva = _mmf.CreateViewAccessor();
            return _mmva.SafeMemoryMappedViewHandle;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ReleaseHandle()
        {
            _mmva.SafeMemoryMappedViewHandle.Close();
            _mmva.Dispose();
            _mmva = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CloseMapping()
        {

            _mmf.Dispose();
            _mmf = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Flush()
        {
            if (_mmva != null)
                _mmva.Flush();
        }

        // 
        MemoryMappedFile _mmf;

        //
        MemoryMappedViewAccessor _mmva;

    }
}
