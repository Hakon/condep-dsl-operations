using System.IO;
using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Operations.Remote.Installation.Zip
{
    public class UnZipOperation : RemoteOperation
    {
        private readonly string _filePath;
        private readonly string _destPath;

        public UnZipOperation(string filePath, string destPath)
        {
            _filePath = filePath;
            _destPath = destPath;
        }

        public override Result Execute(IOfferRemoteOperations remote, ServerConfig server, ConDepSettings settings, CancellationToken token)
        {
            return remote.Execute.DosCommand(string.Format(@"%ProgramData%\chocolatey\tools\7z.exe x -y -o""{1}"" ""{0}""", _filePath, _destPath)).Result;
        }

        public override string Name
        {
            get { return string.Format("UnZip ({0})", Path.GetFileName(_filePath)); }
        }
    }
}
