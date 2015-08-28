using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PheonixRt.ResampleServiceContracts
{
    /// <summary>
    /// response with the resampled image volume guid
    /// </summary>
    [DataContract]
    public class ImageVolumeResampleResponse
    {
        [DataMember]
        public string SeriesInstanceUID
        {
            get;
            set;
        }

        [DataMember]
        public Guid ResampledImageVolumeGuid
        {
            get;
            set;
        }
    }
}
