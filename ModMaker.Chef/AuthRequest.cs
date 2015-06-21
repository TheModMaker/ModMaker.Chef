using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Signers;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ModMaker.Chef
{
    /// <summary>
    /// Defines a request to the server that includes the authentication 
    /// information.  This is used to help generate requests to the server
    /// and handles the actual request.
    /// 
    /// Based on: https://github.com/mattberther/dotnet-chef-api
    /// </summary>
    static class AuthRequest
    {
        /// <summary>
        /// Creates a new AuthRequest using the given data.
        /// </summary>
        /// <param name="client">The name of the client to use.</param>
        /// <param name="requestUri">The Uri of the request.</param>
        /// <param name="privateKey">The client private key.</param>
        /// <param name="method">The method of the request.</param>
        /// <param name="body">The body of the request.</param>
        [Pure]
        public static HttpRequestMessage Create(string client, Uri requestUri, AsymmetricKeyParameter privateKey)
        {
            Contract.Requires(client != null);
            Contract.Requires(requestUri != null);
            Contract.Requires(privateKey != null);
            Contract.Requires(privateKey.IsPrivate);
            Contract.Ensures(Contract.Result<HttpRequestMessage>() != null);

            return Create(client, requestUri, privateKey, HttpMethod.Get, string.Empty);
        }
        /// <summary>
        /// Creates a new AuthRequest using the given data.
        /// </summary>
        /// <param name="client">The name of the client to use.</param>
        /// <param name="requestUri">The Uri of the request.</param>
        /// <param name="privateKey">The client private key.</param>
        /// <param name="method">The method of the request.</param>
        /// <param name="body">The body of the request.</param>
        [Pure]
        public static HttpRequestMessage Create(string client, Uri requestUri, AsymmetricKeyParameter privateKey, HttpMethod method, string body)
        {
            Contract.Requires(client != null);
            Contract.Requires(requestUri != null);
            Contract.Requires(privateKey != null);
            Contract.Requires(method != null);
            Contract.Requires(body != null);
            Contract.Requires(privateKey.IsPrivate);
            Contract.Ensures(Contract.Result<HttpRequestMessage>() != null);

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string signature = GenerateSignature(client, requestUri, privateKey, method, body, timestamp);

            var requestMessage = new HttpRequestMessage(method, requestUri);
            AddHeaders(requestMessage.Headers, signature, requestUri, client, timestamp, body);

            if (method != HttpMethod.Get)
            {
                requestMessage.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(body));
                requestMessage.Content.Headers.Add("Content-Type", "application/json");
            }

            return requestMessage;
        }

        /// <summary>
        /// Adds the security headers to the given object.
        /// </summary>
        /// <param name="client">The name of the client to use.</param>
        /// <param name="requestUri">The Uri of the request.</param>
        /// <param name="body">The body of the request.</param>
        /// <param name="headers">The object to add the headers to.</param>
        /// <param name="signature">The signature of the request.</param>
        /// <param name="timestamp">The timestamp of the request.</param>
        [Pure]
        static void AddHeaders(HttpHeaders headers, string signature, Uri requestUri, string client, string timestamp, string body)
        {
            Contract.Requires(headers != null);
            Contract.Requires(signature != null);
            Contract.Requires(requestUri != null);
            Contract.Requires(client != null);
            Contract.Requires(timestamp != null);
            Contract.Requires(body != null);

            headers.Add("Accept", "application/json");
            headers.Add("X-Ops-Sign", "algorithm=sha1;version=1.0");
            headers.Add("X-Ops-UserId", client);
            headers.Add("X-Ops-Timestamp", timestamp);
            headers.Add("X-Ops-Content-Hash", body.ToBase64EncodedSha1String());
            headers.Add("Host", requestUri.Host + ":" + requestUri.Port);
            headers.Add("X-Chef-Version", "11.4.0");
            
            var i = 1;
            foreach (var line in signature.Split(60))
            {
                headers.Add("X-Ops-Authorization-" + (i++), line);
            }
        }
        /// <summary>
        /// Generates the signature line for the request.
        /// </summary>
        /// <param name="client">The name of the client to use.</param>
        /// <param name="requestUri">The Uri of the request.</param>
        /// <param name="privateKey">The client private key.</param>
        /// <param name="method">The method of the request.</param>
        /// <param name="body">The body of the request.</param>
        /// <param name="timestamp">The string timestamp.</param>
        /// <returns>The signature line for the request.</returns>
        [Pure]
        static string GenerateSignature(string client, Uri requestUri, AsymmetricKeyParameter privateKey, HttpMethod method, string body, string timestamp)
        {
            Contract.Requires(client != null);
            Contract.Requires(requestUri != null);
            Contract.Requires(privateKey != null);
            Contract.Requires(method != null);
            Contract.Requires(body != null);
            Contract.Requires(timestamp != null);
            Contract.Requires(privateKey.IsPrivate);
            Contract.Ensures(Contract.Result<string>() != null);

            string canonicalHeader =
                String.Format(
                    "Method:{0}\nHashed Path:{1}\nX-Ops-Content-Hash:{4}\nX-Ops-Timestamp:{3}\nX-Ops-UserId:{2}",
                    method,
                    requestUri.AbsolutePath.ToBase64EncodedSha1String(),
                    client,
                    timestamp,
                    body.ToBase64EncodedSha1String());

            byte[] input = Encoding.UTF8.GetBytes(canonicalHeader);
            ISigner signer = new RsaDigestSigner(new NullDigest());
            signer.Init(true, privateKey);
            signer.BlockUpdate(input, 0, input.Length);

            return Convert.ToBase64String(signer.GenerateSignature());
        }
    }
}