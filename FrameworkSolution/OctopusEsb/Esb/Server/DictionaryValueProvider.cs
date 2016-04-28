using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Octopus.Esb.Server
{
	internal class DictionaryValueProvider : IValueProvider
	{
		public DictionaryValueProvider(IEnumerable<KeyValuePair<string, object>> dictionary, ISerializer serializer) {
			if(dictionary == null) {
				throw new ArgumentNullException("dictionary");
			}

			AddValues(dictionary, serializer);
		}

		public DictionaryValueProvider(NameValueCollection collection, ISerializer serializer) {
			if(collection == null) {
				throw new ArgumentNullException("collection");
			}

			AddValues(collection, serializer);
		}

		private readonly Dictionary<string, ValueProviderResult> values = new Dictionary<string, ValueProviderResult>(StringComparer.OrdinalIgnoreCase);
		private void AddValues(IEnumerable<KeyValuePair<string, object>> dictionary, ISerializer serializer) {
			foreach(var entry in dictionary) {
				this.values.Add(entry.Key, new ValueProviderResult(entry.Value, serializer));
			}
		}

		private void AddValues(NameValueCollection collection, ISerializer serializer) {
			foreach(var key in collection.AllKeys) {
				this.values.Add(key, new ValueProviderResult(collection[key], serializer));
			}
		}

		public ValueProviderResult GetValue(string name) {
			if(name == null) {
				throw new ArgumentNullException("name");
			}

			ValueProviderResult vpResult;
			this.values.TryGetValue(name, out vpResult);
			return vpResult;
		}
	}
}
