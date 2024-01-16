namespace L58.EPAVR
{
    [System.Serializable]
    public class IntDistributionItem : DistributionItem<int> {}

    public class IntDistribution : Distribution<int, IntDistributionItem> {}
}