using System;

namespace Autumn.WPF;

/// <summary>
/// Exception thrown when the view object cannot be found.
/// </summary>
public class ViewNotFoundException : Exception {
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewNotFoundException"/> class.
    /// </summary>
    public ViewNotFoundException() : base("The view object cannot be found.") {}

}
