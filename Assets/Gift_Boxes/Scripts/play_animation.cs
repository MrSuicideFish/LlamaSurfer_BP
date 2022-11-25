using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class play_animation : MonoBehaviour
{
    public string Animation_Name;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        animator.Play(Animation_Name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
