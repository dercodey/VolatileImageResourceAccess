using NServiceBus;
using System;

namespace Contracts.DicomLoader
{

    public class ImageStored : IEvent
    {
        public Guid ImageGuid { get; set; }
        public double RepoGb { get; set; }
    }


    public class StructureStored : IEvent
    {
        public Guid StructureGuid { get; set; }
    }


    public class AssociationClosed : IEvent
    {
        public string[] SeriesInstanceUids { get; set; }
    }

    public class ScanCompleted : IEvent
    {
        public string ScanDirectory { get; set; }
    }
}
