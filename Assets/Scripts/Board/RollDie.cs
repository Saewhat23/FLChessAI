using System.Collections;
using UnityEngine;

/**
 * RollDie
 *
 * Used to apply force and torque to the die object and get result from roll.
 */
public class RollDie : MonoBehaviour
{
    private new Rigidbody rigidbody;
    public bool rolling;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Roll the die, blocking other tasks
    public IEnumerator Roll()
    {
        rolling = true;
        transform.localPosition = new Vector3();
        AddTorque();
        AddForce();
        SoundManger.PlayDiceRollSound();
        yield return new WaitForSeconds(3);
        if (GetResult() == -1)
        {
            yield return Roll(); // If the die lands on its side or between two values, re-roll
        }
        rolling = false;
    }

    // Add random torque to die
    private void AddTorque()
    {
        Vector3 torque = new Vector3();
        torque.x = Random.Range(-180, 180);
        torque.y = Random.Range(-180, 180);
        torque.z = Random.Range(-180, 180);
        rigidbody.AddTorque(torque);
    }

    // Add random force to die
    private void AddForce()
    {
        Vector3 force = new Vector3();
        force.x = Random.Range(-100, 100);
        force.y = Random.Range(50, 200);
        force.z = Random.Range(-100, 100);
        rigidbody.AddForce(force);
    }

    // Get the result from the die
    public int GetResult()
    {
        const float closeEnough = 0.7f; // 1.0 would be facing directly up. If no side is above "closeEnough", return -1 and reroll
        if (Vector3.Dot(transform.forward, Vector3.up) >= closeEnough)
            return 2;
        if (Vector3.Dot(-transform.forward, Vector3.up) >= closeEnough)
            return 1;
        if (Vector3.Dot(transform.up, Vector3.up) >= closeEnough)
            return 3;
        if (Vector3.Dot(-transform.up, Vector3.up) >= closeEnough)
            return 4;
        if (Vector3.Dot(transform.right, Vector3.up) >= closeEnough)
            return 5;
        if (Vector3.Dot(-transform.right, Vector3.up) >= closeEnough)
            return 6;
        return -1;
    }
}
