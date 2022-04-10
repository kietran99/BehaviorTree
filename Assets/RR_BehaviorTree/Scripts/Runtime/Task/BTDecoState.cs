namespace RR.AI.BehaviorTree
{
    public enum BTDecoState
    {
        SUCCESS,
        FAILURE
    }

    public static class BTDecoStateExtension
    {
        public static BTDecoState ToBTDecoState(this bool val) => val ? BTDecoState.SUCCESS : BTDecoState.FAILURE;
        public static BTDecoState ToBTDecoState(this int val) => val != 0 ? BTDecoState.SUCCESS : BTDecoState.FAILURE;
    }
}
