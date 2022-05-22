namespace RR.AI.BehaviorTree
{
    public enum BTNodeState
    {
        Success,
        Failure,
        Running
    }

    public static class BTNodeStateExtension
    {
        public static BTNodeState ToBTNodeState(this bool val) => val ? BTNodeState.Success : BTNodeState.Failure;
        public static BTNodeState ToBTNodeState(this int val) => val != 0 ? BTNodeState.Success : BTNodeState.Failure;
        public static BTNodeState ToBTNodeState(this BTDecoState val) => val == BTDecoState.SUCCESS ? BTNodeState.Success : BTNodeState.Failure;
    }
}