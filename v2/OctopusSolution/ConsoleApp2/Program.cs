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
	        var test = ObjectService.GetObject<ITestService>();
	        var str = test.Hello("1234");
	        Console.WriteLine(str);
	        var a = test.Test(10);
	        Console.WriteLine(a);
	        var result = test.GetEntity();
	        Console.WriteLine(result.ResultAttachObject.Name);
	        var entity = test.GetRawEntity();
	        Console.WriteLine(entity.Name + "." + entity.Age);
	        entity = test.SetRawEntity(entity);
	        Console.WriteLine(entity.Name + "." + entity.Age);
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
