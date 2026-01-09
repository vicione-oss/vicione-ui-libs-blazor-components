using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services.TypeDescriptors;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Services;

public sealed class PropertyGridItemCollectionBuilderTests
{
    [Fact]
    public void Should_be_resolvable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var itemCollectionBuilder = serviceProvider.GetService<IPropertyGridItemCollectionBuilder<object>>();

        // Assert
        itemCollectionBuilder.Should().NotBeNull();
    }

    [Fact]
    public void Should_return_item_for_property_of_single_instance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>()
            .WithPropertyDescriptorProvider<NamePropertyDescriptorProvider<Foo>>();

        using var serviceProvider = services.BuildServiceProvider();

        var itemCollectionBuilder = serviceProvider.GetRequiredService<IPropertyGridItemCollectionBuilder<object>>();
        var messageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();

        var context = new object();
        var foo = new Foo();

        // Act
        var items = itemCollectionBuilder.Build([foo], context, messageStore);

        // Assert
        items.Should().HaveCount(1);

        var item1 = items.OfType<PropertyGridItem<string>>().Single();
        item1.Name.Should().Be(nameof(IHasName.Name));
    }

    [Fact]
    public void Should_return_item_for_common_property_of_multiple_instances()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>()
            .WithPropertyDescriptorProvider<NamePropertyDescriptorProvider<Foo>>()
            .WithPropertyDescriptorProvider<NamePropertyDescriptorProvider<Bar>>();

        using var serviceProvider = services.BuildServiceProvider();

        var itemCollectionBuilder = serviceProvider.GetRequiredService<IPropertyGridItemCollectionBuilder<object>>();
        var messageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();

        var context = new object();
        var foo = new Foo();
        var bar1 = new Bar();
        var bar2 = new Bar();

        // Act
        var items = itemCollectionBuilder.Build([foo], context, messageStore);

        // Assert
        items.Should().HaveCount(1);

        var item1 = items.OfType<PropertyGridItem<string>>().Single();
        item1.Name.Should().Be(nameof(IHasName.Name));
    }

    [Fact]
    public void Should_return_items_for_properties_of_single_instance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>()
            .WithPropertyDescriptorProvider<FooPropertyDescriptorProvider>();

        using var serviceProvider = services.BuildServiceProvider();

        var itemCollectionBuilder = serviceProvider.GetRequiredService<IPropertyGridItemCollectionBuilder<object>>();
        var messageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();

        var context = new object();
        var foo = new Foo();

        // Act
        var items = itemCollectionBuilder.Build([foo], context, messageStore);

        // Assert
        items.Should().HaveCount(2);

        items.Should().ContainItemsAssignableTo<PropertyGridItem<string>>()
            .And.Contain(i => i.Name == nameof(foo.Name))
            .And.Contain(i => i.Name == nameof(foo.IntValue));
    }

    [Fact]
    public void Should_return_items_for_common_properties_of_multiple_instances()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>()
            .WithPropertyDescriptorProvider<FooPropertyDescriptorProvider>()
            .WithPropertyDescriptorProvider<BarPropertyDescriptorProvider>();

        using var serviceProvider = services.BuildServiceProvider();

        var itemCollectionBuilder = serviceProvider.GetRequiredService<IPropertyGridItemCollectionBuilder<object>>();
        var messageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();

        var context = new object();
        var foo = new Foo();
        var bar1 = new Bar();
        var bar2 = new Bar();

        // Act
        var items = itemCollectionBuilder.Build([foo, bar1, bar2], context, messageStore);

        // Assert
        items.Should().HaveCount(2);

        items.Should().Contain(i => i.Name == nameof(foo.Name))
            .And.Contain(i => i.Name == nameof(foo.IntValue));
    }

    [Theory]
    [InlineData(nameof(Baz.StringValue), typeof(PropertyGridItem<string>))]
    [InlineData(nameof(Baz.NullableStringValue), typeof(PropertyGridItem<string>))]
    [InlineData(nameof(Baz.BooleanValue), typeof(PropertyGridItem<bool>))]
    [InlineData(nameof(Baz.NullableBooleanValue), typeof(PropertyGridItem<bool?>))]
    [InlineData(nameof(Baz.ByteValue), typeof(NumericPropertyGridItem<byte, byte, byte>))]
    [InlineData(nameof(Baz.NullableByteValue), typeof(NumericPropertyGridItem<byte?, byte, byte>))]
    [InlineData(nameof(Baz.SignedByteValue), typeof(NumericPropertyGridItem<sbyte, sbyte, sbyte>))]
    [InlineData(nameof(Baz.NullableSignedByteValue), typeof(NumericPropertyGridItem<sbyte?, sbyte, sbyte>))]
    [InlineData(nameof(Baz.UnsignedShortValue), typeof(NumericPropertyGridItem<ushort, ushort, ushort>))]
    [InlineData(nameof(Baz.NullableUnsignedShortValue), typeof(NumericPropertyGridItem<ushort?, ushort, ushort>))]
    [InlineData(nameof(Baz.UnsignedIntegerValue), typeof(NumericPropertyGridItem<uint, uint, uint>))]
    [InlineData(nameof(Baz.NullableUnsignedIntegerValue), typeof(NumericPropertyGridItem<uint?, uint, uint>))]
    [InlineData(nameof(Baz.UnsignedLongValue), typeof(NumericPropertyGridItem<ulong, ulong, ulong>))]
    [InlineData(nameof(Baz.NullableUnsignedLongValue), typeof(NumericPropertyGridItem<ulong?, ulong, ulong>))]
    [InlineData(nameof(Baz.ShortValue), typeof(NumericPropertyGridItem<short, short, short>))]
    [InlineData(nameof(Baz.NullableShortValue), typeof(NumericPropertyGridItem<short?, short, short>))]
    [InlineData(nameof(Baz.IntValue), typeof(NumericPropertyGridItem<int, int, int>))]
    [InlineData(nameof(Baz.NullableIntValue), typeof(NumericPropertyGridItem<int?, int, int>))]
    [InlineData(nameof(Baz.LongValue), typeof(NumericPropertyGridItem<long, long, long>))]
    [InlineData(nameof(Baz.NullableLongValue), typeof(NumericPropertyGridItem<long?, long, long>))]
    [InlineData(nameof(Baz.DecimalValue), typeof(NumericPropertyGridItem<decimal, decimal, decimal>))]
    [InlineData(nameof(Baz.NullableDecimalValue), typeof(NumericPropertyGridItem<decimal?, decimal, decimal>))]
    [InlineData(nameof(Baz.DoubleValue), typeof(NumericPropertyGridItem<double, double, double>))]
    [InlineData(nameof(Baz.NullableDoubleValue), typeof(NumericPropertyGridItem<double?, double, double>))]
    [InlineData(nameof(Baz.FloatValue), typeof(NumericPropertyGridItem<float, float, float>))]
    [InlineData(nameof(Baz.NullableFloatValue), typeof(NumericPropertyGridItem<float?, float, float>))]
    [InlineData(nameof(Baz.UriValue), typeof(PropertyGridItem<Uri>))]
    [InlineData(nameof(Baz.NullableUriValue), typeof(PropertyGridItem<Uri?>))]
    [InlineData(nameof(Baz.EnumValue), typeof(SelectionPropertyGridItem<UriFormat>))]
    [InlineData(nameof(Baz.NullableEnumValue), typeof(SelectionPropertyGridItem<UriFormat?>))]
    public void Should_return_suitable_item_for_specific_property(string propertyName, Type itemType)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<BazContext>()
            .WithPropertyDescriptorProvider<BazPropertyDescriptorProvider>();

        using var serviceProvider = services.BuildServiceProvider();

        var itemCollectionBuilder = serviceProvider.GetRequiredService<IPropertyGridItemCollectionBuilder<BazContext>>();
        var messageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<BazContext>>();

        var baz = new Baz();
        var bazType = baz.GetType();
        var propertyInfo = bazType.GetProperty(propertyName)
            ?? throw new ArgumentException("Property not found", nameof(propertyName));

        var propertyType = propertyInfo.PropertyType;

        var getValueFuncDelegate = GetValueFunc<Baz, object>;
        var getValueFuncMethod = getValueFuncDelegate.Method.GetGenericMethodDefinition()
            .MakeGenericMethod(bazType, propertyType);

        var getValueFunc = getValueFuncMethod.Invoke(null, [propertyName]);

        IPropertyDescriptor<Baz>? propertyDescriptor;

        var numericValueTypeDescriptors = serviceProvider.GetRequiredService<IEnumerable<INumericValueTypeDescriptor>>();

        if (numericValueTypeDescriptors.Any(d => d.UnderlyingType == propertyType))
        {
            var createNumericPropertyDescriptorDelegate = CreateNumericPropertyDescriptor<object, object>;
            var createNumericPropertyDescriptorMethod = createNumericPropertyDescriptorDelegate.Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(bazType, propertyType);

            propertyDescriptor = createNumericPropertyDescriptorMethod.Invoke(null, [propertyName, getValueFunc, serviceProvider])
                as IPropertyDescriptor<Baz>;
        }
        else if (propertyType.IsEnum || propertyType.MakeNonNullableType().IsEnum)
        {
            var createPropertyDescriptorDelegate = CreateSelectionPropertyDescriptor<object, UriFormat>;
            var createPropertyDescriptorMethod = createPropertyDescriptorDelegate.Method.GetGenericMethodDefinition()
                .MakeGenericMethod(bazType, propertyType);

            propertyDescriptor = createPropertyDescriptorMethod.Invoke(null, [propertyName, getValueFunc]) as IPropertyDescriptor<Baz>;
        }
        else
        {
            var createPropertyDescriptorDelegate = CreatePropertyDescriptor<object, object>;
            var createPropertyDescriptorMethod = createPropertyDescriptorDelegate.Method.GetGenericMethodDefinition()
                .MakeGenericMethod(bazType, propertyType);

            propertyDescriptor = createPropertyDescriptorMethod.Invoke(null, [propertyName, getValueFunc]) as IPropertyDescriptor<Baz>;
        }

        if (propertyDescriptor is null)
            throw new InvalidOperationException("Create property descriptor failed");

        var context = new BazContext { PropertyDescriptor = propertyDescriptor };

        // Act
        var items = itemCollectionBuilder.Build([baz], context, messageStore);

        // Assert
        items.Should().HaveCount(1).And.AllBeOfType(itemType);
    }

    /// <summary>
    /// Returns a compiled function that gets the value of the specified property from an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// https://medium.com/the-pragmatic-tech-review/expressions-in-net-a-practical-guide-to-system-linq-expressions-afb416934ff9
    /// </remarks>
    public static Func<T, TResult> GetValueFunc<T, TResult>(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(T), "obj");
        var property = Expression.Property(parameter, propertyName);
        var lambda = Expression.Lambda<Func<T, TResult>>(property, parameter);

        return lambda.Compile();
    }

    private static PropertyDescriptor<TInstance, TPropertyValue> CreatePropertyDescriptor<TInstance, TPropertyValue>(string name,
        Func<TInstance, TPropertyValue> getValue)
            => new() { Name = name, GetValue = getValue };

    private static IPropertyDescriptor<TInstance> CreateNumericPropertyDescriptor<TInstance, TPropertyValue>(string propertyName,
        Func<TInstance, TPropertyValue> getValue, IServiceProvider serviceProvider)
    {
        var propertyType = typeof(TPropertyValue);
        var propertyTypeNonNullable = propertyType.MakeNonNullableType();

        var propertyTypeDescriptor = serviceProvider.GetRequiredService<INumericValueTypeDescriptor<TPropertyValue>>();

        var interval = propertyTypeDescriptor.One;
        var minimum = propertyTypeDescriptor.Minimum;
        var maximum = propertyTypeDescriptor.Maximum;

        var createPropertyDescriptorDelegate = CreateNumericPropertyDescriptor<TInstance, int, int, int>;
        var createPropertyDescriptorMethodInfo = createPropertyDescriptorDelegate.Method.GetGenericMethodDefinition()
            .MakeGenericMethod(typeof(Baz), propertyType, propertyTypeNonNullable, propertyTypeNonNullable);

        var result = createPropertyDescriptorMethodInfo.Invoke(null, [propertyName, getValue, interval, minimum, maximum]);
        if (result is not IPropertyDescriptor<TInstance> propertyDescriptor)
            throw new ArgumentException("Returned property descriptor is not of suitable type");

        return propertyDescriptor;
    }

    private static NumericPropertyDescriptor<TInstance, TPropertyValue, TInterval, TLimit>
        CreateNumericPropertyDescriptor<TInstance, TPropertyValue, TInterval, TLimit>(
            string name, Func<TInstance, TPropertyValue> getValue, TInterval interval, TLimit minimum, TLimit maximum)
                where TInterval : struct
                where TLimit : struct
            => new() { Name = name, GetValue = getValue, Interval = interval, Minimum = minimum, Maximum = maximum };

    private enum HelperEnum;

    private static SelectionPropertyDescriptor<TInstance, TPropertyValue> CreateSelectionPropertyDescriptor<TInstance, TPropertyValue>(
        string name, Func<TInstance, TPropertyValue> getValue)
    {
        var propertyType = typeof(TPropertyValue);
        var propertyTypeNonNullable = propertyType.MakeNonNullableType();

        var getEnumValuesDelegate = Enum.GetValues<HelperEnum>;

        var getEnumValuesMethod = getEnumValuesDelegate.Method
            .GetGenericMethodDefinition()
            .MakeGenericMethod(propertyTypeNonNullable);

        var getEnumValuesResult = getEnumValuesMethod.Invoke(null, []);

        if (getEnumValuesResult is not Array enumValuesNonNullable)
            throw new InvalidOperationException();

        var enumValues = enumValuesNonNullable.Cast<TPropertyValue>();

        return new()
        {
            Name = name,
            GetValue = getValue,
            GetSelectableValues = _ => enumValues.Select(v => new SelectableValue<TPropertyValue> { Value = v, Text = $"{v}" })
        };
    }

    private interface IHasName
    {
        string Name { get; }
    }

    private sealed class Foo : IHasName
    {
        public string Name { get; init; } = "Unnamed";
        public int IntValue { get; init; }
    }

    private sealed class Bar : IHasName
    {
        public string Name { get; init; } = "Unnamed";
        public int IntValue { get; init; }
    }

    private sealed class Baz
    {
        public string StringValue { get; init; } = string.Empty;
        public string? NullableStringValue { get; set; }
        public bool BooleanValue { get; init; }
        public bool? NullableBooleanValue { get; init; }
        public byte ByteValue { get; init; }
        public byte? NullableByteValue { get; init; }
        public sbyte SignedByteValue { get; init; }
        public sbyte? NullableSignedByteValue { get; init; }
        public ushort UnsignedShortValue { get; init; }
        public ushort? NullableUnsignedShortValue { get; init; }
        public uint UnsignedIntegerValue { get; init; }
        public uint? NullableUnsignedIntegerValue { get; init; }
        public ulong UnsignedLongValue { get; init; }
        public ulong? NullableUnsignedLongValue { get; init; }
        public short ShortValue { get; init; }
        public short? NullableShortValue { get; init; }
        public int IntValue { get; init; }
        public int? NullableIntValue { get; init; }
        public long LongValue { get; init; }
        public long? NullableLongValue { get; init; }
        public decimal DecimalValue { get; init; }
        public decimal? NullableDecimalValue { get; init; }
        public double DoubleValue { get; init; }
        public double? NullableDoubleValue { get; init; }
        public float FloatValue { get; init; }
        public float? NullableFloatValue { get; init; }
        public Uri UriValue { get; init; } = new Uri("https://www.example.com");
        public Uri? NullableUriValue { get; init; }
        public UriFormat EnumValue { get; init; } = UriFormat.SafeUnescaped;
        public UriFormat? NullableEnumValue { get; init; }
    }

    private sealed class NamePropertyDescriptor<TInstance> : PropertyDescriptor<TInstance, string>
        where TInstance : class, IHasName
    {
        [SetsRequiredMembers]
        public NamePropertyDescriptor()
        {
            Name = nameof(IHasName.Name);
            GetValue = instance => instance.Name;
        }
    }

    private sealed class NamePropertyDescriptor : PropertyDescriptor<Foo, string>
    {
        [SetsRequiredMembers]
        public NamePropertyDescriptor()
        {
            Name = nameof(Foo.Name);
            GetValue = foo => foo.Name;
        }
    }

    private sealed class NamePropertyDescriptorProvider<TInstance> : IPropertyDescriptorProvider<object, TInstance>
        where TInstance : class, IHasName
    {
        public IEnumerable<IPropertyDescriptor<TInstance>> GetPropertyDescriptors(object context)
        {
            yield return new NamePropertyDescriptor<TInstance>();
        }
    }

    private sealed class FooPropertyDescriptorProvider : IPropertyDescriptorProvider<object, Foo>
    {
        public IEnumerable<IPropertyDescriptor<Foo>> GetPropertyDescriptors(object context)
        {
            yield return new NamePropertyDescriptor<Foo>();

            yield return new PropertyDescriptor<Foo, int>
            {
                Name = nameof(Foo.IntValue),
                GetValue = bar => bar.IntValue
            };
        }
    }

    private sealed class BarPropertyDescriptorProvider : IPropertyDescriptorProvider<object, Bar>
    {
        public IEnumerable<IPropertyDescriptor<Bar>> GetPropertyDescriptors(object context)
        {
            yield return new NamePropertyDescriptor<Bar>();

            yield return new PropertyDescriptor<Bar, int>
            {
                Name = nameof(Bar.IntValue),
                GetValue = bar => bar.IntValue
            };
        }
    }

    private sealed class BazContext
    {
        public required IPropertyDescriptor<Baz> PropertyDescriptor { get; set; }
    }

    private sealed class BazPropertyDescriptorProvider : IPropertyDescriptorProvider<BazContext, Baz>
    {
        public IEnumerable<IPropertyDescriptor<Baz>> GetPropertyDescriptors(BazContext context)
            => [context.PropertyDescriptor];
    }
}
