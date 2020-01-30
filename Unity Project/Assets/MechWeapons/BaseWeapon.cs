using System;
using UnityEngine;
using System.Collections;

public abstract class BaseWeapon:MonoBehaviour
{
   public bool isFiring;

   public abstract void OpenFire();

   public abstract void StopFire();
}
