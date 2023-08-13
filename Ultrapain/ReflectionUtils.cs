using System.Reflection;

namespace Ultrapain
{
	public static class ReflectionUtils
	{
		public static MethodInfo GetMethod<T>(string name)
		{
			return typeof(T).GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public static MethodInfo GetStaticMethod<T>(string name)
		{
			return typeof(T).GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}
	}
}
