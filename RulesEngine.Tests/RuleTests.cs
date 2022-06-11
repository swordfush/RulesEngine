using RulesEngine.Exceptions;
using RulesEngine.Tests.ConcreteImplementation;

namespace RulesEngine.Tests;

public class RuleTests
{
    #region Utility

    /// <summary>
    /// Tests a single criterion.
    /// </summary>
    private bool TestCriterion<T>(T testObject, string propertyPath, string operatorName, string value)
    {
        var rule = new Rule("")
        {
            new RuleCriterion(propertyPath, operatorName, value)
        };

        var evaluator = new RuleEvaluator(new List<Rule>());
        return evaluator.MatchesRule(testObject, rule);
    }

    /// <summary>
    /// Tests a single criterion for a unary operator.
    /// </summary>
    private bool TestCriterion<T>(T testObject, string propertyPath, string operatorName)
        => TestCriterion<T>(testObject, propertyPath, operatorName, "");
    
    #endregion

    /// <summary>
    /// Prove that we can perform a basic comparison.
    /// </summary>
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
        var rulesEngine = new RuleEvaluator(rules);

        Assert.True(rulesEngine.MatchesRule(matches, rule));
        Assert.Single(rulesEngine.GetMatchingRules(matches));
        Assert.Equal(rulesEngine.GetMatchingRules(matches).First(), rule);

