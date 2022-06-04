using RulesEngine.Exceptions;
using RulesEngine.Tests.ConcreteImplementation;

namespace RulesEngine.Tests;

public class RuleTests
{
    [Fact]
    public void CanCompareEqual()
    {
        var rule = new Rule("Equal to \"test\"")
        {
            new RuleCriterion("TestValue", "Equal", "test")
        };

        var matches = new
        {
            TestValue = "test"
        };

        var doesNotMatch = new
        {
            TestValue = "no match"
        };

        var rules = new List<Rule>() { rule };
        var rulesEngine = new RulesEngine(rules);

        Assert.True(rulesEngine.MatchesRule(matches, rule));
        Assert.Single(rulesEngine.GetMatchingRules(matches));
        Assert.Equal(rulesEngine.GetMatchingRules(matches).First(), rule);

        Assert.False(rulesEngine.MatchesRule(doesNotMatch, rule));
        Assert.Empty(rulesEngine.GetMatchingRules(doesNotMatch));
    }

    [Fact]
    public void InvalidOperatorForPropertyTypeExceptionThrows()
    {
        {
            var rule = new Rule("Contains \"test\"")
            {
                new RuleCriterion("TestValue", "Contains", "test")
            };

            var obj = new
            {
                TestValue = 12
            };

            var rules = new List<Rule>() { rule };
            var rulesEngine = new RulesEngine(rules);

            Assert.Throws<InvalidOperatorForPropertyTypeException>(() =>
            {
                rulesEngine.MatchesRule(obj, rule);
            });

            Assert.Throws<InvalidOperatorForPropertyTypeException>(() =>
            {
                rulesEngine.GetMatchingRules(obj);
            });
        }
    }

    [Fact]
    public void InvalidArgumentTypeForOperatorExceptionThrows()
    {
        {
            var rule = new Rule("Equal to \"test\"")
            {
                new RuleCriterion("TestValue", "Equal", "test")
            };

            var obj = new
            {
                TestValue = 12
            };

            var rules = new List<Rule>() { rule };
            var rulesEngine = new RulesEngine(rules);

            Assert.Throws<InvalidArgumentTypeForOperatorException>(() =>
            {
                rulesEngine.MatchesRule(obj, rule);
            });

            Assert.Throws<InvalidArgumentTypeForOperatorException>(() =>
            {
                rulesEngine.GetMatchingRules(obj);
            });
        }
    }

    [Fact]
    public void UnrecognizedOperatorExceptionThrows()
    {
        var rule = new Rule("Equal to \"test\"")
        {
            // NB: should be Equal not Equals
            new RuleCriterion("TestValue", "Equals", "test")
        };

        var obj = new
        {
            TestValue = "test"
        };

        var rules = new List<Rule>() { rule };
        var rulesEngine = new RulesEngine(rules);

        Assert.Throws<UnrecognizedOperatorException>(() =>
        {
            rulesEngine.MatchesRule(obj, rule);
        });

        Assert.Throws<UnrecognizedOperatorException>(() =>
        {
            rulesEngine.GetMatchingRules(obj);
        });
    }
}
