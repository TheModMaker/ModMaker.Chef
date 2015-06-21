using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ModMaker.Chef
{
    /// <summary>
    /// Defines a file stored on a Chef server.
    /// </summary>
    public sealed class FileInfo : IDisposable
    {
        readonly string path, name, checksum, specificity;
        /// <summary>
        /// Contains the url of the file.
        /// </summary>
        readonly Uri url;
        /// <summary>
        /// Contains the web client used to download the file.
        /// </summary>
        readonly HttpClient webClient;
        /// <summary>
        /// Contains the streams created by this object.
        /// </summary>
        readonly List<Stream> streams;
        /// <summary>
        /// Contains the server this file belongs to.
        /// </summary>
        readonly ChefServer server;
        /// <summary>
        /// Contains whether the object has been disposed.
        /// </summary>
        bool _disposed = false;

        /// <summary>
        /// Gets the relative path of the file.
        /// </summary>
        public string Path
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return path;
            }
        }
        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string Name
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return name;
            }
        }
        /// <summary>
        /// Gets the checksum of the file.
        /// </summary>
        public string Checksum
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return checksum;
            }
        }
        /// <summary>
        /// Gets the specificity of the file (e.g. default).
        /// </summary>
        public string Specificity
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return specificity;
            }
        }

        /// <summary>
        /// Creates a new FileInfo from the given values.
        /// </summary>
        /// <param name="url">The url where the file is.</param>
        /// <param name="path">The relative path in the cookbook.</param>
        /// <param name="name">The name of the file.</param>
        /// <param name="checksum">The checksum of the file.</param>
        /// <param name="specificity">The specificity of the file.</param>
        /// <returns>A new FileInfo object.</returns>
        internal FileInfo(ChefServer server, string url, string path, string name, string specificity, string checksum)
        {
            Contract.Requires(server != null);
            Contract.Requires(url != null);
            Contract.Requires(path != null);
            Contract.Requires(name != null);
            Contract.Requires(checksum != null);
            Contract.Requires(specificity != null);

            this.webClient = new HttpClient();
            this.streams = new List<Stream>();

            this.server = server;
            this.url = new Uri(url);
            this.path = path;
            this.name = name;
            this.checksum = checksum;
            this.specificity = specificity;
        }
        /// <summary>
        /// Finalizer.
        /// </summary>
        ~FileInfo()
        {
            Dispose(false);
        }

        /// <summary>
        /// Opens a stream for the file.  Each call creates a new stream.  The 
        /// stream will be closed when Dispose is called on this object.
        /// </summary>
        /// <returns>A stream to read the file.</returns>
        public Stream OpenStream()
        {
            Contract.Ensures(Contract.Result<Stream>() != null);
            if (_disposed)
                throw new ObjectDisposedException("ChefFile:" + Name);

            return server.SendMessageStream(webClient, url).Result;
        }
        /// <summary>
        /// Opens a stream for the file.  Each call creates a new stream.  The 
        /// stream will be closed when Dispose is called on this object.
        /// </summary>
        /// <returns>A stream to read the file.</returns>
        public async Task<Stream> OpenStreamAsync()
        {
            Contract.Ensures(Contract.Result<Task<Stream>>() != null);
            if (_disposed)
                throw new ObjectDisposedException("ChefFile:" + Name);

            return await server.SendMessageStream(webClient, url);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            return "ChefFile:" + Name;
        }
        /// <summary>
        /// Determines whether the specified System.Object is equal to the current System.Object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj != null && obj is FileInfo &&
                ((FileInfo)obj).url.Equals(url) &&
                ((FileInfo)obj).Name.Equals(Name);
        }
        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current System.Object.</returns>
        public override int GetHashCode()
        {
            return url.GetHashCode() ^ Name.GetHashCode();
        }

        /// <summary>
        /// Disposes the object and closes any open file-streams.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                GC.SuppressFinalize(this);
                Dispose(true);
            }
        }
        /// <summary>
        /// Disposes the object and closes any open file-streams.
        /// </summary>
        /// <param name="disposing">Whether this was called from Dispose.</param>
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var s in streams)
                {
                    s.Dispose();
                }
                streams.Clear();

                webClient.Dispose();
            }
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(name != null);
            Contract.Invariant(path != null);
            Contract.Invariant(checksum != null);
            Contract.Invariant(specificity != null);

            Contract.Invariant(url != null);
            Contract.Invariant(webClient != null);
            Contract.Invariant(streams != null);
            Contract.Invariant(server != null);
        }
    }
}