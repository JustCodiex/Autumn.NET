using System;

using Autumn.Annotations.Internal;

namespace Autumn.WPF.Annotations;

/// <summary>
/// Specifies that the property or method parameter is to be injected with the underlying model for a WPF view.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class DataModelAttribute : Attribute, IInjectAnnotation { }
