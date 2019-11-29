using System;
using System.Reflection;
using OROptimizer.Diagnostics.Log;

namespace OROptimizer.Serializer
{
    public class TypeBasedSimpleSerializerAssembly : TypeBasedSimpleSerializerAbstr<Assembly>
    {
        #region Member Functions

        public override string GenerateCSharpCode(Assembly deserializedValue)
        {
            return $"{typeof(AppDomain).FullName}.{nameof(AppDomain.CurrentDomain)}.{nameof(AppDomain.Load)}(\"{deserializedValue.GetName().Name}\")";
        }

        public override string Serialize(Assembly valueToSerialize)
        {
            return valueToSerialize.GetName().Name; 
        }

        public override bool TryDeserialize(string valueToDeserialize, out Assembly deserializedValue)
        {
            try
            {
                deserializedValue = AppDomain.CurrentDomain.Load(valueToDeserialize);
                return deserializedValue != null;
            }
            catch(Exception e)
            {
                deserializedValue = null;
                LogHelper.Context.Log.Error($"Failed to load assembly '{valueToDeserialize}'.", e);
                return false;
            }
        }

        #endregion
    }
}