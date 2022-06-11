using RulesEngine.Exceptions;
using System.Linq.Expressions;

namespace RulesEngine;

/// <summary>
/// Service that determines whether an object matches a set of rules.
/// </summary>
/// <remarks>
/// Prefer <see cref="RuleEvaluator{T}"/> where the type is known as this caches the compilation of rules.
/// </remarks>
public class RuleEvaluator
{
    private readonly IEnumerable<IRule> rules;

    /// <summary>
    /// Initialises the engine with a given set of rules.
    /// </summary>
    /// <param name="rules">The rules to assess objects against.</param>
    public RuleEvaluator(IEnumerable<IRule> rules)
    {
        this.rules = rules;
    }

    /// <summary>
    /// Gets rules that the given object matches.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The object to check for matching rules.</param>
    /// <returns>The list of rules that the object matches.</returns>
    public IEnumerable<IRule> GetMatchingRules<T>(T value)
    {
        var engine = new RuleEvaluator<T>(this.rules);
        return engine.GetMatchingRules(value);
    }

    /// <summary>
    /// Determines whether the object matches the provided rule.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The object to assess the rule against.</param>
    /// <param name="rule">The rule that will be assessed for the object.</param>
    /// <returns>True if the object matches the rule.</returns>
    public bool MatchesRule<T>(T value, IRule rule)
    {
        var engine = new RuleEvaluator<T>(this.rules);
        return engine.MatchesRule(value, rule);
    }
}

/// <summary>
/// Service that determines whether an object matches a set of rules.
/// </summary>
/// <typeparam name="T">The type of the objects to be checked.</typeparam>
public class RuleEvaluator<T>
{
    private readonly IDictionary<IRule, Func<T, bool>> rules;

    /// <summary>
    /// Initialises the engine with a given set of rules.
    /// </summary>
    /// <param name="rules">The rules to assess objects against.</param>
    public RuleEvaluator(IEnumerable<IRule> rules)
    {
        this.rules = rules.ToDictionary(r => r, r => CompileRule(r));
    }

    /// <summary>
    /// Gets rules that the given object matches.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The object to check for matching rules.</param>
    /// <returns>The list of rules that the object matches. An empty list is returned if <paramref name="value"/> is null.</returns>
    public IEnumerable<IRule> GetMatchingRules(T value)
    {
        if (value == null) return new List<IRule>();

        return this.rules
            .Where(rule => rule.Value(value))
            .Select(rule => rule.Key)
            .ToList();
    }

    /// <summary>
    /// Determines whether the object matches the provided rule.
    /// </summary>
    /// <remarks>
    /// Note: If the rule is not in the list of rules provided to the constructor, its compilation will not be cached.
    /// </remarks>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The object to assess the rule against.</param>
    /// <param name="rule">The rule that will be assessed for the object.</param>
    /// <returns>True if the object matches the rule.</returns>
    public bool MatchesRule(T value, IRule rule)
    {
        if (rule == null) throw new ArgumentNullException(nameof(rule));
        if (value == null) return false;

        Func<T, bool> evaluator;
        if (this.rules.ContainsKey(rule))
        {
            evaluator = this.rules[rule];
        }
        else
        {
            // If the rule isn't cached, compile it on the go
            evaluator = CompileRule(rule);
        }

        return evaluator(value);
    }

    private Func<T, bool> CompileRule(IRule rule)
    {
        var parameterExpression = Expression.Parameter(typeof(T));

        Expression body;
        if (!rule.Criteria.Any())
        {
            body = Expression.Constant(true);
        }
        else
        {
            body = GetExpressionForCriterion(parameterExpression, rule.Criteria.First());

            foreach (var criterion in rule.Criteria.Skip(1).ToList())
            {
                body = Expression.AndAlso(body, GetExpressionForCriterion(parameterExpression, criterion));
            }
        }

        return Expression.Lambda<Func<T, bool>>(body, parameterExpression).Compile();
    }

