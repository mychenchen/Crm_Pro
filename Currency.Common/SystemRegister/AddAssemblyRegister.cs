using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Currency.Common.SystemRegister
{
    /// <summary>
    /// 批量注入
    /// </summary>
    public static class AddAssemblyRegister
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="assemblyName">类库名称</param>
        /// <param name="serviceLifetime">方式</param>
        public static void AddAssembly(this IServiceCollection service, string assemblyName
            , ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var assembly = RuntimeHelper.GetAssemblyByName(assemblyName);

            var types = assembly.GetTypes();
            var list = types.Where(u => u.IsClass && !u.IsAbstract && !u.IsGenericType).ToList();

            foreach (var type in list)
            {
                var interfaceList = type.GetInterfaces();
                if (interfaceList.Any())
                {
                    var inter = interfaceList.First();

                    switch (serviceLifetime)
                    {
                        case ServiceLifetime.Transient:
                            service.AddTransient(inter, type);
                            break;
                        case ServiceLifetime.Scoped:
                            service.AddScoped(inter, type);
                            break;
                        case ServiceLifetime.Singleton:
                            service.AddSingleton(inter, type);
                            break;

                    }
                    //service.AddScoped(inter, type);
                }
            }
        }
    }

    /// <summary>
    /// 通过程序集的名称加载程序集
    /// </summary>
    public class RuntimeHelper
    {
        //通过程序集的名称加载程序集
        public static Assembly GetAssemblyByName(string assemblyName)
        {
            return AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(assemblyName));
        }
    }

}
