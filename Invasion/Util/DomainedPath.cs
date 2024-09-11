using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invasion.Util
{
    public class DomainedPath(string localPath, string domain)
    {
        public string LocalPath { get; private set; } = localPath;
        public string Domain { get; private set; } = domain;

        public string FullPath => $"Assets/{Domain}/{LocalPath}";
    }
}
