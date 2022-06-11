using RulesEngine.Exceptions;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace RulesEngine;

/// <summary>
/// Validates rules, to see if they might be able to run.
/// Note that this performs static analysis of the rules: they may encounter run-time errors due to mismatched types etc.
/// </summary>
public class RuleValidator
{
    /// <summary>
    /// Validates a rule.
    /// </summary>
    /// <param name="rule">The rule to validate.</param>
    /// <returns>The list of validation errors for the rule. If empty then the rule is valid.</returns>
    public List<RuleValidationError> ValidateRule(IRule rule)
    {
        if (rule == null) throw new ArgumentNullException(nameof(rule));
        if (!rule.Criteria.Any()) return new List<RuleValidationError>() { new RuleValidationError("The rule has no criteria.") };

        return rule.Criteria.SelectMany(c => ValidateCriterion(c)).ToList();
    }

    /// <summary>
    /// Validates a rule criterion.
    /// </summary>
    protected virtual List<RuleValidationError> ValidateCriterion(IRuleCriterion criterion)
    {
        var errors = new List<RuleValidationError>();

        errors.AddRange(ValidatePropertyPath(criterion));
        errors.AddRange(ValidateOperator(criterion));

        return errors;
    }

    /// <summary>
    /// Validates the property path for a rule criterion.
    /// </summary>
    protected virtual List<RuleValidationError> ValidatePropertyPath(IRuleCriterion criterion)
    {
        var errors = new List<RuleValidationError>();

        if (criterion.PropertyPath == null)
        {
            errors.Add(new RuleValidationError("No property path is provided."));
        }
        else
        {
            var pathComponents = criterion.PropertyPath.Split(".");
            foreach (var propertyName in pathComponents)
            {
                if (!Regex.IsMatch(propertyName, @"^[@\p{L}_][\p{L}0-9_]*$"))
                {
                    errors.Add(new RuleValidationError($"Property path '{criterion.PropertyPath}' is not a valid series of property names."));
                    continue;
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Returns true if the operator is a known operator.
    /// </summary>
    protected virtual bool IsKnownOperator(string operatorName)
    {
        var validOperators = new[]
        {
            "Contains",
            "StartsWith",
            "EndsWith",
            "IsNull",
            "IsNotNull",
        };

        if (validOperators.Contains(operatorName))
            return true;

        var validExpressionOperators = new[]
        {
            ExpressionType.Equal,
            ExpressionType.GreaterThan,
            ExpressionType.GreaterThanOrEqual,
            ExpressionType.LessThan,
            ExpressionType.LessThanOrEqual,
            ExpressionType.NotEqual,
            ExpressionType.IsTrue,
            ExpressionType.IsFalse,
        };

        if (Enum.TryParse(operatorName, out ExpressionType expressionType) && validExpressionOperators.Contains(expressionType))
            return true;

        return false;
    }

    /// <summary>
    /// Validates the operator for a rule criterion.
    /// </summary>
    protected virtual List<RuleValidationError> ValidateOperator(IRuleCriterion criterion)
    {
        var errors = new List<RuleValidationError>();

        if (!IsKnownOperator(criterion.OperatorName))
        {
            errors.Add(new RuleValidationError($"Operator '{criterion.OperatorName}' is not a recognised operator."));
        }

        return errors;
    }
}