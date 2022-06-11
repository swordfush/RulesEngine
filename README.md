# RulesEngine
A rules engine used to evaluate a set of user-defined business rules against objects.

Rules are defined so that they could easily be entered by non-technical domain experts via a user interface.

The library is designed to be easily extensible and adaptable: the behaviour of the engine can be overriden, and the outcome when rules are matched is entirely up to the implementor.

# Usage

## Implement IRule and IRuleCriterion
`IRule` represents a rule to be evaluated, and `IRuleCriterion` is one of the criteria that must be met for a rule to be matched.  Concrete implementations need to be provided for these interfaces in order to use the rule engine.

`IRuleCriterion` is the most important here: each criterion is an operation that evaluates the value of a property on the object, and returns a boolean indicating whether the criterion was met.

For example, if our object is a `Player`, who has provided an answer indicated by the property `Player.Response.Answer` the property path for the criterion will be `Response.Answer`.  We can then check if this is equal to 42 by using operator "Equal" and value "42".

A simple implementation might be along the lines of:

```c#
class Rule : IRule
{
    public string Name { get; set; }
    public List<RuleCriterion> Criteria { get; set; }
    IEnumerable<IRuleCriterion> IRule.Criteria { get => Criteria; }

    public Rule()
    {
        Name = "";
        Criteria = new List<RuleCriterion>();
    }
}

class RuleCriterion : IRuleCriterion
{
    public string PropertyPath { get; set; } = null!;
    public string OperatorName { get; set; } = null!;
    public string Value { get; set; } = null!;
}

class Response
{
    public int MillisecondsToReply { get; set; }
    public int Answer { get; set; }
}

class Player
{
    public string Name { get; set; } = null!;
    public Response Response { get; set; } = null!;
}

// ...

var rules = new List<Rule>()
{
    new Rule()
    {
        Name = "Greater than 12, but not 13, and not Steve",
        Criteria = new List<RuleCriterion>()
        {
            new RuleCriterion()
            {
                PropertyPath = "Response.Answer",
                OperatorName = "GreaterThan",
                Value = "12"
            },
            new RuleCriterion()
            {
                PropertyPath = "Response.Answer",
                OperatorName = "NotEqual",
                Value = "13"
            },
            new RuleCriterion()
            {
                PropertyPath = "Name",
                OperatorName = "NotEqual",
                Value = "Steve"
            }
        }
    },
    new Rule()
    {
        Name = "Is the answer to life",
        Criteria = new List<RuleCriterion>()
        {
            new RuleCriterion()
            {
                PropertyPath = "Response.Answer",
                OperatorName = "Equal",
                Value = "42"
            }
        }
    }
};
```

## Evaluate Rules
Once your rules are defined you can then instantiate a `RuleEvaluator` to assess what rules an object matches.

```c#
var evaluator = new RuleEvaluator<Player>(rules);

var player = new Player()
{
    Name = "Steve",
    Response = new Response()
    {
        MillisecondsToReply = 1423,
        Answer = 42
    }
};

var matchingRules = evaluator.GetMatchingRules(player);
// => will return "Is the answer to life", and we can now act upon that rule.
```

It is also possible to assess rules against objects of different types using the non-generic `MatchEvaluator`, provided that the rules are valid for both types (e.g. property paths are valid and operations valid for the property data types).  When using this approach there is a performance penalty, as rules are compiled for each check.

```c#
var evaluator = new RuleEvaluator(rules);

var player = new Player()
{
    Name = "Steve",
    Response = new Response()
    {
        MillisecondsToReply = 1423,
        Answer = 42
    }
};

var matchingRules = evaluator.GetMatchingRules(player);
// => will return "Is the answer to life", and we can now act upon that rule.
```

## (Optional) Validate Rules
It is possible to determine if rules are valid before they are run, though note that type and operator mismatches are only caught when evaluating the rule at runtime.

To perform validation on a rule, feed it into the `RuleValidator.ValidateRule(rule)` method to get a list of validation errors, or an empty list if there is no apparent issue with the rule.

```c#
var validator = new RuleValidator();
var validationErrors = validator.ValidateRule(rule);

foreach (var validationError in validationErrors)
{
    Console.WriteLine($"Rule {rule.Name} is invalid: {validationError.ErrorMessage}");
}
```

# Extending Supported Operations
The `RuleEvaluator` class is able to extended in order to provide your own custom implementation of operators.

To do this inherit from `RuleEvaluator` and override the `GetBinaryOperatorExpression()` method.

# Future possibilities
* Tighten validation to find type mismatches and useless rules (e.g. nullable property equals "").
* Support non-string values?
* Support full expression trees?

