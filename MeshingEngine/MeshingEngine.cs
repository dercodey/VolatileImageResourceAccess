using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows.Media.Media3D;
using System.Runtime.InteropServices;

using PheonixRt.DataContracts;
using PheonixRt.MeshingServiceContracts;
using PheonixRt.MeshingEngineService.LocalGeometryResourceServiceReference1;

namespace PheonixRt.MeshingEngineService
{
    /// <summary>
    /// concrete implementation of the meshing engine
    /// </summary>
    public class MeshingEngine : IMeshingEngine
    {
        /// <summary>
        /// synchronous call in to the meshing
        /// </summary>
        /// <param name="request">the request contract describing the meshing to be performed</param>
        /// <returns></returns>
        public MeshingResponse MeshStructure(MeshingRequest request)
        {
            // set up a resource accessor
            LocalGeometryResourceManagerClient cmsc1 =
                new LocalGeometryResourceManagerClient();

            // and get the designated structure
            StructureDataContract sdc = cmsc1.GetStructure(request.StructureGuid);

            System.Diagnostics.Trace.WriteLine(string.Format("Meshing for structure {0} {1} {2}",
                sdc.SeriesInstanceUID,
                sdc.SOPInstanceUID,
                sdc.Id.ToString(),
                sdc.Contours.Count));

            // System.Threading.Thread.Sleep(1000);

            // get the contours for the strucutre
            var contours = from guid in sdc.Contours
                           select cmsc1.GetPolygon(guid);

            // TODO: check that contours match those in the request contract

            // create the resulting surface mesh data contract
            SurfaceMeshDataContract smdc = new SurfaceMeshDataContract();

            // set up and calculate sizes
            smdc.FrameOfReferenceUID = sdc.FrameOfReferenceUID;
            smdc.RelatedStructureId = sdc.Id;
            smdc.VertexCount = contours.Sum(c => c.VertexCount);
            smdc.TriangleCount = smdc.VertexCount / 3;

            // create the mesh object, to allocate the buffers
            smdc = cmsc1.AddSurfaceMesh(smdc);

            // get a handle to the vertices
            var meshVertexHandle = smdc.VertexBuffer.GetHandle();
            ulong currentMeshOffset = 0;

            // now iterate over the the contours
            foreach (var contour in contours)
            {
                Vector3D[] vertices = new Vector3D[contour.VertexBuffer.ElementCount];

                // get access to the buffer
                var contourHandle = contour.VertexBuffer.GetHandle();

                // and copy from the contour...
                contourHandle.ReadArray<Vector3D>(0, vertices, 0, vertices.Length);

                // to the mesh vertex buffer
                meshVertexHandle.WriteArray<Vector3D>(currentMeshOffset, 
                    vertices, 0, vertices.Length);

                // and release
                contour.VertexBuffer.ReleaseHandle();
                contour.VertexBuffer.CloseMapping();

                // increment to the next position
                currentMeshOffset += (ulong)(vertices.Length 
                    * Marshal.SizeOf(typeof(Vector3D)));
            }

            // done with vertex buffer
            smdc.VertexBuffer.ReleaseHandle();
            smdc.VertexBuffer.CloseMapping();

            cmsc1.Close();

            // construct the response
            MeshingResponse response = new MeshingResponse();
            response.StructureGuid = sdc.Id;
            response.SurfaceMeshGuid = smdc.Id;
            return response;
        }
    }
}
