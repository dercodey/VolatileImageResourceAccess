using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MprGenerationContracts
{

    /// <summary>
    /// 
    /// </summary>
    public enum Orientation { Transverse, Coronal, Sagittal };

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class MprGenerationRequestV1
    {
        [DataMember]
        public DateTime RequestTime
        {
            get;
            set;
        }

        [DataMember]
        public Guid ImageVolumeId
        {
            get;
            set;
        }

        [DataMember]
        public Orientation Orientation
        {
            get;
            set;
        }

        [DataMember]
        public int SlicePosition
        {
            get;
            set;
        }

        [DataMember]
        public int WindowCenter
        {
            get;
            set;
        }

        [DataMember]
        public int WindowWidth
        {
            get;
            set;
        }
    }
}
