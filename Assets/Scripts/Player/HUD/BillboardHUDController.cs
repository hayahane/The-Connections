using System;
using UnityEngine;

namespace Character.HUD
{
    public class BillboardHUDController : MonoBehaviour
    {
        private void Update()
        {
            transform.forward = -Camera.main.transform.forward;
        }
    }
}