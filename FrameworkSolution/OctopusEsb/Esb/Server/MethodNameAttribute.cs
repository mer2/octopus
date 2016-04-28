using System;

namespace Octopus.Esb.Server
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class MethodNameAttribute : Attribute
	{
		public MethodNameAttribute(string name) {
            if (String.IsNullOrEmpty(name)) {
                throw new ArgumentNullException("name");
            }
            this.Name = name;
        }
 
        public string Name { get; private set; }
	}
}
