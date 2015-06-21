using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ModMaker.Chef
{
    /// <summary>
    /// Defines a connection to a chef server.  This is the starting point for
    /// all Chef server actions.  Creating a connection involves calling one of
    /// the CreateFrom* methods.
    /// </summary>
    /// <remarks>
    /// It was chosen to make them static Create methods rather than constructors
    /// because constructors would require the using app to have the BouncyCastle
    /// dll as a reference.  The dll is required to run this library, but the
    /// client app does not need to reference it itself.
    /// </remarks>
    public sealed class ChefServer
    {
        readonly string client;
        readonly Uri baseUri;
        /// <summary>
        /// Contains the private-key of the user to connect with.
        /// </summary>
        readonly AsymmetricKeyParameter privateKey;
        /// <summary>
        /// Contains the organizations for the user.
        /// </summary>
        Organization[] orgs;

        /// <summary>
        /// Gets the name of the client.
        /// </summary>
        public string Client
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return client;
            }
        }
        /// <summary>
        /// Gets the base Uri of the server.
        /// </summary>
        public Uri BaseUri
        {
            get
            {
                Contract.Ensures(Contract.Result<Uri>() != null);
                return baseUri;
            }
        }
        /// <summary>
        /// Contains the organizations for the current user.
        /// </summary>
        public ReadOnlyCollection<Organization> Organizations
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<Organization>>() != null);

                lock (this)
                {
                    if (orgs == null)
                        orgs = GetOrgs();

                    return new ReadOnlyCollection<Organization>(orgs);
                }
            }
        }

        /// <summary>
        /// Creates a new ChefServer connection that uses the given info.
        /// </summary>
        /// <param name="baseUri">The base Uri for the server.</param>
        /// <param name="privateKey">The private key of the client.</param>
        /// <param name="name">The name of the client.</param>
        private ChefServer(Uri baseUri, AsymmetricKeyParameter privateKey, string name)
        {
            Contract.Requires(baseUri != null);
            Contract.Requires(privateKey != null);
            Contract.Requires(name != null);
            Contract.Requires(privateKey.IsPrivate);

            this.baseUri = baseUri;
            this.privateKey = privateKey;
            this.client = name;
        }

        /// <summary>
        /// Creates a new ChefServer connection that uses the private key in the string.
        /// </summary>
        /// <param name="baseUri">The base Uri for the server.</param>
        /// <param name="privateKey">The private key of the client.</param>
        /// <param name="name">The name of the client.</param>
        /// <returns>A new ChefServer instance.</returns>
        public static ChefServer CreateFromString(Uri baseUri, string privateKey, string name)
        {
            Contract.Requires<ArgumentNullException>(baseUri != null);
            Contract.Requires<ArgumentNullException>(privateKey != null);
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Ensures(Contract.Result<ChefServer>() != null);

            var pemReader = new PemReader(new StringReader(privateKey));
            AsymmetricKeyParameter key = ((AsymmetricCipherKeyPair)pemReader.ReadObject()).Private;

            return new ChefServer(baseUri, key, name);
        }
        /// <summary>
        /// Creates a new ChefServer connection that uses the private key in the file
        /// </summary>
        /// <param name="baseUri">The base Uri for the server.</param>
        /// <param name="privateKeyPath">The path to the private key of the client.</param>
        /// <param name="name">The name of the client.</param>
        /// <returns>A new ChefServer instance.</returns>
        public static ChefServer CreateFromPath(Uri baseUri, string privateKeyPath, string name)
        {
            Contract.Requires<ArgumentNullException>(baseUri != null);
            Contract.Requires<ArgumentNullException>(privateKeyPath != null);
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<ArgumentException>(File.Exists(privateKeyPath));
            Contract.Ensures(Contract.Result<ChefServer>() != null);

            string pk = File.ReadAllText(privateKeyPath);
            return CreateFromString(baseUri, pk, name);
        }
        /// <summary>
        /// Creates a new ChefServer connection that uses the private key in the file
        /// </summary>
        /// <param name="baseUri">The base Uri for the server.</param>
        /// <param name="privateKey">The the private key of the client.</param>
        /// <param name="name">The name of the client.</param>
        /// <returns>A new ChefServer instance.</returns>
        public static ChefServer CreateFromPath(Uri baseUri, Stream privateKey, string name)
        {
            Contract.Requires<ArgumentNullException>(baseUri != null);
            Contract.Requires<ArgumentNullException>(privateKey != null);
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Ensures(Contract.Result<ChefServer>() != null);

            using (var reader = new StreamReader(privateKey, null, true, 0, true))
            {
                string pk = reader.ReadToEnd();
                return CreateFromString(baseUri, pk, name);
            }
        }
        /// <summary>
        /// Creates a new ChefServer connection that uses the private key provided.
        /// </summary>
        /// <param name="baseUri">The base Uri for the server.</param>
        /// <param name="privateKey">The private key of the client.</param>
        /// <param name="name">The name of the client.</param>
        /// <returns>A new ChefServer instance.</returns>
        public static ChefServer CreateFromPrivateKey(Uri baseUri, AsymmetricKeyParameter privateKey, string name)
        {
            Contract.Requires<ArgumentNullException>(baseUri != null);
            Contract.Requires<ArgumentNullException>(privateKey != null);
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<ArgumentException>(privateKey.IsPrivate);
            Contract.Ensures(Contract.Result<ChefServer>() != null);

            return new ChefServer(baseUri, privateKey, name);
        }

        /// <summary>
        /// Clears the cache of data.
        /// </summary>
        public void ClearCache()
        {
            orgs = null;
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="relativePath">The relative path of the request.</param>
        /// <param name="method">The http method to send the request.</param>
        /// <param name="body">The body of the request.</param>
        /// <returns>The string result of the request.</returns>
        internal async Task<string> SendMessage(string relativePath, HttpMethod method = null, string body = null)
        {
            Contract.Requires(relativePath != null);
            Contract.Ensures(Contract.Result<Task<string>>() != null);

            method = method ?? HttpMethod.Get;
            body = body ?? string.Empty;

            var request = AuthRequest.Create(Client, new Uri(BaseUri, relativePath), privateKey, method, body);
            return await Helpers.SendRequestAsync(request);
        }
        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="relativePath">The relative path of the request.</param>
        /// <param name="method">The http method to send the request.</param>
        /// <param name="body">The body of the request.</param>
        /// <returns>The string result of the request.</returns>
        internal async Task<string> SendMessageRaw(Uri path, HttpMethod method = null, string body = null)
        {
            Contract.Requires(path != null);
            Contract.Ensures(Contract.Result<Task<string>>() != null);

            method = method ?? HttpMethod.Get;
            body = body ?? string.Empty;

            var request = AuthRequest.Create(Client, path, privateKey, method, body);
            return await Helpers.SendRequestAsync(request);
        }
        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="relativePath">The relative path of the request.</param>
        /// <param name="method">The http method to send the request.</param>
        /// <param name="body">The body of the request.</param>
        /// <returns>The string result of the request.</returns>
        internal async Task<Stream> SendMessageStream(HttpClient client, Uri path, HttpMethod method = null, string body = null)
        {
            Contract.Requires(client != null);
            Contract.Requires(path != null);
            Contract.Ensures(Contract.Result<Task<Stream>>() != null);

            method = method ?? HttpMethod.Get;
            body = body ?? string.Empty;

            var request = AuthRequest.Create(Client, path, privateKey, method, body);
            var result = await client.SendAsync(request);
            result.EnsureSuccessStatusCode();

            return await result.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            return "Chef:" + BaseUri;
        }
        /// <summary>
        /// Determines whether the specified System.Object is equal to the current System.Object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj != null && obj is ChefServer &&
                ((ChefServer)obj).BaseUri.Equals(BaseUri) &&
                ((ChefServer)obj).privateKey.Equals(privateKey) &&
                ((ChefServer)obj).Client.Equals(Client);
        }
        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current System.Object.</returns>
        public override int GetHashCode()
        {
            return BaseUri.GetHashCode() ^ Client.GetHashCode();
        }

        /// <summary>
        /// Loads the organizations for the current user.
        /// </summary>
        /// <returns>The organizations for the current user.</returns>
        Organization[] GetOrgs()
        {
            Contract.Ensures(Contract.Result<Organization[]>() != null);

            HttpRequestMessage request = AuthRequest.Create(Client, new Uri(BaseUri, "/users/" + Client + "/organizations"), privateKey);
            string result = Helpers.SendRequestAsync(request).Result;
            return Organization.GetOrgs(this, result);
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(baseUri != null);
            Contract.Invariant(privateKey != null);
            Contract.Invariant(privateKey.IsPrivate);
            Contract.Invariant(client != null);
        }
    }
}