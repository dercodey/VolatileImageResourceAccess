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
    public class MprGenerationResponseV1
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public MprGenerationRequestV1 Request
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public Guid MprImageId
        {
            get;
            set;
        }
    }
}
