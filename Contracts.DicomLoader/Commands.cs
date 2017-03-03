using NServiceBus;

namespace Contracts.DicomLoader
{
    /// <summary>
    /// TODO: separate these in to an interface / contracts assembly
    /// </summary>
    public class ScanDirectory : ICommand
    {
        public string Path { get; set; }
    }
}
