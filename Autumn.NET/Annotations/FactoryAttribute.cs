using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autumn.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class FactoryAttribute : Attribute {

}
