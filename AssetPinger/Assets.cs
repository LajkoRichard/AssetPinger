using System;
using System.Collections.Generic;

namespace AssetPinger
{
    public partial class Assets
    {
        public long Id { get; set; }
        public string DeviceType { get; set; }
        public string Serial { get; set; }
        public string Mac { get; set; }
        public string User { get; set; }
        public string Knox { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string Ip { get; set; }
        public string Output { get; set; }
        public string Input { get; set; }
        public string Repair { get; set; }
        public bool IsArchive { get; set; }
        public bool IsActive { get; set; }
        public bool IsScrapped { get; set; }
        public DateTime LastActiveTime { get; set; }
    }
}
