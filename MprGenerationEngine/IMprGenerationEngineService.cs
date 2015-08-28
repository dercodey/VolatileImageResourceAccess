using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using PheonixRt.DataContracts;

namespace MprGenerationEngine
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IMprGenerationEngineService
    {
        [OperationContract(IsOneWay=true)]
        void GenerateMpr(MprGenerationRequestV1 request);
    }

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
