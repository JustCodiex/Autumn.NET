using System;
using System.Reflection;

namespace Autumn.WPF.Annotations;

/// <summary>
/// Specifies the model type associated with a WPF view.
/// </summary>
/// <remarks>
/// The <see cref="ModelAttribute"/> is used to associate a specific model type with a WPF view.
/// It can be applied to a class to indicate the model type that should be used in conjunction with the view.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ModelAttribute : Attribute {

    /// <summary>
    /// Gets the model type associated with the WPF view.
    /// </summary>
    public Type ModelType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelAttribute"/> class with the specified model type.
    /// </summary>
    /// <param name="modelType">The type of the model associated with the WPF view.</param>
    public ModelAttribute(Type modelType) {
        this.ModelType = modelType;
    }

    public static Type? GetModel(Type view) => view.GetCustomAttribute<ModelAttribute>() is ModelAttribute ma ? ma.ModelType : null;

}
