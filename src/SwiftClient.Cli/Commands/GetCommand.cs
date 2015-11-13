﻿using Humanizer.Bytes;
using System;
using System.IO;

namespace SwiftClient.Cli
{
    public static class GetCommand
    {
        public static int Run(GetOptions options, SwiftClient client)
        {
            int bufferSize = Convert.ToInt32(ByteSize.FromMegabytes(options.BufferSize).Bytes);
            var headObject = client.HeadObject(options.Container, options.Object).Result;

            if (headObject.IsSuccess)
            {
                var stream = new BufferedHTTPStream((start, end) =>
                {
                    var response = client.GetObjectRange(options.Container, options.Object, start, end).Result;

                    if (!response.IsSuccess)
                    {
                        Logger.LogError(response.Reason);
                    }

                    return response.Stream;

                }, () => headObject.ContentLength);

                using (var fs = File.OpenWrite(options.File))
                {
                    stream.CopyTo(fs, bufferSize);
                }

                stream.Dispose();

                Console.WriteLine($"{options.Container}/{options.Object} downloaded to {options.File} ");
                return 0;
            }
            else
            {
                Logger.LogError(headObject.Reason);
                return 404;
            }
        }
    }
}
