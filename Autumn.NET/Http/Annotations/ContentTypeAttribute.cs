using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autumn.Http.Annotations;

[AttributeUsage(AttributeTargets.ReturnValue)]
public class ContentTypeAttribute : Attribute {
    public string ContentType { get; }
    public ContentTypeAttribute(string contentType) { 
        ContentType = contentType; 
    }
}
