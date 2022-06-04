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
}


// Invalid data type throws exception, e.g. int for CONTAINS