using System;

namespace Octopus.Web
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	public class ContextItemAttribute : Attribute
	{
		public ContextItemAttribute(string contextKey, string itemKey, object itemValue) {
			if(string.IsNullOrEmpty(contextKey)) {
				throw new ArgumentException("ContextKey Required", "contextKey");
			}
			if(string.IsNullOrEmpty(itemKey)) {
				throw new ArgumentException("ItemKey Required", "itemKey");
			}
			this.ContextKey = contextKey;
			this.ItemKey = itemKey;
			this.ItemValue = itemValue;
		}

		public string ContextKey { get; private set; }
		public string ItemKey { get; private set; }
		public object ItemValue { get; private set; }
		public string ValueName { get; set; }
	}
}
