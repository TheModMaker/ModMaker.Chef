using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ModMaker.Chef
{
    /// <summary>
    /// Defines metadata about a cookbook version.
    /// </summary>
    public sealed class CookbookMetadata
    {
        readonly string name, version, description, longDescription,
            maintainer, maintainerEmail, license;
        readonly AttributeList recipes, attributes, dependencies,
            suggestions, platforms, groupings, recommendations,
            providing, conflicting, replacing;

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
        /// Gets the version of the cookbook.
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
        /// Gets the description of the cookbook.
        /// </summary>
        public string Description
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return description;
            }
        }
        /// <summary>
        /// Gets the long description of the cookbook.
        /// </summary>
        public string LongDescription
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return longDescription;
            }
        }
        /// <summary>
        /// Gets the name of the maintainer of the cookbook.
        /// </summary>
        public string Maintainer
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return maintainer;
            }
        }
        /// <summary>
        /// Gets the email of the maintainer of the cookbook.
        /// </summary>
        public string MaintainerEmail
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return maintainerEmail;
            }
        }
        /// <summary>
        /// Gets the license of the cookbook.
        /// </summary>
        public string License
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return license;
            }
        }

        /// <summary>
        /// Gets the recipies in the cookbook.  This is only the metadata and
        /// may not reflect the actual recipies.  These attributes are read-only.
        /// </summary>
        public AttributeList Recipes
        {
            get
            {
                Contract.Ensures(Contract.Result<AttributeList>() != null);
                Contract.Ensures(!Contract.Result<AttributeList>().IsReadOnly);
                return recipes;
            }
        }
        /// <summary>
        /// Gets the attributes in the cookbook.  This is only the metadata and
        /// may not reflect the actual attributes.  These attributes are read-only.
        /// </summary>
        public AttributeList Attributes
        {
            get
            {
                Contract.Ensures(Contract.Result<AttributeList>() != null);
                Contract.Ensures(!Contract.Result<AttributeList>().IsReadOnly);
                return attributes;
            }
        }
        /// <summary>
        /// Gets the dependencies in the cookbook.  These attributes are read-only.
        /// </summary>
        public AttributeList Dependencies
        {
            get
            {
                Contract.Ensures(Contract.Result<AttributeList>() != null);
                Contract.Ensures(!Contract.Result<AttributeList>().IsReadOnly);
                return dependencies;
            }
        }
        /// <summary>
        /// Gets the suggestions in the cookbook.  These attributes are read-only.
        /// </summary>
        public AttributeList Suggestions
        {
            get
            {
                Contract.Ensures(Contract.Result<AttributeList>() != null);
                Contract.Ensures(!Contract.Result<AttributeList>().IsReadOnly);
                return suggestions;
            }
        }
        /// <summary>
        /// Gets the platforms in the cookbook.  These attributes are read-only.
        /// </summary>
        public AttributeList Platforms
        {
            get
            {
                Contract.Ensures(Contract.Result<AttributeList>() != null);
                Contract.Ensures(!Contract.Result<AttributeList>().IsReadOnly);
                return platforms;
            }
        }
        /// <summary>
        /// Gets the groupings in the cookbook.  These attributes are read-only.
        /// </summary>
        public AttributeList Groupings
        {
            get
            {
                Contract.Ensures(Contract.Result<AttributeList>() != null);
                Contract.Ensures(!Contract.Result<AttributeList>().IsReadOnly);
                return groupings;
            }
        }
        /// <summary>
        /// Gets the recommendations in the cookbook.  These attributes are read-only.
        /// </summary>
        public AttributeList Recommendations
        {
            get
            {
                Contract.Ensures(Contract.Result<AttributeList>() != null);
                Contract.Ensures(!Contract.Result<AttributeList>().IsReadOnly);
                return recommendations;
            }
        }
        /// <summary>
        /// Gets the providing in the cookbook.  These attributes are read-only.
        /// </summary>
        public AttributeList Providing
        {
            get
            {
                Contract.Ensures(Contract.Result<AttributeList>() != null);
                Contract.Ensures(!Contract.Result<AttributeList>().IsReadOnly);
                return providing;
            }
        }
        /// <summary>
        /// Gets the conflicting in the cookbook.  These attributes are read-only.
        /// </summary>
        public AttributeList Conflicting
        {
            get
            {
                Contract.Ensures(Contract.Result<AttributeList>() != null);
                Contract.Ensures(!Contract.Result<AttributeList>().IsReadOnly);
                return conflicting;
            }
        }
        /// <summary>
        /// Gets the replacing in the cookbook.  These attributes are read-only.
        /// </summary>
        public AttributeList Replacing
        {
            get
            {
                Contract.Ensures(Contract.Result<AttributeList>() != null);
                Contract.Ensures(!Contract.Result<AttributeList>().IsReadOnly);
                return replacing;
            }
        }

        internal CookbookMetadata(
            string name,
            string version,
            string description,
            string longDescription,
            string maintainer,
            string maintainerEmail,
            string license,
            AttributeList recipes,
            AttributeList attributes,
            AttributeList dependencies,
            AttributeList suggestions,
            AttributeList platforms,
            AttributeList groupings,
            AttributeList recommendations,
            AttributeList providing,
            AttributeList conflicting,
            AttributeList replacing) 
        {
            Contract.Requires(name != null);
            Contract.Requires(version != null);
            Contract.Requires(description != null);
            Contract.Requires(longDescription != null);
            Contract.Requires(maintainer != null);
            Contract.Requires(maintainerEmail != null);
            Contract.Requires(license != null);
            Contract.Requires(recipes != null);
            Contract.Requires(attributes != null);
            Contract.Requires(dependencies != null);
            Contract.Requires(suggestions != null);
            Contract.Requires(platforms != null);
            Contract.Requires(groupings != null);
            Contract.Requires(recommendations != null);
            Contract.Requires(providing != null);
            Contract.Requires(conflicting != null);
            Contract.Requires(replacing != null);

            this.name = name;
            this.version = version;
            this.description = description;
            this.longDescription = longDescription;
            this.maintainer = maintainer;
            this.maintainerEmail = maintainerEmail;
            this.license = license;
            this.recipes = recipes;
            this.attributes = attributes;
            this.dependencies = dependencies;
            this.suggestions = suggestions;
            this.platforms = platforms;
            this.groupings = groupings;
            this.recommendations = recommendations;
            this.providing = providing;
            this.conflicting = conflicting;
            this.replacing = replacing;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            return Name + " v" + Version + " Metadata";
        }
        /// <summary>
        /// Determines whether the specified System.Object is equal to the current System.Object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj != null && obj is CookbookMetadata &&
                ((CookbookMetadata)obj).Name.Equals(Name) &&
                ((CookbookMetadata)obj).Version.Equals(Version);
        }
        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current System.Object.</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ ~Version.GetHashCode();
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(name != null);
            Contract.Invariant(version != null);
            Contract.Invariant(description != null);
            Contract.Invariant(longDescription != null);
            Contract.Invariant(maintainer != null);
            Contract.Invariant(maintainerEmail != null);
            Contract.Invariant(license != null);

            Contract.Invariant(recipes != null);
            Contract.Invariant(attributes != null);
            Contract.Invariant(dependencies != null);
            Contract.Invariant(suggestions != null);
            Contract.Invariant(platforms != null);
            Contract.Invariant(groupings != null);
            Contract.Invariant(recommendations != null);
            Contract.Invariant(providing != null);
            Contract.Invariant(conflicting != null);
            Contract.Invariant(replacing != null);
        }
    }
}