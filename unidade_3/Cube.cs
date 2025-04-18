// #define _DebugFull

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
  // private Transform Plano;
  private float sinal = 1;

  // Start is called before the first frame update
  void Start()
  {
    // Plano = this.transform.parent;
    // Debug.Log(Plano.transform.position);
  }

  // Update is called once per frame
  void Update()
  {
    if (transform.position.x >= 4.5)
    {
      sinal = -1;
    }
    if (transform.position.x <= -4.5)
    {
      sinal = 1;
    }
    transform.position = transform.position + new Vector3(0.01f * sinal, 0, 0);
#if DEBUG
    Debug.Log(transform.position);
#elif (_DebugFull)
    Debug.Log(Plano.transform.position);
#else
    Debug.Log("release");
#endif
  }
}
