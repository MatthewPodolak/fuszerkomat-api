using Microsoft.AspNetCore.Mvc.ModelBinding;

public sealed class InvariantNumberModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var t = context.Metadata.ModelType;

        if (t == typeof(double) || t == typeof(double?))
            return new InvariantDoubleModelBinder();

        if (t == typeof(decimal) || t == typeof(decimal?))
            return new InvariantDecimalModelBinder();

        if (t == typeof(float) || t == typeof(float?))
            return new InvariantFloatModelBinder();

        return null;
    }
}
