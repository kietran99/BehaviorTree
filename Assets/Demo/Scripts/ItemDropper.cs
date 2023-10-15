using UnityEngine;

namespace RR.AI.BehaviorTree
{
    public class ItemDropper : MonoBehaviour
    {
        [SerializeField]
        private GameObject _foodPrefab = null;

        [SerializeField]
        private GameObject _enemyPrefab = null;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                SpawnAtMousePos(_foodPrefab);
                return;
            }

            if (Input.GetMouseButtonDown(1))
            {
                GameObject enemy = SpawnAtMousePos(_enemyPrefab);

                if (!enemy.TryGetComponent<DirectionalMove>(out var move))
                {
                    return;
                }

                GameObject target = GameObject.FindGameObjectWithTag("Actor");

                if (target == null)
                {
                    return;
                }

                move.Towards(target.transform.position);
                return;
            }
        }

        private GameObject SpawnAtMousePos(GameObject prefab)
        {
            Vector3 mousePosOnScreen = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosOnScreen.z = 0.0f;
            GameObject clone = Instantiate(prefab, mousePosOnScreen, Quaternion.identity);
            return clone;
        }
    }
}