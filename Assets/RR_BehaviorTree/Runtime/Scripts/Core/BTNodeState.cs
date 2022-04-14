namespace RR.AI.BehaviorTree
{
    public enum BTNodeState
    {
        SUCCESS,
        FAILURE,
        RUNNING
    }

    public static class BTNodeStateExtension
    {
        public static BTNodeState ToBTNodeState(this bool val) => val ? BTNodeState.SUCCESS : BTNodeState.FAILURE;
        public static BTNodeState ToBTNodeState(this int val) => val != 0 ? BTNodeState.SUCCESS : BTNodeState.FAILURE;
        public static BTNodeState ToBTNodeState(this BTDecoState val) => val == BTDecoState.SUCCESS ? BTNodeState.SUCCESS : BTNodeState.FAILURE;
    }
}