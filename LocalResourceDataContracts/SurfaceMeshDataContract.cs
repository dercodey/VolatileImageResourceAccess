using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PheonixRt.DataContracts
{
    public struct TriangleIndex
    {
        public TriangleIndex(int i0, int i1, int i2)
        {
            Index0 = i0;
            Index1 = i1;
            Index2 = i2;
        }

        int Index0;
        int Index1;
        int Index2;
    };

    [DataContract]
    public class SurfaceMeshDataContract
    {
        [DataMember]
        public Guid Id
        {
            get;
            set;
        }

        [DataMember]
        public string FrameOfReferenceUID
        {
            get;
            set;
        }

        [DataMember]
        public Guid RelatedStructureId
        {
            get;
            set;
        }

        [DataMember]
        public long VertexCount
        {
            get;
            set;
        }

        [DataMember]
        public SharedBuffer VertexBuffer
        {
            get;
            set;
        }

        [DataMember]
        public SharedBuffer NormalBuffer
        {
            get;
            set;
        }

        [DataMember]
        public long TriangleCount
        {
            get;
            set;
        }

        [DataMember]
        public SharedBuffer TriangleIndexBuffer
        {
            get;
            set;
        }
    }
}
