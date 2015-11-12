﻿using Humanizer.Bytes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SwiftClient.Cli
{
    public static class PutCommand
    {
        static long bufferSize = Convert.ToInt64(ByteSize.FromMegabytes(2).Bytes);

        public static int Run(PutOptions options, SwiftClient client)
        {
            options.File = options.File.Replace('"', ' ').Trim();
            if (!File.Exists(options.File))
            {
                Console.WriteLine($"File not found {options.File}");
                return 404;
            }

            Console.WriteLine($"Uploading {options.File} to {options.Object}");

            var response = new SwiftBaseResponse();
            var fileName = Path.GetFileNameWithoutExtension(options.File);
            string containerTemp = options.Container + "_tmp";
            byte[] buffer = new byte[bufferSize];

            response = client.PutContainer(containerTemp).Result;

            if (!response.IsSuccess)
            {
                Console.WriteLine($"Put tmp container error {response.Reason}");
                return 500;
            }

            response = client.PutContainer(options.Container).Result;

            if (!response.IsSuccess)
            {
                Console.WriteLine($"Put container error {response.Reason}");
                return 500;
            }

            using (var stream = File.OpenRead(options.File))
            {
                int chunks = 0;
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    Console.Write('.');
                    using (MemoryStream tmpStream = new MemoryStream())
                    {
                        tmpStream.Write(buffer, 0, bytesRead);

                        response = client.PutChunkedObject(containerTemp, fileName, tmpStream.ToArray(), chunks).Result;
                        if (!response.IsSuccess)
                        {
                            Console.WriteLine($"Uploading error {response.Reason}");
                            return 500;
                        }
                    }
                    chunks++;
                }

                Console.Write(Environment.NewLine);

                // use manifest to merge chunks
                response = client.PutManifest(containerTemp, fileName).Result;

                if (!response.IsSuccess)
                {
                    Console.WriteLine($"Put manifest error {response.Reason}");
                    return 500;
                }

                // copy chunks to new file and set some meta data info about the file (filename, contentype)
                response = client.CopyObject(containerTemp, fileName, options.Container, options.Object, new Dictionary<string, string>
                {
                    { "X-Object-Meta-Filename", fileName },
                    { "X-Object-Meta-Contenttype", Path.GetExtension(options.File) }
                }).Result;

                if (!response.IsSuccess)
                {
                    Console.WriteLine($"Copy object error {response.Reason}");
                    return 500;
                }

                // cleanup temp
                response = client.DeleteContainerWithContents(containerTemp).Result;

                if (!response.IsSuccess)
                {
                    Console.WriteLine($"Cleanup error {response.Reason}");
                    return 500;
                }

                Console.WriteLine($"Upload done");
            }

            return 0;
        }
    }
}
