namespace RR.AI
{
    public class BBEventBroker : Events.EventBroker
    {
        private static BBEventBroker _instance;

        public static BBEventBroker Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = new BBEventBroker();
                return _instance;
            }
        }
    }
}
