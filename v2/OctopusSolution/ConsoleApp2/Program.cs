using System;
using System.Reflection;
using ClassLibrary1;
using HTB.DevFx;

namespace ConsoleApp2
{
    public class Program
    {
        static void Main(string[] args)
        {
	        try {
		        throw new NotImplementedException();
	        } catch { }
			/*var test = DispatchProxy.Create<ITestService, MyDispatchProxy>();
			test.Hello("");*/
			var service = ObjectService.GetObject<ITestService>();
			var str = service.Hello("ConsoleApp2");
            Console.WriteLine(str);
			Console.ReadLine();
        }

	    /*public class MyDispatchProxy : DispatchProxy
	    {
		    public MyDispatchProxy() {
				var type = this.GetType();
				type.ToString();
		    }

		    protected override object Invoke(MethodInfo targetMethod, object[] args) {
				targetMethod.ToString();
				return null; 
		    }
	    }*/
    }
}
