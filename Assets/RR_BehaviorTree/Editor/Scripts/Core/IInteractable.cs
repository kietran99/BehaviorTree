using System;

namespace RR.AI.BehaviorTree
{
    public interface IInteractable
    {
        Action MoveStarted { get; set; }
        Action MoveEnded { get; set; }
        Action Selected { get; set; }
    }
}
