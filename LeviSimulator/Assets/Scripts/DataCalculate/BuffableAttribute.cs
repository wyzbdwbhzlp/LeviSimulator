using System;

namespace DataCalculate
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class BuffableAttribute : Attribute
    {
    }
}