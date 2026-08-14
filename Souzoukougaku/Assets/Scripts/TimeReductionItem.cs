using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class TimeReductionItem : MonoBehaviour
{
    [SerializeField] private float timeReduction = 15f;

    private void Awake()
    {
        GetComponent<SphereCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null)
        {
            return;
        }

        CountdownTimer timer = FindFirstObjectByType<CountdownTimer>();
        if (timer != null)
        {
            timer.ReduceTime(timeReduction);
        }

        Destroy(gameObject);
    }
}
