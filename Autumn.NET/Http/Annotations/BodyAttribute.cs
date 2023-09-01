using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autumn.Http.Annotations;

[AttributeUsage(AttributeTargets.Parameter)]
public class BodyAttribute : Attribute {}
