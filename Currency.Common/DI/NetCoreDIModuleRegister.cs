using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Currency.Common
{

    public static class NetCoreDIModuleRegister
    {
        /// <summary>
        /// 标注要运用DI的类 被此属性标注的类 要被注册到依赖注入容器中 并且可以指定类要映射的接口或者类
        /// 此属性只能运用于类，并且此属性不能继承
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
        public class UseDIAttribute : Attribute
        {
            //Targets用于指示 哪些接口或者类 要被 "被属性修饰了的类" 进行依赖注入
            public List<Type> TargetTypes = new List<Type>();
            public ServiceLifetime lifetime;
            public UseDIAttribute(ServiceLifetime argLifetime, params Type[] argTargets)
            {
                lifetime = argLifetime;
                foreach (var argTarget in argTargets)
                {
                    TargetTypes.Add(argTarget);
                }
            }

            public List<Type> GetTargetTypes()
            {
                return TargetTypes;
            }
            public ServiceLifetime Lifetime
            {
                get
                {
                    return this.lifetime;
                }
            }
        }

        /// <summary>
        /// 自动注册服务
        /// </summary>
        /// <param name="services">注册服务的集合（向其中注册）</param>
        /// <param name="ImplementationType">要注册的类型</param>
        public static void AutoRegisterService(this IServiceCollection services, Type ImplementationType)
        {
            //获取类型的 UseDIAttribute 属性 对应的对象
            UseDIAttribute attr = ImplementationType.GetCustomAttribute(typeof(UseDIAttribute)) as UseDIAttribute;
            ////获取类实现的所有接口
            //Type[] types = ImplementationType.GetInterfaces();
            List<Type> types = attr.GetTargetTypes();
            var lifetime = attr.Lifetime;
            //遍历类实现的每一个接口
            foreach (var t in types)
            {
                //将类注册为接口的实现-----但是存在一个问题，就是担心 如果一个类实现了IDisposible接口 担心这个类变成了这个接口的实现
                ServiceDescriptor serviceDescriptor = new ServiceDescriptor(t, ImplementationType, lifetime);
                services.Add(serviceDescriptor);
            }

        }
        /// <summary>
        /// 根据程序集的名字获取程序集中所有的类型集合
        /// </summary>
        /// <param name="AssemblyName">程序集名字</param>
        /// <returns>类型集合</returns>
        public static Type[] GetTypesByAssemblyName(String AssemblyName)
        {
            Assembly assembly = Assembly.Load(AssemblyName);
            return assembly.GetTypes();
        }


        #region 将程序集中的所有符合条件的类型全部注册到 IServiceCollection 中 重载1
        /// <summary>
        /// 将程序集中的所有符合条件的类型全部注册到 IServiceCollection 中
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="AassemblyName">程序集名字</param>
        public static void AutoRegisterServicesFromAssembly(this IServiceCollection services,
            string AassemblyName)
        {
            //根据程序集的名字 获取程序集中所有的类型
            Type[] types = GetTypesByAssemblyName(AassemblyName);
            //过滤上述程序集 首先按照传进来的条件进行过滤 接着要求Type必须是类，而且不能是抽象类
            IEnumerable<Type> _types = types.Where(t => t.IsClass && !t.IsAbstract);
            foreach (var t in _types)
            {
                IEnumerable<Attribute> attrs = t.GetCustomAttributes();
                //遍历类的所有特性
                foreach (var attr in attrs)
                {
                    //如果在其特性中发现特性是 UseDIAttribute 特性 就将这个类注册到DI容器中去
                    //并跳出当前的循环 开始对下一个类进行循环
                    if (attr is UseDIAttribute)
                    {
                        services.AutoRegisterService(t);
                        break;
                    }
                }
            }
        }
        #endregion


        #region 将程序集中的所有符合条件的类型全部注册到 IServiceCollection 中 重载2
        /// <summary>
        /// 将程序集中的所有符合条件的类型全部注册到 IServiceCollection 中
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="AassemblyName">程序集名字</param>
        /// <param name="wherelambda">过滤类型的表达式</param>
        public static void AutoRegisterServicesFromAssembly(this IServiceCollection services,
            string AassemblyName, Func<Type, bool> wherelambda)
        {
            //根据程序集的名字 获取程序集中所有的类型
            Type[] types = GetTypesByAssemblyName(AassemblyName);
            //过滤上述程序集 首先按照传进来的条件进行过滤 接着要求Type必须是类，而且不能是抽象类
            IEnumerable<Type> _types = types.Where(wherelambda).Where(t => t.IsClass && !t.IsAbstract);
            foreach (var t in _types)
            {
                IEnumerable<Attribute> attrs = t.GetCustomAttributes();
                //遍历类的所有特性
                foreach (var attr in attrs)
                {
                    //如果在其特性中发现特性是 UseDIAttribute 特性 就将这个类注册到DI容器中去
                    //并跳出当前的循环 开始对下一个类进行循环
                    if (attr is UseDIAttribute)
                    {
                        services.AutoRegisterService(t);
                        break;
                    }
                }
            }
        }
        #endregion

    }
}
