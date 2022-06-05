using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using RulesEngine;
using RulesEngine.Benchmark;

var summary = BenchmarkRunner.Run<RulesBenchmark>();

[MarkdownExporter, AllStatisticsColumn]
public class RulesBenchmark
{
    private readonly Organization organization;
    private readonly List<Rule> rules;

    private readonly RuleEvaluator rulesEngine;
    private readonly RuleEvaluator<Organization> genericRulesEngine;

    public RulesBenchmark()
    {
        this.organization = Organization.FakeData.Generate();
        this.rules = Rule.FakeData.Generate(500);

        this.rulesEngine = new RuleEvaluator(this.rules);
        this.genericRulesEngine = new RuleEvaluator<Organization>(this.rules);
    }

    [Benchmark]
    public RuleEvaluator InitializeRulesEngine()
    {
        return new RuleEvaluator(this.rules);
    }

    [Benchmark]
    public RuleEvaluator<Organization> InitializeGenericRulesEngine()
    {
        return new RuleEvaluator<Organization>(this.rules);
    }

    [Benchmark]
    public List<Rule> MatchRules()
    {
        var result = new List<Rule>();
        result.AddRange(this.rulesEngine.GetMatchingRules(organization).Cast<Rule>());
        return result;
    }

    [Benchmark]
    public List<Rule> MatchRulesGeneric()
    {
        var result = new List<Rule>();
        result.AddRange(this.genericRulesEngine.GetMatchingRules(organization).Cast<Rule>());
        return result;
    }
}
