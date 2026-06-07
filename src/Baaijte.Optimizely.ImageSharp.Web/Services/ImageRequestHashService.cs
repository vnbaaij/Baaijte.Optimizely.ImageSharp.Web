using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using EPiServer;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Baaijte.Optimizely.ImageSharp.Web.Services
{
    public class ImageRequestHashService 
    {
        private const string HashParameterName = "h";

        private static readonly HashSet<string> ProcessingCommands = new(StringComparer.OrdinalIgnoreCase)
        {
            "bgcolor",
            "compand",
            "format",
            "height",
            "quality",
            "ranchor",
            "rmode",
            "rsampler",
            "rxy",
            "width"
        };

        private readonly IOptionsMonitor<ImageRequestSigningOptions> _options;

        public ImageRequestHashService(IOptionsMonitor<ImageRequestSigningOptions> options)
        {
            _options = options;
        }

        public bool IsEnabled => _options.CurrentValue.Enabled != false;

        public bool IsAuthorized(HttpRequest request)
        {
            if (!IsEnabled)
            {
                return true;
            }

            var analysis = AnalyzeRequestTarget(GetRequestTarget(request));

            if (analysis.HasProcessingCommandsAfterHash)
            {
                return false;
            }

            if (!analysis.HasProcessingCommands)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(analysis.HashValue))
            {
                return false;
            }

            return string.Equals(analysis.HashValue, ComputeHash(analysis.SignedRequestTarget), StringComparison.OrdinalIgnoreCase);
        }

        public void Sign(UrlBuilder urlBuilder)
        {
            if (!IsEnabled || urlBuilder == null || urlBuilder.IsEmpty)
            {
                return;
            }

            urlBuilder.QueryCollection.Remove(HashParameterName);

            var analysis = AnalyzeRequestTarget(GetRequestTarget(urlBuilder.ToString()));

            if (!analysis.HasProcessingCommands)
            {
                return;
            }

            urlBuilder.QueryCollection.Add(HashParameterName, ComputeHash(analysis.SignedRequestTarget));
        }

        private string ComputeHash(string signedRequestTarget)
        {
            var payload = string.Concat(_options.CurrentValue.Salt, signedRequestTarget);
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));

            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private static string GetRequestTarget(HttpRequest request)
        {
            var path = request.PathBase.Add(request.Path).ToString();
            var query = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;

            return string.Concat(path, query);
        }

        private static string GetRequestTarget(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            var fragmentIndex = url.IndexOf('#');

            if (fragmentIndex >= 0)
            {
                url = url.Substring(0, fragmentIndex);
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
            {
                return string.Concat(absoluteUri.AbsolutePath, absoluteUri.Query);
            }

            return url;
        }

        private static RequestTargetAnalysis AnalyzeRequestTarget(string requestTarget)
        {
            var queryIndex = requestTarget.IndexOf('?');
            var path = queryIndex >= 0 ? requestTarget.Substring(0, queryIndex) : requestTarget;
            var rawQuery = queryIndex >= 0 && queryIndex + 1 < requestTarget.Length
                ? requestTarget.Substring(queryIndex + 1)
                : string.Empty;

            var analysis = new RequestTargetAnalysis
            {
                SignedRequestTarget = path
            };

            if (string.IsNullOrEmpty(rawQuery))
            {
                return analysis;
            }

            var segments = rawQuery.Split('&');
            var hashFound = false;
            var segmentStart = 0;
            var signedQuery = rawQuery;

            foreach (var segment in segments)
            {
                var parameterName = GetParameterName(segment);

                if (!hashFound && parameterName.Equals(HashParameterName, StringComparison.OrdinalIgnoreCase))
                {
                    analysis.HashValue = GetParameterValue(segment);
                    signedQuery = segmentStart == 0 ? string.Empty : rawQuery.Substring(0, segmentStart - 1);
                    hashFound = true;
                }
                else if (IsProcessingCommand(parameterName))
                {
                    if (hashFound)
                    {
                        analysis.HasProcessingCommandsAfterHash = true;
                    }
                    else
                    {
                        analysis.HasProcessingCommands = true;
                    }
                }

                segmentStart += segment.Length + 1;
            }

            if (analysis.HasProcessingCommands && !string.IsNullOrEmpty(signedQuery))
            {
                analysis.SignedRequestTarget = string.Concat(path, "?", signedQuery);
            }

            return analysis;
        }

        private static string GetParameterName(string segment)
        {
            var separatorIndex = segment.IndexOf('=');

            return separatorIndex >= 0 ? segment.Substring(0, separatorIndex) : segment;
        }

        private static string GetParameterValue(string segment)
        {
            var separatorIndex = segment.IndexOf('=');

            return separatorIndex >= 0 && separatorIndex + 1 < segment.Length
                ? segment.Substring(separatorIndex + 1)
                : string.Empty;
        }

        private static bool IsProcessingCommand(string parameterName)
        {
            return !string.IsNullOrEmpty(parameterName) && ProcessingCommands.Contains(parameterName);
        }

        private class RequestTargetAnalysis
        {
            public bool HasProcessingCommands { get; set; }

            public bool HasProcessingCommandsAfterHash { get; set; }

            public string HashValue { get; set; }

            public string SignedRequestTarget { get; set; }
        }
    }
}
