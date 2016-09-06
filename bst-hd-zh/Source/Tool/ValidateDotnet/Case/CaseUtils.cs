using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace CaseDotnet
{
    class CaseUtils
    {
        /// <summary>
        /// iterate type of the assembly to find all method that have CaseDotnetAttribute, then invoke the method
        /// </summary>
        /// <param name="assembly"> if the assembly is null, then iterate the current assembly</param>
        /// <returns></returns>
        public static bool DoCase(Assembly assembly)
        {
            bool result = true;
            if (null == assembly)
            {
                assembly = typeof(CaseUtils).Assembly;
            }
            IList<MethodInfo> caseMethod = new List<MethodInfo>();
            foreach (Type type in assembly.GetTypes())
            {
                if (!result)
                {
                    break;
                }
                caseMethod.Clear();
                foreach (MethodInfo mInfo in type.GetMethods())
                {
                    foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo))
                    {
                        // Check for the CaseDotnetAttribute attribute.
                        if (attr.GetType() == typeof(CaseDotnetAttribute))
                        {
                            caseMethod.Add(mInfo);
                        }
                    }
                }
                if (caseMethod.Count > 0)
                {
                    try
                    {
                        object obj = null;
                        try{
                            obj = Activator.CreateInstance(type);
                        }catch (Exception e)
                        {
                            Console.Out.WriteLine(e.Message);
                        }
                        foreach (MethodInfo mInfo in caseMethod)
                        {
                            ParameterInfo[] pars = mInfo.GetParameters();
                            if ((typeof(bool) == mInfo.ReturnType || typeof(Boolean) == mInfo.ReturnType) && (null == pars || pars.Length < 1))
                            {
                                if (!mInfo.IsStatic && null == obj)
                                {
                                    Console.Out.WriteLine(string.Format("can not use the way Activator.CreateInstance(type) to create instance, the class {0} ", mInfo.ReflectedType.FullName));
                                }
                                else
                                {
                                    object re = mInfo.Invoke(obj, null);
                                    bool bo = Convert.ToBoolean(re);
                                    if (!bo)
                                    {
                                        result = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                Console.Out.WriteLine(string.Format("class {0} method {1} return type is {2} not bool or the parameters is not null",mInfo.ReflectedType.FullName,mInfo.Name,mInfo.ReturnType.ToString()));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Out.WriteLine(e.Message);
                        result = false;
                    }
                }
            }
            return result;
        }
    }
}
