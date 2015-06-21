using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ModMaker.Chef
{
    /// <summary>
    /// Defines a node in Chef.
    /// </summary>
    public sealed class Node
    {
        readonly Organization organization;
        readonly string name;
        readonly AttributeList automatic, normal, @default;
        readonly IList<string> runList;

        /// <summary>
        /// Gets the organization this node belongs to.
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
        /// Gets the name of the node.
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
        /// Gets the normal attributes for the node.
        /// </summary>
        public AttributeList Normal
        {
            get
            {
                Contract.Ensures(Contract.Result<AttributeList>() != null);
                Contract.Ensures(!Contract.Result<AttributeList>().IsReadOnly);
                return normal;
            }
        }
        /// <summary>
        /// Gets the automatic attributes for the node.  This attribute list is
        /// read-only.
        /// </summary>
        public AttributeList Automatic
        {
            get
            {
                Contract.Ensures(Contract.Result<AttributeList>() != null);
                Contract.Ensures(Contract.Result<AttributeList>().IsReadOnly);
                return automatic;
            }
        }
        /// <summary>
        /// Gets the default attributes for the node.  This attribute list is
        /// read-only.
        /// </summary>
        public AttributeList Default
        {
            get
            {
                Contract.Ensures(Contract.Result<AttributeList>() != null);
                Contract.Ensures(Contract.Result<AttributeList>().IsReadOnly);
                return @default;
            }
        }
        /// <summary>
        /// Gets the run-list for the node.
        /// </summary>
        public IList<string> RunList
        {
            get
            {
                Contract.Ensures(Contract.Result<IList<string>>() != null);
                return runList;
            }
        }

        /// <summary>
        /// Loads a node from the given Json string.
        /// </summary>
        /// <param name="org">The organization this node belongs to.</param>
        /// <param name="str">The Json string to parse.</param>
        internal Node(Organization org, string str)
        {
            Contract.Requires(org != null);
            Contract.Requires(str != null);

            JObject data = JObject.Parse(str);
            this.name = data.Value<string>("name");
            this.organization = org;
            this.normal = new AttributeList(data.SelectToken("normal"));
            this.automatic = new AttributeList(data.SelectToken("automatic")).MakeReadOnly();
            this.@default = new AttributeList(data.SelectToken("default")).MakeReadOnly();
            this.runList = data.SelectToken("run_list").Values<string>().ToList();
        }

        /// <summary>
        /// Deletes the node from the server.  After this call, use of this 
        /// object is invalid and may be undefined.
        /// </summary>
        public void Delete()
        {
            DeleteAsync().Wait();
        }
        /// <summary>
        /// Deletes the node from the server.  After this call, use of this 
        /// object is invalid and may be undefined.
        /// </summary>
        public async Task DeleteAsync()
        {
            // Send the DELETE request.
            await Organization.Server.SendMessage(
                "/organizations/" + Organization.Name + "/nodes/" + Name,
                HttpMethod.Delete, string.Empty);

            Organization.ClearCache();
        }

        /// <summary>
        /// Saves any changes made to this node to the server.
        /// </summary>
        public void SaveChanges()
        {
            SaveChangesAsync().Wait();
        }
        /// <summary>
        /// Saves any changes made to this node to the server.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            var data = new
            {
                name = Name,
                run_list = RunList.ToArray(),
                normal = Normal.ToData(),
                automatic = Automatic.ToData(),
                @default = Default.ToData(),
            };
            string body = JsonConvert.SerializeObject(data);
            await Organization.Server.SendMessage(
                "/organizations/" + Organization.Name + "/nodes/" + Name, 
                HttpMethod.Put, body);
        }
        
        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(organization != null);
            Contract.Invariant(name != null);
            Contract.Invariant(automatic != null);
            Contract.Invariant(normal != null);
            Contract.Invariant(@default != null);
            Contract.Invariant(runList != null);
        }
    }
}