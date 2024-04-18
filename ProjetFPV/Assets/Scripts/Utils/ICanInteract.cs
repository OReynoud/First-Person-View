using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICanInteract
{
    public void Interact(Vector3 dir);

    public void ShowContext();
}
