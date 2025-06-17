using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    short val;
    byte val_byte_1 = 0XFF;
    byte val_byte_2 = 0xAA;
    // Start is called before the first frame update
    void Start()
    {
        byte val1 = val_byte_1;
        val = (short)(val1 | val_byte_2<<8);
        print(Convert.ToString(val, 16));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
