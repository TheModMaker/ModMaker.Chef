using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ModMaker.Chef
{
    /// <summary>
    /// Contains a number of static helper methods.
    /// </summary>
    static class Helpers
    {
        /// <summary>
        /// Calculates the Sha1 hash of the current string and base-64 encodes it.
        /// </summary>
        /// <param name="input">The string value to hash and encode.</param>
        /// <returns>The base-64 encoded Sha1 hash of the string.</returns>
        [Pure]
        public static string ToBase64EncodedSha1String(this string input)
        {
            Contract.Requires(input != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(input)));
        }
        /// <summary>
        /// Splits the string into lines of the given length.
        /// </summary>
        /// <param name="input">The input string to split.</param>
        /// <param name="length">The length of the resulting lines.</param>
        /// <returns>An enumerable of the lines of the string.</returns>
        [Pure]
        public static IEnumerable<string> Split(this string input, int length)
        {
            Contract.Requires(input != null);
            Contract.Requires(length > 0);
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            for (int i = 0; i < input.Length; i += length)
                yield return input.Substring(i, Math.Min(length, input.Length - i));
        }
        
        /// <summary>
        /// Sends a web request to the server and reads the result.  This throws
        /// an exception if the result is not success (200).
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>Text result of the request.</returns>
        /// <exception cref="System.Net.Http.HttpRequestException">If the return code is not 200.</exception>
        public static async Task<string> SendRequestAsync(HttpRequestMessage message)
        {
            Contract.Requires(message != null);
            Contract.Ensures(Contract.Result<Task<string>>() != null);

            using (var client = new HttpClient())
            {
                HttpResponseMessage result = await client.SendAsync(message);
                result.EnsureSuccessStatusCode();

                return await result.Content.ReadAsStringAsync();
            }
        }
    }
}