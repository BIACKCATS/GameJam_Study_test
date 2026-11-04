namespace JSH.Test
{
    public class TestSignal : UnityEngine.MonoBehaviour
    {
        [UnityEngine.SerializeField] private string _testMessage = "Orchestration Pipeline Active!";

        private void Start()
        {
            UnityEngine.Debug.Log(_testMessage);
        }
    }
}
