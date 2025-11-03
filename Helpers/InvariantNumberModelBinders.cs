using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public sealed class InvariantDoubleModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext ctx)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));

        var valueResult = ctx.ValueProvider.GetValue(ctx.ModelName);
        if (valueResult == ValueProviderResult.None) return Task.CompletedTask;

        var str = valueResult.FirstValue;
        if (string.IsNullOrWhiteSpace(str)) return Task.CompletedTask;

        if (double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d))
        {
            ctx.Result = ModelBindingResult.Success(d);
        }
        else
        {
            ctx.ModelState.TryAddModelError(ctx.ModelName, $"'{str}' is not a valid number.");
        }

        return Task.CompletedTask;
    }
}

public sealed class InvariantDecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext ctx)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));

        var valueResult = ctx.ValueProvider.GetValue(ctx.ModelName);
        if (valueResult == ValueProviderResult.None) return Task.CompletedTask;

        var str = valueResult.FirstValue;
        if (string.IsNullOrWhiteSpace(str)) return Task.CompletedTask;

        if (decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out var dec))
        {
            ctx.Result = ModelBindingResult.Success(dec);
        }
        else
        {
            ctx.ModelState.TryAddModelError(ctx.ModelName, $"'{str}' is not a valid decimal.");
        }

        return Task.CompletedTask;
    }
}

public sealed class InvariantFloatModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext ctx)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));

        var valueResult = ctx.ValueProvider.GetValue(ctx.ModelName);
        if (valueResult == ValueProviderResult.None) return Task.CompletedTask;

        var str = valueResult.FirstValue;
        if (string.IsNullOrWhiteSpace(str)) return Task.CompletedTask;

        if (float.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var f))
        {
            ctx.Result = ModelBindingResult.Success(f);
        }
        else
        {
            ctx.ModelState.TryAddModelError(ctx.ModelName, $"'{str}' is not a valid number.");
        }

        return Task.CompletedTask;
    }
}