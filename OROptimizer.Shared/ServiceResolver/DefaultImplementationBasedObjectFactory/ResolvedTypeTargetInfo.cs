//using System.Reflection;

//namespace OROptimizer.ServiceResolver.DefaultImplementationBasedObjectFactory
//{
//    /// <summary>
//    /// Information on resolved type target.
//    /// </summary>
//    internal class ResolvedTypeTargetInfo
//    {
//        /// <summary>
//        /// Constructor.
//        /// </summary>
//        /// <param name="constructorInfo">Constructor info of a type into which an instance of resolved type is injected.</param>
//        /// <param name="parameterInfo">Parameter info for which the type is being resolved.</param>
//        public ResolvedTypeTargetInfo(ConstructorInfo constructorInfo, System.Reflection.ParameterInfo parameterInfo)
//        {
//            ConstructorInfo = constructorInfo;
//            ParameterInfo = parameterInfo;
//        }

//        /// <summary>
//        /// Constructor info of a type into which an instance of resolved type is injected.
//        /// </summary>
//        public ConstructorInfo ConstructorInfo { get; }

//        /// <summary>
//        /// Parameter info for which the type is being resolved.
//        /// </summary>
//        public System.Reflection.ParameterInfo ParameterInfo { get; }
//    }
//}