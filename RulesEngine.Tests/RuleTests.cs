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

    [Fact]
    public void CanAccessSubProperties()
    {
        var rule = new Rule("Equal to \"test\"")
        {
            new RuleCriterion("RootObject.TestValue", "Equal", "test")
        };

        var matches = new
        {
            RootObject = new {
                TestValue = "test"
            }
        };

        var doesNotMatch = new
        {
            RootObject = new {
                TestValue = "no match"
            }
        };

        var hasNoProperty = new
        {
            TestValue = "test"
        };

        var rules = new List<Rule>() { rule };
        var rulesEngine = new RulesEngine(rules);

        Assert.True(rulesEngine.MatchesRule(matches, rule));
        Assert.Single(rulesEngine.GetMatchingRules(matches));
        Assert.Equal(rulesEngine.GetMatchingRules(matches).First(), rule);

        Assert.False(rulesEngine.MatchesRule(doesNotMatch, rule));
        Assert.Empty(rulesEngine.GetMatchingRules(doesNotMatch));

        Assert.Throws<InvalidPropertyPathException>(() =>
        {
            Assert.False(rulesEngine.MatchesRule(hasNoProperty, rule));
        });
        Assert.Throws<InvalidPropertyPathException>(() =>
        {
            Assert.Empty(rulesEngine.GetMatchingRules(hasNoProperty));
        });
    }

    [Fact]
    public void CanConsiderMultipleCriteria()
    {
        var rule = new Rule("Matches multiple criteria")
        {
            new RuleCriterion("RootObject.TestValue", "Equal", "42"),
            new RuleCriterion("RootObject.TestValue", "GreaterThan", "12"),
            new RuleCriterion("OtherObject.TestValue", "Equal", "24")
        };

        var meetsAllCriteria = new
        {
            RootObject = new
            {
                TestValue = 42
            },
            OtherObject = new
            {
                TestValue = 24
            }
        };

        var meetsTwoCriteria = new
        {
            RootObject = new
            {
                TestValue = 42
            },
            OtherObject = new
            {
                TestValue = 11
            }
        };

        var meetsOneCriterion = new
        {
            RootObject = new
            {
                TestValue = 11
            },
            OtherObject = new
            {
                TestValue = 24
            }
        };

        var meetsNoCriteria = new
        {
            RootObject = new
            {
                TestValue = 11
            },
            OtherObject = new
            {
                TestValue = 24
            }
        };

        var rules = new List<Rule>() { rule };
        var rulesEngine = new RulesEngine(rules);

        Assert.True(rulesEngine.MatchesRule(meetsAllCriteria, rule));
        Assert.Single(rulesEngine.GetMatchingRules(meetsAllCriteria));
        Assert.Equal(rulesEngine.GetMatchingRules(meetsAllCriteria).First(), rule);

        Assert.False(rulesEngine.MatchesRule(meetsTwoCriteria, rule));
        Assert.Empty(rulesEngine.GetMatchingRules(meetsTwoCriteria));

        Assert.False(rulesEngine.MatchesRule(meetsOneCriterion, rule));
        Assert.Empty(rulesEngine.GetMatchingRules(meetsOneCriterion));

        Assert.False(rulesEngine.MatchesRule(meetsNoCriteria, rule));
        Assert.Empty(rulesEngine.GetMatchingRules(meetsNoCriteria));
    }
}
