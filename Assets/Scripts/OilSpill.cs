using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilSpill : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        RubyController controller = other.GetComponent<RubyController>();

        if (controller != null)
        {
            controller.ChangeCog(-2);
        }
    }
}
