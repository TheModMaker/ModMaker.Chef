using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace ModMaker.Chef
{
    /// <summary>
    /// Defines a cookbook in Chef.
    /// </summary>
    public sealed class Cookbook
    {
        /// <summary>
        /// Contains the backing field for the property that contains the name.
        /// </summary>
        readonly string name;
        /// <summary>
        /// Contains the backing field for the property that contains the organization.
        /// </summary>
        readonly Organization organization;
        /// <summary>
        /// Contains a cache of cookbook versions.
        /// </summary>
        CookbookVersion[] versions;

        /// <summary>
        /// Gets the name of the cookbook.
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
        /// Gets the organization that this cookbook is in.
        /// </summary>
        public Organization Organization
        {
            get
            {
                Contract.Ensures(Contract.Result<Organization>() != null);
                return organization;
            }
        }
        /// <summary>
        /// Gets the cookbook versions available.
        /// </summary>
        public ReadOnlyCollection<CookbookVersion> Versions 
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<CookbookVersion>>() != null);

                lock (this)
                {
                    if (versions == null)
                        versions = GetVersions();

                    return new ReadOnlyCollection<CookbookVersion>(versions);
                }
            }
        }

        /// <summary>
        /// Loads the cookbook for the given organization.
        /// </summary>
        /// <param name="str">The string JSON data.</param>
        /// <param name="org">The organization to load from.</param>
        internal Cookbook(Organization org, string name)
        {
            Contract.Requires(org != null);
            Contract.Requires(name != null);

            this.organization = org;
            this.name = name;
        }

        /// <summary>
        /// Gets a version with the given name.  Returns null if not found.
        /// </summary>
        /// <param name="version">The version of the cookbook to get.</param>
        /// <returns>The version with the given number; or null on error.</returns>
        public CookbookVersion FindVersion(string version)
        {
            Contract.Requires(version != null);

            try
            {
                string client = Organization.Server.SendMessage("/organizations/" + Organization.Name + "/cookbooks/" + Name + "/" + name).Result;
                return new CookbookVersion(this, client);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes the cookbook from the server.  After this call, use of this 
        /// object is invalid and may be undefined.
        /// </summary>
        public void Delete()
        {
            DeleteAsync().Wait();
        }
        /// <summary>
        /// Deletes the cookbook from the server.  After this call, use of this 
        /// object is invalid and may be undefined.
        /// </summary>
        public async Task DeleteAsync()
        {
            // Send the DELETE request.
            CookbookVersion[] versions = this.versions ?? GetVersions();
            Task[] deletes = new Task[versions.Length];
            for (int i = 0; i < versions.Length; i++)
            {
                deletes[i] = versions[i].DeleteAsync();
            }

            // Await the requests
            await Task.WhenAll(deletes);

            ClearCache();
            Organization.ClearCache();
        }

        /// <summary>
        /// Clears the cache of data.
        /// </summary>
        public void ClearCache()
        {
            versions = null;
        }

        /// <summary>
        /// Loads the versions for the current cookbook.
        /// </summary>
        /// <returns>The versions for the cookbook.</returns>
        CookbookVersion[] GetVersions()
        {
            Contract.Ensures(Contract.Result<CookbookVersion[]>() != null);

            string result = Organization.Server.SendMessage(
                    "/organizations/" + Organization.Name + "/cookbooks/" + Name
                ).Result;

            JObject data = JObject.Parse(result);
            CookbookVersion[] ret = new CookbookVersion[data.Count];
            int x = 0;
            foreach (var tok in data.SelectToken(Name).SelectToken("versions"))
            {
                string version = Organization.Server.SendMessageRaw(new Uri(tok.Value<string>("url"))).Result;
                ret[x++] = new CookbookVersion(this, version);
            }

            return ret;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(name != null);
            Contract.Invariant(organization != null);
        }
    }
}