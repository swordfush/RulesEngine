using System.Collections;

namespace RulesEngine.Tests.ConcreteImplementation;

class Rule : IRule, IEnumerable<IRuleCriterion>
{
    public string Name { get; set; }
    public List<RuleCriterion> Criteria { get; set; }
    IEnumerable<IRuleCriterion> IRule.Criteria { get => Criteria; }

    public Rule(string name)
    {
        Name = name;
        Criteria = new List<RuleCriterion>();
    }

    public Rule(string name, List<RuleCriterion> criteria)
    {
        Name = name;
        Criteria = criteria;
    }

    public void Add(IRuleCriterion rule)
        => this.Criteria.Add((rule as RuleCriterion) ?? throw new ArgumentException($"Rule must be of type {nameof(RuleCriterion)}."));

    public IEnumerator<IRuleCriterion> GetEnumerator()
        => Criteria.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => Criteria.GetEnumerator();
}
