using System.Collections.Generic;
using UnityEngine;

public class RemovableWall : MonoBehaviour
{
    [Header("移动路径点（按照顺序放在Inspecor）")]
    public List<GameObject> points ;
    
    public float speed = 2f;
    private Transform target;
    private int currentIndex=0;
    private int direction = 1;
    

    // Update is called once per frame
    void Update()
    {
        if (points == null || points.Count < 2) return;
        Transform target = points[currentIndex].transform;
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentIndex += direction;
            if (currentIndex >= points.Count)
            {
                currentIndex = points.Count - 2;
                direction = -1;
            }
            else if (currentIndex < 0)
            {
                currentIndex = 1;
                direction = 1;
            }
        }
    }
}
