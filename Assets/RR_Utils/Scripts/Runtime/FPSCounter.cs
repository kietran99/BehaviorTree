using UnityEngine;
using UnityEngine.UI;

namespace Utils.UI
{
    public class FPSCounter : MonoBehaviour
    {
        // [SerializeField]
        // private Text _currentFPSText = null;

        [SerializeField]
        private Text _scaledFPSText = null;

        [SerializeField]
        private Text _averageFPSText = null;

        [SerializeField]
        private float _refreshTime = .5f;

        int _frameCounter = 0, _totalFrameCounter = 0;
        float _timeCounter = 0f, _lastFramerate = 0f, _totalFPS = 0f; 

        private void Start()
        {
            if (_refreshTime <= 0f)
            {
                Debug.Log("Refresh time must be greater than 0.0");
                gameObject.SetActive(false);
            }
        }

        private void Update() 
        {
            float fps = 1f / Time.unscaledDeltaTime;
            _totalFrameCounter++;
            _totalFPS += fps;
            _averageFPSText.text = Mathf.FloorToInt(_totalFPS / _totalFrameCounter).ToString();
            // _currentFPSText.text = fps.ToString();
            UpdateAvgFPSInfo();
        }

        private void UpdateAvgFPSInfo()
        {
            if (_timeCounter < _refreshTime)
            {
                _timeCounter += Time.unscaledDeltaTime;
                _frameCounter++;
                return;
            }
            
            _lastFramerate = (float)_frameCounter/_timeCounter;
            _frameCounter = 0;
            _timeCounter = 0f;
            _scaledFPSText.text = Mathf.FloorToInt(_lastFramerate).ToString();  
        }
    }
}