    private Expression GetExpressionForCriterion(ParameterExpression parameterExpression, IRuleCriterion criterion)
    {
        // Expression is a either a unary or binary operator in the form (<operator> (property (<parameter>) <propertyName>) [value])
        var propertyAccessExpression = GetExpressionForPropertyPath(parameterExpression, criterion.PropertyPath);
        return GetExpressionForOperator(criterion.OperatorName, propertyAccessExpression, criterion.Value);
    }

    /// <summary>
    /// Gets an expression that accesses the provided property path.
    /// For instance if you have a property path <c>Child.GrandChild</c> it will return an expression to return the value of <c>GrandChild</c>.
    /// </summary>
    private static MemberExpression GetExpressionForPropertyPath(ParameterExpression parameterExpression, string propertyPath)
    {
        var pathComponents = propertyPath.Split(".");

        Expression objectExpression = parameterExpression;

        foreach (var propertyName in pathComponents)
        {
            if (objectExpression.Type.GetProperty(propertyName) == null)
                throw new InvalidPropertyPathException(parameterExpression.Type, propertyPath, propertyName, objectExpression.Type);

            objectExpression = MemberExpression.Property(objectExpression, propertyName);
        }

        return (MemberExpression)objectExpression;
    }

    /// <summary>
    /// Gets an <see cref="Expression"/> for an operator. Can be overriden to support additional operators.
    /// </summary>
    /// <param name="operatorName">The name of the operator.</param>
    /// <param name="objectExpression">An expression that gets the property to be assessed.</param>
    /// <param name="argument">The argument to the operator.</param>
    /// <returns>An expression that performs the operator on the property and argument.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null.</exception>
    /// <exception cref="InvalidOperatorForPropertyTypeException">Thrown when the operator provided is not valid for the data type of the object.</exception>
    /// <exception cref="InvalidArgumentTypeForOperatorException">Thrown when the argument provided is not valid for the operator.</exception>
    protected virtual Expression GetExpressionForOperator(string operatorName, MemberExpression objectExpression, string? argument)
    {
        if (operatorName == null) throw new ArgumentNullException(nameof(operatorName));
        if (objectExpression == null) throw new ArgumentNullException(nameof(objectExpression));

        var propertyType = objectExpression.Type;

        switch (operatorName)
        {
            case "Contains":
            case "StartsWith":
            case "EndsWith":
                if (!propertyType.IsAssignableTo(typeof(string)))
                    throw new InvalidOperatorForPropertyTypeException(operatorName, dataType: propertyType);

                if (argument == null)
                    return Expression.Constant(false);

                if (!argument.GetType().IsAssignableTo(typeof(string)))
                    throw new InvalidArgumentTypeForOperatorException(operatorName, dataType: argument.GetType());

                return GetMethodCallExpression(
                    objectExpression,
                    propertyType,
                    operatorName,
                    parameterTypes: new[] { typeof(string), typeof(StringComparison) },
                    parameterValues: new[] { Expression.Constant(argument), Expression.Constant(StringComparison.InvariantCultureIgnoreCase) }
                );
            case "DoesNotContain":
                return Expression.Not(GetExpressionForOperator("Contains", objectExpression, argument));
            case "DoesNotStartWith":
                return Expression.Not(GetExpressionForOperator("StartsWith", objectExpression, argument));
            case "DoesNotEndWith":
                return Expression.Not(GetExpressionForOperator("EndsWith", objectExpression, argument));

            case "IsNull":
                {
                    if (objectExpression.Type.IsValueType && Nullable.GetUnderlyingType(objectExpression.Type) == null)
                        return Expression.Constant(false);
                    return Expression.Equal(objectExpression, Expression.Constant(null));
                }
            case "IsNotNull":
                {
                    if (objectExpression.Type.IsValueType && Nullable.GetUnderlyingType(objectExpression.Type) == null)
                        return Expression.Constant(true);
                    return Expression.NotEqual(objectExpression, Expression.Constant(null));
                }
            case nameof(ExpressionType.Equal):
            case nameof(ExpressionType.NotEqual):
            case nameof(ExpressionType.GreaterThan):
            case nameof(ExpressionType.GreaterThanOrEqual):
            case nameof(ExpressionType.LessThan):
            case nameof(ExpressionType.LessThanOrEqual):
                {
                    var binaryOperator = Enum.Parse<ExpressionType>(operatorName);

                    if (argument != null)
                    {
                        try
                        {
                            var converter = System.ComponentModel.TypeDescriptor.GetConverter(propertyType);
                            if (converter is System.ComponentModel.NullableConverter && argument.Equals(""))
                            {
                                // Don't allow an empty string to be converted to a null value by TypeDescriptor
                                // We should consider it distinctly different and promote use of IsNull checks instead
                                return Expression.Constant(false);
                            }
                            var argumentExpression = Expression.Constant(converter.ConvertFrom(argument), propertyType);
                            return Expression.MakeBinary(binaryOperator, objectExpression, argumentExpression);
                        }
                        catch (Exception ex) when (ex is NotSupportedException || ex is ArgumentException)
                        {
                            throw new InvalidArgumentTypeForOperatorException(operatorName, argument.GetType(), expectedDataType: propertyType);
                        }
                    }
                    else
                    {
                        // If our property is a non-nullable value type then we know that it can't equate to a null value
                        if (objectExpression.Type.IsValueType && Nullable.GetUnderlyingType(objectExpression.Type) == null)
                        {
                            if (operatorName == nameof(ExpressionType.NotEqual))
                                return Expression.Constant(true);
                            else
                                return Expression.Constant(false);
                        }

                        // Greater than and less than would never be true when compared to a null value
                        if (operatorName == nameof(ExpressionType.GreaterThan) || operatorName == nameof(ExpressionType.LessThan))
                            return Expression.Constant(false);

                        return Expression.MakeBinary(binaryOperator, objectExpression, Expression.Constant(null, propertyType));
                    }
                }
            case nameof(ExpressionType.IsTrue):
            case nameof(ExpressionType.IsFalse):
                if (propertyType == typeof(bool))
                {
                    var unaryOperator = Enum.Parse<ExpressionType>(operatorName);
                    return Expression.MakeUnary(unaryOperator, objectExpression, typeof(bool));
                }
                else if (propertyType == typeof(bool?))
                {
                    var notNullExpression = Expression.NotEqual(objectExpression, Expression.Constant(null));
                    var unaryOperator = Enum.Parse<ExpressionType>(operatorName);
                    var valueExpression = Expression.Property(objectExpression, typeof(bool?).GetProperty("Value")!);
                    var truthyExpression = Expression.MakeUnary(unaryOperator, valueExpression, typeof(bool));
                    return Expression.AndAlso(notNullExpression, truthyExpression);
                }
                else
                {
                    throw new InvalidOperatorForPropertyTypeException(operatorName, propertyType);
                }
            default:
                throw new UnrecognizedOperatorException(operatorName);
        }
    }

    /// <summary>
    /// Utility method that gets an expression that calls the given method on the object provided, with the arguments provided.
    /// </summary>
    /// <param name="objectExpression">The (expression for) object to call the method on.</param>
    /// <param name="objectType">The type of the object that the method is being called on.</param>
    /// <param name="methodName">The name of the method to call.</param>
    /// <param name="parameters">Parameters to the method. If these are derived from <see cref="Expression"/> then the expression is left as-is. Other values are fed as constants to the method.</param>
    /// <exception cref="InvalidOperatorForPropertyTypeException">Thrown if the method does not exist on the object.</exception>
    protected static Expression GetMethodCallExpression(MemberExpression objectExpression, Type objectType, string methodName, Type[] parameterTypes, object[] parameterValues)
    {
        var method = objectType.GetMethod(methodName, parameterTypes) ?? throw new InvalidOperatorForPropertyTypeException(methodName, objectType);

        var parameterExpressions = parameterValues
            .Select(parameter =>
            {
                if (parameter is Expression e)
                    return e;
                else
                    return Expression.Constant(parameter);
            })
            .ToList();

        return Expression.Call(objectExpression, method, parameterExpressions);
    }
}