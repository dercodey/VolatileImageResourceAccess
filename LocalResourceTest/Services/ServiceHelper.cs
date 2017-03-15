using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

//using Elekta.Platform.ServiceModel.Hosting;

using PheonixRt.Mvvm.LocalImageResourceServiceReference1;
using System.ServiceModel;

namespace PheonixRt.Mvvm.Services
{
    public static class ServiceHelper
    {
        public static void StartResponseHosts()
        {
            MeshingManagerHelper.StartResponseHost();
            ResampleDoneResponse.StartResponseHost();

#if STREAM_RESPONSE
            MprGenerationStreamDone.StartResponseHost();
#else
            ImageRenderManagerHelper.StartResponseHost();
#endif
        }
    }
}
