﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Response
{
    public class ResponseExcludeDirectory
    {
        public int id { get; set; }

        public int credId { get; set; }

        public string directoryExclude { get; set; }
    }
}
