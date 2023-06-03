namespace Autumn.Database.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class TransactionalAttribute : Attribute {}
