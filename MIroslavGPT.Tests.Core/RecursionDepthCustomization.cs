using AutoFixture.Kernel;
using System.Reflection;

namespace MiroslavGPT.Tests.Core
{
    public class RecursionDepthCustomization : ISpecimenBuilder
    {
        private readonly int _maxRecursionDepth;
        private readonly Dictionary<object, int> _requestDepths;

        public RecursionDepthCustomization(int maxRecursionDepth)
        {
            _maxRecursionDepth = maxRecursionDepth;
            _requestDepths = new Dictionary<object, int>();
        }

        public object Create(object request, ISpecimenContext context)
        {
            if (!(request is PropertyInfo propertyInfo))
            {
                return new NoSpecimen();
            }

            if (!_requestDepths.ContainsKey(propertyInfo.DeclaringType))
            {
                _requestDepths[propertyInfo.DeclaringType] = 0;
            }

            if (_requestDepths[propertyInfo.DeclaringType] >= _maxRecursionDepth)
            {
                return new OmitSpecimen();
            }

            _requestDepths[propertyInfo.DeclaringType]++;

            var result = context.Resolve(request);

            _requestDepths[propertyInfo.DeclaringType]--;

            return result;
        }
    }
}