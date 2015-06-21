using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ModMaker.Chef
{
    /// <summary>
    /// Defines a cookbook version in Chef.
    /// </summary>
    public sealed class CookbookVersion
    {
        bool frozen;
        readonly string version;
        readonly Cookbook cookbook;
        readonly CookbookMetadata metadata;
        readonly ReadOnlyCollection<FileInfo> files, definitions, libraries,
            attributes, recipes, providers, resources, templates, rootFiles;

        /// <summary>
        /// Gets the files for the cookbook version.
        /// </summary>
        public ReadOnlyCollection<FileInfo> Files
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<FileInfo>>() != null);
                return files;
            }
        }
        /// <summary>
        /// Gets the definitions for the cookbook version.
        /// </summary>
        public ReadOnlyCollection<FileInfo> Definitions
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<FileInfo>>() != null);
                return definitions;
            }
        }
        /// <summary>
        /// Gets the libraries for the cookbook version.
        /// </summary>
        public ReadOnlyCollection<FileInfo> Libraries
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<FileInfo>>() != null);
                return libraries;
            }
        }
        /// <summary>
        /// Gets the attributes for the cookbook version.
        /// </summary>
        public ReadOnlyCollection<FileInfo> Attributes
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<FileInfo>>() != null);
                return attributes;
            }
        }
        /// <summary>
        /// Gets the recipes for the cookbook version.
        /// </summary>
        public ReadOnlyCollection<FileInfo> Recipes
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<FileInfo>>() != null);
                return recipes;
            }
        }
        /// <summary>
        /// Gets the providers for the cookbook version.
        /// </summary>
        public ReadOnlyCollection<FileInfo> Providers
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<FileInfo>>() != null);
                return providers;
            }
        }
        /// <summary>
        /// Gets the resources for the cookbook version.
        /// </summary>
        public ReadOnlyCollection<FileInfo> Resources
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<FileInfo>>() != null);
                return resources;
            }
        }
        /// <summary>
        /// Gets the templates for the cookbook version.
        /// </summary>
        public ReadOnlyCollection<FileInfo> Templates
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<FileInfo>>() != null);
                return templates;
            }
        }
        /// <summary>
        /// Gets the rootFiles for the cookbook version.
        /// </summary>
        public ReadOnlyCollection<FileInfo> RootFiles
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<FileInfo>>() != null);
                return rootFiles;
            }
        }

        /// <summary>
        /// Gets the cookbook this is for.
        /// </summary>
        public Cookbook Cookbook
        {
            get
            {
                Contract.Ensures(Contract.Result<Cookbook>() != null);
                return cookbook;
            }
        }
        /// <summary>
        /// Gets the version string for this version.
        /// </summary>
        public string Version
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return version;
            }
        }
        /// <summary>
        /// Gets the metadata about this version.
        /// </summary>
        public CookbookMetadata Metadata
        {
            get
            {
                Contract.Ensures(Contract.Result<CookbookMetadata>() != null);
                return metadata;
            }
        }
        /// <summary>
        /// Gets or sets whether the cookbook is frozen.
        /// </summary>
        public bool IsFrozen 
        {
            get { return frozen; }
            //private set; 
        }

        /// <summary>
        /// Loads the cookbook version for the given cookbook.
        /// </summary>
        /// <param name="str">The string JSON data.</param>
        /// <param name="org">The cookbook to load from.</param>
        /// <returns>The resulting cookbook version.</returns>
        internal CookbookVersion(Cookbook cookbook, string str)
        {
            Contract.Requires(cookbook != null);
            Contract.Requires(str != null);

            JObject data = JObject.Parse(str);
            ChefServer server = cookbook.Organization.Server;

            this.files = Convert(server, data.SelectToken("files"));
            this.definitions = Convert(server, data.SelectToken("definitions"));
            this.libraries = Convert(server, data.SelectToken("libraries"));
            this.attributes = Convert(server, data.SelectToken("attributes"));
            this.recipes = Convert(server, data.SelectToken("recipes"));
            this.providers = Convert(server, data.SelectToken("providers"));
            this.resources = Convert(server, data.SelectToken("resources"));
            this.templates = Convert(server, data.SelectToken("templates"));
            this.rootFiles = Convert(server, data.SelectToken("root_files"));

            this.cookbook = cookbook;
            this.version = data.Value<string>("version");
            this.frozen = data.Value<bool>("frozen?");

            JToken metadata = data.SelectToken("metadata");
            this.metadata = new CookbookMetadata(
                name: metadata.Value<string>("name") ?? cookbook.Name,
                version: metadata.Value<string>("version") ?? data.Value<string>("version") ?? "",
                description: metadata.Value<string>("description") ?? "",
                longDescription: metadata.Value<string>("long_description") ?? "",
                maintainer: metadata.Value<string>("maintainer") ?? "",
                maintainerEmail: metadata.Value<string>("maintainer_email") ?? "",
                license: metadata.Value<string>("license") ?? "",
                recipes: new AttributeList(metadata.SelectToken("recipes")),
                attributes: new AttributeList(metadata.SelectToken("attributes")),
                dependencies: new AttributeList(metadata.SelectToken("dependencies")),
                suggestions: new AttributeList(metadata.SelectToken("suggestions")),
                platforms: new AttributeList(metadata.SelectToken("platforms")),
                groupings: new AttributeList(metadata.SelectToken("groupings")),
                recommendations: new AttributeList(metadata.SelectToken("recommendations")),
                providing: new AttributeList(metadata.SelectToken("providing")),
                conflicting: new AttributeList(metadata.SelectToken("conflicting")),
                replacing: new AttributeList(metadata.SelectToken("replacing")));
        }

        /// <summary>
        /// Deletes the cookbook version from the server.  After this call, use
        /// of this object is invalid and may be undefined.
        /// </summary>
        public void Delete()
        {
            DeleteAsync().Wait();
        }
        /// <summary>
        /// Deletes the cookbook version from the server.  After this call, use 
        /// of this object is invalid and may be undefined.
        /// </summary>
        public async Task DeleteAsync()
        {
            // Send the DELETE request.
            await Cookbook.Organization.Server.SendMessage(
                "/organizations/" + Cookbook.Organization.Name + "/cookbooks/" + Cookbook.Name + "/" + Version,
                HttpMethod.Delete, string.Empty);

            Cookbook.ClearCache();
        }

        /// <summary>
        /// Converts an array of data files to a read-only collection of FileInfo objects.
        /// </summary>
        /// <param name="files">The file array to convert, may be null.</param>
        /// <returns>A read-only collection of FileInfo's.</returns>
        static ReadOnlyCollection<FileInfo> Convert(ChefServer server, JToken files)
        {
            Contract.Requires(server != null);
            Contract.Requires(files != null);
            Contract.Ensures(Contract.Result<ReadOnlyCollection<FileInfo>>() != null);

            return new ReadOnlyCollection<FileInfo>(
                files
                .Select(s => new FileInfo(server, s.Value<string>("url"), s.Value<string>("path"), s.Value<string>("name"), s.Value<string>("specificity"), s.Value<string>("checksum")))
                .ToList());
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(files != null);
            Contract.Invariant(definitions != null);
            Contract.Invariant(libraries != null);
            Contract.Invariant(attributes != null);
            Contract.Invariant(recipes != null);
            Contract.Invariant(providers != null);
            Contract.Invariant(resources != null);
            Contract.Invariant(templates != null);
            Contract.Invariant(rootFiles != null);

            Contract.Invariant(cookbook != null);
            Contract.Invariant(version != null);
            Contract.Invariant(metadata != null);
        }
    }
}