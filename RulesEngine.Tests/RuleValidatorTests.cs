using RulesEngine.Exceptions;
using RulesEngine.Tests.ConcreteImplementation;

namespace RulesEngine.Tests;

public class RuleValidatorTests
{
    [Fact]
    public void CanValidateRules()
    {
        var validator = new RuleValidator();

        {
            var rule = new Rule("Example")
            {
                new RuleCriterion("TestValue", "Equal", "test")
            };

            Assert.Empty(validator.ValidateRule(rule));
        }

        {
            var rule = new Rule("Example")
            {
                new RuleCriterion("TestValue", "Equals", "test")
            };

            Assert.Single(validator.ValidateRule(rule));
            Assert.Contains("is not a recognised operator", validator.ValidateRule(rule).First().ErrorMessage);
        }

        {
            // Test case sensitivity
            var rule = new Rule("Example")
            {
                new RuleCriterion("TestValue", "EQUAL", "test")
            };

            Assert.Single(validator.ValidateRule(rule));
            Assert.Contains("is not a recognised operator", validator.ValidateRule(rule).First().ErrorMessage);
        }

        {
            var rule = new Rule("Example")
            {
                new RuleCriterion("0Invalid", "Equal", "test")
            };

            Assert.Single(validator.ValidateRule(rule));
            Assert.Contains("is not a valid series of property names", validator.ValidateRule(rule).First().ErrorMessage);
        }

        {
            var rule = new Rule("Example")
            {
                new RuleCriterion("Ab@", "Equal", "test")
            };

            Assert.Single(validator.ValidateRule(rule));
            Assert.Contains("is not a valid series of property names", validator.ValidateRule(rule).First().ErrorMessage);
        }

        {
            var rule = new Rule("Example")
            {
                new RuleCriterion("Abc.@123", "Equal", "test")
            };

            Assert.Empty(validator.ValidateRule(rule));
        }

        {
            var rule = new Rule("Example")
            {
                new RuleCriterion("Abc.0123", "Equal", "test")
            };

            Assert.Single(validator.ValidateRule(rule));
            Assert.Contains("is not a valid series of property names", validator.ValidateRule(rule).First().ErrorMessage);
        }

        {
            var rule = new Rule("Example")
            {
                new RuleCriterion("\u0414\u0416\u0065\u006d", "Equal", "test")
            };

            Assert.Empty(validator.ValidateRule(rule));
        }
    }
}
