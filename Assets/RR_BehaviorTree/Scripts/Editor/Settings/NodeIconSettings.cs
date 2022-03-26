namespace RR.AI.BehaviorTree
{
    [System.Serializable]
    public class NodeIconSettings
    {
        public string taskname, icon;

        public override string ToString()
        {
            return $"{taskname}: {icon}";
        }
    }
}
