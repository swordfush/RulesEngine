using RulesEngine.Tests.ExampleImplementation;

namespace RulesEngine.Tests;

/// <summary>
/// Verify that the example in README.md works.
/// </summary>
public class ExampleTests
{
    [Fact]
    public void ExampleIsFunctional()
    {
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

        var player = new Player()
        {
            Name = "Steve",
            Response = new Response()
            {
                MillisecondsToReply = 1423,
                Answer = 42
            }
        };

        {
            var evaluator = new RuleEvaluator<Player>(rules);
            Assert.Single(evaluator.GetMatchingRules(player));
        }

        {
            var evaluator = new RuleEvaluator(rules);
            Assert.Single(evaluator.GetMatchingRules(player));
        }
    }
}
