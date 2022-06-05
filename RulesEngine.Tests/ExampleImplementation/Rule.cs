namespace RulesEngine.Tests.ExampleImplementation;

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
