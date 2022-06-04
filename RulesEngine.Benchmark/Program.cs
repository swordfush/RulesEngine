using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using RulesEngine;
using RulesEngine.Benchmark;

var summary = BenchmarkRunner.Run<RulesBenchmark>();

[MarkdownExporter, AllStatisticsColumn]
public class RulesBenchmark
{
    private readonly Organization organization;
    private readonly List<Rule> rules;

    private readonly RulesEngine.RulesEngine rulesEngine;
    private readonly RulesEngine<Organization> genericRulesEngine;

    public RulesBenchmark()
    {
        this.organization = Organization.FakeData.Generate();
        this.rules = Rule.FakeData.Generate(500);

        this.rulesEngine = new RulesEngine.RulesEngine(this.rules);
        this.genericRulesEngine = new RulesEngine<Organization>(this.rules);
    }

    [Benchmark]
    public RulesEngine.RulesEngine InitializeRulesEngine()
    {
        return new RulesEngine.RulesEngine(this.rules);
    }

    [Benchmark]
    public RulesEngine<Organization> InitializeGenericRulesEngine()
    {
        return new RulesEngine<Organization>(this.rules);
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
