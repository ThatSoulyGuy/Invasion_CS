using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invasion.Util
{
    public class DomainedPath
    {
        public string LocalPath { get; private set; }
        public string Domain { get; private set; }

        public string FullPath { get; }

        public DomainedPath(string localPath, string domain)
        {
            LocalPath = localPath;
            Domain = domain;
            FullPath = $"Assets/{Domain}/{LocalPath}";
        }
        public DomainedPath(string fullPath)
        {
            LocalPath = fullPath.Split('/')[2];
            Domain = fullPath.Split('/')[1];
            FullPath = fullPath;
        }
    }
}
