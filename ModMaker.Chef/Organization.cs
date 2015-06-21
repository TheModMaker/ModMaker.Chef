using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ModMaker.Chef
{
    /// <summary>
    /// Defines an organization within Chef.
    /// </summary>
    public sealed class Organization
    {
        readonly ChefServer server;
        readonly string name, fullName;
        /// <summary>
        /// Contains the clients for the organization.
        /// </summary>
        Client[] clients;
        /// <summary>
        /// Contains the cookbooks for the organization.
        /// </summary>
        Cookbook[] cookbooks;
        /// <summary>
        /// Contains the nodes for the organization.
        /// </summary>
        Node[] nodes;

        /// <summary>
        /// Gets the name of the organization.
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
        /// Gets the full name of the organization.
        /// </summary>
        public string FullName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return fullName;
            }
        }
        /// <summary>
        /// Gets the Guid of the organization.
        /// </summary>
        public Guid Guid { get; private set; }
        /// <summary>
        /// Gets the server this organization belongs to.
        /// </summary>
        public ChefServer Server
        {
            get
            {
                Contract.Ensures(Contract.Result<ChefServer>() != null);
                return server;
            }
        }
        /// <summary>
        /// Gets the clients for the organization.
        /// </summary>
        public ReadOnlyCollection<Client> Clients
        {
            get 
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<Client>>() != null);

                lock (this)
                {
                    if (clients == null)
                        clients = GetClients();
                    return new ReadOnlyCollection<Client>(clients);
                }
            }
        }
        /// <summary>
        /// Gets the cookbooks for the organization.
        /// </summary>
        public ReadOnlyCollection<Cookbook> Cookbooks
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<Cookbook>>() != null);

                lock (this)
                {
                    if (cookbooks == null)
                        cookbooks = GetCookbooks();
                    return new ReadOnlyCollection<Cookbook>(cookbooks);
                }
            }
        }
        /// <summary>
        /// Gets the nodes for the organization.
        /// </summary>
        public ReadOnlyCollection<Node> Nodes
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<Node>>() != null);

                lock (this)
                {
                    if (nodes == null)
                        nodes = GetNodes();
                    return new ReadOnlyCollection<Node>(nodes);
                }
            }
        }

        /// <summary>
        /// Creates a new Organization.
        /// </summary>
        /// <param name="server">The server to connect to.</param>
        /// <param name="name">The name of the organization.</param>
        private Organization(ChefServer server, string name, string fullName, Guid guid)
        {
            Contract.Requires(server != null);
            Contract.Requires(name != null);
            Contract.Requires(fullName != null);

            this.server = server;
            this.name = name;
            this.fullName = fullName;
            this.Guid = guid;
        }

        /// <summary>
        /// Clears the cache of data.
        /// </summary>
        public void ClearCache()
        {
            clients = null;
            cookbooks = null;
            nodes = null;
        }

        /// <summary>
        /// Gets a client with the given name.  Returns null if not found.
        /// </summary>
        /// <param name="name">The name of the client to get.</param>
        /// <returns>The client with the given name; or null on error.</returns>
        public Client FindClient(string name)
        {
            Contract.Requires(name != null);

            try
            {
                string client = Server.SendMessage("/organizations/" + Name + "/clients/" + name).Result;
                return new Client(this, client);
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// Gets a cookbook with the given name.  Returns null if not found.
        /// </summary>
        /// <param name="name">The name of the cookbook to get.</param>
        /// <returns>The cookbook with the given name; or null on error.</returns>
        public Cookbook FindCookbook(string name)
        {
            Contract.Requires(name != null);

            try
            {
                // Simply make the request to ensure it exists.
                Server.SendMessage("/organizations/" + Name + "/cookbooks/" + name).Wait();
                return new Cookbook(this, name);
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// Gets a node with the given name.  Returns null if not found.
        /// </summary>
        /// <param name="name">The name of the node to get.</param>
        /// <returns>The node with the given name; or null on error.</returns>
        public Node FindNode(string name)
        {
            Contract.Requires(name != null);

            try
            {
                string node = Server.SendMessage("/organizations/" + Name + "/nodes/" + name).Result;
                return new Node(this, node);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Loads the organizations from the given json string.
        /// </summary>
        /// <param name="orgs">The Json string to load from.</param>
        /// <returns>An array of organizations.</returns>
        internal static Organization[] GetOrgs(ChefServer server, string orgs)
        {
            Contract.Requires(server != null);
            Contract.Requires(orgs != null);
            Contract.Ensures(Contract.Result<Organization[]>() != null);

            JArray data = JArray.Parse(orgs);
            return data
                .Select(d => d.SelectToken("organization"))
                .Select(d => new Organization(
                    server, 
                    d.Value<string>("name"),
                    d.Value<string>("full_name"),
                    Guid.Parse(d.Value<string>("guid"))))
                .ToArray();
        }
        /// <summary>
        /// Gets the clients for the current organization.
        /// </summary>
        /// <returns>The clients for this organization.</returns>
        Client[] GetClients()
        {
            Contract.Ensures(Contract.Result<Client[]>() != null);

            string result = Server.SendMessage(
                    "/organizations/" + Name + "/clients"
                ).Result;
            Dictionary<string, string> clients = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

            Client[] ret = new Client[clients.Count];
            int x = 0;
            foreach (var i in clients)
            {
                string client = Server.SendMessageRaw(new Uri(i.Value)).Result;
                ret[x++] = new Client(this, client);
            }

            return ret;
        }
        /// <summary>
        /// Gets the cookbooks for the current organization.
        /// </summary>
        /// <returns>The cookbooks for this organization.</returns>
        Cookbook[] GetCookbooks()
        {
            Contract.Ensures(Contract.Result<Cookbook[]>() != null);

            string result = Server.SendMessage(
                    "/organizations/" + Name + "/cookbooks"
                ).Result;
            JObject data = JObject.Parse(result);
            return data.Select<KeyValuePair<string, JToken>, Cookbook>(t => new Cookbook(this, t.Key)).ToArray();
        }
        /// <summary>
        /// Gets the nodes for the current organization.
        /// </summary>
        /// <returns>The nodes for this organization.</returns>
        Node[] GetNodes()
        {
            Contract.Ensures(Contract.Result<Node[]>() != null);

            string result = Server.SendMessage(
                    "/organizations/" + Name + "/nodes"
                ).Result;
            Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            Node[] ret = new Node[data.Count];
            int x = 0;
            foreach (var path in data)
            {
                string node = Server.SendMessageRaw(new Uri(path.Value)).Result;
                ret[x++] = new Node(this, node);
            }

            return ret;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            return "Chef Organization:" + Name;
        }
        /// <summary>
        /// Determines whether the specified System.Object is equal to the current System.Object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj != null && obj is Organization &&
                ((Organization)obj).Server.Equals(Server) &&
                ((Organization)obj).Name.Equals(Name);
        }
        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current System.Object.</returns>
        public override int GetHashCode()
        {
            return Server.GetHashCode() ^ Name.GetHashCode();
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(server != null);
            Contract.Invariant(name != null);
            Contract.Invariant(fullName != null);
        }
    }
}