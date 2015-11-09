﻿using System;
using System.Collections.Generic;
using System.Net;
using System.IO;

using SwiftClient.Extensions;
using System.Net.Http;
using System.Threading.Tasks;

namespace SwiftClient
{
    public abstract partial class SwiftClientBase : ISwiftClient, IDisposable
    {

        public Task<SwiftAccountResponse> HeadAccount()
        {
            return AuthorizeAndExecute(async (auth) =>
            {
                var url = SwiftUrlBuilder.GetAccountUrl(auth.StorageUrl);

                var request = new HttpRequestMessage(HttpMethod.Head, url);

                FillRequest(request, auth);

                try
                {
                    using (var response = await _client.SendAsync(request))
                    {
                        var result = GetResponse<SwiftAccountResponse>(response);

                        long totalBytes, containersCount, objectsCount;

                        if (long.TryParse(response.GetHeader(SwiftHeaderKeys.AccountBytesUsed), out totalBytes))
                        {
                            result.TotalBytes = totalBytes;
                        }

                        if (long.TryParse(response.GetHeader(SwiftHeaderKeys.AccountContainerCount), out containersCount))
                        {
                            result.ContainersCount = containersCount;
                        }

                        if (long.TryParse(response.GetHeader(SwiftHeaderKeys.AccountObjectCount), out objectsCount))
                        {
                            result.ObjectsCount = objectsCount;
                        }

                        return result;
                    }
                }
                catch (WebException e)
                {
                    return GetExceptionResponse<SwiftAccountResponse>(e, auth.StorageUrl);
                }
            });
        }

        public Task<SwiftAccountResponse> GetAccount(Dictionary<string, string> queryParams = null)
        {
            return AuthorizeAndExecute(async (auth) =>
            {
                var url = SwiftUrlBuilder.GetAccountUrl(auth.StorageUrl, queryParams);

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                FillRequest(request, auth);

                try
                {
                    using (var response = await _client.SendAsync(request))
                    {
                        var result = GetResponse<SwiftAccountResponse>(response);

                        long totalBytes, containersCount, objectsCount;

                        if (long.TryParse(response.GetHeader(SwiftHeaderKeys.AccountBytesUsed), out totalBytes))
                        {
                            result.TotalBytes = totalBytes;
                        }

                        if (long.TryParse(response.GetHeader(SwiftHeaderKeys.AccountContainerCount), out containersCount))
                        {
                            result.ContainersCount = containersCount;
                        }

                        if (long.TryParse(response.GetHeader(SwiftHeaderKeys.AccountObjectCount), out objectsCount))
                        {
                            result.ObjectsCount = objectsCount;
                        }

                        result.Info = await response.Content.ReadAsStringAsync();

                        return result;
                    }
                }
                catch (WebException e)
                {
                    return GetExceptionResponse<SwiftAccountResponse>(e, auth.StorageUrl);
                }
            });
        }

        public Task<SwiftResponse> PostAccount(Dictionary<string, string> headers = null)
        {
            return AuthorizeAndExecute(async (auth) =>
            {
                var url = SwiftUrlBuilder.GetAccountUrl(auth.StorageUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, url);

                FillRequest(request, auth, headers);

                try
                {
                    using (var response = await _client.SendAsync(request))
                    {
                        return GetResponse<SwiftResponse>(response);
                    }
                }
                catch (WebException e)
                {
                    return GetExceptionResponse<SwiftResponse>(e, auth.StorageUrl);
                }
            });
        }

    }
}
