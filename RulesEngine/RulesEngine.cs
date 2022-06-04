using RulesEngine.Exceptions;
using System.Linq.Expressions;

namespace RulesEngine;

/// <summary>
/// Service that determines whether an object matches a set of rules.
/// </summary>
public class RulesEngine
{
    private readonly IEnumerable<IRule> rules;

    /// <summary>
    /// Initialises the engine with a given set of rules.
    /// </summary>
    /// <param name="rules">The rules to assess objects against.</param>
    public RulesEngine(IEnumerable<IRule> rules)
    {
        this.rules = rules;
    }

    /// <summary>
    /// Gets rules that the given object matches.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The object to check for matching rules.</param>
    /// <returns>The list of rules that the object matches.</returns>
    public List<IRule> GetMatchingRules<T>(T value)
    {
        if (value == null) return new List<IRule>();

        return this.rules.Where(rule => MatchesRule(value, rule)).ToList();
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
        if (rule == null) throw new ArgumentNullException(nameof(rule));
        if (value == null) return false;

        return rule.Criteria.All(criterion => MatchesCriterion(value, criterion));
    }

    private bool MatchesCriterion<T>(T value, IRuleCriterion criterion)
    {
        var check = CompileCriterion<T>(criterion);
        return check(value);
    }

    private Func<T, bool> CompileCriterion<T>(IRuleCriterion criterion)
    {
        // Expression is (<operator> (property (<parameter>) <propertyName>) <value>)
        var parameterExpression = Expression.Parameter(typeof(T));
        var propertyAccessExpression = MemberExpression.Property(parameterExpression, criterion.PropertyName);
        var operatorExpression = GetBinaryOperatorExpression(criterion.OperatorName, propertyAccessExpression, criterion.Value);

        return Expression.Lambda<Func<T, bool>>(body: operatorExpression, parameters: parameterExpression).Compile();
    }

    /// <summary>
    /// Gets a Expression for an operator.
    /// </summary>
    /// <param name="operatorName">The name of the operator.</param>
    /// <param name="objectExpression">An expression for the object to perform the operator on (as a property access expression).</param>
    /// <param name="argument">The argument to the operator.</param>
    /// <returns>An expression that performs the operator on the object and argument.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null.</exception>
    /// <exception cref="InvalidOperatorForPropertyTypeException">Thrown when the operator provided is not valid for the data type of the object.</exception>
    /// <exception cref="InvalidArgumentTypeForOperatorException">Thrown when the argument provided is not valid for the operator.</exception>
    protected virtual Expression GetBinaryOperatorExpression(string operatorName, MemberExpression objectExpression, object argument)
    {
        if (operatorName == null) throw new ArgumentNullException(nameof(operatorName));
        if (objectExpression == null) throw new ArgumentNullException(nameof(objectExpression));
        if (argument == null) throw new ArgumentNullException(nameof(argument));

        var propertyType = objectExpression.Type;

        switch (operatorName)
        {
            case "Contains":
            case "StartsWith":
            case "EndsWith":
                if (!propertyType.IsAssignableTo(typeof(string)))
                    throw new InvalidOperatorForPropertyTypeException(operatorName, dataType: propertyType);

                if (!argument.GetType().IsAssignableTo(typeof(string)))
                    throw new InvalidArgumentTypeForOperatorException(operatorName, dataType: argument.GetType());

                return GetMethodCallExpression(objectExpression, propertyType, operatorName, Expression.Constant(argument), Expression.Constant(StringComparison.InvariantCultureIgnoreCase));
        }

        if (Enum.TryParse(operatorName, out ExpressionType binaryOperator))
        {
            try
            {
                var argumentExpression = Expression.Constant(Convert.ChangeType(argument, propertyType));
                return Expression.MakeBinary(binaryOperator, objectExpression, argumentExpression);
            }
            catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
            {
                throw new InvalidArgumentTypeForOperatorException(operatorName, argument.GetType(), expectedDataType: propertyType);
            }
        }

        throw new UnrecognizedOperatorException(operatorName);
    }

    /// <summary>
    /// Utility method that gets an expression that calls the given method on the object provided, with the arguments provided.
    /// </summary>
    /// <param name="objectExpression">The (expression for) object to call the method on.</param>
    /// <param name="objectType">The type of the object that the method is being called on.</param>
    /// <param name="methodName">The name of the method to call.</param>
    /// <param name="parameters">Parameters to the method. If these are derived from <see cref="Expression"/> then the expression is left as-is. Other values are fed as constants to the method.</param>
    /// <exception cref="InvalidOperatorForPropertyTypeException">Thrown if the method does not exist on the object.</exception>
    protected static Expression GetMethodCallExpression(MemberExpression objectExpression, Type objectType, string methodName, params object[] parameters)
    {
        var method = objectType.GetMethod(methodName) ?? throw new InvalidOperatorForPropertyTypeException(methodName, objectType);

        var parameterExpressions = parameters
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