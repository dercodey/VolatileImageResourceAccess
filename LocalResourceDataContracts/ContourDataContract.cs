using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PheonixRt.DataContracts
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class ContourDataContract
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
        public string FrameOfReferenceUID
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public long VertexCount
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public SharedBuffer VertexBuffer
        {
            get;
            set;
        }
    }
}