        Assert.False(rulesEngine.MatchesRule(doesNotMatch, rule));
        Assert.Empty(rulesEngine.GetMatchingRules(doesNotMatch));
    }

    /// <summary>
    /// Expect an exception when attempting to use an operator that is not supported for the property type.
    /// </summary>
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
            var rulesEngine = new RuleEvaluator(rules);

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

    /// <summary>
    /// Expect an exception when the argument to the criteria is invalid for the operator.
    /// </summary>
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
            var rulesEngine = new RuleEvaluator(rules);

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

    /// <summary>
    /// Expect an exception when using an unknown operator.
    /// </summary>
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
        var rulesEngine = new RuleEvaluator(rules);

        Assert.Throws<UnrecognizedOperatorException>(() =>
        {
            rulesEngine.MatchesRule(obj, rule);
        });

        Assert.Throws<UnrecognizedOperatorException>(() =>
        {
            rulesEngine.GetMatchingRules(obj);
        });
    }

    /// <summary>
    /// Check that we can access properties beyond the root object, i.e. property paths like "Child.GrandChild".
    /// </summary>
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
        var rulesEngine = new RuleEvaluator(rules);

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

    /// <summary>
    /// Verify that multiple criteria are assessed, and all required in order for the rule to match.
    /// </summary>
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
        var rulesEngine = new RuleEvaluator(rules);

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

    internal class VaryingPropertiesAndFields
    {
        public string GetSetProperty { get; set; } = "test";
        public string ReadonlyProperty { get; } = "test";
        public string InitProperty { get; init; } = "test";
        public string PrivateGetterProperty { private get; set; } = "test";
        public string Field = "test";
    }

    /// <summary>
    /// Verify that the criteria apply only to properties with a get accessor, not fields.
    /// </summary>
    [Fact]
    public void WorksOnPropertiesWithAGetAccessor()
    {
        var rule = new Rule("")
        {
            new RuleCriterion("TestValue", "Equal", "test")
        };

        var testObject = new VaryingPropertiesAndFields();

        Assert.True(TestCriterion(testObject, "GetSetProperty", "Equal", "test"));
        Assert.True(TestCriterion(testObject, "ReadonlyProperty", "Equal", "test"));
        Assert.True(TestCriterion(testObject, "InitProperty", "Equal", "test"));
        Assert.True(TestCriterion(testObject, "PrivateGetterProperty", "Equal", "test"));
        Assert.Throws<InvalidPropertyPathException>(() =>
        {
            TestCriterion(testObject, "Field", "Equal", "test");
        });
    }

    /// <summary>
    /// Verify support for the "Contains" operator.
    /// </summary>
    [Fact]
    public void SupportsContainsOperator()
    {
        var testObject = new
        {
            Property1 = "test",
            Property2 = "abcdefg"
        };

        Assert.True(TestCriterion(testObject, "Property1", "Contains", "es"));
        Assert.False(TestCriterion(testObject, "Property1", "Contains", "efg"));

        Assert.False(TestCriterion(testObject, "Property2", "Contains", "es"));
        Assert.True(TestCriterion(testObject, "Property2", "Contains", "efg"));

        Assert.True(TestCriterion(testObject, "Property1", "Contains", ""));
    }

    /// <summary>
    /// Verify support for the "StartsWith" and "EndsWith" operators.
    /// </summary>
    [Fact]
    public void SupportsStartsWithAndEndsWithOperators()
    {
        var testObject = new
        {
            Property1 = "test",
            Property2 = "abcdefg"
        };

        Assert.True(TestCriterion(testObject, "Property1", "StartsWith", "te"));
        Assert.False(TestCriterion(testObject, "Property1", "StartsWith", "abc"));
        Assert.False(TestCriterion(testObject, "Property2", "StartsWith", "te"));
        Assert.True(TestCriterion(testObject, "Property2", "StartsWith", "abc"));

        Assert.True(TestCriterion(testObject, "Property1", "EndsWith", "st"));
        Assert.False(TestCriterion(testObject, "Property1", "EndsWith", "efg"));
        Assert.False(TestCriterion(testObject, "Property2", "EndsWith", "st"));
        Assert.True(TestCriterion(testObject, "Property2", "EndsWith", "efg"));
    }

    /// <summary>
    /// Verify support for the "IsNull" and "IsNotNull" operators.
    /// </summary>
    [Fact]
    public void SupportsIsNullAndIsNotNullOperators()
    {
        var testObject = new VaryingDataTypes();

        Assert.False(TestCriterion(testObject, "NullableIntValue", "IsNull"));
        Assert.True(TestCriterion(testObject, "NullNullableIntValue", "IsNull"));

        Assert.True(TestCriterion(testObject, "NullableIntValue", "IsNotNull"));
        Assert.False(TestCriterion(testObject, "NullNullableIntValue", "IsNotNull"));

        // Non-nullable types should return false to IsNull
        Assert.False(TestCriterion(testObject, "IntValue", "IsNull"));
        Assert.True(TestCriterion(testObject, "IntValue", "IsNotNull"));
    }

    internal class Truthy
    {
        public bool TrueValue { get; set; } = true;
        public bool FalseValue { get; set; } = false;
        public bool? TrueNullableBooleanValue { get; set; } = true;
        public bool? FalseNullableBooleanValue { get; set; } = false;
        public bool? NullNullableBooleanValue { get; set; } = null;

        public int IntValue { get; set; } = 1234;
    }

    /// <summary>
    /// Verify support for the "IsTrue" and "IsFalse" operators.
    /// </summary>
    [Fact]
    public void SupportsIsTrueAndIsFalseOperators()
    {
        var testObject = new Truthy();
        
        Assert.True(TestCriterion(testObject, "TrueValue", "IsTrue"));
        Assert.False(TestCriterion(testObject, "TrueValue", "IsFalse"));

        Assert.False(TestCriterion(testObject, "FalseValue", "IsTrue"));
        Assert.True(TestCriterion(testObject, "FalseValue", "IsFalse"));

        Assert.True(TestCriterion(testObject, "TrueNullableBooleanValue", "IsTrue"));
        Assert.False(TestCriterion(testObject, "TrueNullableBooleanValue", "IsFalse"));

        Assert.False(TestCriterion(testObject, "FalseNullableBooleanValue", "IsTrue"));
        Assert.True(TestCriterion(testObject, "FalseNullableBooleanValue", "IsFalse"));

        Assert.False(TestCriterion(testObject, "NullNullableBooleanValue", "IsTrue"));
        Assert.False(TestCriterion(testObject, "NullNullableBooleanValue", "IsFalse"));

        Assert.Throws<InvalidOperatorForPropertyTypeException>(() =>
        {
            Assert.False(TestCriterion(testObject, "IntValue", "IsTrue"));
        });
    }

    /// <summary>
    /// Verify lenient support for booleans, i.e. "True", "true", and "TRUE" all accepted.
    /// </summary>
    [Fact]
    public void SupportsLenientBooleanStrings()
    {
        {
            var rule = new Rule("")
            {
                new RuleCriterion("TestValue", "Equal", "true")
            };

            var matches = new
            {
                TestValue = true
            };

            var doesNotMatch = new
            {
                TestValue = false
            };

            var rules = new List<Rule>() { rule };
            var rulesEngine = new RuleEvaluator(rules);

            Assert.True(rulesEngine.MatchesRule(matches, rule));
            Assert.Single(rulesEngine.GetMatchingRules(matches));
            Assert.Equal(rulesEngine.GetMatchingRules(matches).First(), rule);

            Assert.False(rulesEngine.MatchesRule(doesNotMatch, rule));
            Assert.Empty(rulesEngine.GetMatchingRules(doesNotMatch));
        }

        {
            var rule = new Rule("")
            {
                new RuleCriterion("TestValue", "Equal", "True")
            };

            var matches = new
            {
                TestValue = true
            };

            var doesNotMatch = new
            {
                TestValue = false
            };

            var rules = new List<Rule>() { rule };
            var rulesEngine = new RuleEvaluator(rules);

            Assert.True(rulesEngine.MatchesRule(matches, rule));
            Assert.Single(rulesEngine.GetMatchingRules(matches));
            Assert.Equal(rulesEngine.GetMatchingRules(matches).First(), rule);

            Assert.False(rulesEngine.MatchesRule(doesNotMatch, rule));
            Assert.Empty(rulesEngine.GetMatchingRules(doesNotMatch));
        }

        {
            var rule = new Rule("")
            {
                new RuleCriterion("TestValue", "Equal", "TRUE")
            };

            var matches = new
            {
                TestValue = true
            };

            var doesNotMatch = new
            {
                TestValue = false
            };

            var rules = new List<Rule>() { rule };
            var rulesEngine = new RuleEvaluator(rules);

            Assert.True(rulesEngine.MatchesRule(matches, rule));
            Assert.Single(rulesEngine.GetMatchingRules(matches));
            Assert.Equal(rulesEngine.GetMatchingRules(matches).First(), rule);

            Assert.False(rulesEngine.MatchesRule(doesNotMatch, rule));
            Assert.Empty(rulesEngine.GetMatchingRules(doesNotMatch));
        }
    }

    internal class VaryingDataTypes
    {
        public string StringValue { get; init; } = "test";
        public string? NullableStringValue { get; init; } = "test";
        public string? NullNullableStringValue { get; init; } = null;

        public int IntValue { get; init; } = 1234;
        public int? NullableIntValue { get; init; } = 1234;
        public int? NullNullableIntValue { get; init; } = null;

        public double DoubleValue { get; init; } = 1234.56;
        public double? NullableDoubleValue { get; init; } = 1234.56;
        public double? NullNullableDoubleValue { get; init; } = null;

        public decimal DecimalValue { get; init; } = 1234.56m;
        public decimal? NullableDecimalValue { get; init; } = 1234.56m;
        public decimal? NullNullableDecimalValue { get; init; } = null;

        public bool BooleanValue { get; init; } = true;
        public bool? NullableBooleanValue { get; init; } = true;
        public bool? NullNullableBooleanValue { get; init; } = null;

        public DateTime DateTimeValue { get; init; } = new DateTime(2000, 01, 01);
        public DateTime? NullableDateTimeValue { get; init; } = new DateTime(2000, 01, 01);
        public DateTime? NullNullableDateTimeValue { get; init; } = null;

        public Guid GuidValue { get; init; } = new Guid("79281f39-6f5f-4010-bb91-23558af088f6");
        public Guid? NullableGuidValue { get; init; } = new Guid("79281f39-6f5f-4010-bb91-23558af088f6");
        public Guid? NullNullableGuidValue { get; init; } = null;
    }

    [Fact]
    public void HandlesVaryingDataTypes()
    {
        var testObject = new VaryingDataTypes();

        Assert.True(TestCriterion(testObject, "StringValue", "Equal", "test"));
        Assert.True(TestCriterion(testObject, "NullableStringValue", "Equal", "test"));
        Assert.False(TestCriterion(testObject, "NullNullableStringValue", "Equal", ""));

        Assert.True(TestCriterion(testObject, "IntValue", "Equal", "1234"));
        Assert.True(TestCriterion(testObject, "NullableIntValue", "Equal", "1234"));
        Assert.False(TestCriterion(testObject, "NullNullableIntValue", "Equal", ""));

        Assert.True(TestCriterion(testObject, "DoubleValue", "Equal", "1234.56"));
        Assert.True(TestCriterion(testObject, "NullableDoubleValue", "Equal", "1234.56"));
        Assert.False(TestCriterion(testObject, "NullNullableDoubleValue", "Equal", ""));

        Assert.True(TestCriterion(testObject, "DecimalValue", "Equal", "1234.56"));
        Assert.True(TestCriterion(testObject, "NullableDecimalValue", "Equal", "1234.56"));
        Assert.False(TestCriterion(testObject, "NullNullableDecimalValue", "Equal", ""));

        Assert.True(TestCriterion(testObject, "BooleanValue", "Equal", "true"));
        Assert.True(TestCriterion(testObject, "NullableBooleanValue", "Equal", "true"));
        Assert.False(TestCriterion(testObject, "NullNullableBooleanValue", "Equal", ""));

        Assert.True(TestCriterion(testObject, "DateTimeValue", "Equal", "2000-01-01"));
        Assert.True(TestCriterion(testObject, "NullableDateTimeValue", "Equal", "2000-01-01"));
        Assert.False(TestCriterion(testObject, "NullNullableDateTimeValue", "Equal", ""));

        Assert.True(TestCriterion(testObject, "GuidValue", "Equal", "79281f39-6f5f-4010-bb91-23558af088f6"));
        Assert.True(TestCriterion(testObject, "NullableGuidValue", "Equal", "79281f39-6f5f-4010-bb91-23558af088f6"));
        Assert.False(TestCriterion(testObject, "NullNullableGuidValue", "Equal", ""));
    }
}
