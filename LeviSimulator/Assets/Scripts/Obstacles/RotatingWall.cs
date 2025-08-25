using UnityEngine;

public class RotatingWall : MonoBehaviour
{
    public Transform center;
    public float rotationSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        center=GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (center != null)
        {
            center.RotateAround(center.position, Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
