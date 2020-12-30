using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Crm.WebApp.Infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    [Aspect(Scope.Global)]
    [Injection(typeof(TimeAspect))]
    public class TimeAspect : Attribute
    {
        /// <summary>
        /// 方法之前调用
        /// </summary>
        /// <param name="name"></param>
        [Advice(Kind.Before)] // you can have also After (async-aware), and Around(Wrap/Instead) kinds
        public void LogHome([Argument(Source.Name)] string name)
        {
            Console.WriteLine($"开始毫秒:" + DateTime.Now.Millisecond);   //you can debug it	
            Console.WriteLine($"开始 '{name}' method...");   //you can debug it	
        }
        /// <summary>
        /// 方法之后前调用
        /// </summary>
        /// <param name="name"></param>
        [Advice(Kind.After)] // you can have also After (async-aware), and Around(Wrap/Instead) kinds
        public void LogEnter([Argument(Source.Name)] string name)
        {
            Console.WriteLine($"结束毫秒:" + DateTime.Now.Millisecond);   //you can debug it	
            Console.WriteLine($"结束 '{name}' method...");   //you can debug it	
        }
    }
}
