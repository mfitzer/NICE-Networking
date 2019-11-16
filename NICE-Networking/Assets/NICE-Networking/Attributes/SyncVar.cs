using System;

namespace NICE_Networking
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SyncVar : Attribute { }
}