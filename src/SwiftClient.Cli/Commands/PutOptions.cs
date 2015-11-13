﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CommandLine;
using CommandLine.Text;

namespace SwiftClient.Cli
{
    [Verb("put", HelpText = "upload file")]
    public class PutOptions
    {
        [Option('c', "container", Required = true, HelpText = "swift container id")]
        public string Container { get; set; }

        [Option('o', "object", Required = false, HelpText = "swift object id, required only on single file upload")]
        public string Object { get; set; }

        [Option('f', "file", Required = true, HelpText = "input file or directory to be uploaded")]
        public string File { get; set; }

        [Option('b', "buffer", Required = false, Default = 2, HelpText = "buffer size in MB, default is 2MB")]
        public int BufferSize { get; set; }
    }
}
