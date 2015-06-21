using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ModMaker.Chef
{
    /// <summary>
    /// Defines a client within Chef.
    /// </summary>
    public sealed class Client
    {
        readonly string name, nodeName;
        readonly Organization organization;
        AsymmetricKeyParameter publicKey;

        /// <summary>
        /// Gets the name of the client.
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
        /// Gets the name of the node.
        /// </summary>
        public string NodeName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return nodeName;
            }
        }
        /// <summary>
        /// Gets the organization this client is for.
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
        /// Gets the public key of the client.
        /// </summary>
        public AsymmetricKeyParameter PublicKey
        {
            get
            {
                Contract.Ensures(Contract.Result<AsymmetricKeyParameter>() != null);
                return publicKey;
            }
        }
        /// <summary>
        /// Gets whether the client is a validator.
        /// </summary>
        public bool IsValidator { get; private set; }

        /// <summary>
        /// Loads the client from the given json string.
        /// </summary>
        /// <param name="organization">The organization this client belongs to.</param>
        /// <param name="str">The Json string to load from.</param>
        /// <returns>The resulting client.</returns>
        internal Client(Organization organization, string str)
        {
            Contract.Requires(organization != null);
            Contract.Requires(str != null);

            JObject data = JObject.Parse(str);

            var pemReader = new PemReader(new StringReader(data.Value<string>("public_key")));
            AsymmetricKeyParameter key = (AsymmetricKeyParameter)pemReader.ReadObject();

            this.name = data.Value<string>("clientname");
            this.nodeName = data.Value<string>("name");
            this.organization = organization;
            this.publicKey = key;
            this.IsValidator = data.Value<bool>("validator");
        }

        /// <summary>
        /// Regenerates the key-pair for the client.  This will cause the
        /// old keys to become invalid and must be changed to the given
        /// return value.
        /// </summary>
        /// <returns>The new key-pair for the client.</returns>
        public AsymmetricCipherKeyPair RegenerateKey()
        {
            return RegenerateKeyAsync().Result;
        }
        /// <summary>
        /// Regenerates the key-pair for the client.  This will cause the
        /// old keys to become invalid and must be changed to the given
        /// return value.
        /// </summary>
        /// <returns>The new key-pair for the client.</returns>
        public async Task<AsymmetricCipherKeyPair> RegenerateKeyAsync()
        {
            var data = new { private_key = true };
            string body = JsonConvert.SerializeObject(data);

            // Send the PUT request.
            string result = await Organization.Server.SendMessage(
                "/organizations/" + Organization.Name + "/clients/" + Name,
                HttpMethod.Put, body);

            // Parse the new key-pair.
            JObject resultData = JObject.Parse(result);
            var pemReader = new PemReader(new StringReader(resultData.Value<string>("private_key")));
            AsymmetricCipherKeyPair key = (AsymmetricCipherKeyPair)pemReader.ReadObject();

            publicKey = key.Public;
            return key;
        }

        /// <summary>
        /// Deletes the client from the server.  After this call, use of this 
        /// object is invalid and may be undefined.
        /// </summary>
        public void Delete()
        {
            DeleteAsync().Wait();
        }
        /// <summary>
        /// Deletes the client from the server.  After this call, use of this 
        /// object is invalid and may be undefined.
        /// </summary>
        public async Task DeleteAsync()
        {
            // Send the DELETE request.
            await Organization.Server.SendMessage(
                "/organizations/" + Organization.Name + "/clients/" + Name,
                HttpMethod.Delete, string.Empty);

            Organization.ClearCache();
        }
        
        [ContractInvariantMethod]
        void Invariant()
        {
            Contract.Invariant(name != null);
            Contract.Invariant(nodeName != null);
            Contract.Invariant(organization != null);
            Contract.Invariant(publicKey != null);
        }
    }
}