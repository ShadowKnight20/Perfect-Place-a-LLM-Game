using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject[] customerPrefabs;
    public Transform spawnPoint;
    public Transform counterPoint;
    public Transform exitPoint;

    private GameObject currentCustomer;

    public void SpawnCustomer()
    {
        if (currentCustomer != null) return;

        int index = Random.Range(0, customerPrefabs.Length);
        GameObject selectedCustomer = customerPrefabs[index];

        currentCustomer = Instantiate(selectedCustomer, spawnPoint.position, Quaternion.identity);
        CustomerMovement moveScript = currentCustomer.GetComponent<CustomerMovement>();
        moveScript.MoveTo(counterPoint.position);
    }

    public void SendCustomerAway()
    {
        if (currentCustomer == null) return;

        CustomerMovement moveScript = currentCustomer.GetComponent<CustomerMovement>();
        moveScript.MoveTo(exitPoint.position);

        // Despawn when they reach the exit
        moveScript.OnDestinationReached = () =>
        {
            Destroy(currentCustomer);
            currentCustomer = null;
        };
    }
}
