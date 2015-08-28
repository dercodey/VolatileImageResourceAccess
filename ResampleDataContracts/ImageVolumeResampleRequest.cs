using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PheonixRt.ResampleServiceContracts
{
    /// <summary>
    /// represents the request to resample a stored series of images
    /// </summary>
    [DataContract]
    public class ImageVolumeResampleRequest
    {
        [DataMember]
        public string SeriesInstanceUID
        {
            get;
            set;
        }
    }
}
