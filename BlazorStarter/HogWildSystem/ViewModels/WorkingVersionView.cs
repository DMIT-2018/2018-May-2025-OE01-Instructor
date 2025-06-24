using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HogWildSystem.ViewModels
{
    public class WorkingVersionView
    {
        public int VersionId { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTime AsOfDate { get; set; }
        public string Comments { get; set; } = string.Empty;
    }
}
