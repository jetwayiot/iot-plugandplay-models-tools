﻿using Azure.Core;
using Azure.Core.Pipeline;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.Resolver.Fetchers
{
    public class RemoteModelFetcher : IModelFetcher
    {
        private readonly ILogger _logger;
        private readonly HttpPipeline _pipeline;
        private readonly bool _tryExpanded;

        public RemoteModelFetcher(ILogger logger, ResolverClientOptions clientOptions)
        {
            _logger = logger;
            _pipeline = CreatePipeline(clientOptions);
            _tryExpanded = clientOptions.DependencyResolution == DependencyResolutionOption.TryFromExpanded;
        }

        public FetchResult Fetch(string dtmi, Uri repositoryUri, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<FetchResult> FetchAsync(string dtmi, Uri repositoryUri, CancellationToken cancellationToken = default)
        {
            Queue<string> work = new Queue<string>();

            if (_tryExpanded)
                work.Enqueue(GetPath(dtmi, repositoryUri, true));

            work.Enqueue(GetPath(dtmi, repositoryUri, false));

            string remoteFetchError = string.Empty;
            while (work.Count != 0 && !cancellationToken.IsCancellationRequested)
            {
                string tryContentPath = work.Dequeue();
                _logger.LogTrace(StandardStrings.FetchingContent(tryContentPath));

                string content = await EvaluatePathAsync(tryContentPath, cancellationToken);
                if (!string.IsNullOrEmpty(content))
                {
                    return new FetchResult()
                    {
                        Definition = content,
                        Path = tryContentPath
                    };
                }

                remoteFetchError = StandardStrings.ErrorAccessRemoteRepositoryModel(tryContentPath);
                _logger.LogWarning(remoteFetchError);
            }

            throw new RequestFailedException(remoteFetchError);
        }

        public string GetPath(string dtmi, Uri repositoryUri, bool expanded = false)
        {
            string absoluteUri = repositoryUri.AbsoluteUri;
            return DtmiConventions.DtmiToQualifiedPath(dtmi, absoluteUri, expanded);
        }

        private async Task<string> EvaluatePathAsync(string path, CancellationToken cancellationToken)
        {
            Request request = _pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.Uri = new RequestUriBuilder();
            request.Uri.Reset(new Uri(path));

            Response response = await _pipeline.SendRequestAsync(request, cancellationToken);
            if (response.Status >= 200 && response.Status <= 299)
            {
                return await GetContentAsync(response.ContentStream, cancellationToken);
            }

            return null;
        }

        public static async Task<string> GetContentAsync(Stream content, CancellationToken cancellationToken)
        {
            using (JsonDocument json = await JsonDocument.ParseAsync(content, default, cancellationToken))
            {
                JsonElement root = json.RootElement;
                return root.GetRawText();
            }
        }

        private static HttpPipeline CreatePipeline(ResolverClientOptions options)
        {
            return HttpPipelineBuilder.Build(options);
        }
    }
}
